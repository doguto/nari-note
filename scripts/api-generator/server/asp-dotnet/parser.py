import sys
import importlib.util
from pathlib import Path
from typing import Dict, List, Optional, Tuple

_DIR = Path(__file__).parent
_API_GEN_ROOT = _DIR.parent.parent.parent  # scripts/api-generator/
_SCRIPTS_ROOT = _API_GEN_ROOT.parent       # scripts/

if str(_SCRIPTS_ROOT) not in sys.path:
    sys.path.insert(0, str(_SCRIPTS_ROOT))

from models import CSharpClass, CSharpProperty


def _load(name: str, path: Path):
    spec = importlib.util.spec_from_file_location(name, path)
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


_p = _load("asp_dotnet_patterns", _DIR / "templates" / "patterns.py")
_proto = _load("api_gen_http", _API_GEN_ROOT / "protocols" / "http.py")
HttpApi = _proto.HttpApi
HttpMethod = _proto.HttpMethod
HttpRequest = _proto.HttpRequest
HttpResponse = _proto.HttpResponse
HttpPayload = _proto.HttpPayload
HttpHeader = _proto.HttpHeader
Endpoint = _proto.Endpoint
EndpointEntity = _proto.EndpointEntity
EndpointType = _proto.EndpointType


def _csharp_type_to_typescript(
    csharp_type: str, value_object_types: Dict[str, str]
) -> tuple[str, bool]:
    """C# 型を TypeScript 型へ変換。戻り値: (ts_type, is_nullable)"""
    import re

    type_mapping = {
        "string": "string",
        "int": "number",
        "long": "number",
        "float": "number",
        "double": "number",
        "decimal": "number",
        "bool": "boolean",
        "DateTime": "string",
        "Guid": "string",
    }
    is_nullable = csharp_type.endswith("?")
    base_type = csharp_type.rstrip("?")

    if base_type in value_object_types:
        return value_object_types[base_type], is_nullable

    dict_match = re.match(r"Dictionary<(.+),\s*(.+)>", base_type)
    if dict_match:
        ts_k, _ = _csharp_type_to_typescript(dict_match.group(1).strip(), value_object_types)
        ts_v, _ = _csharp_type_to_typescript(dict_match.group(2).strip(), value_object_types)
        return f"Record<{ts_k}, {ts_v}>", is_nullable

    list_match = re.match(r"List<(.+)>", base_type)
    if list_match:
        inner, _ = _csharp_type_to_typescript(list_match.group(1), value_object_types)
        return f"{inner}[]", is_nullable

    return type_mapping.get(base_type, base_type), is_nullable


class AspDotnetParser:
    """ASP.NET Core Controller / DTO ファイルをパースして HttpApi を生成する"""

    def parse_value_objects(self, value_object_file: Path) -> Dict[str, str]:
        """ValueObject ファイルから struct 名 → TypeScript 型のマッピングを返す"""
        result: Dict[str, str] = {}
        if not value_object_file.exists():
            print(f"⚠️  ValueObject file not found: {value_object_file}")
            return result
        try:
            content = value_object_file.read_text(encoding="utf-8")
            for m in _p.VALUE_OBJECT.finditer(content):
                underlying = m.group(1)
                struct_name = m.group(2)
                result[struct_name] = "string" if underlying == "Guid" else "number"
            if result:
                print(f"📦 Loaded {len(result)} ValueObject types: {', '.join(sorted(result))}")
        except Exception as e:
            print(f"⚠️  Error loading ValueObject types: {e}")
        return result

    def parse_class(self, file_path: Path) -> Optional[CSharpClass]:
        """C# クラスファイルをパースして CSharpClass を返す（内部用）"""
        try:
            content = file_path.read_text(encoding="utf-8")
        except Exception as e:
            print(f"Error reading {file_path}: {e}")
            return None

        class_match = _p.CLASS_NAME.search(content)
        if not class_match:
            return None
        class_name = class_match.group(1)

        namespace_match = _p.NAMESPACE.search(content)
        namespace = namespace_match.group(1) if namespace_match else ""

        properties = []
        for m in _p.PROPERTY.finditer(content):
            prop_type = m.group(2).strip()
            prop_name = m.group(3)
            properties.append(CSharpProperty(
                name=prop_name,
                type=prop_type,
                is_optional=False,
                is_list="List<" in prop_type,
            ))

        return CSharpClass(name=class_name, properties=properties, namespace=namespace)

    def parse_controller(
        self,
        file_path: Path,
        all_request_types: set,
        all_response_types: set,
    ) -> Tuple[List[HttpApi], List[str]]:
        """Controller ファイルをパースして (HttpApi リスト, スキップメソッド名リスト) を返す"""
        try:
            content = file_path.read_text(encoding="utf-8")
        except Exception as e:
            print(f"Error reading {file_path}: {e}")
            return [], []

        ctrl_match = _p.CONTROLLER_CLASS.search(content)
        if not ctrl_match:
            return [], []
        controller_name = ctrl_match.group(1).lower()

        endpoints: List[HttpApi] = []
        skipped: List[str] = []

        for m in _p.HTTP_METHOD_ATTR.finditer(content):
            http_method_str = m.group(1).upper()
            route = m.group(2) or ""
            explicit_response_type = m.group(3)
            function_name = m.group(4)
            parameters = m.group(5)

            request_type: Optional[str] = None
            has_body_param = False
            has_path_params = bool(_p.PATH_PARAM.findall(route))
            is_form_file = False
            form_file_param = "file"

            if parameters:
                fb_match = _p.FROM_BODY.search(parameters)
                if fb_match:
                    request_type = fb_match.group(1)
                    has_body_param = True
                ff_match = _p.FORM_FILE.search(parameters)
                if ff_match:
                    is_form_file = True
                    form_file_param = ff_match.group(1)

            inferred_req = f"{function_name}Request"
            inferred_res = f"{function_name}Response"

            if not request_type and inferred_req in all_request_types:
                if has_body_param or has_path_params or http_method_str == "GET":
                    request_type = inferred_req

            if explicit_response_type:
                response_type: Optional[str] = explicit_response_type
            elif inferred_res in all_response_types:
                response_type = inferred_res
            else:
                if http_method_str == "DELETE" or function_name.lower() in ["logout", "signout"]:
                    response_type = "void"
                else:
                    response_type = None

            path = f"/api/{controller_name}"
            if route:
                path += f"/{route}"

            if response_type:
                if is_form_file:
                    http_request: Optional[HttpRequest] = HttpRequest(
                        body=HttpPayload(type_name=request_type) if request_type else None,
                        header=HttpHeader(content_type="multipart/form-data"),
                    )
                elif has_body_param or (http_method_str != "GET" and request_type):
                    http_request = HttpRequest(
                        body=HttpPayload(type_name=request_type),
                    ) if request_type else None
                elif request_type:
                    http_request = HttpRequest(
                        query=HttpPayload(type_name=request_type),
                    )
                else:
                    http_request = None

                http_response = HttpResponse(
                    payload=HttpPayload(type_name=response_type) if response_type != "void" else None,
                ) if response_type else None

                endpoints.append(HttpApi(
                    method=HttpMethod(http_method_str),
                    endpoint=Endpoint.from_path_string(path),
                    function_name=function_name,
                    controller_name=controller_name,
                    request=http_request,
                    response=http_response,
                    is_form_file=is_form_file,
                    form_file_param=form_file_param,
                ))
            else:
                skipped.append(f"{function_name} (no response type found)")

        return endpoints, skipped

import re
import sys
import importlib.util
from pathlib import Path
from typing import Dict, List, Optional

_DIR = Path(__file__).parent
_API_GEN_ROOT = _DIR.parent.parent.parent  # scripts/api-generator/
_SCRIPTS_ROOT = _API_GEN_ROOT.parent       # scripts/

if str(_SCRIPTS_ROOT) not in sys.path:
    sys.path.insert(0, str(_SCRIPTS_ROOT))

from models import CSharpClass


def _load(name: str, path: Path):
    spec = importlib.util.spec_from_file_location(name, path)
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


_ts = _load("axios_ts_templates", _DIR / "templates" / "typescript.py")
_proto = _load("api_gen_http", _API_GEN_ROOT / "protocols" / "http.py")
HttpApi = _proto.HttpApi
HttpMethod = _proto.HttpMethod


class AxiosGenerator:
    """HttpApi リストから Axios クライアント用 TypeScript コードを生成する"""

    def __init__(self, value_object_types: Dict[str, str], class_map: Dict[str, CSharpClass]):
        self.value_object_types = value_object_types
        self.class_map = class_map

    def generate(self, apis: List) -> str:
        lines: List[str] = [_ts.FILE_HEADER, _ts.IMPORT_HEADER]

        all_types = self._collect_types(apis)
        lines.extend(f"  {t}," for t in sorted(all_types))
        lines.append(_ts.IMPORT_FOOTER)
        lines.append("")

        by_controller = self._group_by_controller(apis)
        for controller, eps in sorted(by_controller.items()):
            lines.append(_ts.CONTROLLER_COMMENT.format(controller=controller.capitalize()))
            lines.append(_ts.CONTROLLER_OPEN.format(controller=controller))
            for ep in eps:
                lines.extend(self._render_function(ep))
            lines.append(_ts.CONTROLLER_CLOSE)
            lines.append("")

        return "\n".join(lines)

    def _render_function(self, ep) -> List[str]:
        lines: List[str] = []
        func_name = self._to_camel(ep.function_name)
        request_type = (
            ep.request.body.type_name if ep.request and ep.request.body else
            ep.request.query.type_name if ep.request and ep.request.query else
            "void"
        )
        response_type = ep.response.payload.type_name if ep.response and ep.response.payload else "void"
        path_param_entities = ep.endpoint.path_params()

        if path_param_entities:
            parts = []
            for entity in ep.endpoint.entities:
                if entity.type is not None:
                    resolved = self._resolve_path_param(entity.name, request_type)
                    parts.append(f"${{data.{resolved}}}")
                else:
                    parts.append(entity.name)
            url_expr = f"`/{'/'.join(parts)}`"
        else:
            url_expr = f"'{ep.endpoint.to_path_string()}'"

        send_body = ep.request is not None and ep.request.body is not None

        if request_type == "void" and path_param_entities:
            request_type = "{ " + ", ".join(
                f"{e.name}: {e.type.value}" for e in path_param_entities
            ) + " }"

        if ep.is_form_file:
            pname = ep.form_file_param
            lines.append(f"  {func_name}: async ({pname}: File): Promise<{response_type}> => {{")
            lines.append(f"    const formData = new FormData();")
            lines.append(f"    formData.append('{pname}', {pname});")
            lines.append(f"    const response = await apiClient.{ep.method.value.lower()}<{response_type}>({url_expr}, formData);")
            lines.append("    return response;")
        elif request_type == "void":
            lines.append(f"  {func_name}: async (): Promise<{response_type}> => {{")
            if ep.method == HttpMethod.DELETE:
                lines.append(f"    await apiClient.delete({url_expr});")
            else:
                lines.append(f"    const response = await apiClient.{ep.method.value.lower()}<{response_type}>({url_expr});")
                lines.append("    return response;")
        else:
            lines.append(f"  {func_name}: async (data: {request_type}): Promise<{response_type}> => {{")
            if ep.method == HttpMethod.DELETE:
                lines.append(f"    await apiClient.delete({url_expr});")
            elif ep.method == HttpMethod.GET:
                if path_param_entities:
                    lines.append(f"    const response = await apiClient.get<{response_type}>({url_expr});")
                else:
                    lines.append(f"    const response = await apiClient.get<{response_type}>({url_expr}, {{ params: data }});")
                lines.append("    return response;")
            else:
                if send_body:
                    lines.append(f"    const response = await apiClient.{ep.method.value.lower()}<{response_type}>({url_expr}, data);")
                else:
                    lines.append(f"    const response = await apiClient.{ep.method.value.lower()}<{response_type}>({url_expr});")
                lines.append("    return response;")

        lines.append("  },")
        return lines

    # --- utilities ---

    def _to_camel(self, name: str) -> str:
        return name[0].lower() + name[1:] if name else name


    def _collect_types(self, apis: List) -> set:
        types: set = set()
        for ep in apis:
            if ep.request:
                if ep.request.body:
                    types.add(ep.request.body.type_name)
                elif ep.request.query:
                    types.add(ep.request.query.type_name)
            if ep.response and ep.response.payload:
                types.add(ep.response.payload.type_name)
        return types

    def _group_by_controller(self, apis: List) -> Dict[str, List]:
        result: Dict[str, List] = {}
        for ep in apis:
            result.setdefault(ep.controller_name, []).append(ep)
        return result

    def _resolve_path_param(self, param: str, request_type: str) -> str:
        camel = self._to_camel(param)
        if request_type not in self.class_map:
            return camel
        req_class = self.class_map[request_type]
        prop_names = [self._to_camel(p.name) for p in req_class.properties]
        if camel == "id":
            if "id" in prop_names:
                return "id"
            id_like = [p for p in prop_names if p.endswith("Id")]
            if len(id_like) == 1:
                return id_like[0]
            candidates = [
                self._to_camel(vo) for vo in self.value_object_types if vo.endswith("Id")
            ]
            for c in candidates:
                if c in prop_names:
                    return c
        return camel

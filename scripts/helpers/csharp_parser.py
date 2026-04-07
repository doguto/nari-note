"""C# code parser for converting to TypeScript."""

import re
from pathlib import Path
from typing import List, Dict, Tuple, Optional

from models import CSharpProperty, CSharpClass, EndpointInfo


def load_value_object_types(value_object_file: Path) -> set[str]:
    """Load ValueObject types from C# file."""
    value_object_types = set()

    if not value_object_file.exists():
        print(f"⚠️  ValueObject file not found: {value_object_file}")
        return value_object_types

    try:
        content = value_object_file.read_text(encoding='utf-8')
        # [ValueObject<int>(Conversions.EfCoreValueConverter)] public partial struct XxxId; のパターンを探す
        pattern = r'\[ValueObject<\w+>(?:\([^)]+\))?\]\s+public\s+partial\s+struct\s+(\w+);'
        for match in re.finditer(pattern, content):
            type_name = match.group(1)
            value_object_types.add(type_name)

        if value_object_types:
            print(f"📦 Loaded {len(value_object_types)} ValueObject types: {', '.join(sorted(value_object_types))}")
    except Exception as e:
        print(f"⚠️  Error loading ValueObject types: {e}")

    return value_object_types


def csharp_type_to_typescript(csharp_type: str, value_object_types: set[str]) -> tuple[str, bool]:
    """Convert C# type to TypeScript type.

    Returns:
        tuple: (typescript_type, is_nullable)
    """
    type_mapping = {
        'string': 'string',
        'int': 'number',
        'long': 'number',
        'float': 'number',
        'double': 'number',
        'decimal': 'number',
        'bool': 'boolean',
        'DateTime': 'string',
        'Guid': 'string',
    }

    # Nullable型（?）をチェック
    is_nullable = csharp_type.endswith('?')
    base_type = csharp_type.rstrip('?')

    # ValueObject型をnumberに変換
    if base_type in value_object_types:
        return 'number', is_nullable

    # Dictionary<TKey, TValue> を Record<K, V> に変換
    dict_match = re.match(r'Dictionary<(.+),\s*(.+)>', base_type)
    if dict_match:
        key_type = dict_match.group(1).strip()
        value_type = dict_match.group(2).strip()
        # 再帰的に内部の型も変換
        ts_key, _ = csharp_type_to_typescript(key_type, value_object_types)
        ts_value, _ = csharp_type_to_typescript(value_type, value_object_types)
        return f"Record<{ts_key}, {ts_value}>", is_nullable

    # List<T> を T[] に変換
    list_match = re.match(r'List<(.+)>', base_type)
    if list_match:
        inner_type = list_match.group(1)
        converted, _ = csharp_type_to_typescript(inner_type, value_object_types)
        return f"{converted}[]", is_nullable

    ts_type = type_mapping.get(base_type, base_type)
    return ts_type, is_nullable


def parse_csharp_class(file_path: Path) -> Optional[CSharpClass]:
    """C#クラスファイルをパースしてクラス情報を抽出"""
    try:
        content = file_path.read_text(encoding='utf-8')
    except Exception as e:
        print(f"Error reading {file_path}: {e}")
        return None

    # クラス名を抽出
    class_match = re.search(r'public class (\w+)', content)
    if not class_match:
        return None
    class_name = class_match.group(1)

    # 名前空間を抽出
    namespace_match = re.search(r'namespace ([^;]+)', content)
    namespace = namespace_match.group(1) if namespace_match else ""

    # プロパティを抽出
    properties = []
    # int, DateTime, List<T>などをサポート、requiredキーワードとnullable型(?）を扱う
    # required キーワードをグループ1でキャプチャし、型をグループ2、プロパティ名をグループ3でキャプチャ
    property_pattern = r'public\s+(required\s+)?([\w<>,\s?]+?)\s+(\w+)\s*\{\s*get;\s*set;\s*\}'
    for match in re.finditer(property_pattern, content):
        prop_type = match.group(2).strip()
        prop_name = match.group(3)

        is_list = 'List<' in prop_type
        # nullable判定は csharp_type_to_typescript が型末尾の ? で行うため、
        # is_optional は常に False とする（required の有無に関わらず）
        is_optional = False

        properties.append(CSharpProperty(
            name=prop_name,
            type=prop_type,
            is_optional=is_optional,
            is_list=is_list
        ))

    return CSharpClass(name=class_name, properties=properties, namespace=namespace)


def parse_controller(file_path: Path, all_request_types: set, all_response_types: set) -> Tuple[List[EndpointInfo], List[str]]:
    """コントローラーをパースしてエンドポイント情報とスキップされたメソッドを返す"""
    try:
        content = file_path.read_text(encoding='utf-8')
    except Exception as e:
        print(f"Error reading {file_path}: {e}")
        return [], []

    # コントローラー名を抽出
    controller_match = re.search(r'public class (\w+)Controller', content)
    if not controller_match:
        return [], []
    controller_name = controller_match.group(1).lower()

    endpoints = []
    skipped_methods = []

    # エンドポイントを抽出
    # [HttpGet], [HttpPost]などのアトリビュートとメソッド、パラメータを見つける
    # 複数の属性（[RequireAuth]、[AllowAnonymous]、[OptionalAuth]、[ValidateModelState]など）に対応
    method_pattern = r'\[Http(Get|Post|Put|Delete)(?:\("([^"]+)"\))?\](?:\s*\[\w+\])*\s+public\s+async\s+Task<ActionResult(?:<(\w+)>)?>\s+(\w+)\s*\(([^)]*)\)'

    for match in re.finditer(method_pattern, content):
        http_method = match.group(1).upper()
        route = match.group(2) or ""
        explicit_response_type = match.group(3)
        function_name = match.group(4)
        parameters = match.group(5)

        # パラメータからリクエスト型を抽出
        request_type = None
        has_body_param = False
        has_path_params = bool(re.findall(r'\{(\w+)\}', route or ""))

        if parameters:
            # [FromBody] XxxRequest のパターンを探す
            from_body_match = re.search(r'\[FromBody\]\s+(\w+Request)\s+\w+', parameters)
            if from_body_match:
                request_type = from_body_match.group(1)
                has_body_param = True

        # メソッド名からRequest/Response型を推測
        inferred_request = f"{function_name}Request"
        inferred_response = f"{function_name}Response"

        # リクエスト型: 明示的に指定されていない場合、推測した型が存在すれば使用
        # GET メソッドでパスパラメータがある場合も Request DTO があれば使用
        if not request_type and inferred_request in all_request_types:
            if has_body_param or has_path_params or http_method == "GET":
                request_type = inferred_request

        # レスポンス型: 明示的に指定されている場合はそれを使用、なければ推測
        if explicit_response_type:
            response_type = explicit_response_type
        elif inferred_response in all_response_types:
            response_type = inferred_response
        else:
            # ActionResultでジェネリック型がない場合は推測を試みる
            # DELETE/Logout などで NoContent を返す場合は void として扱う
            if http_method == "DELETE" or function_name.lower() in ["logout", "signout"]:
                response_type = "void"
            elif inferred_response in all_response_types:
                response_type = inferred_response
                skipped_methods.append(f"{function_name} (inferred response: {inferred_response})")
            else:
                response_type = None

        # ルートパスを構築
        path = f"/api/{controller_name}"
        if route:
            path += f"/{route}"

        if response_type:
            endpoints.append(EndpointInfo(
                method=http_method,
                path=path,
                function_name=function_name,
                request_type=request_type,
                response_type=response_type,
                controller_name=controller_name,
                has_body_param=has_body_param
            ))
        else:
            skipped_methods.append(f"{function_name} (no response type found)")

    return endpoints, skipped_methods

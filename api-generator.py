#!/usr/bin/env python3

import os
import re
from pathlib import Path
from typing import List, Dict, Tuple, Optional
from dataclasses import dataclass
from templates import (
    TYPES_HEADER, ENDPOINTS_HEADER, HOOKS_HEADER,
    generate_interface_declaration, generate_property_declaration,
    generate_api_function, generate_query_hook, generate_mutation_hook
)

# 設定
BACKEND_ROOT = Path("nari-note-backend/Src")
FRONTEND_API_DIR = Path("nari-note-frontend/src/lib/api")
CONTROLLER_DIR = BACKEND_ROOT / "Controller"
REQUEST_DIR = BACKEND_ROOT / "Application/Dto/Request"
RESPONSE_DIR = BACKEND_ROOT / "Application/Dto/Response"
DTO_DIR = BACKEND_ROOT / "Application/Dto"


@dataclass
class CSharpProperty:
    """C#のプロパティ情報"""
    name: str
    type: str
    is_optional: bool = False
    is_list: bool = False


@dataclass
class CSharpClass:
    """C#のクラス情報"""
    name: str
    properties: List[CSharpProperty]
    namespace: str


@dataclass
class EndpointInfo:
    """エンドポイント情報"""
    method: str  # GET, POST, PUT, DELETE
    path: str
    function_name: str
    request_type: Optional[str]
    response_type: Optional[str]
    controller_name: str
    has_body_param: bool = False  # [FromBody]パラメータがあるかどうか


def csharp_type_to_typescript(csharp_type: str) -> tuple[str, bool]:
    """C#の型をTypeScriptの型に変換（型とnullable情報を返す）"""
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
    
    # Dictionary<TKey, TValue> を Record<K, V> に変換
    dict_match = re.match(r'Dictionary<(.+),\s*(.+)>', base_type)
    if dict_match:
        key_type = dict_match.group(1).strip()
        value_type = dict_match.group(2).strip()
        # 再帰的に内部の型も変換
        ts_key, _ = csharp_type_to_typescript(key_type)
        ts_value, _ = csharp_type_to_typescript(value_type)
        return f"Record<{ts_key}, {ts_value}>", is_nullable
    
    # List<T> を T[] に変換
    list_match = re.match(r'List<(.+)>', base_type)
    if list_match:
        inner_type = list_match.group(1)
        converted, _ = csharp_type_to_typescript(inner_type)
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
    # 改良版: int, DateTime, List<T>などをサポート、requiredキーワードとnullable型(?）を扱う
    property_pattern = r'public\s+(?:required\s+)?([\w<>,\s?]+?)\s+(\w+)\s*\{\s*get;\s*set;\s*\}'
    for match in re.finditer(property_pattern, content):
        prop_type = match.group(1).strip()
        prop_name = match.group(2)
        
        is_list = 'List<' in prop_type
        is_optional = '= string.Empty' not in content or '= new()' in content or '= false' not in content
        
        properties.append(CSharpProperty(
            name=prop_name,
            type=prop_type,
            is_optional=is_optional,
            is_list=is_list
        ))
    
    return CSharpClass(name=class_name, properties=properties, namespace=namespace)


def parse_controller(file_path: Path, all_request_types: set, all_response_types: set) -> List[EndpointInfo]:
    """コントローラーファイルをパースしてエンドポイント情報を抽出
    
    Args:
        file_path: コントローラーファイルのパス
        all_request_types: 利用可能なRequest型の集合
        all_response_types: 利用可能なResponse型の集合
    """
    try:
        content = file_path.read_text(encoding='utf-8')
    except Exception as e:
        print(f"Error reading {file_path}: {e}")
        return []

    # コントローラー名を抽出
    controller_match = re.search(r'public class (\w+)Controller', content)
    if not controller_match:
        return []
    controller_name = controller_match.group(1).lower()

    endpoints = []
    
    # エンドポイントを抽出（改良版）
    # [HttpGet], [HttpPost]などのアトリビュートとメソッド、パラメータを見つける
    method_pattern = r'\[Http(Get|Post|Put|Delete)(?:\("([^"]+)"\))?\]\s+(?:\[ValidateModelState\]\s+)?public\s+async\s+Task<ActionResult(?:<(\w+)>)?>\s+(\w+)\s*\(([^)]*)\)'

    for match in re.finditer(method_pattern, content):
        http_method = match.group(1).upper()
        route = match.group(2) or ""
        explicit_response_type = match.group(3)
        function_name = match.group(4)
        parameters = match.group(5)
        
        # パラメータからリクエスト型を抽出
        request_type = None
        has_body_param = False
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
        if not request_type and inferred_request in all_request_types:
            request_type = inferred_request
        
        # レスポンス型: 明示的に指定されている場合はそれを使用、なければ推測
        if explicit_response_type:
            response_type = explicit_response_type
        elif inferred_response in all_response_types:
            response_type = inferred_response
        else:
            response_type = None
        
        # ルートパスを構築
        path = f"/api/{controller_name}"
        if route:
            path += f"/{route}"
        
        endpoints.append(EndpointInfo(
            method=http_method,
            path=path,
            function_name=function_name,
            request_type=request_type,
            response_type=response_type,
            controller_name=controller_name,
            has_body_param=has_body_param
        ))
    
    return endpoints


def generate_types_file(classes: List[CSharpClass]) -> str:
    """types.tsファイルを生成"""
    lines = [TYPES_HEADER, ""]
    
    for cls in sorted(classes, key=lambda x: x.name):
        lines.append(generate_interface_declaration(cls.name, bool(cls.properties)))
        for prop in cls.properties:
            ts_type, is_nullable = csharp_type_to_typescript(prop.type)
            is_optional = is_nullable or prop.is_optional
            lines.append(generate_property_declaration(prop.name, ts_type, is_optional))
        lines.append("}")
        lines.append("")
    
    return "\n".join(lines)


def generate_endpoints_file(endpoints: List[EndpointInfo], classes: List[CSharpClass]) -> str:
    """endpoints.tsファイルを生成
    
    Args:
        endpoints: エンドポイント情報のリスト
        classes: C#クラス情報のリスト（リクエスト型のプロパティを調べるため）
    """
    lines = [ENDPOINTS_HEADER]
    
    # クラス情報を名前でマッピング
    class_map = {cls.name: cls for cls in classes}
    
    # 型のインポートを追加
    all_types = set()
    for ep in endpoints:
        if ep.request_type:
            all_types.add(ep.request_type)
        if ep.response_type:
            all_types.add(ep.response_type)
    
    for type_name in sorted(all_types):
        lines.append(f"  {type_name},")
    lines.append("} from './types';")
    lines.append("")
    
    # コントローラー別にグループ化
    by_controller: Dict[str, List[EndpointInfo]] = {}
    for ep in endpoints:
        if ep.controller_name not in by_controller:
            by_controller[ep.controller_name] = []
        by_controller[ep.controller_name].append(ep)
    
    # 各コントローラーのAPI関数を生成
    for controller, eps in sorted(by_controller.items()):
        lines.append(f"// {controller.capitalize()} API")
        lines.append(f"export const {controller}Api = {{")
        
        for ep in eps:
            func_name = ep.function_name[0].lower() + ep.function_name[1:]
            request_type = ep.request_type or "void"
            response_type = ep.response_type or "void"
            
            # パスパラメータを検出
            path_params = re.findall(r'\{(\w+)\}', ep.path)
            
            # パスパラメータがある場合、テンプレートリテラルを使用
            if path_params:
                # リクエストクラスのプロパティを取得してマッピング
                url_path = ep.path
                for param in path_params:
                    # キャメルケース化（id -> id, authorId -> authorId）
                    camel_param = param[0].lower() + param[1:] if param else param
                    
                    # リクエストクラスがある場合、プロパティ名を確認
                    if request_type in class_map:
                        req_class = class_map[request_type]
                        # プロパティ名（キャメルケース）を探す
                        prop_names = [prop.name[0].lower() + prop.name[1:] for prop in req_class.properties]
                        
                        # {id} の場合、articleId や id などのプロパティを探す
                        if camel_param == 'id':
                            # コントローラー名に基づいた候補を探す
                            candidates = [f'{ep.controller_name}Id', 'articleId', 'userId', 'id']
                            for candidate in candidates:
                                if candidate in prop_names:
                                    camel_param = candidate
                                    break
                        
                    url_path = url_path.replace(f'{{{param}}}', f'${{data.{camel_param}}}')
                url_expression = f"`{url_path}`"
            else:
                url_expression = f"'{ep.path}'"
            
            # API関数を生成
            send_body = ep.has_body_param or (not path_params and request_type != "void")
            func_lines = generate_api_function(
                func_name, request_type, response_type,
                ep.method, url_expression, path_params, send_body
            )
            lines.extend(func_lines)

        lines.append("};")
        lines.append("")
    
    return "\n".join(lines)


def generate_hooks_file(endpoints: List[EndpointInfo]) -> str:
    """hooks.tsファイルを生成"""
    lines = [HOOKS_HEADER]
    
    # コントローラーごとにグループ化
    by_controller: Dict[str, List[EndpointInfo]] = {}
    for ep in endpoints:
        if ep.controller_name not in by_controller:
            by_controller[ep.controller_name] = []
        by_controller[ep.controller_name].append(ep)
    
    # インポート文を生成
    controller_imports = ", ".join([f"{c}Api" for c in sorted(by_controller.keys())])
    lines.append(f"import {{ {controller_imports} }} from './endpoints';")
    
    # 型のインポート
    all_types = set()
    for ep in endpoints:
        if ep.request_type:
            all_types.add(ep.request_type)
        if ep.response_type:
            all_types.add(ep.response_type)
    
    if all_types:
        lines.append("import type {")
        for type_name in sorted(all_types):
            lines.append(f"  {type_name},")
        lines.append("} from './types';")
    lines.append("")
    
    # Query Keysを生成
    lines.append("// Query Keys")
    lines.append("export const queryKeys = {")
    for controller in sorted(by_controller.keys()):
        lines.append(f"  {controller}: {{")
        for ep in by_controller[controller]:
            if ep.method == "GET":
                func_name = ep.function_name[0].lower() + ep.function_name[1:]
                lines.append(f"    {func_name}: ['{controller}', '{func_name}'] as const,")
        lines.append("  },")
    lines.append("};")
    lines.append("")
    
    # フックを生成
    for controller, eps in sorted(by_controller.items()):
        lines.append(f"// {controller.capitalize()} Hooks")
        for ep in eps:
            func_name = ep.function_name[0].lower() + ep.function_name[1:]
            hook_name = f"use{ep.function_name}"
            
            if ep.method == "GET":
                # Query hook
                return_type = ep.response_type or "void"
                request_type = ep.request_type or "void"
                hook_lines = generate_query_hook(hook_name, func_name, controller, request_type, return_type)
                lines.extend(hook_lines)
                lines.append("")
            else:
                # Mutation hook
                request_type = ep.request_type or "void"
                response_type = ep.response_type or "void"
                hook_lines = generate_mutation_hook(hook_name, func_name, controller, request_type, response_type)
                lines.extend(hook_lines)
                lines.append("")
    
    return "\n".join(lines)


def main():
    """メイン処理"""
    print("🚀 API Generator - Starting...")
    
    # ディレクトリの存在確認
    if not BACKEND_ROOT.exists():
        print(f"❌ Backend directory not found: {BACKEND_ROOT}")
        return
    
    # Request/Response/DTOクラスをパース
    print("\n📖 Parsing Request/Response/DTO classes...")
    classes: List[CSharpClass] = []
    all_request_types: set = set()
    all_response_types: set = set()
    
    for dir_path in [REQUEST_DIR, RESPONSE_DIR, DTO_DIR]:
        if not dir_path.exists():
            print(f"⚠️  Directory not found: {dir_path}")
            continue
        
        for file_path in dir_path.glob("*.cs"):
            cls = parse_csharp_class(file_path)
            if cls:
                classes.append(cls)
                print(f"  ✓ {cls.name}")
                # Request/Response型を収集
                if cls.name.endswith("Request"):
                    all_request_types.add(cls.name)
                elif cls.name.endswith("Response"):
                    all_response_types.add(cls.name)
    
    print(f"\n📊 Found {len(all_request_types)} Request types, {len(all_response_types)} Response types")
    
    # コントローラーをパース
    print("\n📖 Parsing Controllers...")
    all_endpoints: List[EndpointInfo] = []
    
    if CONTROLLER_DIR.exists():
        for file_path in CONTROLLER_DIR.glob("*Controller.cs"):
            endpoints = parse_controller(file_path, all_request_types, all_response_types)
            all_endpoints.extend(endpoints)
            if endpoints:
                print(f"  ✓ {file_path.name}: {len(endpoints)} endpoints")
    
    # 出力ディレクトリを作成
    FRONTEND_API_DIR.mkdir(parents=True, exist_ok=True)
    
    # 既存ファイルの確認
    types_file = FRONTEND_API_DIR / "types.ts"
    endpoints_file = FRONTEND_API_DIR / "endpoints.ts"
    hooks_file = FRONTEND_API_DIR / "hooks.ts"
    
    existing_files = [f for f in [types_file, endpoints_file, hooks_file] if f.exists()]
    
    if existing_files:
        print("\n⚠️  警告: 以下のファイルが既に存在します:")
        for f in existing_files:
            print(f"   - {f}")
        print("\n生成を続行すると、これらのファイルが上書きされます。")
        response = input("続行しますか？ (y/N): ").strip().lower()
        if response not in ['y', 'yes']:
            print("生成を中止しました。")
            return
    
    # types.tsを生成
    print("\n✏️  Generating types.ts...")
    types_content = generate_types_file(classes)
    types_file.write_text(types_content, encoding='utf-8')
    print(f"  ✓ {types_file}")
    
    # endpoints.tsを生成
    print("\n✏️  Generating endpoints.ts...")
    endpoints_content = generate_endpoints_file(all_endpoints, classes)
    endpoints_file.write_text(endpoints_content, encoding='utf-8')
    print(f"  ✓ {endpoints_file}")
    
    # hooks.tsを生成（骨組みのみ）
    print("\n✏️  Generating hooks.ts template...")
    hooks_content = generate_hooks_file(all_endpoints)
    hooks_file.write_text(hooks_content, encoding='utf-8')
    print(f"  ✓ {hooks_file}")
    
    print("\n✅ API generation completed!")
    print("\n📝 Note: Generated files are committed to git.")
    print("   Review and customize them as needed for your project.")
    print(f"\n📊 Summary:")
    print(f"   - {len(classes)} types generated")
    print(f"   - {len(all_endpoints)} endpoints found")


if __name__ == "__main__":
    main()

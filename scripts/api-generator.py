#!/usr/bin/env python3

import os
import re
import sys
import argparse
from pathlib import Path
from typing import List, Dict, Tuple, Optional
from templates import (
    TYPES_HEADER, ENDPOINTS_HEADER, HOOKS_HEADER, SERVER_HEADER,
    generate_interface_declaration, generate_property_declaration,
    generate_api_function, generate_query_hook, generate_mutation_hook,
    generate_server_function
)
from models import CSharpProperty, CSharpClass, EndpointInfo
from helpers import (
    load_value_object_types,
    csharp_type_to_typescript,
    parse_csharp_class,
    parse_controller
)

# 設定
BACKEND_ROOT = Path("nari-note-backend/Src")
FRONTEND_API_DIR = Path("nari-note-frontend/src/lib/api")
CONTROLLER_DIR = BACKEND_ROOT / "Controller"
REQUEST_DIR = BACKEND_ROOT / "Application/Dto/Request"
RESPONSE_DIR = BACKEND_ROOT / "Application/Dto/Response"
DTO_DIR = BACKEND_ROOT / "Application/Dto"
VALUE_OBJECT_FILE = BACKEND_ROOT / "Domain/ValueObject/EntityKeyObject.cs"


def generate_types_file(classes: List[CSharpClass], value_object_types: set[str]) -> str:
    lines = [TYPES_HEADER, ""]
    
    for cls in sorted(classes, key=lambda x: x.name):
        lines.append(generate_interface_declaration(cls.name, bool(cls.properties)))
        for prop in cls.properties:
            ts_type, is_nullable = csharp_type_to_typescript(prop.type, value_object_types)
            is_optional = is_nullable or prop.is_optional
            lines.append(generate_property_declaration(prop.name, ts_type, is_optional))
        lines.append("}")
        lines.append("")
    
    return "\n".join(lines)


def generate_endpoints_file(endpoints: List[EndpointInfo], classes: List[CSharpClass], value_object_types: set[str]) -> str:
    lines = [ENDPOINTS_HEADER]
    
    # クラス情報を名前でマッピング
    class_map = {cls.name: cls for cls in classes}
    
    # 型のインポートを追加
    all_types = set()
    for ep in endpoints:
        if ep.request_type and ep.request_type != "void":
            all_types.add(ep.request_type)
        if ep.response_type and ep.response_type != "void":
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
                        
                        # {id} の場合のより堅牢な推測:
                        # 1) 同名プロパティがあればそれを使う
                        # 2) requestに "*Id" がちょうど1つならそれを使う
                        # 3) ValueObject型から候補を生成してマッチング
                        if camel_param == 'id':
                            if 'id' in prop_names:
                                camel_param = 'id'
                            else:
                                id_like = [p for p in prop_names if p.endswith('Id')]
                                if len(id_like) == 1:
                                    camel_param = id_like[0]
                                else:
                                    # ValueObject型から候補を動的に生成 (ArticleId -> articleId)
                                    candidates = [vo_type[0].lower() + vo_type[1:] for vo_type in value_object_types if vo_type.endswith('Id')]
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
        if ep.request_type and ep.request_type != "void":
            all_types.add(ep.request_type)
        if ep.response_type and ep.response_type != "void":
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


def generate_server_file(endpoints: List[EndpointInfo], classes: List[CSharpClass], value_object_types: set[str]) -> str:
    """Server-side fetch関数を生成（GETエンドポイントのみ）"""
    lines = [SERVER_HEADER]

    # クラス情報を名前でマッピング
    class_map = {cls.name: cls for cls in classes}

    # 型のインポートを追加（GETエンドポイントのみ）
    all_types = set()
    get_endpoints = [ep for ep in endpoints if ep.method == "GET"]

    for ep in get_endpoints:
        if ep.request_type and ep.request_type != "void":
            all_types.add(ep.request_type)
        if ep.response_type and ep.response_type != "void":
            all_types.add(ep.response_type)

    for type_name in sorted(all_types):
        lines.append(f"  {type_name},")
    lines.append("} from './types';")
    lines.append("")

    # コントローラー別にグループ化
    by_controller: Dict[str, List[EndpointInfo]] = {}
    for ep in get_endpoints:
        if ep.controller_name not in by_controller:
            by_controller[ep.controller_name] = []
        by_controller[ep.controller_name].append(ep)

    # 各コントローラーのServer関数を生成
    for controller, eps in sorted(by_controller.items()):
        lines.append(f"// {controller.capitalize()} Server Functions")

        for ep in eps:
            func_name = ep.function_name[0].lower() + ep.function_name[1:]
            request_type = ep.request_type or "void"
            response_type = ep.response_type or "void"

            # パスパラメータを検出
            path_params = re.findall(r'\{(\w+)\}', ep.path)

            # パスパラメータがある場合、テンプレートリテラルを使用
            if path_params:
                url_path = ep.path
                for param in path_params:
                    camel_param = param[0].lower() + param[1:] if param else param

                    # リクエストクラスがある場合、プロパティ名を確認
                    if request_type in class_map:
                        req_class = class_map[request_type]
                        prop_names = [prop.name[0].lower() + prop.name[1:] for prop in req_class.properties]

                        if camel_param == 'id':
                            if 'id' in prop_names:
                                camel_param = 'id'
                            else:
                                id_like = [p for p in prop_names if p.endswith('Id')]
                                if len(id_like) == 1:
                                    camel_param = id_like[0]
                                else:
                                    # ValueObject型から候補を動的に生成 (ArticleId -> articleId)
                                    candidates = [vo_type[0].lower() + vo_type[1:] for vo_type in value_object_types if vo_type.endswith('Id')]
                                    for candidate in candidates:
                                        if candidate in prop_names:
                                            camel_param = candidate
                                            break
                    url_path = url_path.replace(f'{{{param}}}', f'${{params.{camel_param}}}')
                url_expression = f"`{url_path}`"
            else:
                url_expression = f"'{ep.path}'"

            # Server関数を生成
            func_lines = generate_server_function(
                func_name, request_type, response_type,
                url_expression, path_params
            )
            lines.extend(func_lines)
            lines.append("")

    return "\n".join(lines)


def main():
    # コマンドライン引数をパース
    parser = argparse.ArgumentParser(
        description='Generate TypeScript API definitions from C# backend code'
    )
    parser.add_argument(
        '--force', '-f',
        action='store_true',
        help='Force regeneration of all files'
    )
    parser.add_argument(
        '--verbose', '-v',
        action='store_true',
        help='Show detailed parsing information'
    )
    args = parser.parse_args()
    
    print("🚀 API Generator - Starting...")
    if args.force:
        print("⚡ Force mode enabled - will regenerate all files")
    
    # ディレクトリの存在確認
    if not BACKEND_ROOT.exists():
        print(f"❌ Backend directory not found: {BACKEND_ROOT}")
        return
    
    # ValueObject型を読み込み
    print("\n📦 Loading ValueObject types...")
    value_object_types = load_value_object_types(VALUE_OBJECT_FILE)
    
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
    all_skipped: Dict[str, List[str]] = {}
    
    if CONTROLLER_DIR.exists():
        for file_path in CONTROLLER_DIR.glob("*Controller.cs"):
            endpoints, skipped = parse_controller(file_path, all_request_types, all_response_types)
            all_endpoints.extend(endpoints)
            if skipped:
                all_skipped[file_path.name] = skipped
            if endpoints:
                print(f"  ✓ {file_path.name}: {len(endpoints)} endpoints")
            if args.verbose and skipped:
                print(f"    ⚠️  Skipped {len(skipped)} methods (missing response types)")
    
    # 出力ディレクトリを作成
    FRONTEND_API_DIR.mkdir(parents=True, exist_ok=True)

    # 出力先
    types_file = FRONTEND_API_DIR / "types.ts"
    endpoints_file = FRONTEND_API_DIR / "endpoints.ts"
    hooks_file = FRONTEND_API_DIR / "hooks.ts"
    server_file = FRONTEND_API_DIR / "server.ts"

    # 何も検出できない場合は上書きを避ける（空ファイル化の防止）
    if len(classes) == 0 and len(all_endpoints) == 0:
        print("\n❌ No DTO classes or endpoints detected. Aborting to avoid overwriting with empty content.")
        return

    # types.ts を生成（クラスがある場合のみ）
    if len(classes) > 0:
        print("\n✏️  Generating types.ts (overwrite)...")
        types_content = generate_types_file(classes, value_object_types)
        types_file.write_text(types_content, encoding='utf-8')
        print(f"  ✓ {types_file}")
    else:
        print("\n↷  Skip types.ts (no classes found)")

    # endpoints.ts を生成（エンドポイントがある場合のみ）
    if len(all_endpoints) > 0:
        print("\n✏️  Generating endpoints.ts (overwrite)...")
        endpoints_content = generate_endpoints_file(all_endpoints, classes, value_object_types)
        endpoints_file.write_text(endpoints_content, encoding='utf-8')
        print(f"  ✓ {endpoints_file}")

        # hooks.ts を生成
        print("\n✏️  Generating hooks.ts (overwrite)...")
        hooks_content = generate_hooks_file(all_endpoints)
        hooks_file.write_text(hooks_content, encoding='utf-8')
        print(f"  ✓ {hooks_file}")

        # server.ts を生成 (GETエンドポイントのみ)
        get_endpoints = [ep for ep in all_endpoints if ep.method == "GET"]
        if get_endpoints:
            print("\n✏️  Generating server.ts (overwrite)...")
            server_content = generate_server_file(all_endpoints, classes, value_object_types)
            server_file.write_text(server_content, encoding='utf-8')
            print(f"  ✓ {server_file}")
            print(f"     ({len(get_endpoints)} GET endpoints)")
        else:
            print("\n↷  Skip server.ts (no GET endpoints found)")
    else:
        print("\n↷  Skip endpoints.ts/hooks.ts/server.ts (no endpoints found)")

    print("\n✅ API generation completed!")
    print(f"\n📊 Summary:")
    print(f"   - {len(classes)} types generated")
    print(f"   - {len(all_endpoints)} endpoints found")
    get_endpoints = [ep for ep in all_endpoints if ep.method == "GET"]
    if get_endpoints:
        print(f"   - {len(get_endpoints)} server-side fetch functions generated")
    
    # スキップされたメソッドを報告
    if all_skipped:
        print(f"\n⚠️  Skipped methods (missing response type definitions):")
        for controller, methods in sorted(all_skipped.items()):
            print(f"   {controller}:")
            for method in methods:
                print(f"     - {method}")
        print("\n💡 Tip: Add explicit ActionResult<TResponse> types or create Response DTOs")


if __name__ == "__main__":
    main()

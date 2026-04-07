#!/usr/bin/env python3

import os
import sys
import argparse
from pathlib import Path
from typing import List, Dict
from templates import TypesTemplate, EndpointsTemplate, HooksTemplate, ServerTemplate
from models import CSharpClass, EndpointInfo
from helpers import (
    load_value_object_types,
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

    # テンプレートインスタンスを作成
    class_map = {cls.name: cls for cls in classes}
    types_template = TypesTemplate(value_object_types)
    endpoints_template = EndpointsTemplate(value_object_types, class_map)
    hooks_template = HooksTemplate(value_object_types)
    server_template = ServerTemplate(value_object_types, class_map)

    # types.ts を生成（クラスがある場合のみ）
    if len(classes) > 0:
        print("\n✏️  Generating types.ts (overwrite)...")
        types_content = types_template.generate(classes=classes)
        types_file.write_text(types_content, encoding='utf-8')
        print(f"  ✓ {types_file}")
    else:
        print("\n↷  Skip types.ts (no classes found)")

    # endpoints.ts を生成（エンドポイントがある場合のみ）
    if len(all_endpoints) > 0:
        print("\n✏️  Generating endpoints.ts (overwrite)...")
        endpoints_content = endpoints_template.generate(endpoints=all_endpoints)
        endpoints_file.write_text(endpoints_content, encoding='utf-8')
        print(f"  ✓ {endpoints_file}")

        # hooks.ts を生成
        print("\n✏️  Generating hooks.ts (overwrite)...")
        hooks_content = hooks_template.generate(endpoints=all_endpoints)
        hooks_file.write_text(hooks_content, encoding='utf-8')
        print(f"  ✓ {hooks_file}")

        # server.ts を生成 (GETエンドポイントのみ)
        get_endpoints = [ep for ep in all_endpoints if ep.method == "GET"]
        if get_endpoints:
            print("\n✏️  Generating server.ts (overwrite)...")
            server_content = server_template.generate(endpoints=all_endpoints)
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

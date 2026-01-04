#!/usr/bin/env python3
"""
API Generator Script

このスクリプトは、バックエンドのC#コード（Controllers, Request, Response）から
フロントエンド用のTypeScript API関数とTanStack Queryフックを生成します。

使用方法:
    python api-generator.py

生成されるファイル:
    - nari-note-frontend/src/lib/api/types.ts
    - nari-note-frontend/src/lib/api/endpoints.ts
    - nari-note-frontend/src/lib/api/hooks.ts
"""

import os
import re
from pathlib import Path
from typing import List, Dict, Tuple, Optional
from dataclasses import dataclass

# 設定
BACKEND_ROOT = Path("nari-note-backend/Src")
FRONTEND_API_DIR = Path("nari-note-frontend/src/lib/api")
CONTROLLER_DIR = BACKEND_ROOT / "Controller"
REQUEST_DIR = BACKEND_ROOT / "Application/Dto/Request"
RESPONSE_DIR = BACKEND_ROOT / "Application/Dto/Response"


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


def csharp_type_to_typescript(csharp_type: str) -> str:
    """C#の型をTypeScriptの型に変換"""
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
    
    # List<T> を T[] に変換
    list_match = re.match(r'List<(.+)>', csharp_type)
    if list_match:
        inner_type = list_match.group(1)
        return f"{type_mapping.get(inner_type, inner_type)}[]"
    
    return type_mapping.get(csharp_type, csharp_type)


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
    property_pattern = r'public\s+(\w+(?:<\w+>)?)\s+(\w+)\s*{\s*get;\s*set;\s*}(?:\s*=\s*[^;]+)?;'
    for match in re.finditer(property_pattern, content):
        prop_type = match.group(1)
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


def parse_controller(file_path: Path) -> List[EndpointInfo]:
    """コントローラーファイルをパースしてエンドポイント情報を抽出"""
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
    
    # エンドポイントを抽出（簡易版）
    # [HttpGet], [HttpPost]などのアトリビュートとメソッドを見つける
    method_pattern = r'\[Http(Get|Post|Put|Delete)(?:\("([^"]+)"\))?\]\s+(?:\[ValidateModelState\]\s+)?public\s+async\s+Task<ActionResult(?:<(\w+)>)?>\s+(\w+)'
    
    for match in re.finditer(method_pattern, content):
        http_method = match.group(1).upper()
        route = match.group(2) or ""
        response_type = match.group(3)
        function_name = match.group(4)
        
        # リクエスト型を推測（簡易版）
        request_type = None
        if 'Request' in function_name:
            request_type = f"{function_name}Request"
        
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
            controller_name=controller_name
        ))
    
    return endpoints


def generate_types_file(classes: List[CSharpClass]) -> str:
    """types.tsファイルを生成"""
    lines = ["// Auto-generated by api-generator.py", "// Do not edit manually", ""]
    
    for cls in sorted(classes, key=lambda x: x.name):
        lines.append(f"export interface {cls.name} {{")
        for prop in cls.properties:
            ts_type = csharp_type_to_typescript(prop.type)
            optional = "?" if prop.is_optional else ""
            lines.append(f"  {prop.name[0].lower()}{prop.name[1:]}{optional}: {ts_type};")
        lines.append("}")
        lines.append("")
    
    return "\n".join(lines)


def generate_endpoints_file(endpoints: List[EndpointInfo]) -> str:
    """endpoints.tsファイルを生成"""
    lines = [
        "// Auto-generated by api-generator.py",
        "// Do not edit manually",
        "",
        "import { apiClient } from './client';",
        "import type {",
    ]
    
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
            
            if ep.method == "GET":
                # TODO: パラメータを適切に処理
                lines.append(f"  {func_name}: async (): Promise<{ep.response_type or 'void'}> => {{")
                lines.append(f"    const response = await apiClient.get<{ep.response_type or 'void'}>('{ep.path}');")
                lines.append("    return response.data;")
                lines.append("  },")
            elif ep.method == "POST":
                req_param = f"data: {ep.request_type}" if ep.request_type else ""
                lines.append(f"  {func_name}: async ({req_param}): Promise<{ep.response_type or 'void'}> => {{")
                lines.append(f"    const response = await apiClient.post<{ep.response_type or 'void'}>('{ep.path}'{', data' if req_param else ''});")
                lines.append("    return response.data;")
                lines.append("  },")
            # PUT, DELETEも同様に処理
        
        lines.append("};")
        lines.append("")
    
    return "\n".join(lines)


def generate_hooks_file(endpoints: List[EndpointInfo]) -> str:
    """hooks.tsファイルを生成（骨組みのみ）"""
    lines = [
        "// Auto-generated by api-generator.py",
        "// Do not edit manually",
        "",
        "import { useMutation, useQuery, useQueryClient, type UseMutationOptions, type UseQueryOptions } from '@tanstack/react-query';",
        "import { authApi, articlesApi, usersApi, healthApi } from './endpoints';",
        "import type {",
        "  // Import necessary types",
        "} from './types';",
        "",
        "// Query Keys",
        "export const queryKeys = {",
        "  // Define query keys",
        "};",
        "",
        "// TODO: Implement hooks based on endpoints",
        "// This is a template - customize based on your needs",
    ]
    
    return "\n".join(lines)


def main():
    """メイン処理"""
    print("🚀 API Generator - Starting...")
    
    # ディレクトリの存在確認
    if not BACKEND_ROOT.exists():
        print(f"❌ Backend directory not found: {BACKEND_ROOT}")
        return
    
    # Request/Responseクラスをパース
    print("\n📖 Parsing Request/Response classes...")
    classes: List[CSharpClass] = []
    
    for dir_path in [REQUEST_DIR, RESPONSE_DIR]:
        if not dir_path.exists():
            print(f"⚠️  Directory not found: {dir_path}")
            continue
        
        for file_path in dir_path.glob("*.cs"):
            cls = parse_csharp_class(file_path)
            if cls:
                classes.append(cls)
                print(f"  ✓ {cls.name}")
    
    # コントローラーをパース
    print("\n📖 Parsing Controllers...")
    all_endpoints: List[EndpointInfo] = []
    
    if CONTROLLER_DIR.exists():
        for file_path in CONTROLLER_DIR.glob("*Controller.cs"):
            endpoints = parse_controller(file_path)
            all_endpoints.extend(endpoints)
            if endpoints:
                print(f"  ✓ {file_path.name}: {len(endpoints)} endpoints")
    
    # 出力ディレクトリを作成
    FRONTEND_API_DIR.mkdir(parents=True, exist_ok=True)
    
    # types.tsを生成
    print("\n✏️  Generating types.ts...")
    types_content = generate_types_file(classes)
    types_file = FRONTEND_API_DIR / "types.ts"
    types_file.write_text(types_content, encoding='utf-8')
    print(f"  ✓ {types_file}")
    
    # endpoints.tsを生成
    print("\n✏️  Generating endpoints.ts...")
    endpoints_content = generate_endpoints_file(all_endpoints)
    endpoints_file = FRONTEND_API_DIR / "endpoints.ts"
    endpoints_file.write_text(endpoints_content, encoding='utf-8')
    print(f"  ✓ {endpoints_file}")
    
    # hooks.tsを生成（骨組みのみ）
    print("\n✏️  Generating hooks.ts template...")
    hooks_content = generate_hooks_file(all_endpoints)
    hooks_file = FRONTEND_API_DIR / "hooks.ts"
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

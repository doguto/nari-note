import re

# --- クラス・名前空間 ---

CLASS_NAME = re.compile(r'public class (\w+)')
CONTROLLER_CLASS = re.compile(r'public class (\w+)Controller')
NAMESPACE = re.compile(r'namespace ([^;]+)')

# --- プロパティ (required キーワードと nullable 型に対応) ---
# group(1): "required " or None
# group(2): C# 型文字列
# group(3): プロパティ名
PROPERTY = re.compile(
    r'public\s+(required\s+)?([\w<>,\s?]+?)\s+(\w+)\s*\{\s*get;\s*set;\s*\}'
)

# --- エンドポイント属性 + メソッドシグネチャ ---
# group(1): Http メソッド名 (Get/Post/Put/Delete)
# group(2): ルートパス文字列 (省略可)
# group(3): ActionResult のジェネリック型 (省略可)
# group(4): C# メソッド名
# group(5): パラメータ文字列
HTTP_METHOD_ATTR = re.compile(
    r'\[Http(Get|Post|Put|Delete)(?:\("([^"]+)"\))?\](?:\s*\[\w+\])*'
    r'\s+public\s+async\s+Task<ActionResult(?:<(\w+)>)?>\s+(\w+)\s*\(([^)]*)\)'
)

# --- パラメータ ---
FROM_BODY = re.compile(r'\[FromBody\]\s+(\w+Request)\s+\w+')
FORM_FILE = re.compile(r'IFormFile\s+(\w+)')
PATH_PARAM = re.compile(r'\{(\w+)\}')

# --- ValueObject ---
# group(1): 基底型 (Guid / int など)
# group(2): struct 名 (例: UserId)
VALUE_OBJECT = re.compile(
    r'\[ValueObject<(\w+)>(?:\([^)]+\))?\]\s+public\s+partial\s+struct\s+(\w+);'
)

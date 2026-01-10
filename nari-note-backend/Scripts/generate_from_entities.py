import re
from pathlib import Path
from typing import List

# プロジェクトのルートディレクトリ
BACKEND_ROOT = Path(__file__).parent.parent
SCRIPTS_DIR = Path(__file__).parent
TEMPLATES_DIR = SCRIPTS_DIR / "templates"

ENTITY_DIR = BACKEND_ROOT / "Src" / "Domain" / "Entity"
REPOSITORY_DIR = BACKEND_ROOT / "Src" / "Domain" / "Repository"
VALUE_OBJECT_FILE = BACKEND_ROOT / "Src" / "Domain" / "ValueObject" / "EntityKeyObject.cs"
CONVERTER_FILE = BACKEND_ROOT / "Src" / "Middleware" / "ValueObjectJsonConverterFactory.cs"


# エンティティファイルから、IDを持つエンティティ名を抽出
def extract_entities_from_files() -> List[str]:
    entities = []

    for file in ENTITY_DIR.glob("*.cs"):
        if file.name == "EntityBase.cs":
            continue

        with open(file, "r", encoding="utf-8") as f:
            content = f.read()

        # public class EntityName : EntityBase のパターンを探す
        match = re.search(r'public class (\w+)\s*:\s*EntityBase', content)
        if not match:
            continue

        entity_name = match.group(1)
        entities.append(entity_name)
    return sorted(entities)


# テンプレート読み込み
def load_template(template_name: str) -> str:
    template_path = TEMPLATES_DIR / template_name
    with open(template_path, "r", encoding="utf-8") as f:
        return f.read()


# ValueObject定義の生成
def generate_value_objects(entities: List[str]) -> str:
    template = load_template("value_object.template")

    definitions = []
    for entity in entities:
        definitions.append(f"[ValueObject<int>]")
        definitions.append(f"public partial struct {entity}Id;")
        definitions.append("")

    return template.replace("{{VALUE_OBJECT_DEFINITIONS}}", "\n".join(definitions))


# Jsonコンバーターの生成
def generate_converter(entities: List[str]) -> str:
    template = load_template("json_converter.template")

    can_convert_conditions = " ||\n               ".join(
        [f"typeToConvert == typeof({entity}Id)" for entity in entities]
    )

    create_converter_cases = "\n        ".join([
        f"if (typeToConvert == typeof({entity}Id))\n            return new ValueObjectJsonConverter<{entity}Id>();"
        for entity in entities
    ])

    return template.replace(
        "{{CAN_CONVERT_CONDITIONS}}", can_convert_conditions
    ).replace(
        "{{CREATE_CONVERTER_CASES}}", create_converter_cases
    )


# IRepositoryインターフェースの生成（既存ファイルはスキップ）
def generate_repository_interfaces(entities: List[str]) -> List[str]:
    template = load_template("repository_interface.template")
    generated = []

    for entity in entities:
        file_path = REPOSITORY_DIR / f"I{entity}Repository.cs"
        if file_path.exists():
            continue

        content = template.replace("{{ENTITY_NAME}}", entity)
        with open(file_path, "w", encoding="utf-8") as f:
            f.write(content)
        generated.append(entity)

    return generated


def main():
    print("🔍 Entityファイルを解析中...")
    entities = extract_entities_from_files()
    
    if not entities:
        print("❌ エンティティが見つかりませんでした")
        return
    
    print(f"✅ {len(entities)}個のエンティティを検出: {', '.join(entities)}")
    
    # ValueObject生成
    print("\n📝 ValueObjectを生成中...")
    value_object_content = generate_value_objects(entities)
    with open(VALUE_OBJECT_FILE, "w", encoding="utf-8") as f:
        f.write(value_object_content)
    print(f"✅ 生成完了: {VALUE_OBJECT_FILE.relative_to(BACKEND_ROOT)}")
    
    # Converter生成
    print("\n📝 JSONコンバーターを生成中...")
    converter_content = generate_converter(entities)
    with open(CONVERTER_FILE, "w", encoding="utf-8") as f:
        f.write(converter_content)
    print(f"✅ 生成完了: {CONVERTER_FILE.relative_to(BACKEND_ROOT)}")
    
    # IRepository生成（既存ファイルはスキップ）
    print("\n📝 IRepositoryインターフェースを生成中...")
    generated_repos = generate_repository_interfaces(entities)
    if generated_repos:
        print(f"✅ 新規生成: {', '.join([f'I{e}Repository.cs' for e in generated_repos])}")
    else:
        print("ℹ️ 新規生成なし（すべて既存）")
    
    print("\n🎉 すべての生成が完了しました！")


if __name__ == "__main__":
    main()

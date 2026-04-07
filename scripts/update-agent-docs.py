#!/usr/bin/env python3

"""
Agent Documentation Updater
各エージェントの.agent.mdファイルを最新のプロジェクト情報に基づいて更新する
"""

import os
import argparse
from pathlib import Path
from typing import Dict, List, Optional
from datetime import datetime


# プロジェクトルート
PROJECT_ROOT = Path(__file__).parent.parent
AGENTS_DIR = PROJECT_ROOT / ".github" / "agents"
BACKEND_ROOT = PROJECT_ROOT / "nari-note-backend"
FRONTEND_ROOT = PROJECT_ROOT / "nari-note-frontend"
SPEC_ROOT = PROJECT_ROOT / "spec"


class AgentDocUpdater:
    """エージェントドキュメント更新クラス"""

    def __init__(self, dry_run: bool = False, verbose: bool = False):
        self.dry_run = dry_run
        self.verbose = verbose

    def log(self, message: str):
        """ログ出力"""
        if self.verbose:
            print(f"[INFO] {message}")

    def analyze_backend_structure(self) -> Dict:
        """バックエンドの構造を分析"""
        self.log("バックエンド構造を分析中...")
        
        structure = {
            "documents": [],
            "src_structure": {},
        }
        
        # Documents配下のファイルをチェック
        docs_dir = BACKEND_ROOT / "Documents"
        if docs_dir.exists():
            for doc_file in sorted(docs_dir.glob("*.md")):
                structure["documents"].append(doc_file.name)
                self.log(f"  - ドキュメント発見: {doc_file.name}")
        
        # Src配下の主要ディレクトリをチェック
        src_dir = BACKEND_ROOT / "Src"
        if src_dir.exists():
            for item in sorted(src_dir.iterdir()):
                if item.is_dir():
                    structure["src_structure"][item.name] = True
                    self.log(f"  - ディレクトリ発見: Src/{item.name}")
        
        return structure

    def analyze_frontend_structure(self) -> Dict:
        """フロントエンドの構造を分析"""
        self.log("フロントエンド構造を分析中...")
        
        structure = {
            "documents": [],
            "src_structure": {},
        }
        
        # docs配下のファイルをチェック
        docs_dir = FRONTEND_ROOT / "docs"
        if docs_dir.exists():
            for doc_file in sorted(docs_dir.glob("*.md")):
                structure["documents"].append(doc_file.name)
                self.log(f"  - ドキュメント発見: {doc_file.name}")
        
        # src配下の主要ディレクトリをチェック
        src_dir = FRONTEND_ROOT / "src"
        if src_dir.exists():
            for item in sorted(src_dir.iterdir()):
                if item.is_dir():
                    structure["src_structure"][item.name] = True
                    self.log(f"  - ディレクトリ発見: src/{item.name}")
        
        return structure

    def analyze_spec_structure(self) -> Dict:
        """仕様書の構造を分析"""
        self.log("仕様書構造を分析中...")
        
        structure = {
            "spec_files": [],
        }
        
        # spec配下のファイルをチェック
        if SPEC_ROOT.exists():
            for spec_file in sorted(SPEC_ROOT.glob("*.md")):
                structure["spec_files"].append(spec_file.name)
                self.log(f"  - 仕様書発見: {spec_file.name}")
        
        return structure

    def verify_document_links(self, agent_type: str, content: str) -> List[str]:
        """ドキュメントリンクの検証"""
        self.log(f"{agent_type}のドキュメントリンクを検証中...")
        
        broken_links = []
        
        # マークダウンリンクを抽出
        import re
        link_pattern = r'\[([^\]]+)\]\(([^\)]+)\)'
        links = re.findall(link_pattern, content)
        
        for link_text, link_path in links:
            # 外部リンクやアンカーリンクはスキップ
            if link_path.startswith('http') or link_path.startswith('#'):
                continue
            
            # プロジェクトルートからの相対パスに変換
            if link_path.startswith('/'):
                link_path = link_path[1:]
            
            full_path = PROJECT_ROOT / link_path
            if not full_path.exists():
                broken_links.append(f"{link_text} -> {link_path}")
                self.log(f"  ⚠ リンク切れ: {link_path}")
        
        return broken_links

    def update_backend_agent(self):
        """バックエンドエージェントのドキュメントを更新"""
        self.log("Backend-Engineer-Agentのドキュメントを更新中...")
        
        agent_file = AGENTS_DIR / "backend-engineer.agent.md"
        if not agent_file.exists():
            self.log(f"⚠ {agent_file} が見つかりません")
            return
        
        # 現在の内容を読み込み
        with open(agent_file, 'r', encoding='utf-8') as f:
            content = f.read()
        
        # 構造を分析
        structure = self.analyze_backend_structure()
        
        # リンク検証
        broken_links = self.verify_document_links("Backend-Engineer-Agent", content)
        
        if broken_links:
            print(f"\n⚠ Backend-Engineer-Agentでリンク切れを発見:")
            for link in broken_links:
                print(f"  - {link}")
        else:
            self.log("✓ 全てのドキュメントリンクが有効です")
        
        # ドキュメントファイルの存在確認
        expected_docs = [
            "backend-implementation-guide.md",
            "error-handling-strategy.md",
            "architecture-overview.md",
            "architecture.md",
            "development-workflow.md",
            "authentication-strategy.md",
            "er-diagram.md"
        ]
        
        missing_docs = [doc for doc in expected_docs if doc not in structure["documents"]]
        if missing_docs:
            print(f"\n⚠ Backend-Engineer-Agentで参照されているドキュメントが見つかりません:")
            for doc in missing_docs:
                print(f"  - {doc}")
        else:
            self.log("✓ 全ての参照ドキュメントが存在します")
        
        self.log(f"Backend-Engineer-Agent: 分析完了")
        self.log(f"  - ドキュメント数: {len(structure['documents'])}")
        self.log(f"  - Srcディレクトリ数: {len(structure['src_structure'])}")

    def update_frontend_agent(self):
        """フロントエンドエージェントのドキュメントを更新"""
        self.log("Frontend-Engineer-Agentのドキュメントを更新中...")
        
        agent_file = AGENTS_DIR / "frontend-engineer.agent.md"
        if not agent_file.exists():
            self.log(f"⚠ {agent_file} が見つかりません")
            return
        
        # 現在の内容を読み込み
        with open(agent_file, 'r', encoding='utf-8') as f:
            content = f.read()
        
        # 構造を分析
        structure = self.analyze_frontend_structure()
        
        # リンク検証
        broken_links = self.verify_document_links("Frontend-Engineer-Agent", content)
        
        if broken_links:
            print(f"\n⚠ Frontend-Engineer-Agentでリンク切れを発見:")
            for link in broken_links:
                print(f"  - {link}")
        else:
            self.log("✓ 全てのドキュメントリンクが有効です")
        
        # ドキュメントファイルの存在確認
        expected_docs = [
            "implementation-guide.md",
            "architecture.md",
            "api-usage.md",
            "architecture-diagram.md",
            "quick-reference.md"
        ]
        
        missing_docs = [doc for doc in expected_docs if doc not in structure["documents"]]
        if missing_docs:
            print(f"\n⚠ Frontend-Engineer-Agentで参照されているドキュメントが見つかりません:")
            for doc in missing_docs:
                print(f"  - {doc}")
        else:
            self.log("✓ 全ての参照ドキュメントが存在します")
        
        self.log(f"Frontend-Engineer-Agent: 分析完了")
        self.log(f"  - ドキュメント数: {len(structure['documents'])}")
        self.log(f"  - srcディレクトリ数: {len(structure['src_structure'])}")

    def update_spec_agent(self):
        """仕様確認エージェントのドキュメントを更新"""
        self.log("Spec-Agentのドキュメントを更新中...")
        
        agent_file = AGENTS_DIR / "spec.agent.md"
        if not agent_file.exists():
            self.log(f"⚠ {agent_file} が見つかりません")
            return
        
        # 現在の内容を読み込み
        with open(agent_file, 'r', encoding='utf-8') as f:
            content = f.read()
        
        # 構造を分析
        structure = self.analyze_spec_structure()
        
        # リンク検証
        broken_links = self.verify_document_links("Spec-Agent", content)
        
        if broken_links:
            print(f"\n⚠ Spec-Agentでリンク切れを発見:")
            for link in broken_links:
                print(f"  - {link}")
        else:
            self.log("✓ 全てのドキュメントリンクが有効です")
        
        self.log(f"Spec-Agent: 分析完了")
        self.log(f"  - 仕様書数: {len(structure['spec_files'])}")

    def update_agent(self, agent_name: str):
        """指定されたエージェントのドキュメントを更新"""
        if agent_name == "backend-engineer":
            self.update_backend_agent()
        elif agent_name == "frontend-engineer":
            self.update_frontend_agent()
        elif agent_name == "spec":
            self.update_spec_agent()
        else:
            print(f"⚠ 不明なエージェント名: {agent_name}")

    def update_all_agents(self):
        """全エージェントのドキュメントを更新"""
        self.log("全エージェントのドキュメントを更新中...")
        print(f"\n{'='*60}")
        print(f"Agent Documentation Update - {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
        print(f"{'='*60}\n")
        
        if self.dry_run:
            print("🔍 DRY RUN モード: 実際の更新は行いません\n")
        
        self.update_backend_agent()
        print()
        self.update_frontend_agent()
        print()
        self.update_spec_agent()
        
        print(f"\n{'='*60}")
        if self.dry_run:
            print("✓ ドライラン完了")
        else:
            print("✓ ドキュメント検証完了")
        print(f"{'='*60}\n")


def main():
    parser = argparse.ArgumentParser(
        description='各エージェントの.agent.mdファイルを最新のプロジェクト情報に基づいて検証・更新する'
    )
    parser.add_argument(
        '--agent',
        choices=['backend-engineer', 'frontend-engineer', 'spec'],
        help='更新対象のエージェントを指定（未指定の場合は全エージェント）'
    )
    parser.add_argument(
        '--dry-run',
        action='store_true',
        help='実際には更新せず、変更内容のプレビューのみ表示'
    )
    parser.add_argument(
        '--verbose',
        action='store_true',
        help='詳細な実行ログを表示'
    )
    
    args = parser.parse_args()
    
    updater = AgentDocUpdater(dry_run=args.dry_run, verbose=args.verbose)
    
    if args.agent:
        updater.update_agent(args.agent)
    else:
        updater.update_all_agents()


if __name__ == "__main__":
    main()

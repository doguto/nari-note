"""
テンプレート生成の基底クラスと共通ユーティリティ
"""

import re
from abc import ABC, abstractmethod
from typing import List, Dict
from models import EndpointInfo


class BaseTemplate(ABC):
    """テンプレート生成の基底クラス"""

    def __init__(self, value_object_types: set[str]):
        self.value_object_types = value_object_types

    @abstractmethod
    def get_header(self) -> str:
        """ファイルヘッダーを返す"""
        pass

    @abstractmethod
    def generate(self, **kwargs) -> str:
        """完全なファイル内容を生成"""
        pass

    def _join_lines(self, lines: List[str]) -> str:
        """行リストを結合してファイル内容にする"""
        return "\n".join(lines)

    def _group_by_controller(self, endpoints: List[EndpointInfo]) -> Dict[str, List[EndpointInfo]]:
        """エンドポイントをコントローラー別にグループ化"""
        by_controller: Dict[str, List[EndpointInfo]] = {}
        for ep in endpoints:
            if ep.controller_name not in by_controller:
                by_controller[ep.controller_name] = []
            by_controller[ep.controller_name].append(ep)
        return by_controller

    def _to_camel_case(self, name: str) -> str:
        """先頭を小文字にする"""
        return name[0].lower() + name[1:] if name else name

    def _extract_path_params(self, path: str) -> List[str]:
        """パスからパラメータを抽出"""
        return re.findall(r'\{(\w+)\}', path)

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

    def _collect_types_from_endpoints(self, endpoints: List[EndpointInfo]) -> set[str]:
        """エンドポイントから使用される型を収集"""
        all_types = set()
        for ep in endpoints:
            if ep.request_type and ep.request_type != "void":
                all_types.add(ep.request_type)
            if ep.response_type and ep.response_type != "void":
                all_types.add(ep.response_type)
        return all_types

    def _resolve_path_param(self, param: str, request_type: str, class_map: Dict) -> str:
        """
        パスパラメータ名を解決（{id} -> 実際のプロパティ名）

        Args:
            param: パスパラメータ名
            request_type: リクエスト型名
            class_map: クラス名->CSharpClassのマッピング

        Returns:
            解決されたプロパティ名（キャメルケース）
        """
        camel_param = self._to_camel_case(param) if param else param

        # リクエストクラスがある場合、プロパティ名を確認
        if request_type in class_map:
            req_class = class_map[request_type]
            prop_names = [self._to_camel_case(prop.name) for prop in req_class.properties]

            # {id} の場合の堅牢な推測
            if camel_param == 'id':
                if 'id' in prop_names:
                    return 'id'

                # "*Id" がちょうど1つならそれを使う
                id_like = [p for p in prop_names if p.endswith('Id')]
                if len(id_like) == 1:
                    return id_like[0]

                # ValueObject型から候補を生成してマッチング
                candidates = [
                    self._to_camel_case(vo_type)
                    for vo_type in self.value_object_types
                    if vo_type.endswith('Id')
                ]
                for candidate in candidates:
                    if candidate in prop_names:
                        return candidate

        return camel_param

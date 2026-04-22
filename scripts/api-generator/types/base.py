from abc import ABC, abstractmethod
from pathlib import Path


class BaseParser(ABC):
    @abstractmethod
    def parse(self, file_path: Path) -> list:
        """バックエンドファイルを解析して HttpApi リストを返す"""
        ...


class BaseGenerator(ABC):
    @abstractmethod
    def generate(self, apis: list) -> str:
        """HttpApi リストからコード文字列を生成する"""
        ...

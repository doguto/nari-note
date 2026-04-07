"""Data models for C# code parsing."""

from dataclasses import dataclass
from typing import List, Optional


@dataclass
class CSharpProperty:
    name: str
    type: str
    is_optional: bool = False
    is_list: bool = False


@dataclass
class CSharpClass:
    name: str
    properties: List[CSharpProperty]
    namespace: str


@dataclass
class EndpointInfo:
    method: str  # GET, POST, PUT, DELETE
    path: str
    function_name: str
    request_type: Optional[str]
    response_type: Optional[str]
    controller_name: str
    has_body_param: bool = False  # [FromBody]パラメータがあるかどうか

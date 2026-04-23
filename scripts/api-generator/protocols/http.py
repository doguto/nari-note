from enum import Enum
from dataclasses import dataclass, field
from typing import Dict, List, Optional


class HttpMethod(Enum):
    GET = "GET"
    POST = "POST"
    PUT = "PUT"
    DELETE = "DELETE"
    PATCH = "PATCH"


class EndpointType(Enum):
    """パスパラメータのデータ型"""
    STRING = "string"
    NUMBER = "number"


@dataclass
class EndpointEntity:
    """URL パスの1セグメント。type が None = 静的セグメント、not None = パスパラメータ"""
    name: str
    type: Optional[EndpointType] = None


@dataclass
class Endpoint:
    """URL パス全体を構造化して保持する"""
    entities: List[EndpointEntity]

    @staticmethod
    def from_path_string(
        path: str,
        param_types: Optional[Dict[str, EndpointType]] = None,
    ) -> "Endpoint":
        """"/api/courses/{id}" → Endpoint。param_types で各パスパラメータの型を指定できる"""
        entities: List[EndpointEntity] = []
        for segment in path.strip("/").split("/"):
            if not segment:
                continue
            if segment.startswith("{") and segment.endswith("}"):
                name = segment[1:-1]
                dtype = (param_types or {}).get(name, EndpointType.STRING)
                entities.append(EndpointEntity(name=name, type=dtype))
            else:
                entities.append(EndpointEntity(name=segment, type=None))
        return Endpoint(entities=entities)

    def to_path_string(self) -> str:
        """Endpoint → "/api/courses/{id}" 形式の文字列"""
        parts = [
            f"{{{e.name}}}" if e.type is not None else e.name
            for e in self.entities
        ]
        return "/" + "/".join(parts)

    def path_params(self) -> List[EndpointEntity]:
        """パスパラメータ（type が not None）のエンティティ一覧"""
        return [e for e in self.entities if e.type is not None]


@dataclass
class HttpPayload:
    """JSON でやり取りされるボディの型情報"""
    type_name: str


@dataclass
class HttpHeader:
    """HTTP ヘッダー情報"""
    content_type: str = "application/json"
    custom: Dict[str, str] = field(default_factory=dict)


@dataclass
class HttpRequest:
    """HTTP リクエスト = body (JSON ボディ) or query (クエリ文字列) + Header"""
    body: Optional[HttpPayload] = None    # [FromBody] / POST・PUT・PATCH のボディ
    query: Optional[HttpPayload] = None   # GET クエリ文字列パラメータ
    header: HttpHeader = field(default_factory=HttpHeader)


@dataclass
class HttpResponse:
    """HTTP レスポンス = Payload (受信 JSON 型) + Header"""
    payload: Optional[HttpPayload] = None
    header: HttpHeader = field(default_factory=HttpHeader)


@dataclass
class HttpApi:
    method: HttpMethod
    endpoint: Endpoint
    function_name: str
    controller_name: str
    request: Optional[HttpRequest] = None
    response: Optional[HttpResponse] = None
    is_form_file: bool = False
    form_file_param: str = "file"

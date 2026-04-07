"""
TypeScript生成用のテンプレートクラス
"""

from .types_template import TypesTemplate
from .endpoints_template import EndpointsTemplate
from .hooks_template import HooksTemplate
from .server_template import ServerTemplate

__all__ = [
    'TypesTemplate',
    'EndpointsTemplate',
    'HooksTemplate',
    'ServerTemplate',
]

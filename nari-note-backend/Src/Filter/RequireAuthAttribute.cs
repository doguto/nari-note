using Microsoft.AspNetCore.Authorization;

namespace NariNoteBackend.Filter;

/// <summary>
/// 認証が必須であることを示す属性
/// この属性が付与されたアクションは認証済みユーザーのみがアクセス可能
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireAuthAttribute : AuthorizeAttribute
{
    public RequireAuthAttribute()
    {
        AuthenticationSchemes = "Bearer";
    }
}

namespace NariNoteBackend.Filter;

/// <summary>
/// 認証不要であることを示す属性
/// この属性が付与されたアクションは認証なしでアクセス可能
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AllowAnonymousAttribute : Attribute
{
}

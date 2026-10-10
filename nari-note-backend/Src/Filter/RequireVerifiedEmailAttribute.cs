namespace NariNoteBackend.Filter;

/// <summary>
/// メールアドレスの認証が完了していることを要求する属性
/// この属性が付与されたアクションは、メール認証済みのユーザーのみがアクセス可能（未認証の場合は 403）
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireVerifiedEmailAttribute : Attribute
{
}

namespace NariNoteBackend.Filter;

/// <summary>
/// 認証が任意であることを示す属性
/// この属性が付与されたアクションは認証なしでもアクセス可能だが、
/// 認証されている場合は追加の情報や機能が利用可能になる
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class OptionalAuthAttribute : Attribute
{
}

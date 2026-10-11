namespace NariNoteBackend.Filter;

/// <summary>
/// リクエスト全体を DB トランザクションで包まないことを示す属性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class NoTransactionAttribute : Attribute
{
}

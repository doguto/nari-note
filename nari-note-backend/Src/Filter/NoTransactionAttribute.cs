namespace NariNoteBackend.Filter;

// リクエスト全体を DB トランザクションで包まないことを示す属性
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class NoTransactionAttribute : Attribute
{
}

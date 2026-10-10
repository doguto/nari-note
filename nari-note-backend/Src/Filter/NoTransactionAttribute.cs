namespace NariNoteBackend.Filter;

/// <summary>
/// リクエスト全体を DB トランザクションで包まないことを示す属性
/// 外部 I/O（画像ストレージ操作など）を含み、トランザクションを保持したまま待機させたくないアクションに付与する
/// この属性を付与したアクションは、DB 書き込みが単一の SaveChanges で完結していることを前提とする
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class NoTransactionAttribute : Attribute
{
}

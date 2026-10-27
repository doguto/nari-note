namespace NariNoteBackend.Application.Dto.Response;

public class SearchArticlesResponse
{
    public List<ArticleThumbnailDto> Articles { get; set; } = new();

    // 総件数は数えず、次のページがあるかどうかだけを返す（記事検索は本文の部分一致で全件を走査するため、COUNT を足すと走査が 2 回になる）
    public bool HasNext { get; set; }
}

namespace NariNoteBackend.Application.Dto.Response;

public class DeleteArticleResponse
{
    public bool IsSuccess { get; set; }
    public DeleteArticleError? Error { get; set; }
    
    public static DeleteArticleResponse Success() => new DeleteArticleResponse { IsSuccess = true };
    
    public static DeleteArticleResponse NotFound(int articleId) => new DeleteArticleResponse 
    { 
        IsSuccess = false, 
        Error = new DeleteArticleError 
        { 
            Type = DeleteArticleErrorType.NotFound,
            Message = "記事が見つかりません",
            ArticleId = articleId
        }
    };
    
    public static DeleteArticleResponse Forbidden(int articleId) => new DeleteArticleResponse 
    { 
        IsSuccess = false, 
        Error = new DeleteArticleError 
        { 
            Type = DeleteArticleErrorType.Forbidden,
            Message = "この記事を削除する権限がありません",
            ArticleId = articleId
        }
    };
}

public class DeleteArticleError
{
    public DeleteArticleErrorType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public int ArticleId { get; set; }
}

public enum DeleteArticleErrorType
{
    NotFound,
    Forbidden
}

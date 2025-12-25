using NariNoteBackend.Application.Exception;
using NariNoteBackend.Application.Repository;
using NariNoteBackend.Domain;

namespace NariNoteBackend.Application.Service;

/// <summary>
/// 記事取得サービス
/// エラーハンドリングの実装例を示す
/// </summary>
public class GetArticleService
{
    private readonly IArticleRepository articleRepository;

    public GetArticleService(IArticleRepository articleRepository)
    {
        this.articleRepository = articleRepository;
    }

    /// <summary>
    /// 記事IDで記事を取得する
    /// </summary>
    /// <param name="id">記事ID</param>
    /// <returns>記事エンティティ</returns>
    /// <exception cref="NotFoundException">記事が見つからない場合</exception>
    public async Task<Article> ExecuteAsync(int id)
    {
        var article = await this.articleRepository.FindByIdAsync(id);
        
        if (article == null)
        {
            throw new NotFoundException($"Article with ID {id} not found");
        }
        
        return article;
    }
}

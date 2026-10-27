using System.ComponentModel.DataAnnotations;

namespace NariNoteBackend.Application.Helper;

// limit / offset でページングする一覧 API の共通の入力チェック
public static class PagingValidator
{
    public const int MaxLimit = 100;

    public static void Validate(int limit, int offset)
    {
        if (limit is < 1 or > MaxLimit)
        {
            throw new ValidationException($"取得件数は1から{MaxLimit}の範囲で指定してください");
        }

        if (offset < 0)
        {
            throw new ValidationException("オフセットは0以上の値を指定してください");
        }
    }
}

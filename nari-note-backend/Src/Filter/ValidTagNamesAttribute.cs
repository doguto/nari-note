using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace NariNoteBackend.Filter;

public class ValidTagNamesAttribute : ValidationAttribute
{
    // URLエンコード不要な文字のみ許可（英数字・日本語・ハイフン・アンダースコア・ピリオド）
    public const string ValidPattern = @"^[a-zA-Z0-9\u3040-\u30FF\u4E00-\u9FFF_\-\.]+$";
    internal static readonly Regex TagNameRegex = new(ValidPattern, RegexOptions.Compiled);

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not List<string> tags) return ValidationResult.Success;

        var invalidTags = tags
            .Where(tag => string.IsNullOrWhiteSpace(tag) || !TagNameRegex.IsMatch(tag))
            .ToList();

        if (invalidTags.Count > 0)
        {
            return new ValidationResult($"タグ名に使用できない文字が含まれています: {string.Join(", ", invalidTags)}");
        }

        return ValidationResult.Success;
    }
}

using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace NariNoteBackend.Application.Validation;

public class StrongPasswordAttribute : ValidationAttribute
{
    static readonly HashSet<string> CommonWeakPasswords = new()
    {
        "password", "password1", "password123", "12345678", "123456789", "1234567890",
        "qwerty", "qwertyui", "qwertyuiop", "asdfghjk", "asdfghjkl", "zxcvbnm",
        "abc12345", "password!", "Pass1234", "Welcome1", "admin123", "letmein"
    };

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return ValidationResult.Success;
        }

        var password = value.ToString()!;

        // 最小文字数チェック（8文字以上）は別の属性で行うため、ここではスキップ
        
        // 一般的な脆弱パスワードのチェック
        if (CommonWeakPasswords.Contains(password.ToLower()))
        {
            return new ValidationResult("このパスワードは一般的すぎるため使用できません");
        }

        // 複雑性要件のチェック: 英大文字、英小文字、数字、特殊文字のうち3種類以上
        int complexityScore = 0;
        
        if (Regex.IsMatch(password, @"[A-Z]")) complexityScore++; // 英大文字
        if (Regex.IsMatch(password, @"[a-z]")) complexityScore++; // 英小文字
        if (Regex.IsMatch(password, @"[0-9]")) complexityScore++; // 数字
        if (Regex.IsMatch(password, @"[^A-Za-z0-9]")) complexityScore++; // 特殊文字

        if (complexityScore < 3)
        {
            return new ValidationResult("パスワードは英大文字、英小文字、数字、特殊文字のうち3種類以上を含む必要があります");
        }

        return ValidationResult.Success;
    }
}

using Microsoft.AspNetCore.Mvc.ModelBinding;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Middleware;

public class ValueObjectModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var modelType = context.Metadata.ModelType;

        // ValueObjectかどうかをチェック（From(int)メソッドを持つstruct型）
        if (modelType.IsValueType && 
            modelType.Namespace == typeof(ArticleId).Namespace &&
            modelType.GetMethod("From", new[] { typeof(int) }) != null)
        {
            return new ValueObjectModelBinder();
        }

        return null;
    }
}

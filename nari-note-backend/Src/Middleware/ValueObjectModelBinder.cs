using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace NariNoteBackend.Middleware;

public class ValueObjectModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var modelName = bindingContext.ModelName;
        var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

        var value = valueProviderResult.FirstValue;

        if (string.IsNullOrEmpty(value))
        {
            return Task.CompletedTask;
        }

        if (!int.TryParse(value, out var intValue))
        {
            bindingContext.ModelState.TryAddModelError(
                modelName,
                $"The value '{value}' is not valid for {bindingContext.ModelType.Name}.");
            return Task.CompletedTask;
        }

        var fromMethod = bindingContext.ModelType.GetMethod("From", new[] { typeof(int) });
        if (fromMethod != null)
        {
            var result = fromMethod.Invoke(null, new object[] { intValue });
            bindingContext.Result = ModelBindingResult.Success(result);
        }

        return Task.CompletedTask;
    }
}

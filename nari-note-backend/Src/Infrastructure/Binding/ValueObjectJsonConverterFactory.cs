using System.Text.Json;
using System.Text.Json.Serialization;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Infrastructure.Binding;

public class ValueObjectJsonConverterFactory : JsonConverterFactory
{
    // ID 型を追加したときの登録漏れを防ぐため、ValueObject の名前空間にある struct をすべて対象にする
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsValueType && typeToConvert.Namespace == typeof(UserId).Namespace;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return (JsonConverter?)Activator.CreateInstance(typeof(ValueObjectJsonConverter<>).MakeGenericType(typeToConvert));
    }
}

public class ValueObjectJsonConverter<T> : JsonConverter<T>
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            var intValue = reader.GetInt32();
            var fromInt = typeToConvert.GetMethod("From", new[] { typeof(int) });
            if (fromInt != null)
                return (T)fromInt.Invoke(null, new object[] { intValue })!;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var strValue = reader.GetString()!;
            var fromGuid = typeToConvert.GetMethod("From", new[] { typeof(Guid) });
            if (fromGuid != null && Guid.TryParse(strValue, out var guid))
                return (T)fromGuid.Invoke(null, new object[] { guid })!;
        }

        throw new JsonException($"Unable to deserialize {typeToConvert.Name}");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var valueProperty = typeof(T).GetProperty("Value");
        if (valueProperty == null)
            throw new JsonException($"Unable to serialize {typeof(T).Name}");

        var rawValue = valueProperty.GetValue(value)!;
        switch (rawValue)
        {
            case int intValue:
                writer.WriteNumberValue(intValue);
                break;
            case Guid guidValue:
                writer.WriteStringValue(guidValue);
                break;
            default:
                writer.WriteStringValue(rawValue.ToString());
                break;
        }
    }
}

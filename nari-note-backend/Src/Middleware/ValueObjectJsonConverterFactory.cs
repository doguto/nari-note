using System.Text.Json;
using System.Text.Json.Serialization;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Middleware;

public class ValueObjectJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(ArticleId) ||
               typeToConvert == typeof(ArticleTagId) ||
               typeToConvert == typeof(CommentId) ||
               typeToConvert == typeof(CourseId) ||
               typeToConvert == typeof(CourseLikeId) ||
               typeToConvert == typeof(FollowId) ||
               typeToConvert == typeof(LikeId) ||
               typeToConvert == typeof(NotificationId) ||
               typeToConvert == typeof(TagId) ||
               typeToConvert == typeof(UserId);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeToConvert == typeof(ArticleId))
            return new ValueObjectJsonConverter<ArticleId>();
        if (typeToConvert == typeof(ArticleTagId))
            return new ValueObjectJsonConverter<ArticleTagId>();
        if (typeToConvert == typeof(CommentId))
            return new ValueObjectJsonConverter<CommentId>();
        if (typeToConvert == typeof(CourseId))
            return new ValueObjectJsonConverter<CourseId>();
        if (typeToConvert == typeof(CourseLikeId))
            return new ValueObjectJsonConverter<CourseLikeId>();
        if (typeToConvert == typeof(FollowId))
            return new ValueObjectJsonConverter<FollowId>();
        if (typeToConvert == typeof(LikeId))
            return new ValueObjectJsonConverter<LikeId>();
        if (typeToConvert == typeof(NotificationId))
            return new ValueObjectJsonConverter<NotificationId>();
        if (typeToConvert == typeof(TagId))
            return new ValueObjectJsonConverter<TagId>();
        if (typeToConvert == typeof(UserId))
            return new ValueObjectJsonConverter<UserId>();

        return null;
    }
}

public class ValueObjectJsonConverter<T> : JsonConverter<T>
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            var value = reader.GetInt32();
            var fromMethod = typeToConvert.GetMethod("From", new[] { typeof(int) });
            if (fromMethod != null)
            {
                return (T)fromMethod.Invoke(null, new object[] { value })!;
            }
        }

        throw new JsonException($"Unable to deserialize {typeToConvert.Name}");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var valueProperty = typeof(T).GetProperty("Value");
        if (valueProperty != null)
        {
            var intValue = (int)valueProperty.GetValue(value)!;
            writer.WriteNumberValue(intValue);
        }
        else
        {
            throw new JsonException($"Unable to serialize {typeof(T).Name}");
        }
    }
}

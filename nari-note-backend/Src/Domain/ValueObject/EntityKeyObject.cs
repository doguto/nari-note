using Vogen;

namespace NariNoteBackend.Domain.ValueObject;

[ValueObject<int>(Conversions.EfCoreValueConverter)]
public partial struct ArticleId;

[ValueObject<int>(Conversions.EfCoreValueConverter)]
public partial struct ArticleTagId;

[ValueObject<int>(Conversions.EfCoreValueConverter)]
public partial struct CommentId;

[ValueObject<int>(Conversions.EfCoreValueConverter)]
public partial struct FollowId;

[ValueObject<int>(Conversions.EfCoreValueConverter)]
public partial struct LikeId;

[ValueObject<int>(Conversions.EfCoreValueConverter)]
public partial struct NotificationId;

[ValueObject<int>(Conversions.EfCoreValueConverter)]
public partial struct SessionId;

[ValueObject<int>(Conversions.EfCoreValueConverter)]
public partial struct TagId;

[ValueObject<int>(Conversions.EfCoreValueConverter)]
public partial struct UserId;

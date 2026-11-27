using Vogen;

namespace NariNoteBackend.Domain.ValueObject;

[ValueObject<Guid>(Conversions.EfCoreValueConverter)]
public partial struct ArticleId;

[ValueObject<int>(Conversions.EfCoreValueConverter)]
public partial struct ArticleTagId;

[ValueObject<int>(Conversions.EfCoreValueConverter)]
public partial struct CommentId;

[ValueObject<Guid>(Conversions.EfCoreValueConverter)]
public partial struct CourseId;

[ValueObject<int>(Conversions.EfCoreValueConverter)]
public partial struct CourseLikeId;

[ValueObject<int>(Conversions.EfCoreValueConverter)]
public partial struct EmailVerificationId;

[ValueObject<int>(Conversions.EfCoreValueConverter)]
public partial struct FollowId;

[ValueObject<int>(Conversions.EfCoreValueConverter)]
public partial struct KifuId;

[ValueObject<int>(Conversions.EfCoreValueConverter)]
public partial struct LikeId;

[ValueObject<int>(Conversions.EfCoreValueConverter)]
public partial struct NotificationId;

[ValueObject<int>(Conversions.EfCoreValueConverter)]
public partial struct PasswordResetTokenId;

[ValueObject<Guid>(Conversions.EfCoreValueConverter)]
public partial struct TagId;

[ValueObject<Guid>(Conversions.EfCoreValueConverter)]
public partial struct UserId;

[ValueObject<Guid>(Conversions.EfCoreValueConverter)]
public partial struct PlaybookId;

[ValueObject<int>(Conversions.EfCoreValueConverter)]
public partial struct PlaybookNodeId;

[ValueObject<Guid>(Conversions.EfCoreValueConverter)]
public partial struct PlaybookBoardId;

[ValueObject<int>(Conversions.EfCoreValueConverter)]
public partial struct PlaybookBoardTransitionId;

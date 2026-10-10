namespace NariNoteBackend.Domain.Entity;

public abstract class EntityBase
{
    // 未設定の場合は NariNoteDbContext が保存時に現在時刻を設定する
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

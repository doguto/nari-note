namespace NariNoteBackend.Tests.Support;

/// <summary>
///     テストから現在時刻を自由に設定できる TimeProvider。
///     FakeTimeProvider と異なり時刻を巻き戻せるため、結合テストでテストごとに初期値へ戻せる。
/// </summary>
public class TestTimeProvider : TimeProvider
{
    public static readonly DateTime DefaultUtcNow = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    DateTime utcNow = DefaultUtcNow;

    public override DateTimeOffset GetUtcNow()
    {
        return new DateTimeOffset(this.utcNow, TimeSpan.Zero);
    }

    public void SetUtcNow(DateTime value)
    {
        this.utcNow = DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    public void Advance(TimeSpan delta)
    {
        this.utcNow += delta;
    }

    public void Reset()
    {
        this.utcNow = DefaultUtcNow;
    }
}

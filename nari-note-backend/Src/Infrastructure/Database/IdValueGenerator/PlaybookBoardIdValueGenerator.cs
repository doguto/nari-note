using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Infrastructure.Database;

// 盤面は ID の順を登録順として扱う（ノードの代表の盤面など）。1 回の保存で同じミリ秒に複数の盤面を作るので、
// ミリ秒の中でも順序が保たれるよう、UUIDv7 の rand_a をカウンターにする（RFC 9562 6.2 Method 1）
public class PlaybookBoardIdValueGenerator : ValueGenerator<PlaybookBoardId>
{
    const int CounterMax = 0xFFF;

    static readonly Lock gate = new();
    static long lastMilliseconds;
    static int counter;

    public override bool GeneratesTemporaryValues => false;

    public override PlaybookBoardId Next(EntityEntry entry)
    {
        return PlaybookBoardId.From(NextGuid());
    }

    static Guid NextGuid()
    {
        var bytes = Guid.CreateVersion7().ToByteArray(bigEndian: true);
        var milliseconds = 0L;
        for (var i = 0; i < 6; i++) milliseconds = (milliseconds << 8) | bytes[i];

        lock (gate)
        {
            if (milliseconds > lastMilliseconds)
            {
                lastMilliseconds = milliseconds;
                counter = 0;
            }
            else if (counter < CounterMax)
            {
                counter++;
            }
            else
            {
                lastMilliseconds++;
                counter = 0;
            }
            milliseconds = lastMilliseconds;

            for (var i = 5; i >= 0; i--)
            {
                bytes[i] = (byte)milliseconds;
                milliseconds >>= 8;
            }
            bytes[6] = (byte)(0x70 | (counter >> 8));
            bytes[7] = (byte)counter;
        }

        return new Guid(bytes, bigEndian: true);
    }
}

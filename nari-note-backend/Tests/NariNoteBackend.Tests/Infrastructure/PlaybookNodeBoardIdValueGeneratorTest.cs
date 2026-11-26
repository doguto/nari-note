using NariNoteBackend.Infrastructure.Database;

namespace NariNoteBackend.Tests.Infrastructure;

public class PlaybookNodeBoardIdValueGeneratorTest
{
    [Fact]
    public void 同じミリ秒に続けて作っても作った順に大きくなる()
    {
        var generator = new PlaybookNodeBoardIdValueGenerator();

        var ids = Enumerable.Range(0, 10000).Select(_ => generator.Next(null!).Value).ToList();

        Assert.Equal(ids, ids.Order());
        Assert.Equal(ids.Count, ids.Distinct().Count());
        Assert.All(ids, id => Assert.Equal(7, id.Version));
    }
}

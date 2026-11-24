using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ReadModel;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Infrastructure.Database;

namespace NariNoteBackend.Infrastructure.Repository;

public class BookRepository : IBookRepository
{
    readonly NariNoteDbContext context;

    public BookRepository(NariNoteDbContext context)
    {
        this.context = context;
    }

    public async Task<Book> CreateAsync(Book entity)
    {
        this.context.Books.Add(entity);
        await this.context.SaveChangesAsync();
        return entity;
    }

    public async Task<Book?> FindByIdAsync(BookId id)
    {
        return await this.context.Books.FindAsync(id);
    }

    public async Task<Book> FindForceByIdAsync(BookId id)
    {
        var book = await this.FindByIdAsync(id);
        if (book == null) throw new KeyNotFoundException($"定跡{id}が存在しません");
        return book;
    }

    public async Task<Book> UpdateAsync(Book entity)
    {
        this.context.Books.Update(entity);
        await this.context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(BookId id)
    {
        await this.context.Books.Where(b => b.Id == id).ExecuteDeleteAsync();
    }

    public async Task<List<BookSummary>> FindSummariesByUserAsync(UserId userId)
    {
        return await this.context.Books
                         .Where(b => b.UserId == userId)
                         .OrderByDescending(b => b.UpdatedAt)
                         .Select(b => new BookSummary
                         {
                             Id = b.Id,
                             Name = b.Name,
                             PositionCount = b.Positions.Count,
                             UpdatedAt = b.UpdatedAt,
                         })
                         .ToListAsync();
    }

    public async Task<Book> FindForceGraphByIdAsync(BookId id)
    {
        var book = await this.context.Books
                             .Include(b => b.Positions)
                             .Include(b => b.Moves)
                             .Include(b => b.ShapeGroups)
                             .Include(b => b.ShapeCandidates)
                             .AsSplitQuery()
                             .FirstOrDefaultAsync(b => b.Id == id);
        if (book == null) throw new KeyNotFoundException($"定跡{id}が存在しません");
        return book;
    }

    public void AddPosition(BookPosition position) => this.context.BookPositions.Add(position);

    public void AddMove(BookMove move) => this.context.BookMoves.Add(move);

    public void AddShapeGroup(ShapeGroup group) => this.context.ShapeGroups.Add(group);

    public void AddShapeCandidate(ShapeCandidate candidate) => this.context.ShapeCandidates.Add(candidate);

    public void RemovePosition(BookPosition position) => this.context.BookPositions.Remove(position);

    public void RemoveMove(BookMove move) => this.context.BookMoves.Remove(move);

    public void RemoveShapeGroup(ShapeGroup group) => this.context.ShapeGroups.Remove(group);

    public void RemoveShapeCandidate(ShapeCandidate candidate) => this.context.ShapeCandidates.Remove(candidate);

    public async Task SaveAsync()
    {
        await this.context.SaveChangesAsync();
    }

    public async Task<int> CountPendingJudgesAsync(BookId id)
    {
        var unscanned = await this.context.BookPositions.CountAsync(p => p.BookId == id && !p.CandidatesScanned);
        var queued = await this.context.ShapeCandidates.CountAsync(c =>
            c.BookId == id && (c.Status == ShapeCandidateStatus.Queued || c.Status == ShapeCandidateStatus.Running)
        );
        return unscanned + queued;
    }

    public async Task<List<BookPosition>> FindUnscannedPositionsAsync(int limit)
    {
        return await this.context.BookPositions
                         .Where(p => !p.CandidatesScanned)
                         .OrderBy(p => p.Id)
                         .Take(limit)
                         .ToListAsync();
    }

    public async Task<List<BookPosition>> FindPositionsAsync(BookId bookId)
    {
        return await this.context.BookPositions.Where(p => p.BookId == bookId).ToListAsync();
    }

    public async Task<ShapeCandidate?> FindNextQueuedCandidateAsync()
    {
        return await this.context.ShapeCandidates
                         .Include(c => c.PositionA)
                         .Include(c => c.PositionB)
                         .Where(c => c.Status == ShapeCandidateStatus.Queued)
                         .OrderBy(c => c.Id)
                         .FirstOrDefaultAsync();
    }

    public async Task ResetRunningCandidatesAsync()
    {
        await this.context.ShapeCandidates
                  .Where(c => c.Status == ShapeCandidateStatus.Running)
                  .ExecuteUpdateAsync(s => s.SetProperty(c => c.Status, ShapeCandidateStatus.Queued));
    }
}

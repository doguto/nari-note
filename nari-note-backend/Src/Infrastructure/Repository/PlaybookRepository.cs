using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ReadModel;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Infrastructure.Database;

namespace NariNoteBackend.Infrastructure.Repository;

public class PlaybookRepository : IPlaybookRepository
{
    readonly NariNoteDbContext context;

    public PlaybookRepository(NariNoteDbContext context)
    {
        this.context = context;
    }

    public async Task<Playbook> CreateAsync(Playbook entity)
    {
        this.context.Playbooks.Add(entity);
        await this.context.SaveChangesAsync();
        return entity;
    }

    public async Task<Playbook?> FindByIdAsync(PlaybookId id)
    {
        return await this.context.Playbooks.FindAsync(id);
    }

    public async Task<Playbook> FindForceByIdAsync(PlaybookId id)
    {
        var playbook = await this.FindByIdAsync(id);
        if (playbook == null) throw new KeyNotFoundException($"定跡{id}が存在しません");
        return playbook;
    }

    public async Task<Playbook> UpdateAsync(Playbook entity)
    {
        this.context.Playbooks.Update(entity);
        await this.context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(PlaybookId id)
    {
        await this.context.Playbooks.Where(b => b.Id == id).ExecuteDeleteAsync();
    }

    public async Task<List<PlaybookSummary>> FindSummariesByUserAsync(UserId userId)
    {
        return await this.context.Playbooks
                         .Where(b => b.UserId == userId)
                         .OrderByDescending(b => b.UpdatedAt)
                         .Select(b => new PlaybookSummary
                         {
                             Id = b.Id,
                             Name = b.Name,
                             PositionCount = b.Positions.Count,
                             UpdatedAt = b.UpdatedAt,
                         })
                         .ToListAsync();
    }

    public async Task<Playbook> FindForceGraphByIdAsync(PlaybookId id)
    {
        var playbook = await this.context.Playbooks
                             .Include(b => b.Positions)
                             .Include(b => b.Moves)
                             .Include(b => b.ShapeGroups)
                             .Include(b => b.ShapeCandidates)
                             .AsSplitQuery()
                             .FirstOrDefaultAsync(b => b.Id == id);
        if (playbook == null) throw new KeyNotFoundException($"定跡{id}が存在しません");
        return playbook;
    }

    public void AddPosition(PlaybookPosition position) => this.context.PlaybookPositions.Add(position);

    public void AddMove(PlaybookMove move) => this.context.PlaybookMoves.Add(move);

    public void AddShapeGroup(ShapeGroup group) => this.context.ShapeGroups.Add(group);

    public void AddShapeCandidate(ShapeCandidate candidate) => this.context.ShapeCandidates.Add(candidate);

    public void RemovePosition(PlaybookPosition position) => this.context.PlaybookPositions.Remove(position);

    public void RemoveMove(PlaybookMove move) => this.context.PlaybookMoves.Remove(move);

    public void RemoveShapeGroup(ShapeGroup group) => this.context.ShapeGroups.Remove(group);

    public void RemoveShapeCandidate(ShapeCandidate candidate) => this.context.ShapeCandidates.Remove(candidate);

    public async Task SaveAsync()
    {
        await this.context.SaveChangesAsync();
    }

    public async Task<int> CountPendingJudgesAsync(PlaybookId id)
    {
        var unscanned = await this.context.PlaybookPositions.CountAsync(p => p.PlaybookId == id && !p.CandidatesScanned);
        var queued = await this.context.ShapeCandidates.CountAsync(c =>
            c.PlaybookId == id && (c.Status == ShapeCandidateStatus.Queued || c.Status == ShapeCandidateStatus.Running)
        );
        return unscanned + queued;
    }

    public async Task<List<PlaybookPosition>> FindUnscannedPositionsAsync(int limit)
    {
        return await this.context.PlaybookPositions
                         .Where(p => !p.CandidatesScanned)
                         .OrderBy(p => p.Id)
                         .Take(limit)
                         .ToListAsync();
    }

    public async Task<List<PlaybookPosition>> FindPositionsAsync(PlaybookId playbookId)
    {
        return await this.context.PlaybookPositions.Where(p => p.PlaybookId == playbookId).ToListAsync();
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

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
                             UpdatedAt = b.UpdatedAt,
                         })
                         .ToListAsync();
    }

    public async Task<Playbook> FindForceGraphByIdAsync(PlaybookId id)
    {
        var playbook = await this.context.Playbooks
                             .Include(b => b.Nodes)
                             .Include(b => b.Boards)
                             .Include(b => b.Transitions)
                             .AsSplitQuery()
                             .FirstOrDefaultAsync(b => b.Id == id);
        if (playbook == null) throw new KeyNotFoundException($"定跡{id}が存在しません");
        return playbook;
    }

    public void AddNode(PlaybookNode node) => this.context.PlaybookNodes.Add(node);

    public void AddBoard(PlaybookBoard board) => this.context.PlaybookBoards.Add(board);

    public void AddTransition(PlaybookBoardTransition transition) => this.context.PlaybookBoardTransitions.Add(transition);

    public void RemoveNode(PlaybookNode node) => this.context.PlaybookNodes.Remove(node);

    public void RemoveBoard(PlaybookBoard board) => this.context.PlaybookBoards.Remove(board);

    public void RemoveTransition(PlaybookBoardTransition transition) => this.context.PlaybookBoardTransitions.Remove(transition);

    public async Task SaveAsync()
    {
        await this.context.SaveChangesAsync();
    }
}

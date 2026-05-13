using Microsoft.EntityFrameworkCore;
using Nebula.Application.Interfaces;
using Nebula.Domain.Entities;
using Nebula.Infrastructure.Persistence;

namespace Nebula.Infrastructure.Repositories;

public class CommunicationEventRepository(AppDbContext db) : ICommunicationEventRepository
{
    public async Task<CommunicationEvent?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await db.CommunicationEvents.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<(IReadOnlyList<CommunicationEvent> Items, int TotalCount)> ListByEntityAsync(
        string entityType, Guid entityId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.CommunicationEvents
            .Where(e => e.PrimaryEntityType == entityType && e.PrimaryEntityId == entityId);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(e => e.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task AddAsync(CommunicationEvent communicationEvent, CancellationToken ct = default)
    {
        await db.CommunicationEvents.AddAsync(communicationEvent, ct);
    }

    public async Task<bool> EntityExistsAsync(string entityType, Guid entityId, CancellationToken ct = default)
    {
        return entityType switch
        {
            "Broker"     => await db.Brokers.AnyAsync(e => e.Id == entityId && !e.IsDeleted, ct),
            "Account"    => await db.Accounts.AnyAsync(e => e.Id == entityId && !e.IsDeleted, ct),
            "Submission" => await db.Submissions.AnyAsync(e => e.Id == entityId && !e.IsDeleted, ct),
            "Policy"     => await db.Policies.AnyAsync(e => e.Id == entityId && !e.IsDeleted, ct),
            "Renewal"    => await db.Renewals.AnyAsync(e => e.Id == entityId && !e.IsDeleted, ct),
            _            => false,
        };
    }
}

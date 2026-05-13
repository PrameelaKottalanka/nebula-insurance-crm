using Nebula.Domain.Entities;

namespace Nebula.Application.Interfaces;

public interface ICommunicationEventRepository
{
    Task<CommunicationEvent?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<CommunicationEvent> Items, int TotalCount)> ListByEntityAsync(
        string entityType, Guid entityId, int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(CommunicationEvent communicationEvent, CancellationToken ct = default);
    Task<bool> EntityExistsAsync(string entityType, Guid entityId, CancellationToken ct = default);
}

using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Shouldly;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nebula.Application.DTOs;
using Nebula.Domain.Entities;
using Nebula.Infrastructure.Persistence;

namespace Nebula.Tests.Integration;

[Collection(IntegrationTestCollection.Name)]
public class CommunicationEndpointTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    public async Task InitializeAsync()
    {
        TestAuthHandler.TestSubject = "test-user-001";
        TestAuthHandler.TestRole = "Admin";
        TestAuthHandler.TestDisplayName = "Test Admin";
        TestAuthHandler.TestNebulaRoles = null;
        TestAuthHandler.ResetF0009Overrides();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.ExecuteSqlRawAsync(
            "DELETE FROM \"Tasks\" WHERE \"LinkedEntityType\" = 'CommunicationEvent'; " +
            "DELETE FROM \"CommunicationEvents\";");
    }

    public Task DisposeAsync()
    {
        TestAuthHandler.TestSubject = "test-user-001";
        TestAuthHandler.TestRole = "Admin";
        TestAuthHandler.TestDisplayName = "Test User";
        TestAuthHandler.ResetF0009Overrides();
        return Task.CompletedTask;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  POST /communications — Log Communication
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task LogCommunication_Note_Returns201()
    {
        var brokerId = await CreateTestBrokerAsync();
        var dto = NoteRequest(brokerId);

        var response = await _client.PostAsJsonAsync("/communications", dto);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<CommunicationEventSummaryDto>();
        result.ShouldNotBeNull();
        result!.EventType.ShouldBe("Note");
        result.Body.ShouldBe("Test body");
        result.PrimaryEntityType.ShouldBe("Broker");
        result.PrimaryEntityId.ShouldBe(brokerId);
        result.IsRedacted.ShouldBeFalse();
    }

    [Fact]
    public async Task LogCommunication_Call_Returns201WithDirectionAndDuration()
    {
        var brokerId = await CreateTestBrokerAsync();
        var dto = new CommunicationLogRequest("Call", "Inbound", 45, "Renewal discussion",
            "Discussed renewal terms", "Positive", null, "Broker", brokerId);

        var response = await _client.PostAsJsonAsync("/communications", dto);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<CommunicationEventSummaryDto>();
        result!.Direction.ShouldBe("Inbound");
        result.DurationMinutes.ShouldBe(45);
    }

    [Fact]
    public async Task LogCommunication_Meeting_Returns201()
    {
        var brokerId = await CreateTestBrokerAsync();
        var dto = new CommunicationLogRequest("Meeting", "Outbound", 90, "Quarterly review",
            "Discussed portfolio", "Neutral", null, "Broker", brokerId);

        var response = await _client.PostAsJsonAsync("/communications", dto);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<CommunicationEventSummaryDto>();
        result!.EventType.ShouldBe("Meeting");
    }

    [Fact]
    public async Task LogCommunication_InvalidEventType_Returns400()
    {
        var brokerId = await CreateTestBrokerAsync();
        var dto = new CommunicationLogRequest("Fax", null, null, null,
            "Body", null, null, "Broker", brokerId);

        var response = await _client.PostAsJsonAsync("/communications", dto);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task LogCommunication_CallMissingDirection_Returns400()
    {
        var brokerId = await CreateTestBrokerAsync();
        var dto = new CommunicationLogRequest("Call", null, 30, null,
            "Body", null, null, "Broker", brokerId);

        var response = await _client.PostAsJsonAsync("/communications", dto);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task LogCommunication_CallMissingDuration_Returns400()
    {
        var brokerId = await CreateTestBrokerAsync();
        var dto = new CommunicationLogRequest("Call", "Inbound", null, null,
            "Body", null, null, "Broker", brokerId);

        var response = await _client.PostAsJsonAsync("/communications", dto);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task LogCommunication_EmptyBody_Returns400()
    {
        var brokerId = await CreateTestBrokerAsync();
        var dto = new CommunicationLogRequest("Note", null, null, null,
            "", null, null, "Broker", brokerId);

        var response = await _client.PostAsJsonAsync("/communications", dto);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task LogCommunication_BrokerUser_Returns403()
    {
        TestAuthHandler.TestRole = "BrokerUser";
        TestAuthHandler.TestNebulaRoles = ["BrokerUser"];
        var brokerId = await CreateTestBrokerAsync();
        var dto = NoteRequest(brokerId);

        var response = await _client.PostAsJsonAsync("/communications", dto);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task LogCommunication_EntityNotFound_Returns404()
    {
        var dto = NoteRequest(Guid.NewGuid());

        var response = await _client.PostAsJsonAsync("/communications", dto);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  GET /communications — List by Entity
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task ListCommunications_ValidEntity_Returns200WithPaginatedData()
    {
        var brokerId = await CreateTestBrokerAsync();
        await LogTestCommunicationAsync(brokerId);
        await LogTestCommunicationAsync(brokerId);

        var response = await _client.GetAsync($"/communications?entityType=Broker&entityId={brokerId}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedCommunicationListDto>();
        result.ShouldNotBeNull();
        result!.TotalCount.ShouldBeGreaterThanOrEqualTo(2);
        result.Data.ShouldNotBeEmpty();
        result.Page.ShouldBe(1);
    }

    [Fact]
    public async Task ListCommunications_EmptyEntity_Returns200WithEmptyData()
    {
        var brokerId = await CreateTestBrokerAsync();

        var response = await _client.GetAsync($"/communications?entityType=Broker&entityId={brokerId}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedCommunicationListDto>();
        result!.Data.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task ListCommunications_BrokerUser_Returns403()
    {
        TestAuthHandler.TestRole = "BrokerUser";
        TestAuthHandler.TestNebulaRoles = ["BrokerUser"];
        var brokerId = await CreateTestBrokerAsync();

        var response = await _client.GetAsync($"/communications?entityType=Broker&entityId={brokerId}");

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ListCommunications_EntityNotFound_Returns404()
    {
        var response = await _client.GetAsync($"/communications?entityType=Broker&entityId={Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ListCommunications_PaginationReturnsCorrectPage()
    {
        var brokerId = await CreateTestBrokerAsync();
        for (var i = 0; i < 3; i++)
            await LogTestCommunicationAsync(brokerId);

        var response = await _client.GetAsync($"/communications?entityType=Broker&entityId={brokerId}&page=1&pageSize=2");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedCommunicationListDto>();
        result!.Data.Count.ShouldBe(2);
        result.TotalCount.ShouldBeGreaterThanOrEqualTo(3);
        result.TotalPages.ShouldBeGreaterThanOrEqualTo(2);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  GET /communications/{id} — Get by ID
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetCommunication_Exists_Returns200()
    {
        var brokerId = await CreateTestBrokerAsync();
        var commId = await LogTestCommunicationAsync(brokerId);

        var response = await _client.GetAsync($"/communications/{commId}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<CommunicationEventSummaryDto>();
        result.ShouldNotBeNull();
        result!.Id.ShouldBe(commId);
    }

    [Fact]
    public async Task GetCommunication_NotFound_Returns404()
    {
        var response = await _client.GetAsync($"/communications/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCommunication_BrokerUser_Returns403()
    {
        var brokerId = await CreateTestBrokerAsync();
        var commId = await LogTestCommunicationAsync(brokerId);
        TestAuthHandler.TestRole = "BrokerUser";
        TestAuthHandler.TestNebulaRoles = ["BrokerUser"];

        var response = await _client.GetAsync($"/communications/{commId}");

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  PUT /communications/{id} — Edit
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task EditCommunication_AsAuthor_Returns200()
    {
        var brokerId = await CreateTestBrokerAsync();
        var commId = await LogTestCommunicationAsync(brokerId);
        var req = new CommunicationEditRequest("Updated body", null, null, null);

        var response = await _client.PutAsJsonAsync($"/communications/{commId}", req);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<CommunicationEventSummaryDto>();
        result!.Body.ShouldBe("Updated body");
        result.LastEditedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task EditCommunication_AsAdmin_Returns200()
    {
        // Log as DistributionUser, then edit as Admin
        TestAuthHandler.TestSubject = "test-comm-dist-001";
        TestAuthHandler.TestRole = "DistributionUser";
        TestAuthHandler.TestNebulaRoles = ["DistributionUser"];
        TestAuthHandler.TestDisplayName = "Distribution Author";
        var brokerId = await CreateTestBrokerAsync();
        var commId = await LogTestCommunicationAsync(brokerId);

        TestAuthHandler.TestSubject = "test-user-001";
        TestAuthHandler.TestRole = "Admin";
        TestAuthHandler.TestNebulaRoles = null;
        TestAuthHandler.TestDisplayName = "Admin User";
        var req = new CommunicationEditRequest("Admin edit", null, null, null);

        var response = await _client.PutAsJsonAsync($"/communications/{commId}", req);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<CommunicationEventSummaryDto>();
        result!.Body.ShouldBe("Admin edit");
    }

    [Fact]
    public async Task EditCommunication_NotAuthor_Returns403()
    {
        // Log as user-001
        TestAuthHandler.TestSubject = "test-user-001";
        TestAuthHandler.TestRole = "DistributionUser";
        TestAuthHandler.TestNebulaRoles = ["DistributionUser"];
        TestAuthHandler.TestDisplayName = "First User";
        var brokerId = await CreateTestBrokerAsync();
        var commId = await LogTestCommunicationAsync(brokerId);

        // Edit as a different (non-Admin) user
        TestAuthHandler.TestSubject = "test-comm-other-001";
        TestAuthHandler.TestRole = "DistributionUser";
        TestAuthHandler.TestNebulaRoles = ["DistributionUser"];
        TestAuthHandler.TestDisplayName = "Other User";
        // Trigger profile creation for new user
        await _client.GetAsync("/my/tasks");

        var req = new CommunicationEditRequest("Unauthorized edit", null, null, null);

        var response = await _client.PutAsJsonAsync($"/communications/{commId}", req);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task EditCommunication_NoChanges_Returns400()
    {
        var brokerId = await CreateTestBrokerAsync();
        var commId = await LogTestCommunicationAsync(brokerId);
        var req = new CommunicationEditRequest(null, null, null, null);

        var response = await _client.PutAsJsonAsync($"/communications/{commId}", req);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task EditCommunication_AfterRedaction_Returns409()
    {
        var brokerId = await CreateTestBrokerAsync();
        var commId = await LogTestCommunicationAsync(brokerId);

        // Redact first
        var redactReq = new CommunicationRedactRequest("GDPR");
        await _client.PostAsJsonAsync($"/communications/{commId}/redact", redactReq);

        // Then try to edit
        var editReq = new CommunicationEditRequest("Attempt edit", null, null, null);
        var response = await _client.PutAsJsonAsync($"/communications/{commId}", editReq);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task EditCommunication_NotFound_Returns404()
    {
        var req = new CommunicationEditRequest("Body", null, null, null);

        var response = await _client.PutAsJsonAsync($"/communications/{Guid.NewGuid()}", req);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  POST /communications/{id}/redact — Redact
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task RedactCommunication_Admin_Returns200()
    {
        var brokerId = await CreateTestBrokerAsync();
        var commId = await LogTestCommunicationAsync(brokerId);
        var req = new CommunicationRedactRequest("Compliance order");

        var response = await _client.PostAsJsonAsync($"/communications/{commId}/redact", req);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<CommunicationEventSummaryDto>();
        result!.IsRedacted.ShouldBeTrue();
        result.Body.ShouldBeNull();
        result.RedactionReason.ShouldBe("Compliance order");
    }

    [Fact]
    public async Task RedactCommunication_NonAdmin_Returns403()
    {
        TestAuthHandler.TestRole = "DistributionUser";
        TestAuthHandler.TestNebulaRoles = ["DistributionUser"];
        var brokerId = await CreateTestBrokerAsync();
        var commId = await LogTestCommunicationAsync(brokerId);
        var req = new CommunicationRedactRequest("Unauthorized");

        // Re-set to DistributionUser before the redact call
        TestAuthHandler.TestRole = "DistributionUser";
        TestAuthHandler.TestNebulaRoles = ["DistributionUser"];
        var response = await _client.PostAsJsonAsync($"/communications/{commId}/redact", req);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RedactCommunication_AlreadyRedacted_Returns409()
    {
        var brokerId = await CreateTestBrokerAsync();
        var commId = await LogTestCommunicationAsync(brokerId);
        var req = new CommunicationRedactRequest("First redaction");

        await _client.PostAsJsonAsync($"/communications/{commId}/redact", req);

        var second = await _client.PostAsJsonAsync($"/communications/{commId}/redact",
            new CommunicationRedactRequest("Second attempt"));

        second.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task RedactCommunication_MissingReason_Returns400()
    {
        var brokerId = await CreateTestBrokerAsync();
        var commId = await LogTestCommunicationAsync(brokerId);
        var json = JsonSerializer.Serialize(new { reason = "" });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync($"/communications/{commId}/redact", content);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RedactCommunication_NotFound_Returns404()
    {
        var req = new CommunicationRedactRequest("Reason");

        var response = await _client.PostAsJsonAsync($"/communications/{Guid.NewGuid()}/redact", req);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  POST /communications/{id}/follow-up — Create Follow-Up Task
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task CreateFollowUp_Returns201WithTask()
    {
        var brokerId = await CreateTestBrokerAsync();
        var commId = await LogTestCommunicationAsync(brokerId);
        var req = new CommunicationFollowUpRequest("Follow up on meeting", null, null, null);

        var response = await _client.PostAsJsonAsync($"/communications/{commId}/follow-up", req);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<TaskSummaryDto>();
        result.ShouldNotBeNull();
        result!.Title.ShouldBe("Follow up on meeting");
        result.Status.ShouldBe("Open");
    }

    [Fact]
    public async Task CreateFollowUp_AlreadyExists_Returns409()
    {
        var brokerId = await CreateTestBrokerAsync();
        var commId = await LogTestCommunicationAsync(brokerId);
        var req = new CommunicationFollowUpRequest("First follow up", null, null, null);
        await _client.PostAsJsonAsync($"/communications/{commId}/follow-up", req);

        var second = await _client.PostAsJsonAsync($"/communications/{commId}/follow-up",
            new CommunicationFollowUpRequest("Second follow up", null, null, null));

        second.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateFollowUp_MissingTitle_Returns400()
    {
        var brokerId = await CreateTestBrokerAsync();
        var commId = await LogTestCommunicationAsync(brokerId);
        var json = JsonSerializer.Serialize(new { title = "" });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync($"/communications/{commId}/follow-up", content);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateFollowUp_BrokerUser_Returns403()
    {
        var brokerId = await CreateTestBrokerAsync();
        var commId = await LogTestCommunicationAsync(brokerId);
        TestAuthHandler.TestRole = "BrokerUser";
        TestAuthHandler.TestNebulaRoles = ["BrokerUser"];
        var req = new CommunicationFollowUpRequest("Unauthorized follow up", null, null, null);

        var response = await _client.PostAsJsonAsync($"/communications/{commId}/follow-up", req);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateFollowUp_AppearInCommunicationGet()
    {
        var brokerId = await CreateTestBrokerAsync();
        var commId = await LogTestCommunicationAsync(brokerId);
        var followUpReq = new CommunicationFollowUpRequest("Task from follow-up", null, null, "High");
        var followUpResp = await _client.PostAsJsonAsync($"/communications/{commId}/follow-up", followUpReq);
        followUpResp.EnsureSuccessStatusCode();
        var task = await followUpResp.Content.ReadFromJsonAsync<TaskSummaryDto>();

        var commResp = await _client.GetAsync($"/communications/{commId}");
        var comm = await commResp.Content.ReadFromJsonAsync<CommunicationEventSummaryDto>();

        comm!.FollowUpTaskId.ShouldBe(task!.Id);
        comm.FollowUpTaskTitle.ShouldBe("Task from follow-up");
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  Full Lifecycle
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task FullLifecycle_LogEditRedact()
    {
        var brokerId = await CreateTestBrokerAsync();

        // Log a Note
        var logResp = await _client.PostAsJsonAsync("/communications", NoteRequest(brokerId));
        logResp.StatusCode.ShouldBe(HttpStatusCode.Created);
        var comm = await logResp.Content.ReadFromJsonAsync<CommunicationEventSummaryDto>();

        // Edit the note (as author/Admin)
        var editResp = await _client.PutAsJsonAsync($"/communications/{comm!.Id}",
            new CommunicationEditRequest("Edited body", "Updated subject", null, null));
        editResp.StatusCode.ShouldBe(HttpStatusCode.OK);
        var edited = await editResp.Content.ReadFromJsonAsync<CommunicationEventSummaryDto>();
        edited!.Body.ShouldBe("Edited body");
        edited.LastEditedAt.ShouldNotBeNull();

        // Redact
        var redactResp = await _client.PostAsJsonAsync($"/communications/{comm.Id}/redact",
            new CommunicationRedactRequest("Compliance"));
        redactResp.StatusCode.ShouldBe(HttpStatusCode.OK);
        var redacted = await redactResp.Content.ReadFromJsonAsync<CommunicationEventSummaryDto>();
        redacted!.IsRedacted.ShouldBeTrue();
        redacted.Body.ShouldBeNull();

        // Verify edit fails after redaction
        var postRedactEdit = await _client.PutAsJsonAsync($"/communications/{comm.Id}",
            new CommunicationEditRequest("After redact", null, null, null));
        postRedactEdit.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  Helpers
    // ═══════════════════════════════════════════════════════════════════════

    private async Task<Guid> CreateTestBrokerAsync()
    {
        var now = DateTime.UtcNow;
        var systemUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var broker = new Broker
        {
            Id = Guid.NewGuid(),
            LegalName = $"Test Broker {Guid.NewGuid():N}",
            LicenseNumber = $"LIC-{Guid.NewGuid():N}"[..20],
            State = "CA",
            Status = "Active",
            CreatedAt = now,
            UpdatedAt = now,
            CreatedByUserId = systemUserId,
            UpdatedByUserId = systemUserId,
        };

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Brokers.Add(broker);
        await db.SaveChangesAsync();
        return broker.Id;
    }

    private async Task<Guid> LogTestCommunicationAsync(Guid brokerId)
    {
        var dto = NoteRequest(brokerId);
        var response = await _client.PostAsJsonAsync("/communications", dto);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CommunicationEventSummaryDto>();
        return result!.Id;
    }

    private static CommunicationLogRequest NoteRequest(Guid brokerId) =>
        new("Note", null, null, "Test subject", "Test body", null, null, "Broker", brokerId);
}

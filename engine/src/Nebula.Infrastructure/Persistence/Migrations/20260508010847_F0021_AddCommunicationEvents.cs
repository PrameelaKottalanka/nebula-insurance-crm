using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nebula.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class F0021_AddCommunicationEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Accounts_Region",
                table: "Accounts");

            migrationBuilder.RenameColumn(
                name: "Carrier",
                table: "Policies",
                newName: "ExternalPolicyReference");

            migrationBuilder.RenameColumn(
                name: "PrimaryState",
                table: "Accounts",
                newName: "State");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Accounts",
                newName: "DisplayName");

            migrationBuilder.AddColumn<string>(
                name: "AccountDisplayNameAtLink",
                table: "Submissions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AccountStatusAtRead",
                table: "Submissions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "AccountSurvivorId",
                table: "Submissions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountDisplayNameAtLink",
                table: "Renewals",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AccountStatusAtRead",
                table: "Renewals",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "AccountSurvivorId",
                table: "Renewals",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Premium",
                table: "Policies",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PolicyNumber",
                table: "Policies",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "LineOfBusiness",
                table: "Policies",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CurrentStatus",
                table: "Policies",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldDefaultValue: "Active");

            migrationBuilder.AddColumn<string>(
                name: "AccountDisplayNameAtLink",
                table: "Policies",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AccountStatusAtRead",
                table: "Policies",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "AccountSurvivorId",
                table: "Policies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BoundAt",
                table: "Policies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancellationEffectiveDate",
                table: "Policies",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReasonCode",
                table: "Policies",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReasonDetail",
                table: "Policies",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "Policies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CarrierId",
                table: "Policies",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CurrentVersionId",
                table: "Policies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiredAt",
                table: "Policies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImportSource",
                table: "Policies",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "manual");

            migrationBuilder.AddColumn<DateTime>(
                name: "IssuedAt",
                table: "Policies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PredecessorPolicyId",
                table: "Policies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PremiumCurrency",
                table: "Policies",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "USD");

            migrationBuilder.AddColumn<Guid>(
                name: "ProducerUserId",
                table: "Policies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReinstatementDeadline",
                table: "Policies",
                type: "date",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Region",
                table: "Accounts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Industry",
                table: "Accounts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "State",
                table: "Accounts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2)",
                oldMaxLength: 2);

            migrationBuilder.AddColumn<string>(
                name: "Address1",
                table: "Accounts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address2",
                table: "Accounts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BrokerOfRecordId",
                table: "Accounts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Accounts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Accounts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeleteReasonCode",
                table: "Accounts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeleteReasonDetail",
                table: "Accounts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegalName",
                table: "Accounts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MergedIntoAccountId",
                table: "Accounts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Accounts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryLineOfBusiness",
                table: "Accounts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PrimaryProducerUserId",
                table: "Accounts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RemovedAt",
                table: "Accounts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StableDisplayName",
                table: "Accounts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TaxId",
                table: "Accounts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TerritoryCode",
                table: "Accounts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AccountContacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Role = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountContacts_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccountRelationshipHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelationshipType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PreviousValue = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    NewValue = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EffectiveAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountRelationshipHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountRelationshipHistory_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CarrierRefs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    NaicCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarrierRefs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommunicationEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Direction = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Body = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Outcome = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PrimaryEntityType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PrimaryEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthoredByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthoredByDisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LastEditedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastEditedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsRedacted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RedactedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RedactedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RedactedByDisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RedactionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FollowUpTaskId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunicationEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IdempotencyRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Operation = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponseStatusCode = table.Column<int>(type: "integer", nullable: false),
                    ResponsePayloadJson = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdempotencyRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PolicyVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    VersionReason = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    EndorsementId = table.Column<Guid>(type: "uuid", nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "date", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "date", nullable: false),
                    TotalPremium = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PremiumCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    ProfileSnapshotJson = table.Column<string>(type: "jsonb", nullable: false),
                    CoverageSnapshotJson = table.Column<string>(type: "jsonb", nullable: false),
                    PremiumSnapshotJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicyVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PolicyVersions_Policies_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "Policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PolicyCoverageLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyId = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    CoverageCode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CoverageName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Limit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Deductible = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Premium = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PremiumCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    ExposureBasis = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ExposureQuantity = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicyCoverageLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PolicyCoverageLines_Policies_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "Policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PolicyCoverageLines_PolicyVersions_PolicyVersionId",
                        column: x => x.PolicyVersionId,
                        principalTable: "PolicyVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PolicyEndorsements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyId = table.Column<Guid>(type: "uuid", nullable: false),
                    EndorsementNumber = table.Column<int>(type: "integer", nullable: false),
                    PolicyVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    EndorsementReasonCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    EndorsementReasonDetail = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "date", nullable: false),
                    PremiumDelta = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PremiumCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicyEndorsements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PolicyEndorsements_Policies_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "Policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PolicyEndorsements_PolicyVersions_PolicyVersionId",
                        column: x => x.PolicyVersionId,
                        principalTable: "PolicyVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Policies_CarrierId",
                table: "Policies",
                column: "CarrierId");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_CurrentStatus",
                table: "Policies",
                column: "CurrentStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_CurrentVersionId",
                table: "Policies",
                column: "CurrentVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_PredecessorPolicyId",
                table: "Policies",
                column: "PredecessorPolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_ProducerUserId",
                table: "Policies",
                column: "ProducerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_BrokerOfRecordId",
                table: "Accounts",
                column: "BrokerOfRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_DisplayName_Trgm",
                table: "Accounts",
                column: "DisplayName")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_MergedIntoAccountId",
                table: "Accounts",
                column: "MergedIntoAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_PrimaryProducerUserId",
                table: "Accounts",
                column: "PrimaryProducerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Status",
                table: "Accounts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Status_Region",
                table: "Accounts",
                columns: new[] { "Status", "Region" });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_TaxId_Active",
                table: "Accounts",
                column: "TaxId",
                unique: true,
                filter: "\"Status\" = 'Active' AND \"TaxId\" IS NOT NULL AND \"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_TerritoryCode",
                table: "Accounts",
                column: "TerritoryCode");

            migrationBuilder.CreateIndex(
                name: "IX_AccountContacts_AccountId_Primary",
                table: "AccountContacts",
                column: "AccountId",
                unique: true,
                filter: "\"IsPrimary\" = true AND \"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AccountRelationshipHistory_AccountId_EffectiveAt",
                table: "AccountRelationshipHistory",
                columns: new[] { "AccountId", "EffectiveAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "UX_CarrierRefs_Name",
                table: "CarrierRefs",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationEvents_AuthoredByUserId",
                table: "CommunicationEvents",
                column: "AuthoredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationEvents_Entity_OccurredAt",
                table: "CommunicationEvents",
                columns: new[] { "PrimaryEntityType", "PrimaryEntityId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationEvents_FollowUpTaskId",
                table: "CommunicationEvents",
                column: "FollowUpTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_IdempotencyRecords_Key_Operation",
                table: "IdempotencyRecords",
                columns: new[] { "IdempotencyKey", "Operation" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PolicyCoverageLines_PolicyId_IsCurrent",
                table: "PolicyCoverageLines",
                columns: new[] { "PolicyId", "IsCurrent" });

            migrationBuilder.CreateIndex(
                name: "IX_PolicyCoverageLines_PolicyVersionId",
                table: "PolicyCoverageLines",
                column: "PolicyVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyEndorsements_PolicyVersionId",
                table: "PolicyEndorsements",
                column: "PolicyVersionId");

            migrationBuilder.CreateIndex(
                name: "UX_PolicyEndorsements_PolicyId_EndorsementNumber",
                table: "PolicyEndorsements",
                columns: new[] { "PolicyId", "EndorsementNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_PolicyVersions_PolicyId_VersionNumber",
                table: "PolicyVersions",
                columns: new[] { "PolicyId", "VersionNumber" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Accounts_MergedIntoAccountId",
                table: "Accounts",
                column: "MergedIntoAccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Brokers_BrokerOfRecordId",
                table: "Accounts",
                column: "BrokerOfRecordId",
                principalTable: "Brokers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_UserProfiles_PrimaryProducerUserId",
                table: "Accounts",
                column: "PrimaryProducerUserId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Policies_CarrierRefs_CarrierId",
                table: "Policies",
                column: "CarrierId",
                principalTable: "CarrierRefs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Policies_Policies_PredecessorPolicyId",
                table: "Policies",
                column: "PredecessorPolicyId",
                principalTable: "Policies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Policies_UserProfiles_ProducerUserId",
                table: "Policies",
                column: "ProducerUserId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Accounts_MergedIntoAccountId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Brokers_BrokerOfRecordId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_UserProfiles_PrimaryProducerUserId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Policies_CarrierRefs_CarrierId",
                table: "Policies");

            migrationBuilder.DropForeignKey(
                name: "FK_Policies_Policies_PredecessorPolicyId",
                table: "Policies");

            migrationBuilder.DropForeignKey(
                name: "FK_Policies_UserProfiles_ProducerUserId",
                table: "Policies");

            migrationBuilder.DropTable(
                name: "AccountContacts");

            migrationBuilder.DropTable(
                name: "AccountRelationshipHistory");

            migrationBuilder.DropTable(
                name: "CarrierRefs");

            migrationBuilder.DropTable(
                name: "CommunicationEvents");

            migrationBuilder.DropTable(
                name: "IdempotencyRecords");

            migrationBuilder.DropTable(
                name: "PolicyCoverageLines");

            migrationBuilder.DropTable(
                name: "PolicyEndorsements");

            migrationBuilder.DropTable(
                name: "PolicyVersions");

            migrationBuilder.DropIndex(
                name: "IX_Policies_CarrierId",
                table: "Policies");

            migrationBuilder.DropIndex(
                name: "IX_Policies_CurrentStatus",
                table: "Policies");

            migrationBuilder.DropIndex(
                name: "IX_Policies_CurrentVersionId",
                table: "Policies");

            migrationBuilder.DropIndex(
                name: "IX_Policies_PredecessorPolicyId",
                table: "Policies");

            migrationBuilder.DropIndex(
                name: "IX_Policies_ProducerUserId",
                table: "Policies");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_BrokerOfRecordId",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_DisplayName_Trgm",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_MergedIntoAccountId",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_PrimaryProducerUserId",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_Status",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_Status_Region",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_TaxId_Active",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_TerritoryCode",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "AccountDisplayNameAtLink",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "AccountStatusAtRead",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "AccountSurvivorId",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "AccountDisplayNameAtLink",
                table: "Renewals");

            migrationBuilder.DropColumn(
                name: "AccountStatusAtRead",
                table: "Renewals");

            migrationBuilder.DropColumn(
                name: "AccountSurvivorId",
                table: "Renewals");

            migrationBuilder.DropColumn(
                name: "AccountDisplayNameAtLink",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "AccountStatusAtRead",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "AccountSurvivorId",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "BoundAt",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "CancellationEffectiveDate",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "CancellationReasonCode",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "CancellationReasonDetail",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "CarrierId",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "CurrentVersionId",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "ExpiredAt",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "ImportSource",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "IssuedAt",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "PredecessorPolicyId",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "PremiumCurrency",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "ProducerUserId",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "ReinstatementDeadline",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "Address1",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Address2",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "BrokerOfRecordId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "DeleteReasonCode",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "DeleteReasonDetail",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "LegalName",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "MergedIntoAccountId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "PrimaryLineOfBusiness",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "PrimaryProducerUserId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "RemovedAt",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "StableDisplayName",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "TaxId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "TerritoryCode",
                table: "Accounts");

            migrationBuilder.RenameColumn(
                name: "ExternalPolicyReference",
                table: "Policies",
                newName: "Carrier");

            migrationBuilder.RenameColumn(
                name: "State",
                table: "Accounts",
                newName: "PrimaryState");

            migrationBuilder.RenameColumn(
                name: "DisplayName",
                table: "Accounts",
                newName: "Name");

            migrationBuilder.AlterColumn<decimal>(
                name: "Premium",
                table: "Policies",
                type: "numeric(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "PolicyNumber",
                table: "Policies",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "LineOfBusiness",
                table: "Policies",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "CurrentStatus",
                table: "Policies",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Active",
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldDefaultValue: "Pending");

            migrationBuilder.AlterColumn<string>(
                name: "Region",
                table: "Accounts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Industry",
                table: "Accounts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PrimaryState",
                table: "Accounts",
                type: "character varying(2)",
                maxLength: 2,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Region",
                table: "Accounts",
                column: "Region");
        }
    }
}

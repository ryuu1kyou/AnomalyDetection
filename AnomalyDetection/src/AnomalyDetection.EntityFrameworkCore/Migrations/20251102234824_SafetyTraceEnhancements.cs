using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnomalyDetection.Migrations
{
    /// <inheritdoc />
    public partial class SafetyTraceEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Action = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    OldValues = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExecutionDuration = table.Column<long>(type: "bigint", nullable: true),
                    Exception = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppKnowledgeArticles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ViewCount = table.Column<int>(type: "int", nullable: false),
                    UsefulCount = table.Column<int>(type: "int", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RelatedAnomalyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DetectionLogicId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CanSignalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AnomalyType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SignalName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Symptom = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Cause = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Countermeasure = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    HasSolution = table.Column<bool>(type: "bit", nullable: false),
                    SolutionSteps = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PreventionMeasures = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AverageRating = table.Column<double>(type: "float(5)", precision: 5, scale: 2, nullable: false),
                    RatingCount = table.Column<int>(type: "int", nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppKnowledgeArticles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppSafetyTraceRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequirementId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SafetyGoalId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AsilLevel = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    BaselineId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    DetectionLogicId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RelatedDocumentsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TraceabilityLinksJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LifecycleEventsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangeRequestsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApprovalStatus = table.Column<int>(type: "int", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovalComments = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VerificationsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValidationsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AuditTrailJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSafetyTraceRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CanSpecImports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileFormat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FileHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ImportedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParsedMessageCount = table.Column<int>(type: "int", nullable: false),
                    ParsedSignalCount = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Manufacturer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CanSpecImports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompatibilityAnalyses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OldSpecId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NewSpecId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnalysisDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AnalyzedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompatibilityLevel = table.Column<int>(type: "int", nullable: false),
                    BreakingChangeCount = table.Column<int>(type: "int", nullable: false),
                    WarningCount = table.Column<int>(type: "int", nullable: false),
                    InfoCount = table.Column<int>(type: "int", nullable: false),
                    CompatibilityScore = table.Column<double>(type: "float", nullable: false),
                    MigrationRisk = table.Column<int>(type: "int", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Recommendations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompatibilityAnalyses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IntegrationEndpoints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    BaseUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EndpointUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApiKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Timeout = table.Column<int>(type: "int", nullable: false),
                    RequireAuthentication = table.Column<bool>(type: "bit", nullable: false),
                    AuthenticationScheme = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Configuration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastSyncDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SuccessCount = table.Column<int>(type: "int", nullable: false),
                    FailureCount = table.Column<int>(type: "int", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationEndpoints", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppKnowledgeArticleComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KnowledgeArticleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AuthorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppKnowledgeArticleComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppKnowledgeArticleComments_AppKnowledgeArticles_KnowledgeArticleId",
                        column: x => x.KnowledgeArticleId,
                        principalTable: "AppKnowledgeArticles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CanSpecDiff",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CanSpecImportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PreviousSpecId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ComparisonDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MessageId = table.Column<long>(type: "bigint", nullable: true),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangeSummary = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CanSpecDiff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CanSpecDiff_CanSpecImports_CanSpecImportId",
                        column: x => x.CanSpecImportId,
                        principalTable: "CanSpecImports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CanSpecMessage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CanSpecImportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Dlc = table.Column<int>(type: "int", nullable: false),
                    Transmitter = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CycleTime = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CanSpecMessage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CanSpecMessage_CanSpecImports_CanSpecImportId",
                        column: x => x.CanSpecImportId,
                        principalTable: "CanSpecImports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompatibilityIssue",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnalysisId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MessageId = table.Column<long>(type: "bigint", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Recommendation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompatibilityAnalysisId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompatibilityIssue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompatibilityIssue_CompatibilityAnalyses_CompatibilityAnalysisId",
                        column: x => x.CompatibilityAnalysisId,
                        principalTable: "CompatibilityAnalyses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ImpactAssessment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnalysisId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AffectedArea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AffectedMessageCount = table.Column<int>(type: "int", nullable: false),
                    AffectedSignalCount = table.Column<int>(type: "int", nullable: false),
                    Risk = table.Column<int>(type: "int", nullable: false),
                    Impact = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MitigationStrategy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstimatedEffortHours = table.Column<int>(type: "int", nullable: false),
                    CompatibilityAnalysisId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImpactAssessment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImpactAssessment_CompatibilityAnalyses_CompatibilityAnalysisId",
                        column: x => x.CompatibilityAnalysisId,
                        principalTable: "CompatibilityAnalyses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DataImportRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EndpointId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Filter = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RecordsImported = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IntegrationEndpointId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataImportRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataImportRequest_IntegrationEndpoints_IntegrationEndpointId",
                        column: x => x.IntegrationEndpointId,
                        principalTable: "IntegrationEndpoints",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IntegrationLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EndpointId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Operation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    RequestData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusCode = table.Column<int>(type: "int", nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false),
                    IntegrationEndpointId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntegrationLog_IntegrationEndpoints_IntegrationEndpointId",
                        column: x => x.IntegrationEndpointId,
                        principalTable: "IntegrationEndpoints",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WebhookSubscription",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EndpointId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WebhookUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Secret = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaxRetries = table.Column<int>(type: "int", nullable: false),
                    TimeoutSeconds = table.Column<int>(type: "int", nullable: false),
                    LastTriggeredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliverySuccessCount = table.Column<int>(type: "int", nullable: false),
                    DeliveryFailureCount = table.Column<int>(type: "int", nullable: false),
                    IntegrationEndpointId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookSubscription", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebhookSubscription_IntegrationEndpoints_IntegrationEndpointId",
                        column: x => x.IntegrationEndpointId,
                        principalTable: "IntegrationEndpoints",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CanSpecSignal",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartBit = table.Column<int>(type: "int", nullable: false),
                    BitLength = table.Column<int>(type: "int", nullable: false),
                    IsBigEndian = table.Column<bool>(type: "bit", nullable: false),
                    IsSigned = table.Column<bool>(type: "bit", nullable: false),
                    Factor = table.Column<double>(type: "float", nullable: false),
                    Offset = table.Column<double>(type: "float", nullable: false),
                    Min = table.Column<double>(type: "float", nullable: false),
                    Max = table.Column<double>(type: "float", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Receiver = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CanSpecMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CanSpecSignal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CanSpecSignal_CanSpecMessage_CanSpecMessageId",
                        column: x => x.CanSpecMessageId,
                        principalTable: "CanSpecMessage",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppOemCustomizations_EntityType_Status",
                table: "AppOemCustomizations",
                columns: new[] { "EntityType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppOemCustomizations_TenantId_EntityType_Status_CreationTime",
                table: "AppOemCustomizations",
                columns: new[] { "TenantId", "EntityType", "Status", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppOemCustomizations_TenantId_Status",
                table: "AppOemCustomizations",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppOemCustomizations_Type_Status_ApprovedAt",
                table: "AppOemCustomizations",
                columns: new[] { "Type", "Status", "ApprovedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AppOemApprovals_RequestedBy_Status_RequestedAt",
                table: "AppOemApprovals",
                columns: new[] { "RequestedBy", "Status", "RequestedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AppOemApprovals_Status_DueDate",
                table: "AppOemApprovals",
                columns: new[] { "Status", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AppOemApprovals_Status_Priority",
                table: "AppOemApprovals",
                columns: new[] { "Status", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_AppOemApprovals_TenantId_Status",
                table: "AppOemApprovals",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppOemApprovals_TenantId_Status_DueDate_Priority",
                table: "AppOemApprovals",
                columns: new[] { "TenantId", "Status", "DueDate", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_AppOemApprovals_Type_Status_RequestedAt",
                table: "AppOemApprovals",
                columns: new[] { "Type", "Status", "RequestedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AppCanSignals_IsStandard_Status",
                table: "AppCanSignals",
                columns: new[] { "IsStandard", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppCanSignals_SystemType_IsStandard_Status",
                table: "AppCanSignals",
                columns: new[] { "SystemType", "IsStandard", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppCanSignals_SystemType_Status",
                table: "AppCanSignals",
                columns: new[] { "SystemType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppCanSignals_TenantId_SystemType_Status",
                table: "AppCanSignals",
                columns: new[] { "TenantId", "SystemType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppCanAnomalyDetectionLogics_ExecutionCount",
                table: "AppCanAnomalyDetectionLogics",
                column: "ExecutionCount");

            migrationBuilder.CreateIndex(
                name: "IX_AppCanAnomalyDetectionLogics_LastExecutedAt",
                table: "AppCanAnomalyDetectionLogics",
                column: "LastExecutedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AppCanAnomalyDetectionLogics_LastExecutedAt_ExecutionCount",
                table: "AppCanAnomalyDetectionLogics",
                columns: new[] { "LastExecutedAt", "ExecutionCount" });

            migrationBuilder.CreateIndex(
                name: "IX_AppCanAnomalyDetectionLogics_Status_SharingLevel",
                table: "AppCanAnomalyDetectionLogics",
                columns: new[] { "Status", "SharingLevel" });

            migrationBuilder.CreateIndex(
                name: "IX_AppCanAnomalyDetectionLogics_TenantId_SharingLevel",
                table: "AppCanAnomalyDetectionLogics",
                columns: new[] { "TenantId", "SharingLevel" });

            migrationBuilder.CreateIndex(
                name: "IX_AppCanAnomalyDetectionLogics_TenantId_Status",
                table: "AppCanAnomalyDetectionLogics",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAnomalyDetectionResults_AnomalyLevel_ResolutionStatus",
                table: "AppAnomalyDetectionResults",
                columns: new[] { "AnomalyLevel", "ResolutionStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAnomalyDetectionResults_AnomalyType_DetectedAt",
                table: "AppAnomalyDetectionResults",
                columns: new[] { "AnomalyType", "DetectedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAnomalyDetectionResults_CanSignalId_DetectedAt",
                table: "AppAnomalyDetectionResults",
                columns: new[] { "CanSignalId", "DetectedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAnomalyDetectionResults_CanSignalId_DetectedAt_AnomalyLevel_ConfidenceScore",
                table: "AppAnomalyDetectionResults",
                columns: new[] { "CanSignalId", "DetectedAt", "AnomalyLevel", "ConfidenceScore" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAnomalyDetectionResults_DetectedAt_AnomalyLevel",
                table: "AppAnomalyDetectionResults",
                columns: new[] { "DetectedAt", "AnomalyLevel" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAnomalyDetectionResults_DetectionLogicId_DetectedAt",
                table: "AppAnomalyDetectionResults",
                columns: new[] { "DetectionLogicId", "DetectedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAnomalyDetectionResults_DetectionLogicId_DetectedAt_AnomalyLevel_ResolutionStatus",
                table: "AppAnomalyDetectionResults",
                columns: new[] { "DetectionLogicId", "DetectedAt", "AnomalyLevel", "ResolutionStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAnomalyDetectionResults_IsValidated_IsFalsePositiveFlag_DetectedAt",
                table: "AppAnomalyDetectionResults",
                columns: new[] { "IsValidated", "IsFalsePositiveFlag", "DetectedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAnomalyDetectionResults_TenantId_DetectedAt_AnomalyLevel",
                table: "AppAnomalyDetectionResults",
                columns: new[] { "TenantId", "DetectedAt", "AnomalyLevel" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAuditLogs_Action",
                table: "AppAuditLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_AppAuditLogs_Action_CreationTime",
                table: "AppAuditLogs",
                columns: new[] { "Action", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAuditLogs_CreationTime",
                table: "AppAuditLogs",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppAuditLogs_CreatorId",
                table: "AppAuditLogs",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAuditLogs_EntityId",
                table: "AppAuditLogs",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAuditLogs_EntityType",
                table: "AppAuditLogs",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_AppAuditLogs_EntityType_EntityId",
                table: "AppAuditLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAuditLogs_Level",
                table: "AppAuditLogs",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_AppAuditLogs_Level_CreationTime",
                table: "AppAuditLogs",
                columns: new[] { "Level", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAuditLogs_TenantId",
                table: "AppAuditLogs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppKnowledgeArticleComments_CreationTime",
                table: "AppKnowledgeArticleComments",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppKnowledgeArticleComments_KnowledgeArticleId",
                table: "AppKnowledgeArticleComments",
                column: "KnowledgeArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_AppKnowledgeArticles_AnomalyType",
                table: "AppKnowledgeArticles",
                column: "AnomalyType");

            migrationBuilder.CreateIndex(
                name: "IX_AppKnowledgeArticles_CanSignalId",
                table: "AppKnowledgeArticles",
                column: "CanSignalId");

            migrationBuilder.CreateIndex(
                name: "IX_AppKnowledgeArticles_Category",
                table: "AppKnowledgeArticles",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_AppKnowledgeArticles_DetectionLogicId",
                table: "AppKnowledgeArticles",
                column: "DetectionLogicId");

            migrationBuilder.CreateIndex(
                name: "IX_AppKnowledgeArticles_IsPublished",
                table: "AppKnowledgeArticles",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_AppKnowledgeArticles_SignalName",
                table: "AppKnowledgeArticles",
                column: "SignalName");

            migrationBuilder.CreateIndex(
                name: "IX_AppSafetyTraceRecords_ApprovalStatus",
                table: "AppSafetyTraceRecords",
                column: "ApprovalStatus");

            migrationBuilder.CreateIndex(
                name: "IX_AppSafetyTraceRecords_AsilLevel",
                table: "AppSafetyTraceRecords",
                column: "AsilLevel");

            migrationBuilder.CreateIndex(
                name: "IX_AppSafetyTraceRecords_ProjectId",
                table: "AppSafetyTraceRecords",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_CanSpecDiff_CanSpecImportId",
                table: "CanSpecDiff",
                column: "CanSpecImportId");

            migrationBuilder.CreateIndex(
                name: "IX_CanSpecMessage_CanSpecImportId",
                table: "CanSpecMessage",
                column: "CanSpecImportId");

            migrationBuilder.CreateIndex(
                name: "IX_CanSpecSignal_CanSpecMessageId",
                table: "CanSpecSignal",
                column: "CanSpecMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_CompatibilityIssue_CompatibilityAnalysisId",
                table: "CompatibilityIssue",
                column: "CompatibilityAnalysisId");

            migrationBuilder.CreateIndex(
                name: "IX_DataImportRequest_IntegrationEndpointId",
                table: "DataImportRequest",
                column: "IntegrationEndpointId");

            migrationBuilder.CreateIndex(
                name: "IX_ImpactAssessment_CompatibilityAnalysisId",
                table: "ImpactAssessment",
                column: "CompatibilityAnalysisId");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationLog_IntegrationEndpointId",
                table: "IntegrationLog",
                column: "IntegrationEndpointId");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookSubscription_IntegrationEndpointId",
                table: "WebhookSubscription",
                column: "IntegrationEndpointId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppAuditLogs");

            migrationBuilder.DropTable(
                name: "AppKnowledgeArticleComments");

            migrationBuilder.DropTable(
                name: "AppSafetyTraceRecords");

            migrationBuilder.DropTable(
                name: "CanSpecDiff");

            migrationBuilder.DropTable(
                name: "CanSpecSignal");

            migrationBuilder.DropTable(
                name: "CompatibilityIssue");

            migrationBuilder.DropTable(
                name: "DataImportRequest");

            migrationBuilder.DropTable(
                name: "ImpactAssessment");

            migrationBuilder.DropTable(
                name: "IntegrationLog");

            migrationBuilder.DropTable(
                name: "WebhookSubscription");

            migrationBuilder.DropTable(
                name: "AppKnowledgeArticles");

            migrationBuilder.DropTable(
                name: "CanSpecMessage");

            migrationBuilder.DropTable(
                name: "CompatibilityAnalyses");

            migrationBuilder.DropTable(
                name: "IntegrationEndpoints");

            migrationBuilder.DropTable(
                name: "CanSpecImports");

            migrationBuilder.DropIndex(
                name: "IX_AppOemCustomizations_EntityType_Status",
                table: "AppOemCustomizations");

            migrationBuilder.DropIndex(
                name: "IX_AppOemCustomizations_TenantId_EntityType_Status_CreationTime",
                table: "AppOemCustomizations");

            migrationBuilder.DropIndex(
                name: "IX_AppOemCustomizations_TenantId_Status",
                table: "AppOemCustomizations");

            migrationBuilder.DropIndex(
                name: "IX_AppOemCustomizations_Type_Status_ApprovedAt",
                table: "AppOemCustomizations");

            migrationBuilder.DropIndex(
                name: "IX_AppOemApprovals_RequestedBy_Status_RequestedAt",
                table: "AppOemApprovals");

            migrationBuilder.DropIndex(
                name: "IX_AppOemApprovals_Status_DueDate",
                table: "AppOemApprovals");

            migrationBuilder.DropIndex(
                name: "IX_AppOemApprovals_Status_Priority",
                table: "AppOemApprovals");

            migrationBuilder.DropIndex(
                name: "IX_AppOemApprovals_TenantId_Status",
                table: "AppOemApprovals");

            migrationBuilder.DropIndex(
                name: "IX_AppOemApprovals_TenantId_Status_DueDate_Priority",
                table: "AppOemApprovals");

            migrationBuilder.DropIndex(
                name: "IX_AppOemApprovals_Type_Status_RequestedAt",
                table: "AppOemApprovals");

            migrationBuilder.DropIndex(
                name: "IX_AppCanSignals_IsStandard_Status",
                table: "AppCanSignals");

            migrationBuilder.DropIndex(
                name: "IX_AppCanSignals_SystemType_IsStandard_Status",
                table: "AppCanSignals");

            migrationBuilder.DropIndex(
                name: "IX_AppCanSignals_SystemType_Status",
                table: "AppCanSignals");

            migrationBuilder.DropIndex(
                name: "IX_AppCanSignals_TenantId_SystemType_Status",
                table: "AppCanSignals");

            migrationBuilder.DropIndex(
                name: "IX_AppCanAnomalyDetectionLogics_ExecutionCount",
                table: "AppCanAnomalyDetectionLogics");

            migrationBuilder.DropIndex(
                name: "IX_AppCanAnomalyDetectionLogics_LastExecutedAt",
                table: "AppCanAnomalyDetectionLogics");

            migrationBuilder.DropIndex(
                name: "IX_AppCanAnomalyDetectionLogics_LastExecutedAt_ExecutionCount",
                table: "AppCanAnomalyDetectionLogics");

            migrationBuilder.DropIndex(
                name: "IX_AppCanAnomalyDetectionLogics_Status_SharingLevel",
                table: "AppCanAnomalyDetectionLogics");

            migrationBuilder.DropIndex(
                name: "IX_AppCanAnomalyDetectionLogics_TenantId_SharingLevel",
                table: "AppCanAnomalyDetectionLogics");

            migrationBuilder.DropIndex(
                name: "IX_AppCanAnomalyDetectionLogics_TenantId_Status",
                table: "AppCanAnomalyDetectionLogics");

            migrationBuilder.DropIndex(
                name: "IX_AppAnomalyDetectionResults_AnomalyLevel_ResolutionStatus",
                table: "AppAnomalyDetectionResults");

            migrationBuilder.DropIndex(
                name: "IX_AppAnomalyDetectionResults_AnomalyType_DetectedAt",
                table: "AppAnomalyDetectionResults");

            migrationBuilder.DropIndex(
                name: "IX_AppAnomalyDetectionResults_CanSignalId_DetectedAt",
                table: "AppAnomalyDetectionResults");

            migrationBuilder.DropIndex(
                name: "IX_AppAnomalyDetectionResults_CanSignalId_DetectedAt_AnomalyLevel_ConfidenceScore",
                table: "AppAnomalyDetectionResults");

            migrationBuilder.DropIndex(
                name: "IX_AppAnomalyDetectionResults_DetectedAt_AnomalyLevel",
                table: "AppAnomalyDetectionResults");

            migrationBuilder.DropIndex(
                name: "IX_AppAnomalyDetectionResults_DetectionLogicId_DetectedAt",
                table: "AppAnomalyDetectionResults");

            migrationBuilder.DropIndex(
                name: "IX_AppAnomalyDetectionResults_DetectionLogicId_DetectedAt_AnomalyLevel_ResolutionStatus",
                table: "AppAnomalyDetectionResults");

            migrationBuilder.DropIndex(
                name: "IX_AppAnomalyDetectionResults_IsValidated_IsFalsePositiveFlag_DetectedAt",
                table: "AppAnomalyDetectionResults");

            migrationBuilder.DropIndex(
                name: "IX_AppAnomalyDetectionResults_TenantId_DetectedAt_AnomalyLevel",
                table: "AppAnomalyDetectionResults");
        }
    }
}

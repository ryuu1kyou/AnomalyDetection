using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AnomalyDetection.CanSignals;
using AnomalyDetection.CanSignals.Dtos;
using AnomalyDetection.AnomalyDetection;
using AnomalyDetection.AnomalyDetection.Dtos;
using AnomalyDetection.AnomalyDetection.Services;
using AnomalyDetection.Projects;
using AnomalyDetection.Projects.Dtos;
using AnomalyDetection.OemTraceability;
using AnomalyDetection.OemTraceability.Models;
using AnomalyDetection.Application.Contracts.OemTraceability.Dtos;
using AnomalyDetection.AuditLogging;
using AnomalyDetection.Application.Contracts.AuditLogging;
using AnomalyDetection.KnowledgeBase;
using AnomalyDetection.Safety;
using AnomalyDetection.CanSpecification;
using AnomalyDetection.Integration;

namespace AnomalyDetection;

public class AnomalyDetectionApplicationAutoMapperProfile : Profile
{
    public AnomalyDetectionApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */

        CreateCanSignalMappings();
        CreateDetectionLogicMappings();
        CreateDetectionResultMappings();
        CreateProjectMappings();
        CreateAnomalyAnalysisMappings();
        CreateOemTraceabilityMappings();
        CreateAuditLogMappings();
        CreateKnowledgeBaseMappings();
        CreateSafetyTraceMappings();
        CreateCanSpecificationMappings();
        CreateIntegrationMappings();
    }

    private void CreateCanSignalMappings()
    {
        CreateMap<CanSignal, CanSignalDto>()
            .ForMember(dest => dest.SignalName, opt => opt.MapFrom(src => src.Identifier.SignalName))
            .ForMember(dest => dest.CanId, opt => opt.MapFrom(src => src.Identifier.CanId))
            .ForMember(dest => dest.StartBit, opt => opt.MapFrom(src => src.Specification.StartBit))
            .ForMember(dest => dest.Length, opt => opt.MapFrom(src => src.Specification.Length))
            .ForMember(dest => dest.DataType, opt => opt.MapFrom(src => src.Specification.DataType))
            .ForMember(dest => dest.MinValue, opt => opt.MapFrom(src => src.Specification.ValueRange.MinValue))
            .ForMember(dest => dest.MaxValue, opt => opt.MapFrom(src => src.Specification.ValueRange.MaxValue))
            .ForMember(dest => dest.ByteOrder, opt => opt.MapFrom(src => src.Specification.ByteOrder))
            .ForMember(dest => dest.Factor, opt => opt.MapFrom(src => src.Conversion.Factor))
            .ForMember(dest => dest.Offset, opt => opt.MapFrom(src => src.Conversion.Offset))
            .ForMember(dest => dest.Unit, opt => opt.MapFrom(src => src.Conversion.Unit))
            .ForMember(dest => dest.CycleTime, opt => opt.MapFrom(src => src.Timing.CycleTimeMs))
            .ForMember(dest => dest.TimeoutTime, opt => opt.MapFrom(src => src.Timing.TimeoutMs))
            .ForMember(dest => dest.Version, opt => opt.MapFrom(src => src.Version.ToString()));

        CreateMap<CreateCanSignalDto, CanSignal>()
            .ConvertUsing((src, dest, context) =>
            {
                var identifier = new SignalIdentifier(src.SignalName, src.CanId);
                var valueRange = new SignalValueRange(src.MinValue, src.MaxValue);
                var specification = new SignalSpecification(src.StartBit, src.Length, src.DataType, valueRange, src.ByteOrder);

                var canSignal = new CanSignal(
                    Guid.NewGuid(),
                    context.Items.ContainsKey("TenantId") ? (Guid?)context.Items["TenantId"] : null,
                    identifier,
                    specification,
                    src.SystemType,
                    src.OemCode,
                    src.Description);

                var conversion = new PhysicalValueConversion(src.Factor, src.Offset, src.Unit);
                var timing = new SignalTiming(src.CycleTime, src.TimeoutTime, SignalSendType.Cyclic);

                canSignal.UpdateConversion(conversion);
                canSignal.UpdateTiming(timing);

                if (src.IsStandard)
                    canSignal.SetAsStandard();

                if (!string.IsNullOrEmpty(src.SourceDocument))
                    canSignal.SetSourceDocument(src.SourceDocument);

                if (!string.IsNullOrEmpty(src.Notes))
                    canSignal.AddNote(src.Notes);

                return canSignal;
            });
    }

    private void CreateDetectionLogicMappings()
    {
        CreateMap<CanAnomalyDetectionLogic, CanAnomalyDetectionLogicDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Identity.Name))
            .ForMember(dest => dest.Version, opt => opt.MapFrom(src => src.Identity.Version.ToString()))
            .ForMember(dest => dest.OemCode, opt => opt.MapFrom(src => src.Identity.OemCode))
            .ForMember(dest => dest.DetectionType, opt => opt.MapFrom(src => (DetectionType)(int)src.Specification.DetectionType))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Specification.Description))
            .ForMember(dest => dest.Purpose, opt => opt.MapFrom(src => src.Specification.TargetSystemType.ToString()))
            .ForMember(dest => dest.LogicContent, opt => opt.MapFrom(src => src.Implementation != null ? src.Implementation.Content : string.Empty))
            .ForMember(dest => dest.Algorithm, opt => opt.MapFrom(src => src.Implementation != null ? src.Implementation.Language : string.Empty))
            .ForMember(dest => dest.IsExecutable, opt => opt.MapFrom(src => src.Implementation != null && src.Implementation.IsExecutable()))
            .ForMember(dest => dest.AsilLevel, opt => opt.MapFrom(src => src.Safety.AsilLevel))
            .ForMember(dest => dest.SafetyRequirementId, opt => opt.MapFrom(src => src.Safety.SafetyRequirementId ?? string.Empty))
            .ForMember(dest => dest.SafetyGoalId, opt => opt.MapFrom(src => src.Safety.SafetyGoalId ?? string.Empty));

        CreateMap<DetectionParameter, DetectionParameterDto>()
            .ForMember(dest => dest.MinValue, opt => opt.MapFrom(src => src.Constraints != null ? src.Constraints.MinValue : null))
            .ForMember(dest => dest.MaxValue, opt => opt.MapFrom(src => src.Constraints != null ? src.Constraints.MaxValue : null))
            .ForMember(dest => dest.MinLength, opt => opt.MapFrom(src => src.Constraints != null ? src.Constraints.MinLength : null))
            .ForMember(dest => dest.MaxLength, opt => opt.MapFrom(src => src.Constraints != null ? src.Constraints.MaxLength : null))
            .ForMember(dest => dest.Pattern, opt => opt.MapFrom(src => src.Constraints != null ? src.Constraints.Pattern : string.Empty))
            .ForMember(dest => dest.AllowedValues, opt => opt.MapFrom(src => src.Constraints != null && src.Constraints.AllowedValues.Any()
                ? string.Join(",", src.Constraints.AllowedValues) : string.Empty));

        CreateMap<CanSignalMapping, CanSignalMappingDto>()
            .ForMember(dest => dest.ScalingFactor, opt => opt.MapFrom(src => src.Configuration.ScalingFactor))
            .ForMember(dest => dest.Offset, opt => opt.MapFrom(src => src.Configuration.Offset))
            .ForMember(dest => dest.FilterExpression, opt => opt.MapFrom(src => src.Configuration.FilterExpression))
            .ForMember(dest => dest.CustomProperties, opt => opt.MapFrom(src => src.Configuration.CustomProperties));

        CreateMap<CreateDetectionLogicDto, CanAnomalyDetectionLogic>()
            .ConvertUsing((src, dest, context) =>
            {
                var identity = new DetectionLogicIdentity(src.Name, LogicVersion.Initial(), src.OemCode);
                // Map DetectionType to AnomalyType
                var anomalyType = (AnomalyType)(int)src.DetectionType;
                var specification = new DetectionLogicSpecification(
                    anomalyType,
                    src.Description,
                    CanSystemType.Gateway, // Default to Gateway, adjust as needed
                    LogicComplexity.Simple);
                var safety = new SafetyClassification(src.AsilLevel, src.SafetyRequirementId, src.SafetyGoalId);

                var logic = new CanAnomalyDetectionLogic(
                    Guid.NewGuid(),
                    context.TryGetItems(out var items) && items.ContainsKey("TenantId") ? (Guid?)items["TenantId"] : null,
                    identity,
                    specification,
                    safety);

                if (!string.IsNullOrEmpty(src.LogicContent))
                {
                    var implementation = new LogicImplementation(
                        ImplementationType.Script,
                        src.LogicContent,
                        !string.IsNullOrEmpty(src.Algorithm) ? src.Algorithm : "Default");
                    logic.UpdateImplementation(implementation);
                }

                logic.UpdateSharingLevel(src.SharingLevel);

                return logic;
            });

        CreateMap<CreateDetectionParameterDto, DetectionParameter>()
            .ConvertUsing((src, dest, context) =>
            {
                var constraints = new ParameterConstraints(
                    src.MinValue,
                    src.MaxValue,
                    src.MinLength,
                    src.MaxLength,
                    src.Pattern,
                    !string.IsNullOrEmpty(src.AllowedValues)
                        ? src.AllowedValues.Split(',').ToList()
                        : new List<string>());

                return new DetectionParameter(
                    src.Name,
                    src.DataType,
                    src.DefaultValue,
                    constraints,
                    src.Description,
                    src.IsRequired,
                    src.Unit);
            });

        CreateMap<CreateCanSignalMappingDto, CanSignalMapping>()
            .ConvertUsing((src, dest, context) =>
            {
                var configuration = new SignalMappingConfiguration(
                    src.ScalingFactor,
                    src.Offset,
                    src.FilterExpression,
                    src.CustomProperties);

                return new CanSignalMapping(
                    src.CanSignalId,
                    src.SignalRole,
                    src.IsRequired,
                    src.Description,
                    configuration);
            });
    }

    private void CreateDetectionResultMappings()
    {
        CreateMap<AnomalyDetectionResult, AnomalyDetectionResultDto>()
            .ForMember(dest => dest.SignalValue, opt => opt.MapFrom(src => src.InputData.SignalValue))
            .ForMember(dest => dest.InputTimestamp, opt => opt.MapFrom(src => src.InputData.Timestamp))
            .ForMember(dest => dest.AdditionalInputData, opt => opt.MapFrom(src => src.InputData.AdditionalData))
            .ForMember(dest => dest.DetectionType, opt => opt.MapFrom(src => src.Details.DetectionType))
            .ForMember(dest => dest.TriggerCondition, opt => opt.MapFrom(src => src.Details.TriggerCondition))
            .ForMember(dest => dest.DetectionParameters, opt => opt.MapFrom(src => src.Details.Parameters))
            .ForMember(dest => dest.ExecutionTimeMs, opt => opt.MapFrom(src => src.Details.ExecutionTimeMs));

        CreateMap<CreateDetectionResultDto, AnomalyDetectionResult>()
            .ConvertUsing((src, dest, context) =>
            {
                var inputData = new DetectionInputData(src.SignalValue, src.InputTimestamp, src.AdditionalInputData);
                var details = new DetectionDetails(src.DetectionType, src.TriggerCondition, src.DetectionParameters, src.ExecutionTimeMs);

                var result = new AnomalyDetectionResult(
                    Guid.NewGuid(),
                    context.Items.ContainsKey("TenantId") ? (Guid?)context.Items["TenantId"] : null,
                    src.DetectionLogicId,
                    src.CanSignalId,
                    src.AnomalyLevel,
                    src.ConfidenceScore,
                    src.Description,
                    inputData,
                    details);

                if (src.SharingLevel != SharingLevel.Private)
                {
                    var currentUserId = context.Items.ContainsKey("CurrentUserId") ? (Guid)context.Items["CurrentUserId"] : Guid.Empty;
                    result.ShareResult(src.SharingLevel, currentUserId);
                }

                return result;
            });
    }

    private void CreateProjectMappings()
    {
        CreateMap<AnomalyDetectionProject, AnomalyDetectionProjectDto>()
            .ForMember(dest => dest.AutoProgressTracking, opt => opt.MapFrom(src => src.Configuration.Priority == ProjectPriority.High || src.Configuration.Priority == ProjectPriority.Critical))
            .ForMember(dest => dest.RequireApprovalForChanges, opt => opt.MapFrom(src => src.Configuration.IsConfidential))
            .ForMember(dest => dest.CustomSettings, opt => opt.MapFrom(src => src.Configuration.CustomSettings))
            .ForMember(dest => dest.ConfigurationNotes, opt => opt.MapFrom(src => string.Join("; ", src.Configuration.Notes)))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive()))
            .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(src => src.IsCompleted()))
            .ForMember(dest => dest.IsOverdue, opt => opt.MapFrom(src => src.IsOverdue()))
            .ForMember(dest => dest.RemainingTime, opt => opt.MapFrom(src => src.GetRemainingTime()))
            .ForMember(dest => dest.ProjectDuration, opt => opt.MapFrom(src => src.GetProjectDuration()))
            .ForMember(dest => dest.OverdueMilestonesCount, opt => opt.MapFrom(src => src.GetOverdueMilestones().Count))
            .ForMember(dest => dest.ActiveMembersCount, opt => opt.MapFrom(src => src.GetActiveMembers().Count));

        CreateMap<ProjectMilestone, ProjectMilestoneDto>()
            .ForMember(dest => dest.IsCritical, opt => opt.MapFrom(src => src.Configuration.IsCritical))
            .ForMember(dest => dest.RequiresApproval, opt => opt.MapFrom(src => src.Configuration.RequiresApproval))
            .ForMember(dest => dest.Dependencies, opt => opt.MapFrom(src => src.Configuration.Dependencies))
            .ForMember(dest => dest.CustomProperties, opt => opt.MapFrom(src => src.Configuration.CustomProperties))
            .ForMember(dest => dest.IsOverdue, opt => opt.MapFrom(src => src.IsOverdue()))
            .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(src => src.IsCompleted()))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive()))
            .ForMember(dest => dest.TimeToDeadline, opt => opt.MapFrom(src => src.GetTimeToDeadline()))
            .ForMember(dest => dest.CompletionTime, opt => opt.MapFrom(src => src.GetCompletionTime()));

        CreateMap<ProjectMember, ProjectMemberDto>()
            .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.Configuration.Permissions))
            .ForMember(dest => dest.Settings, opt => opt.MapFrom(src => src.Configuration.Settings))
            .ForMember(dest => dest.CanReceiveNotifications, opt => opt.MapFrom(src => src.Configuration.CanReceiveNotifications))
            .ForMember(dest => dest.CanAccessReports, opt => opt.MapFrom(src => src.Configuration.CanAccessReports))
            .ForMember(dest => dest.MembershipDuration, opt => opt.MapFrom(src => src.GetMembershipDuration()))
            .ForMember(dest => dest.IsManager, opt => opt.MapFrom(src => src.IsManager()))
            .ForMember(dest => dest.IsLeader, opt => opt.MapFrom(src => src.IsLeader()))
            .ForMember(dest => dest.CanManageProject, opt => opt.MapFrom(src => src.CanManageProject()))
            .ForMember(dest => dest.CanEditDetectionLogics, opt => opt.MapFrom(src => src.CanEditDetectionLogics()));

        CreateMap<CreateProjectDto, AnomalyDetectionProject>()
            .ConvertUsing((src, dest, context) =>
            {
                var project = new AnomalyDetectionProject(
                    Guid.NewGuid(),
                    context.Items.ContainsKey("TenantId") ? (Guid?)context.Items["TenantId"] : null,
                    src.ProjectCode,
                    src.Name,
                    src.VehicleModel,
                    src.ModelYear,
                    src.PrimarySystem,
                    src.OemCode,
                    src.ProjectManagerId,
                    src.StartDate,
                    src.EndDate,
                    src.Description);

                var priority = src.AutoProgressTracking ? ProjectPriority.High : ProjectPriority.Normal;
                var tags = new List<string>();
                var configuration = new ProjectConfiguration(
                    priority,
                    src.RequireApprovalForChanges,
                    tags,
                    src.CustomSettings);

                if (!string.IsNullOrEmpty(src.ConfigurationNotes))
                {
                    configuration.AddNote(src.ConfigurationNotes);
                }

                project.UpdateConfiguration(configuration);

                return project;
            });

        CreateMap<CreateProjectMilestoneDto, ProjectMilestone>()
            .ConvertUsing((src, dest, context) =>
            {
                var configuration = new MilestoneConfiguration(
                    src.IsCritical,
                    src.RequiresApproval,
                    src.Dependencies,
                    src.CustomProperties);

                return new ProjectMilestone(
                    src.Name,
                    src.DueDate,
                    src.Description,
                    src.DisplayOrder,
                    configuration);
            });

        CreateMap<CreateProjectMemberDto, ProjectMember>()
            .ConvertUsing((src, dest, context) =>
            {
                var configuration = new MemberConfiguration(
                    src.Permissions,
                    src.Settings,
                    src.CanReceiveNotifications,
                    src.CanAccessReports);

                return new ProjectMember(
                    src.UserId,
                    src.Role,
                    src.Notes,
                    configuration);
            });
    }

    private void CreateAnomalyAnalysisMappings()
    {
        // AnomalyPatternAnalysisResult mappings
        CreateMap<AnomalyPatternAnalysisResult, AnomalyPatternAnalysisDto>();
        CreateMap<AnomalyFrequencyPattern, AnomalyFrequencyPatternDto>();
        CreateMap<AnomalyCorrelation, AnomalyCorrelationDto>();

        // ThresholdRecommendationResult mappings
        CreateMap<ThresholdRecommendationResult, ThresholdRecommendationResultDto>();
        CreateMap<ThresholdRecommendation, ThresholdRecommendationDto>()
            .ForMember(dest => dest.CurrentValue, opt => opt.MapFrom(src => src.CurrentValue.ToString()))
            .ForMember(dest => dest.RecommendedValue, opt => opt.MapFrom(src => src.RecommendedValue.ToString()));
        CreateMap<OptimizationMetrics, OptimizationMetricsDto>();

        // DetectionAccuracyMetrics mappings
        CreateMap<DetectionAccuracyMetrics, DetectionAccuracyMetricsDto>();
        CreateMap<AccuracyByAnomalyType, AccuracyByAnomalyTypeDto>();
        CreateMap<AccuracyByTimeRange, AccuracyByTimeRangeDto>();
    }

    private void CreateOemTraceabilityMappings()
    {
        // OEM Traceability Result mappings
        CreateMap<OemTraceabilityResult, OemTraceabilityDto>();
        CreateMap<OemUsageInfo, OemUsageInfoDto>();
        CreateMap<OemCustomizationSummary, OemCustomizationSummaryDto>();
        CreateMap<OemApprovalSummary, OemApprovalSummaryDto>();
        CreateMap<CrossOemDifferencesAnalysis, CrossOemDifferencesAnalysisDto>();
        CreateMap<OemParameterDifference, OemParameterDifferenceDto>();
        CreateMap<UsagePatternDifference, UsagePatternDifferenceDto>();

        // OEM Customization mappings
        CreateMap<OemCustomization, OemCustomizationDto>()
            .ForMember(dest => dest.OemCode, opt => opt.MapFrom(src => src.OemCode.Code));

        // OEM Approval mappings
        CreateMap<OemApproval, OemApprovalDto>()
            .ForMember(dest => dest.OemCode, opt => opt.MapFrom(src => src.OemCode.Code));
    }

    private void CreateAuditLogMappings()
    {
        CreateMap<AnomalyDetectionAuditLog, AuditLogDto>()
            .ForMember(dest => dest.CreatorName, opt => opt.Ignore()); // Will be populated by the service if needed
    }

    private void CreateKnowledgeBaseMappings()
    {
        CreateMap<KnowledgeArticle, KnowledgeArticleDto>();
        CreateMap<CreateKnowledgeArticleDto, KnowledgeArticle>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ViewCount, opt => opt.Ignore())
            .ForMember(dest => dest.UsefulCount, opt => opt.Ignore())
            .ForMember(dest => dest.IsPublished, opt => opt.Ignore())
            .ForMember(dest => dest.PublishedAt, opt => opt.Ignore());
        CreateMap<KnowledgeArticleComment, KnowledgeArticleCommentDto>();
        CreateMap<KnowledgeArticleRecommendationResult, KnowledgeArticleSummaryDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Article.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Article.Title))
            .ForMember(dest => dest.Summary, opt => opt.MapFrom(src => src.Article.Summary))
            .ForMember(dest => dest.UsefulCount, opt => opt.MapFrom(src => src.Article.UsefulCount))
            .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => src.Article.AverageRating))
            .ForMember(dest => dest.RelevanceScore, opt => opt.MapFrom(src => Math.Round(src.Score, 2)));
    }

    private void CreateSafetyTraceMappings()
    {
        CreateMap<SafetyTraceRecord, SafetyTraceRecordDto>()
            .ForMember(dest => dest.AsilLevel, opt => opt.MapFrom(src => (int)src.AsilLevel))
            .ForMember(dest => dest.ApprovalStatus, opt => opt.MapFrom(src => (int)src.ApprovalStatus));
        CreateMap<VerificationRecord, VerificationRecordDto>();
        CreateMap<ValidationRecord, ValidationRecordDto>();
        CreateMap<AuditEntry, AuditEntryDto>();
        CreateMap<LifecycleEvent, LifecycleEventDto>()
            .ForMember(dest => dest.Stage, opt => opt.MapFrom(src => (int)src.Stage));
        CreateMap<ChangeRequestRecord, ChangeRequestRecordDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status));
        CreateMap<TraceabilityLinkRecord, TraceabilityLinkRecordDto>()
            .ForMember(dest => dest.SourceType, opt => opt.MapFrom(src => (int)src.SourceType))
            .ForMember(dest => dest.TargetType, opt => opt.MapFrom(src => (int)src.TargetType));
        CreateMap<SafetyTraceAuditSnapshot, SafetyTraceAuditSnapshotDto>()
            .ForMember(dest => dest.AsilLevel, opt => opt.MapFrom(src => (int)src.AsilLevel))
            .ForMember(dest => dest.ApprovalStatus, opt => opt.MapFrom(src => (int)src.ApprovalStatus));
        CreateMap<ChangeImpactSummary, ChangeImpactSummaryDto>();
    }

    private void CreateCanSpecificationMappings()
    {
        CreateMap<CanSpecImport, CanSpecImportDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status));

        CreateMap<CanSpecMessage, CanSpecMessageDto>();
        CreateMap<CanSpecSignal, CanSpecSignalDto>();
        CreateMap<CanSpecDiff, CanSpecDiffDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (int)src.Type))
            .ForMember(dest => dest.Severity, opt => opt.MapFrom(src => (int)src.Severity));

        CreateMap<CanSpecDiffSummary, CanSpecDiffSummaryDto>();

        // Compatibility Analysis
        CreateMap<CompatibilityAnalysis, CompatibilityAnalysisDto>()
            .ForMember(dest => dest.CompatibilityLevel, opt => opt.MapFrom(src => (int)src.CompatibilityLevel))
            .ForMember(dest => dest.MigrationRisk, opt => opt.MapFrom(src => (int)src.MigrationRisk));

        CreateMap<CompatibilityIssue, CompatibilityIssueDto>()
            .ForMember(dest => dest.Severity, opt => opt.MapFrom(src => (int)src.Severity))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => (int)src.Category));

        CreateMap<ImpactAssessment, ImpactAssessmentDto>()
            .ForMember(dest => dest.Risk, opt => opt.MapFrom(src => (int)src.Risk));
    }

    private void CreateIntegrationMappings()
    {
        // Integration Endpoints
        CreateMap<IntegrationEndpoint, IntegrationEndpointDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (int)src.Type));

        CreateMap<WebhookSubscription, WebhookSubscriptionDto>();
        CreateMap<IntegrationLog, IntegrationLogDto>();
        CreateMap<DataImportRequest, DataImportRequestDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status));
    }
}


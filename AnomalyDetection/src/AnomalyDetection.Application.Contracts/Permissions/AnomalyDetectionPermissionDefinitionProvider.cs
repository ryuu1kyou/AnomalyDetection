using AnomalyDetection.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace AnomalyDetection.Permissions;

public class AnomalyDetectionPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var anomalyDetectionGroup = context.AddGroup(
            AnomalyDetectionPermissions.GroupName,
            L("Permission:AnomalyDetection"));

        DefineCanSignalPermissions(anomalyDetectionGroup);
        DefineDetectionLogicPermissions(anomalyDetectionGroup);
        DefineDetectionResultPermissions(anomalyDetectionGroup);
        DefineProjectPermissions(anomalyDetectionGroup);
        DefineStatisticsPermissions(anomalyDetectionGroup);
        DefineTenantManagementPermissions(anomalyDetectionGroup);
        DefineOemTraceabilityPermissions(anomalyDetectionGroup);
        DefineAnalysisPermissions(anomalyDetectionGroup);
        DefineAuditLogPermissions(anomalyDetectionGroup);
        DefineAdministrationPermissions(anomalyDetectionGroup);
    }

    private static void DefineCanSignalPermissions(PermissionGroupDefinition group)
    {
        var canSignalsPermission = group.AddPermission(
            AnomalyDetectionPermissions.CanSignals.Default,
            L("Permission:CanSignals"));

        canSignalsPermission.AddChild(
            AnomalyDetectionPermissions.CanSignals.Create,
            L("Permission:CanSignals.Create"));

        canSignalsPermission.AddChild(
            AnomalyDetectionPermissions.CanSignals.Edit,
            L("Permission:CanSignals.Edit"));

        canSignalsPermission.AddChild(
            AnomalyDetectionPermissions.CanSignals.Delete,
            L("Permission:CanSignals.Delete"));

        canSignalsPermission.AddChild(
            AnomalyDetectionPermissions.CanSignals.ManageStandard,
            L("Permission:CanSignals.ManageStandard"));

        canSignalsPermission.AddChild(
            AnomalyDetectionPermissions.CanSignals.Import,
            L("Permission:CanSignals.Import"));

        canSignalsPermission.AddChild(
            AnomalyDetectionPermissions.CanSignals.Export,
            L("Permission:CanSignals.Export"));
    }

    private static void DefineDetectionLogicPermissions(PermissionGroupDefinition group)
    {
        var detectionLogicsPermission = group.AddPermission(
            AnomalyDetectionPermissions.DetectionLogics.Default,
            L("Permission:DetectionLogics"));

        detectionLogicsPermission.AddChild(
            AnomalyDetectionPermissions.DetectionLogics.Create,
            L("Permission:DetectionLogics.Create"));

        detectionLogicsPermission.AddChild(
            AnomalyDetectionPermissions.DetectionLogics.Edit,
            L("Permission:DetectionLogics.Edit"));

        detectionLogicsPermission.AddChild(
            AnomalyDetectionPermissions.DetectionLogics.Delete,
            L("Permission:DetectionLogics.Delete"));

        detectionLogicsPermission.AddChild(
            AnomalyDetectionPermissions.DetectionLogics.Execute,
            L("Permission:DetectionLogics.Execute"));

        detectionLogicsPermission.AddChild(
            AnomalyDetectionPermissions.DetectionLogics.Approve,
            L("Permission:DetectionLogics.Approve"));

        detectionLogicsPermission.AddChild(
            AnomalyDetectionPermissions.DetectionLogics.ManageSharing,
            L("Permission:DetectionLogics.ManageSharing"));

        detectionLogicsPermission.AddChild(
            AnomalyDetectionPermissions.DetectionLogics.ViewSafetyInfo,
            L("Permission:DetectionLogics.ViewSafetyInfo"));

        detectionLogicsPermission.AddChild(
            AnomalyDetectionPermissions.DetectionLogics.ManageTemplates,
            L("Permission:DetectionLogics.ManageTemplates"));
    }

    private static void DefineDetectionResultPermissions(PermissionGroupDefinition group)
    {
        var detectionResultsPermission = group.AddPermission(
            AnomalyDetectionPermissions.DetectionResults.Default,
            L("Permission:DetectionResults"));

        detectionResultsPermission.AddChild(
            AnomalyDetectionPermissions.DetectionResults.View,
            L("Permission:DetectionResults.View"));

        detectionResultsPermission.AddChild(
            AnomalyDetectionPermissions.DetectionResults.Create,
            L("Permission:DetectionResults.Create"));

        detectionResultsPermission.AddChild(
            AnomalyDetectionPermissions.DetectionResults.Edit,
            L("Permission:DetectionResults.Edit"));

        detectionResultsPermission.AddChild(
            AnomalyDetectionPermissions.DetectionResults.Delete,
            L("Permission:DetectionResults.Delete"));

        detectionResultsPermission.AddChild(
            AnomalyDetectionPermissions.DetectionResults.Resolve,
            L("Permission:DetectionResults.Resolve"));

        detectionResultsPermission.AddChild(
            AnomalyDetectionPermissions.DetectionResults.Share,
            L("Permission:DetectionResults.Share"));

        detectionResultsPermission.AddChild(
            AnomalyDetectionPermissions.DetectionResults.ViewShared,
            L("Permission:DetectionResults.ViewShared"));

        detectionResultsPermission.AddChild(
            AnomalyDetectionPermissions.DetectionResults.BulkOperations,
            L("Permission:DetectionResults.BulkOperations"));
    }

    private static void DefineProjectPermissions(PermissionGroupDefinition group)
    {
        var projectsPermission = group.AddPermission(
            AnomalyDetectionPermissions.Projects.Default,
            L("Permission:Projects"));

        projectsPermission.AddChild(
            AnomalyDetectionPermissions.Projects.Create,
            L("Permission:Projects.Create"));

        projectsPermission.AddChild(
            AnomalyDetectionPermissions.Projects.Edit,
            L("Permission:Projects.Edit"));

        projectsPermission.AddChild(
            AnomalyDetectionPermissions.Projects.Delete,
            L("Permission:Projects.Delete"));

        projectsPermission.AddChild(
            AnomalyDetectionPermissions.Projects.ManageMembers,
            L("Permission:Projects.ManageMembers"));

        projectsPermission.AddChild(
            AnomalyDetectionPermissions.Projects.ManageMilestones,
            L("Permission:Projects.ManageMilestones"));

        projectsPermission.AddChild(
            AnomalyDetectionPermissions.Projects.ViewReports,
            L("Permission:Projects.ViewReports"));

        projectsPermission.AddChild(
            AnomalyDetectionPermissions.Projects.ManageStatus,
            L("Permission:Projects.ManageStatus"));
    }

    private static void DefineStatisticsPermissions(PermissionGroupDefinition group)
    {
        var statisticsPermission = group.AddPermission(
            AnomalyDetectionPermissions.Statistics.Default,
            L("Permission:Statistics"));

        statisticsPermission.AddChild(
            AnomalyDetectionPermissions.Statistics.ViewDashboard,
            L("Permission:Statistics.ViewDashboard"));

        statisticsPermission.AddChild(
            AnomalyDetectionPermissions.Statistics.ViewReports,
            L("Permission:Statistics.ViewReports"));

        statisticsPermission.AddChild(
            AnomalyDetectionPermissions.Statistics.GenerateReports,
            L("Permission:Statistics.GenerateReports"));

        statisticsPermission.AddChild(
            AnomalyDetectionPermissions.Statistics.ExportData,
            L("Permission:Statistics.ExportData"));

        statisticsPermission.AddChild(
            AnomalyDetectionPermissions.Statistics.ViewCrossOemData,
            L("Permission:Statistics.ViewCrossOemData"));

        statisticsPermission.AddChild(
            AnomalyDetectionPermissions.Statistics.ScheduleReports,
            L("Permission:Statistics.ScheduleReports"));
    }

    private static void DefineTenantManagementPermissions(PermissionGroupDefinition group)
    {
        var tenantManagementPermission = group.AddPermission(
            AnomalyDetectionPermissions.TenantManagement.Default,
            L("Permission:TenantManagement"));

        tenantManagementPermission.AddChild(
            AnomalyDetectionPermissions.TenantManagement.ManageOemMaster,
            L("Permission:TenantManagement.ManageOemMaster"));

        tenantManagementPermission.AddChild(
            AnomalyDetectionPermissions.TenantManagement.ManageTenantFeatures,
            L("Permission:TenantManagement.ManageTenantFeatures"));

        tenantManagementPermission.AddChild(
            AnomalyDetectionPermissions.TenantManagement.SwitchTenant,
            L("Permission:TenantManagement.SwitchTenant"));

        tenantManagementPermission.AddChild(
            AnomalyDetectionPermissions.TenantManagement.ViewTenantInfo,
            L("Permission:TenantManagement.ViewTenantInfo"));
    }

    private static void DefineOemTraceabilityPermissions(PermissionGroupDefinition group)
    {
        var oemTraceabilityPermission = group.AddPermission(
            AnomalyDetectionPermissions.OemTraceability.Default,
            L("Permission:OemTraceability"));

        oemTraceabilityPermission.AddChild(
            AnomalyDetectionPermissions.OemTraceability.CreateCustomization,
            L("Permission:OemTraceability.CreateCustomization"));

        oemTraceabilityPermission.AddChild(
            AnomalyDetectionPermissions.OemTraceability.EditCustomization,
            L("Permission:OemTraceability.EditCustomization"));

        oemTraceabilityPermission.AddChild(
            AnomalyDetectionPermissions.OemTraceability.DeleteCustomization,
            L("Permission:OemTraceability.DeleteCustomization"));

        oemTraceabilityPermission.AddChild(
            AnomalyDetectionPermissions.OemTraceability.SubmitCustomization,
            L("Permission:OemTraceability.SubmitCustomization"));

        oemTraceabilityPermission.AddChild(
            AnomalyDetectionPermissions.OemTraceability.ApproveCustomization,
            L("Permission:OemTraceability.ApproveCustomization"));

        oemTraceabilityPermission.AddChild(
            AnomalyDetectionPermissions.OemTraceability.RejectCustomization,
            L("Permission:OemTraceability.RejectCustomization"));

        oemTraceabilityPermission.AddChild(
            AnomalyDetectionPermissions.OemTraceability.ViewCustomization,
            L("Permission:OemTraceability.ViewCustomization"));

        oemTraceabilityPermission.AddChild(
            AnomalyDetectionPermissions.OemTraceability.ViewTraceability,
            L("Permission:OemTraceability.ViewTraceability"));

        oemTraceabilityPermission.AddChild(
            AnomalyDetectionPermissions.OemTraceability.ManageApprovals,
            L("Permission:OemTraceability.ManageApprovals"));

        oemTraceabilityPermission.AddChild(
            AnomalyDetectionPermissions.OemTraceability.ViewApprovalHistory,
            L("Permission:OemTraceability.ViewApprovalHistory"));

        oemTraceabilityPermission.AddChild(
            AnomalyDetectionPermissions.OemTraceability.CancelApproval,
            L("Permission:OemTraceability.CancelApproval"));
    }

    private static void DefineAnalysisPermissions(PermissionGroupDefinition group)
    {
        var analysisPermission = group.AddPermission(
            AnomalyDetectionPermissions.Analysis.Default,
            L("Permission:Analysis"));

        analysisPermission.AddChild(
            AnomalyDetectionPermissions.Analysis.AnalyzePatterns,
            L("Permission:Analysis.AnalyzePatterns"));

        analysisPermission.AddChild(
            AnomalyDetectionPermissions.Analysis.GenerateRecommendations,
            L("Permission:Analysis.GenerateRecommendations"));

        analysisPermission.AddChild(
            AnomalyDetectionPermissions.Analysis.ViewMetrics,
            L("Permission:Analysis.ViewMetrics"));

        analysisPermission.AddChild(
            AnomalyDetectionPermissions.Analysis.SearchSimilarSignals,
            L("Permission:Analysis.SearchSimilarSignals"));

        analysisPermission.AddChild(
            AnomalyDetectionPermissions.Analysis.CompareTestData,
            L("Permission:Analysis.CompareTestData"));

        analysisPermission.AddChild(
            AnomalyDetectionPermissions.Analysis.CalculateSimilarity,
            L("Permission:Analysis.CalculateSimilarity"));

        analysisPermission.AddChild(
            AnomalyDetectionPermissions.Analysis.ViewAnalysisReports,
            L("Permission:Analysis.ViewAnalysisReports"));

        analysisPermission.AddChild(
            AnomalyDetectionPermissions.Analysis.ExportAnalysisData,
            L("Permission:Analysis.ExportAnalysisData"));

        analysisPermission.AddChild(
            AnomalyDetectionPermissions.Analysis.ManageAnalysisSettings,
            L("Permission:Analysis.ManageAnalysisSettings"));
    }

    private static void DefineAuditLogPermissions(PermissionGroupDefinition group)
    {
        var auditLogPermission = group.AddPermission(
            AnomalyDetectionPermissions.AuditLogs.Default,
            L("Permission:AuditLogs"));

        auditLogPermission.AddChild(
            AnomalyDetectionPermissions.AuditLogs.View,
            L("Permission:AuditLogs.View"));

        auditLogPermission.AddChild(
            AnomalyDetectionPermissions.AuditLogs.ViewSecurity,
            L("Permission:AuditLogs.ViewSecurity"));

        auditLogPermission.AddChild(
            AnomalyDetectionPermissions.AuditLogs.ViewUserActivity,
            L("Permission:AuditLogs.ViewUserActivity"));

        auditLogPermission.AddChild(
            AnomalyDetectionPermissions.AuditLogs.ViewSystemActivity,
            L("Permission:AuditLogs.ViewSystemActivity"));

        auditLogPermission.AddChild(
            AnomalyDetectionPermissions.AuditLogs.Export,
            L("Permission:AuditLogs.Export"));
    }

    private static void DefineAdministrationPermissions(PermissionGroupDefinition group)
    {
        var administrationPermission = group.AddPermission(
            AnomalyDetectionPermissions.Administration.Default,
            L("Permission:Administration"));

        administrationPermission.AddChild(
            AnomalyDetectionPermissions.Administration.ManagePermissions,
            L("Permission:Administration.ManagePermissions"));

        administrationPermission.AddChild(
            AnomalyDetectionPermissions.Administration.ViewAuditLogs,
            L("Permission:Administration.ViewAuditLogs"));

        administrationPermission.AddChild(
            AnomalyDetectionPermissions.Administration.ManageSystemSettings,
            L("Permission:Administration.ManageSystemSettings"));

        administrationPermission.AddChild(
            AnomalyDetectionPermissions.Administration.ViewSystemHealth,
            L("Permission:Administration.ViewSystemHealth"));

        administrationPermission.AddChild(
            AnomalyDetectionPermissions.Administration.ManageBackups,
            L("Permission:Administration.ManageBackups"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<AnomalyDetectionResource>(name);
    }
}

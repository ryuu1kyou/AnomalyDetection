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

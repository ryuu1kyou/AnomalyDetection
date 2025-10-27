namespace AnomalyDetection.Permissions;

public static class AnomalyDetectionPermissions
{
    public const string GroupName = "AnomalyDetection";

    // CAN Signal Permissions
    public static class CanSignals
    {
        public const string Default = GroupName + ".CanSignals";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string ManageStandard = Default + ".ManageStandard";
        public const string Import = Default + ".Import";
        public const string Export = Default + ".Export";
    }

    // Detection Logic Permissions
    public static class DetectionLogics
    {
        public const string Default = GroupName + ".DetectionLogics";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Execute = Default + ".Execute";
        public const string Approve = Default + ".Approve";
        public const string ManageSharing = Default + ".ManageSharing";
        public const string ViewSafetyInfo = Default + ".ViewSafetyInfo";
        public const string ManageTemplates = Default + ".ManageTemplates";
        public const string Import = Default + ".Import";
        public const string Export = Default + ".Export";
    }

    // Detection Result Permissions
    public static class DetectionResults
    {
        public const string Default = GroupName + ".DetectionResults";
        public const string View = Default + ".View";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Resolve = Default + ".Resolve";
        public const string Share = Default + ".Share";
        public const string ViewShared = Default + ".ViewShared";
        public const string BulkOperations = Default + ".BulkOperations";
    }

    // Project Management Permissions
    public static class Projects
    {
        public const string Default = GroupName + ".Projects";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string ManageMembers = Default + ".ManageMembers";
        public const string ManageMilestones = Default + ".ManageMilestones";
        public const string ViewReports = Default + ".ViewReports";
        public const string ManageStatus = Default + ".ManageStatus";
    }

    // Statistics and Reports Permissions
    public static class Statistics
    {
        public const string Default = GroupName + ".Statistics";
        public const string ViewDashboard = Default + ".ViewDashboard";
        public const string ViewReports = Default + ".ViewReports";
        public const string GenerateReports = Default + ".GenerateReports";
        public const string ExportData = Default + ".ExportData";
        public const string ViewCrossOemData = Default + ".ViewCrossOemData";
        public const string ScheduleReports = Default + ".ScheduleReports";
    }

    // Multi-Tenant Management Permissions
    public static class TenantManagement
    {
        public const string Default = GroupName + ".TenantManagement";
        public const string ManageOemMaster = Default + ".ManageOemMaster";
        public const string ManageTenantFeatures = Default + ".ManageTenantFeatures";
        public const string SwitchTenant = Default + ".SwitchTenant";
        public const string ViewTenantInfo = Default + ".ViewTenantInfo";
    }

    // System Administration Permissions
    public static class Administration
    {
        public const string Default = GroupName + ".Administration";
        public const string ManagePermissions = Default + ".ManagePermissions";
        public const string ViewAuditLogs = Default + ".ViewAuditLogs";
        public const string ManageSystemSettings = Default + ".ManageSystemSettings";
        public const string ViewSystemHealth = Default + ".ViewSystemHealth";
        public const string ManageBackups = Default + ".ManageBackups";
    }
}

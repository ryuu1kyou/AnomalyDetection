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

    // OEM Traceability Permissions
    public static class OemTraceability
    {
        public const string Default = GroupName + ".OemTraceability";
        public const string CreateCustomization = Default + ".CreateCustomization";
        public const string EditCustomization = Default + ".EditCustomization";
        public const string DeleteCustomization = Default + ".DeleteCustomization";
        public const string SubmitCustomization = Default + ".SubmitCustomization";
        public const string ApproveCustomization = Default + ".ApproveCustomization";
        public const string RejectCustomization = Default + ".RejectCustomization";
        public const string ViewCustomization = Default + ".ViewCustomization";
        public const string ViewTraceability = Default + ".ViewTraceability";
        public const string ManageApprovals = Default + ".ManageApprovals";
        public const string ViewApprovalHistory = Default + ".ViewApprovalHistory";
        public const string CancelApproval = Default + ".CancelApproval";
    }

    // Analysis Permissions
    public static class Analysis
    {
        public const string Default = GroupName + ".Analysis";
        public const string AnalyzePatterns = Default + ".AnalyzePatterns";
        public const string GenerateRecommendations = Default + ".GenerateRecommendations";
        public const string ViewMetrics = Default + ".ViewMetrics";
        public const string SearchSimilarSignals = Default + ".SearchSimilarSignals";
        public const string CompareTestData = Default + ".CompareTestData";
        public const string CalculateSimilarity = Default + ".CalculateSimilarity";
        public const string ViewAnalysisReports = Default + ".ViewAnalysisReports";
        public const string ExportAnalysisData = Default + ".ExportAnalysisData";
        public const string ManageAnalysisSettings = Default + ".ManageAnalysisSettings";
    }

    // Audit Log Permissions
    public static class AuditLogs
    {
        public const string Default = GroupName + ".AuditLogs";
        public const string View = Default + ".View";
        public const string ViewSecurity = Default + ".ViewSecurity";
        public const string ViewUserActivity = Default + ".ViewUserActivity";
        public const string ViewSystemActivity = Default + ".ViewSystemActivity";
        public const string Export = Default + ".Export";
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

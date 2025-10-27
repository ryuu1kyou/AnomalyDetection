using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;

namespace AnomalyDetection.Data;

public class AnomalyDetectionRoleAndPermissionSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly ILogger<AnomalyDetectionRoleAndPermissionSeedContributor> _logger;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;
    private readonly IIdentityRoleRepository _roleRepository;
    private readonly IPermissionManager _permissionManager;
    private readonly IPermissionDefinitionManager _permissionDefinitionManager;

    public AnomalyDetectionRoleAndPermissionSeedContributor(
        ILogger<AnomalyDetectionRoleAndPermissionSeedContributor> logger,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant,
        IIdentityRoleRepository roleRepository,
        IPermissionManager permissionManager,
        IPermissionDefinitionManager permissionDefinitionManager)
    {
        _logger = logger;
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
        _roleRepository = roleRepository;
        _permissionManager = permissionManager;
        _permissionDefinitionManager = permissionDefinitionManager;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        _logger.LogInformation("Starting AnomalyDetection role and permission seeding...");

        await CreateRolesAsync();
        await AssignPermissionsToRolesAsync();

        _logger.LogInformation("AnomalyDetection role and permission seeding completed.");
    }

    private async Task CreateRolesAsync()
    {
        var roles = new[]
        {
            new { Name = "OemAdministrator", DisplayName = "OEM管理者", Description = "OEMテナント内の全権限を持つ管理者" },
            new { Name = "CanSignalEngineer", DisplayName = "CAN信号エンジニア", Description = "CAN信号の管理・編集権限を持つエンジニア" },
            new { Name = "DetectionEngineer", DisplayName = "異常検出エンジニア", Description = "異常検出ロジックの開発・管理権限を持つエンジニア" },
            new { Name = "ProjectManager", DisplayName = "プロジェクトマネージャー", Description = "プロジェクト管理権限を持つマネージャー" },
            new { Name = "Viewer", DisplayName = "閲覧者", Description = "データの閲覧のみ可能なユーザー" },
            new { Name = "SafetyEngineer", DisplayName = "機能安全エンジニア", Description = "機能安全関連の承認・管理権限を持つエンジニア" }
        };

        foreach (var roleData in roles)
        {
            var existingRole = await _roleRepository.FindByNormalizedNameAsync(roleData.Name.ToUpperInvariant());
            if (existingRole == null)
            {
                var role = new IdentityRole(
                    _guidGenerator.Create(),
                    roleData.Name,
                    _currentTenant.Id
                )
                {
                    IsDefault = false,
                    IsStatic = true,
                    IsPublic = true
                };

                await _roleRepository.InsertAsync(role);
                _logger.LogInformation("Created role: {DisplayName}", roleData.DisplayName);
            }
        }
    }

    private async Task AssignPermissionsToRolesAsync()
    {
        // OEM管理者 - 全権限
        await AssignPermissionsToRoleAsync("OemAdministrator", new[]
        {
            // CAN信号関連
            "AnomalyDetection.CanSignals",
            "AnomalyDetection.CanSignals.Create",
            "AnomalyDetection.CanSignals.Edit",
            "AnomalyDetection.CanSignals.Delete",
            "AnomalyDetection.CanSignals.ManageStandard",
            "AnomalyDetection.CanSignals.Import",
            "AnomalyDetection.CanSignals.Export",

            // 異常検出ロジック関連
            "AnomalyDetection.DetectionLogics",
            "AnomalyDetection.DetectionLogics.Create",
            "AnomalyDetection.DetectionLogics.Edit",
            "AnomalyDetection.DetectionLogics.Delete",
            "AnomalyDetection.DetectionLogics.Execute",
            "AnomalyDetection.DetectionLogics.Approve",
            "AnomalyDetection.DetectionLogics.ManageTemplates",

            // 異常検出結果関連
            "AnomalyDetection.DetectionResults",
            "AnomalyDetection.DetectionResults.Create",
            "AnomalyDetection.DetectionResults.Edit",
            "AnomalyDetection.DetectionResults.Delete",
            "AnomalyDetection.DetectionResults.Share",
            "AnomalyDetection.DetectionResults.Resolve",

            // プロジェクト管理関連
            "AnomalyDetection.Projects",
            "AnomalyDetection.Projects.Create",
            "AnomalyDetection.Projects.Edit",
            "AnomalyDetection.Projects.Delete",
            "AnomalyDetection.Projects.ManageMembers",
            "AnomalyDetection.Projects.ManageMilestones",

            // 統計・レポート関連
            "AnomalyDetection.Statistics",
            "AnomalyDetection.Statistics.ViewDashboard",
            "AnomalyDetection.Statistics.GenerateReports",
            "AnomalyDetection.Statistics.ExportData",

            // テナント管理関連
            "AnomalyDetection.TenantManagement",
            "AnomalyDetection.TenantManagement.SwitchTenant"
        });

        // CAN信号エンジニア
        await AssignPermissionsToRoleAsync("CanSignalEngineer", new[]
        {
            // CAN信号関連
            "AnomalyDetection.CanSignals",
            "AnomalyDetection.CanSignals.Create",
            "AnomalyDetection.CanSignals.Edit",
            "AnomalyDetection.CanSignals.Delete",
            "AnomalyDetection.CanSignals.ManageStandard",

            // 異常検出ロジック関連（参照のみ）
            "AnomalyDetection.DetectionLogics",

            // 異常検出結果関連（参照のみ）
            "AnomalyDetection.DetectionResults",

            // 統計・レポート関連（参照のみ）
            "AnomalyDetection.Statistics",
            "AnomalyDetection.Statistics.ViewDashboard",

            // テナント切り替え
            "AnomalyDetection.TenantManagement.SwitchTenant"
        });

        // 異常検出エンジニア
        await AssignPermissionsToRoleAsync("DetectionEngineer", new[]
        {
            // CAN信号関連（参照のみ）
            "AnomalyDetection.CanSignals",

            // 異常検出ロジック関連
            "AnomalyDetection.DetectionLogics",
            "AnomalyDetection.DetectionLogics.Create",
            "AnomalyDetection.DetectionLogics.Edit",
            "AnomalyDetection.DetectionLogics.Delete",
            "AnomalyDetection.DetectionLogics.Execute",
            "AnomalyDetection.DetectionLogics.ManageTemplates",

            // 異常検出結果関連
            "AnomalyDetection.DetectionResults",
            "AnomalyDetection.DetectionResults.Create",
            "AnomalyDetection.DetectionResults.Edit",
            "AnomalyDetection.DetectionResults.Share",
            "AnomalyDetection.DetectionResults.Resolve",

            // 統計・レポート関連
            "AnomalyDetection.Statistics",
            "AnomalyDetection.Statistics.ViewDashboard",
            "AnomalyDetection.Statistics.GenerateReports",

            // テナント切り替え
            "AnomalyDetection.TenantManagement.SwitchTenant"
        });

        // プロジェクトマネージャー
        await AssignPermissionsToRoleAsync("ProjectManager", new[]
        {
            // CAN信号関連（参照のみ）
            "AnomalyDetection.CanSignals",

            // 異常検出ロジック関連（参照のみ）
            "AnomalyDetection.DetectionLogics",

            // 異常検出結果関連（参照のみ）
            "AnomalyDetection.DetectionResults",

            // プロジェクト管理関連
            "AnomalyDetection.Projects",
            "AnomalyDetection.Projects.Create",
            "AnomalyDetection.Projects.Edit",
            "AnomalyDetection.Projects.Delete",
            "AnomalyDetection.Projects.ManageMembers",
            "AnomalyDetection.Projects.ManageMilestones",

            // 統計・レポート関連
            "AnomalyDetection.Statistics",
            "AnomalyDetection.Statistics.ViewDashboard",
            "AnomalyDetection.Statistics.GenerateReports",
            "AnomalyDetection.Statistics.ExportData",

            // テナント切り替え
            "AnomalyDetection.TenantManagement.SwitchTenant"
        });

        // 機能安全エンジニア
        await AssignPermissionsToRoleAsync("SafetyEngineer", new[]
        {
            // CAN信号関連（参照のみ）
            "AnomalyDetection.CanSignals",

            // 異常検出ロジック関連（承認権限含む）
            "AnomalyDetection.DetectionLogics",
            "AnomalyDetection.DetectionLogics.Approve",

            // 異常検出結果関連（参照のみ）
            "AnomalyDetection.DetectionResults",

            // 統計・レポート関連
            "AnomalyDetection.Statistics",
            "AnomalyDetection.Statistics.ViewDashboard",
            "AnomalyDetection.Statistics.GenerateReports",

            // テナント切り替え
            "AnomalyDetection.TenantManagement.SwitchTenant"
        });

        // 閲覧者 - 参照権限のみ
        await AssignPermissionsToRoleAsync("Viewer", new[]
        {
            // CAN信号関連（参照のみ）
            "AnomalyDetection.CanSignals",

            // 異常検出ロジック関連（参照のみ）
            "AnomalyDetection.DetectionLogics",

            // 異常検出結果関連（参照のみ）
            "AnomalyDetection.DetectionResults",

            // プロジェクト管理関連（参照のみ）
            "AnomalyDetection.Projects",

            // 統計・レポート関連（参照のみ）
            "AnomalyDetection.Statistics",
            "AnomalyDetection.Statistics.ViewDashboard",

            // テナント切り替え
            "AnomalyDetection.TenantManagement.SwitchTenant"
        });
    }

    private async Task AssignPermissionsToRoleAsync(string roleName, string[] permissions)
    {
        var role = await _roleRepository.FindByNormalizedNameAsync(roleName.ToUpperInvariant());
        if (role == null)
        {
            _logger.LogWarning("Role {RoleName} not found. Skipping permission assignment.", roleName);
            return;
        }

        foreach (var permission in permissions)
        {
            var permissionDefinition = _permissionDefinitionManager.GetOrNullAsync(permission).Result;
            if (permissionDefinition == null)
            {
                _logger.LogWarning("Permission {Permission} not found. Skipping.", permission);
                continue;
            }

            var currentPermission = await _permissionManager.GetAsync(permission, RolePermissionValueProvider.ProviderName, role.Name);
            if (currentPermission == null || !currentPermission.IsGranted)
            {
                await _permissionManager.SetAsync(permission, RolePermissionValueProvider.ProviderName, role.Name, true);
                _logger.LogDebug("Granted permission {Permission} to role {RoleName}", permission, roleName);
            }
        }

        _logger.LogInformation("Assigned permissions to role: {RoleName}", roleName);
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.Application.Services;
using Volo.Abp.MultiTenancy;

namespace AnomalyDetection.MultiTenancy;

public class MultiTenantDataIsolationService : ApplicationService
{
    private readonly IExtendedTenantRepository _extendedTenantRepository;
    private readonly IOemMasterRepository _oemMasterRepository;
    private readonly ICurrentTenant _currentTenant;

    public MultiTenantDataIsolationService(
        IExtendedTenantRepository extendedTenantRepository,
        IOemMasterRepository oemMasterRepository,
        ICurrentTenant currentTenant)
    {
        _extendedTenantRepository = extendedTenantRepository;
        _oemMasterRepository = oemMasterRepository;
        _currentTenant = currentTenant;
    }

    /// <summary>
    /// テナント別データ分離の動作確認
    /// </summary>
    public async Task<MultiTenantDataIsolationResult> VerifyDataIsolationAsync()
    {
        var result = new MultiTenantDataIsolationResult
        {
            CurrentTenantId = _currentTenant.Id,
            CurrentTenantName = _currentTenant.Name ?? string.Empty,
            TestExecutedAt = DateTime.UtcNow
        };

        try
        {
            // 1. 現在のテナントコンテキストでのデータ取得
            var currentTenantData = await GetCurrentTenantDataAsync();
            result.CurrentTenantDataCount = currentTenantData.Count;
            result.CurrentTenantData = currentTenantData;

            // 2. OEMマスターデータの取得（テナント非依存）
            var oemMasters = await _oemMasterRepository.GetActiveOemsAsync();
            result.OemMasterCount = oemMasters.Count;

            // 3. 全テナントデータの取得（ホストコンテキストでのみ可能）
            if (_currentTenant.Id == null) // Host tenant
            {
                var allTenants = await _extendedTenantRepository.GetActiveTenantsAsync();
                result.AllTenantsCount = allTenants.Count;
                result.IsHostTenant = true;
            }
            else
            {
                result.IsHostTenant = false;
            }

            result.IsSuccess = true;
            result.Message = "データ分離の確認が正常に完了しました。";

            Logger.LogInformation(
                "Multi-tenant data isolation verified. Tenant: {TenantId}, Data Count: {DataCount}",
                _currentTenant.Id,
                result.CurrentTenantDataCount);
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Message = $"データ分離の確認中にエラーが発生しました: {ex.Message}";
            result.ErrorDetails = ex.ToString();

            Logger.LogError(ex,
                "Error verifying multi-tenant data isolation for tenant: {TenantId}",
                _currentTenant.Id);
        }

        return result;
    }

    /// <summary>
    /// テナント切り替え時のデータ表示確認
    /// </summary>
    public async Task<TenantSwitchVerificationResult> VerifyTenantSwitchAsync(Guid? targetTenantId)
    {
        var result = new TenantSwitchVerificationResult
        {
            OriginalTenantId = _currentTenant.Id,
            TargetTenantId = targetTenantId,
            TestExecutedAt = DateTime.UtcNow
        };

        try
        {
            // 切り替え前のデータ取得
            var beforeSwitchData = await GetCurrentTenantDataAsync();
            result.BeforeSwitchDataCount = beforeSwitchData.Count;

            // テナント切り替え後のデータ取得をシミュレート
            // 実際の切り替えはフロントエンドまたは専用サービスで行う
            using (_currentTenant.Change(targetTenantId))
            {
                var afterSwitchData = await GetCurrentTenantDataAsync();
                result.AfterSwitchDataCount = afterSwitchData.Count;
                result.AfterSwitchData = afterSwitchData;
            }

            // データ分離の確認
            result.IsDataIsolated = result.BeforeSwitchDataCount != result.AfterSwitchDataCount ||
                                   !AreDataSetsEqual(beforeSwitchData, result.AfterSwitchData);

            result.IsSuccess = true;
            result.Message = result.IsDataIsolated
                ? "テナント切り替え時のデータ分離が正常に動作しています。"
                : "注意: テナント切り替え前後でデータが同じです。";

            Logger.LogInformation(
                "Tenant switch verification completed. Original: {OriginalTenant}, Target: {TargetTenant}, Isolated: {IsIsolated}",
                result.OriginalTenantId,
                result.TargetTenantId,
                result.IsDataIsolated);
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Message = $"テナント切り替え確認中にエラーが発生しました: {ex.Message}";
            result.ErrorDetails = ex.ToString();

            Logger.LogError(ex,
                "Error verifying tenant switch from {OriginalTenant} to {TargetTenant}",
                result.OriginalTenantId,
                result.TargetTenantId);
        }

        return result;
    }

    private async Task<List<TenantDataSummary>> GetCurrentTenantDataAsync()
    {
        var data = new List<TenantDataSummary>();

        // ExtendedTenant データ
        var tenants = await _extendedTenantRepository.GetActiveTenantsAsync();
        data.Add(new TenantDataSummary
        {
            EntityType = "ExtendedTenant",
            Count = tenants.Count,
            TenantId = _currentTenant.Id
        });

        return data;
    }

    private static bool AreDataSetsEqual(List<TenantDataSummary> data1, List<TenantDataSummary> data2)
    {
        if (data1.Count != data2.Count)
            return false;

        for (int i = 0; i < data1.Count; i++)
        {
            if (data1[i].EntityType != data2[i].EntityType ||
                data1[i].Count != data2[i].Count)
            {
                return false;
            }
        }

        return true;
    }
}

public class MultiTenantDataIsolationResult
{
    public Guid? CurrentTenantId { get; set; }
    public string CurrentTenantName { get; set; } = string.Empty;
    public bool IsHostTenant { get; set; }
    public int CurrentTenantDataCount { get; set; }
    public List<TenantDataSummary> CurrentTenantData { get; set; } = new();
    public int OemMasterCount { get; set; }
    public int? AllTenantsCount { get; set; }
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ErrorDetails { get; set; } = string.Empty;
    public DateTime TestExecutedAt { get; set; }
}

public class TenantSwitchVerificationResult
{
    public Guid? OriginalTenantId { get; set; }
    public Guid? TargetTenantId { get; set; }
    public int BeforeSwitchDataCount { get; set; }
    public int AfterSwitchDataCount { get; set; }
    public List<TenantDataSummary> AfterSwitchData { get; set; } = new();
    public bool IsDataIsolated { get; set; }
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ErrorDetails { get; set; } = string.Empty;
    public DateTime TestExecutedAt { get; set; }
}

public class TenantDataSummary
{
    public string EntityType { get; set; } = string.Empty;
    public int Count { get; set; }
    public Guid? TenantId { get; set; }
}
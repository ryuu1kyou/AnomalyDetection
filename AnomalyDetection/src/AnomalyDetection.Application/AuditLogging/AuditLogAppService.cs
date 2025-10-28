using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnomalyDetection.Application.Contracts.AuditLogging;
using AnomalyDetection.AuditLogging;
using AnomalyDetection.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;

namespace AnomalyDetection.Application.AuditLogging;

/// <summary>
/// 監査ログアプリケーションサービス実装
/// </summary>
[Authorize(AnomalyDetectionPermissions.AuditLogs.Default)]
public class AuditLogAppService : ApplicationService, IAuditLogAppService
{
    private readonly IAnomalyDetectionAuditLogRepository _auditLogRepository;

    public AuditLogAppService(IAnomalyDetectionAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    [Authorize(AnomalyDetectionPermissions.AuditLogs.View)]
    public async Task<List<AuditLogDto>> GetEntityAuditLogsAsync(Guid entityId, string? entityType = null)
    {
        var auditLogs = await _auditLogRepository.GetByEntityIdAsync(entityId);
        
        if (!string.IsNullOrEmpty(entityType))
        {
            auditLogs = auditLogs.Where(x => x.EntityType == entityType).ToList();
        }

        return ObjectMapper.Map<List<AnomalyDetectionAuditLog>, List<AuditLogDto>>(auditLogs);
    }

    [Authorize(AnomalyDetectionPermissions.AuditLogs.View)]
    public async Task<List<AuditLogDto>> GetUserAuditLogsAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var auditLogs = await _auditLogRepository.GetByUserIdAsync(userId);

        if (startDate.HasValue || endDate.HasValue)
        {
            var start = startDate ?? DateTime.MinValue;
            var end = endDate ?? DateTime.MaxValue;
            auditLogs = auditLogs.Where(x => x.CreationTime >= start && x.CreationTime <= end).ToList();
        }

        return ObjectMapper.Map<List<AnomalyDetectionAuditLog>, List<AuditLogDto>>(auditLogs);
    }

    [Authorize(AnomalyDetectionPermissions.AuditLogs.View)]
    public async Task<List<AuditLogDto>> GetAuditLogsByLevelAsync(AuditLogLevel level, DateTime? startDate = null, DateTime? endDate = null)
    {
        var auditLogs = await _auditLogRepository.GetByLevelAsync(level);

        if (startDate.HasValue || endDate.HasValue)
        {
            var start = startDate ?? DateTime.MinValue;
            var end = endDate ?? DateTime.MaxValue;
            auditLogs = auditLogs.Where(x => x.CreationTime >= start && x.CreationTime <= end).ToList();
        }

        return ObjectMapper.Map<List<AnomalyDetectionAuditLog>, List<AuditLogDto>>(auditLogs);
    }

    [Authorize(AnomalyDetectionPermissions.AuditLogs.View)]
    public async Task<List<AuditLogDto>> GetAuditLogsByActionAsync(AuditLogAction action, DateTime? startDate = null, DateTime? endDate = null)
    {
        var auditLogs = await _auditLogRepository.GetByActionAsync(action);

        if (startDate.HasValue || endDate.HasValue)
        {
            var start = startDate ?? DateTime.MinValue;
            var end = endDate ?? DateTime.MaxValue;
            auditLogs = auditLogs.Where(x => x.CreationTime >= start && x.CreationTime <= end).ToList();
        }

        return ObjectMapper.Map<List<AnomalyDetectionAuditLog>, List<AuditLogDto>>(auditLogs);
    }

    [Authorize(AnomalyDetectionPermissions.AuditLogs.ViewSecurity)]
    public async Task<List<AuditLogDto>> GetSecurityAuditLogsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var auditLogs = await _auditLogRepository.GetByDateRangeAsync(start, end);
        
        // セキュリティ関連のログのみフィルタリング
        var securityLogs = auditLogs.Where(x => 
            x.EntityType == "Security" ||
            x.Action == AuditLogAction.Login ||
            x.Action == AuditLogAction.Logout ||
            x.Action == AuditLogAction.PermissionChange ||
            x.Level == AuditLogLevel.Critical ||
            x.Level == AuditLogLevel.Error).ToList();

        return ObjectMapper.Map<List<AnomalyDetectionAuditLog>, List<AuditLogDto>>(securityLogs);
    }
}
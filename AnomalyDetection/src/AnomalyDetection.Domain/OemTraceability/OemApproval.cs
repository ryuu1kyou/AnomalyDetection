using System;
using System.Collections.Generic;
using AnomalyDetection.MultiTenancy;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace AnomalyDetection.OemTraceability;

/// <summary>
/// OEM承認ワークフローを管理するエンティティ
/// </summary>
public class OemApproval : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    
    /// <summary>
    /// 対象エンティティのID
    /// </summary>
    public Guid EntityId { get; private set; }
    
    /// <summary>
    /// 対象エンティティの種類 ("CanSignal", "DetectionLogic", etc.)
    /// </summary>
    public string EntityType { get; private set; }
    
    /// <summary>
    /// OEMコード
    /// </summary>
    public OemCode OemCode { get; private set; }
    
    /// <summary>
    /// 承認の種類
    /// </summary>
    public ApprovalType Type { get; private set; }
    
    /// <summary>
    /// 承認申請者のID
    /// </summary>
    public Guid RequestedBy { get; private set; }
    
    /// <summary>
    /// 承認申請日時
    /// </summary>
    public DateTime RequestedAt { get; private set; }
    
    /// <summary>
    /// 承認者のID
    /// </summary>
    public Guid? ApprovedBy { get; private set; }
    
    /// <summary>
    /// 承認日時
    /// </summary>
    public DateTime? ApprovedAt { get; private set; }
    
    /// <summary>
    /// 承認状態
    /// </summary>
    public ApprovalStatus Status { get; private set; }
    
    /// <summary>
    /// 承認申請の理由
    /// </summary>
    public string ApprovalReason { get; private set; }
    
    /// <summary>
    /// 承認者のコメント
    /// </summary>
    public string? ApprovalNotes { get; private set; }
    
    /// <summary>
    /// 承認に関連するデータ
    /// </summary>
    public Dictionary<string, object> ApprovalData { get; private set; }
    
    /// <summary>
    /// 承認期限
    /// </summary>
    public DateTime? DueDate { get; private set; }
    
    /// <summary>
    /// 優先度 (1: 低, 2: 中, 3: 高, 4: 緊急)
    /// </summary>
    public int Priority { get; private set; }

    protected OemApproval() { }

    public OemApproval(
        Guid? tenantId,
        Guid entityId,
        string entityType,
        OemCode oemCode,
        ApprovalType type,
        Guid requestedBy,
        string approvalReason,
        Dictionary<string, object>? approvalData = null,
        DateTime? dueDate = null,
        int priority = 2)
    {
        TenantId = tenantId;
        EntityId = entityId;
        EntityType = Check.NotNullOrWhiteSpace(entityType, nameof(entityType));
        OemCode = Check.NotNull(oemCode, nameof(oemCode));
        Type = type;
        RequestedBy = requestedBy;
        RequestedAt = DateTime.UtcNow;
        ApprovalReason = Check.NotNullOrWhiteSpace(approvalReason, nameof(approvalReason));
        ApprovalData = approvalData ?? new Dictionary<string, object>();
        DueDate = dueDate;
        Priority = ValidatePriority(priority);
        Status = ApprovalStatus.Pending;
    }

    /// <summary>
    /// 承認する
    /// </summary>
    /// <param name="approvedBy">承認者のID</param>
    /// <param name="notes">承認コメント</param>
    public void Approve(Guid approvedBy, string? notes = null)
    {
        if (Status != ApprovalStatus.Pending)
            throw new BusinessException("AnomalyDetection:ApprovalNotPending")
                .WithData("Status", Status);

        ApprovedBy = approvedBy;
        ApprovedAt = DateTime.UtcNow;
        ApprovalNotes = notes;
        Status = ApprovalStatus.Approved;
    }

    /// <summary>
    /// 却下する
    /// </summary>
    /// <param name="rejectedBy">却下者のID</param>
    /// <param name="notes">却下理由</param>
    public void Reject(Guid rejectedBy, string notes)
    {
        if (Status != ApprovalStatus.Pending)
            throw new BusinessException("AnomalyDetection:ApprovalNotPending")
                .WithData("Status", Status);

        Check.NotNullOrWhiteSpace(notes, nameof(notes));

        ApprovedBy = rejectedBy;
        ApprovedAt = DateTime.UtcNow;
        ApprovalNotes = notes;
        Status = ApprovalStatus.Rejected;
    }

    /// <summary>
    /// 承認申請をキャンセルする
    /// </summary>
    /// <param name="cancelledBy">キャンセル実行者のID</param>
    /// <param name="reason">キャンセル理由</param>
    public void Cancel(Guid cancelledBy, string reason)
    {
        if (Status != ApprovalStatus.Pending)
            throw new BusinessException("AnomalyDetection:ApprovalNotPending")
                .WithData("Status", Status);

        Check.NotNullOrWhiteSpace(reason, nameof(reason));

        // キャンセルした人と理由を記録
        ApprovedBy = cancelledBy;
        ApprovedAt = DateTime.UtcNow;
        ApprovalNotes = $"Cancelled: {reason}";
        Status = ApprovalStatus.Cancelled;
    }

    /// <summary>
    /// 承認期限を更新する
    /// </summary>
    /// <param name="newDueDate">新しい承認期限</param>
    public void UpdateDueDate(DateTime? newDueDate)
    {
        if (Status != ApprovalStatus.Pending)
            throw new BusinessException("AnomalyDetection:CannotUpdateCompletedApproval");

        DueDate = newDueDate;
    }

    /// <summary>
    /// 優先度を更新する
    /// </summary>
    /// <param name="newPriority">新しい優先度</param>
    public void UpdatePriority(int newPriority)
    {
        if (Status != ApprovalStatus.Pending)
            throw new BusinessException("AnomalyDetection:CannotUpdateCompletedApproval");

        Priority = ValidatePriority(newPriority);
    }

    /// <summary>
    /// 承認申請データを更新する
    /// </summary>
    /// <param name="approvalData">新しい承認データ</param>
    /// <param name="reason">更新理由</param>
    public void UpdateApprovalData(Dictionary<string, object> approvalData, string reason)
    {
        if (Status != ApprovalStatus.Pending)
            throw new BusinessException("AnomalyDetection:CannotUpdateCompletedApproval");

        Check.NotNull(approvalData, nameof(approvalData));
        Check.NotNullOrWhiteSpace(reason, nameof(reason));

        ApprovalData = approvalData;
        ApprovalReason = $"{ApprovalReason} | Updated: {reason}";
    }

    /// <summary>
    /// 承認が期限切れかどうかを判定する
    /// </summary>
    /// <returns>期限切れの場合true</returns>
    public bool IsOverdue()
    {
        return Status == ApprovalStatus.Pending && 
               DueDate.HasValue && 
               DateTime.UtcNow > DueDate.Value;
    }

    /// <summary>
    /// 承認が緊急かどうかを判定する
    /// </summary>
    /// <returns>緊急の場合true</returns>
    public bool IsUrgent()
    {
        return Priority >= 4 || 
               (DueDate.HasValue && DateTime.UtcNow.AddDays(1) > DueDate.Value);
    }

    private static int ValidatePriority(int priority)
    {
        if (priority < 1 || priority > 4)
            throw new ArgumentOutOfRangeException(nameof(priority), "Priority must be between 1 and 4");
        return priority;
    }
}
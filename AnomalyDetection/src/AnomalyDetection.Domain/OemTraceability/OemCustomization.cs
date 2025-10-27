using System;
using System.Collections.Generic;
using AnomalyDetection.MultiTenancy;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace AnomalyDetection.OemTraceability;

/// <summary>
/// OEM固有のカスタマイズ情報を管理するエンティティ
/// </summary>
public class OemCustomization : FullAuditedAggregateRoot<Guid>, IMultiTenant
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
    /// カスタマイズの種類
    /// </summary>
    public CustomizationType Type { get; private set; }
    
    /// <summary>
    /// カスタマイズされたパラメータ
    /// </summary>
    public Dictionary<string, object> CustomParameters { get; private set; }
    
    /// <summary>
    /// 元のパラメータ
    /// </summary>
    public Dictionary<string, object> OriginalParameters { get; private set; }
    
    /// <summary>
    /// カスタマイズの理由
    /// </summary>
    public string CustomizationReason { get; private set; }
    
    /// <summary>
    /// 承認者のID
    /// </summary>
    public Guid? ApprovedBy { get; private set; }
    
    /// <summary>
    /// 承認日時
    /// </summary>
    public DateTime? ApprovedAt { get; private set; }
    
    /// <summary>
    /// カスタマイズの状態
    /// </summary>
    public CustomizationStatus Status { get; private set; }
    
    /// <summary>
    /// 承認者のコメント
    /// </summary>
    public string? ApprovalNotes { get; private set; }

    protected OemCustomization() { }

    public OemCustomization(
        Guid? tenantId,
        Guid entityId,
        string entityType,
        OemCode oemCode,
        CustomizationType type,
        Dictionary<string, object> customParameters,
        Dictionary<string, object> originalParameters,
        string customizationReason)
    {
        TenantId = tenantId;
        EntityId = entityId;
        EntityType = Check.NotNullOrWhiteSpace(entityType, nameof(entityType));
        OemCode = Check.NotNull(oemCode, nameof(oemCode));
        Type = type;
        CustomParameters = customParameters ?? new Dictionary<string, object>();
        OriginalParameters = originalParameters ?? new Dictionary<string, object>();
        CustomizationReason = Check.NotNullOrWhiteSpace(customizationReason, nameof(customizationReason));
        Status = CustomizationStatus.Draft;
    }

    /// <summary>
    /// カスタマイズを承認申請する
    /// </summary>
    public void SubmitForApproval()
    {
        if (Status != CustomizationStatus.Draft)
            throw new BusinessException("AnomalyDetection:CustomizationNotDraft")
                .WithData("Status", Status);

        Status = CustomizationStatus.PendingApproval;
    }

    /// <summary>
    /// カスタマイズを承認する
    /// </summary>
    /// <param name="approvedBy">承認者のID</param>
    /// <param name="approvalNotes">承認コメント</param>
    public void Approve(Guid approvedBy, string? approvalNotes = null)
    {
        if (Status != CustomizationStatus.PendingApproval)
            throw new BusinessException("AnomalyDetection:CustomizationNotPendingApproval")
                .WithData("Status", Status);

        ApprovedBy = approvedBy;
        ApprovedAt = DateTime.UtcNow;
        ApprovalNotes = approvalNotes;
        Status = CustomizationStatus.Approved;
    }

    /// <summary>
    /// カスタマイズを却下する
    /// </summary>
    /// <param name="rejectedBy">却下者のID</param>
    /// <param name="rejectionNotes">却下理由</param>
    public void Reject(Guid rejectedBy, string rejectionNotes)
    {
        if (Status != CustomizationStatus.PendingApproval)
            throw new BusinessException("AnomalyDetection:CustomizationNotPendingApproval")
                .WithData("Status", Status);

        Check.NotNullOrWhiteSpace(rejectionNotes, nameof(rejectionNotes));

        ApprovedBy = rejectedBy;
        ApprovedAt = DateTime.UtcNow;
        ApprovalNotes = rejectionNotes;
        Status = CustomizationStatus.Rejected;
    }

    /// <summary>
    /// カスタマイズパラメータを更新する
    /// </summary>
    /// <param name="customParameters">新しいカスタマイズパラメータ</param>
    /// <param name="reason">更新理由</param>
    public void UpdateCustomParameters(Dictionary<string, object> customParameters, string reason)
    {
        if (Status == CustomizationStatus.Approved)
            throw new BusinessException("AnomalyDetection:CannotUpdateApprovedCustomization");

        Check.NotNull(customParameters, nameof(customParameters));
        Check.NotNullOrWhiteSpace(reason, nameof(reason));

        CustomParameters = customParameters;
        CustomizationReason = reason;
        
        // 承認待ちの場合は下書きに戻す
        if (Status == CustomizationStatus.PendingApproval)
        {
            Status = CustomizationStatus.Draft;
        }
    }

    /// <summary>
    /// カスタマイズを廃止する
    /// </summary>
    public void MarkAsObsolete()
    {
        Status = CustomizationStatus.Obsolete;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Domain.Values;
using Volo.Abp.MultiTenancy;

namespace AnomalyDetection.AnomalyDetection;

public class AnomalyDetectionResult : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    
    // 関連エンティティ
    public Guid DetectionLogicId { get; private set; }
    public Guid CanSignalId { get; private set; }
    
    // 検出結果の基本情報
    public DateTime DetectedAt { get; private set; }
    public AnomalyLevel AnomalyLevel { get; private set; }
    public double ConfidenceScore { get; private set; }
    public string Description { get; private set; } = string.Empty;
    
    // 入力データ
    public DetectionInputData InputData { get; private set; } = null!;
    
    // 検出詳細
    public DetectionDetails Details { get; private set; } = null!;
    
    // 異常検出詳細分析用の新しいプロパティ
    public TimeSpan DetectionDuration { get; private set; }
    public AnomalyType AnomalyType { get; private set; }
    public string DetectionCondition { get; private set; } = string.Empty;
    public bool IsValidated { get; private set; }
    public bool IsFalsePositiveFlag { get; private set; }
    public string ValidationNotes { get; private set; } = string.Empty;
    
    // 解決状況
    public ResolutionStatus ResolutionStatus { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public Guid? ResolvedBy { get; private set; }
    public string ResolutionNotes { get; private set; } = string.Empty;
    
    // 共有設定
    public SharingLevel SharingLevel { get; private set; }
    public bool IsShared { get; private set; }
    public DateTime? SharedAt { get; private set; }
    public Guid? SharedBy { get; private set; }

    protected AnomalyDetectionResult() { }

    public AnomalyDetectionResult(
        Guid id,
        Guid? tenantId,
        Guid detectionLogicId,
        Guid canSignalId,
        AnomalyLevel anomalyLevel,
        double confidenceScore,
        string description,
        DetectionInputData inputData,
        DetectionDetails details,
        TimeSpan detectionDuration = default,
        AnomalyType anomalyType = AnomalyType.Custom,
        string detectionCondition = "") : base(id)
    {
        TenantId = tenantId;
        DetectionLogicId = detectionLogicId;
        CanSignalId = canSignalId;
        DetectedAt = DateTime.UtcNow;
        AnomalyLevel = anomalyLevel;
        ConfidenceScore = ValidateConfidenceScore(confidenceScore);
        Description = ValidateDescription(description);
        InputData = inputData ?? throw new ArgumentNullException(nameof(inputData));
        Details = details ?? throw new ArgumentNullException(nameof(details));
        
        // 新しいプロパティの初期化
        DetectionDuration = detectionDuration;
        AnomalyType = anomalyType;
        DetectionCondition = detectionCondition ?? string.Empty;
        IsValidated = false;
        IsFalsePositiveFlag = false;
        ValidationNotes = string.Empty;
        
        ResolutionStatus = ResolutionStatus.Open;
        SharingLevel = SharingLevel.Private;
        IsShared = false;
    }

    // ビジネスメソッド
    public void UpdateAnomalyLevel(AnomalyLevel newLevel, string reason)
    {
        if (ResolutionStatus == ResolutionStatus.Resolved)
            throw new InvalidOperationException("Cannot update anomaly level of resolved result");
            
        AnomalyLevel = newLevel;
        AddResolutionNote($"Anomaly level updated to {newLevel}: {reason}");
    }

    public void UpdateConfidenceScore(double newScore, string reason)
    {
        ConfidenceScore = ValidateConfidenceScore(newScore);
        AddResolutionNote($"Confidence score updated to {newScore:F2}: {reason}");
    }

    public void MarkAsInvestigating(Guid investigatedBy, string? notes = null)
    {
        if (ResolutionStatus == ResolutionStatus.Resolved)
            throw new InvalidOperationException("Cannot investigate resolved result");
            
        ResolutionStatus = ResolutionStatus.InProgress;
        ResolvedBy = investigatedBy;
        
        if (!string.IsNullOrEmpty(notes))
        {
            AddResolutionNote($"Investigation started: {notes}");
        }
    }

    public void MarkAsFalsePositive(Guid resolvedBy, string reason)
    {
        ResolutionStatus = ResolutionStatus.FalsePositive;
        IsFalsePositiveFlag = true;
        ResolvedAt = DateTime.UtcNow;
        ResolvedBy = resolvedBy;
        AddResolutionNote($"Marked as false positive: {reason}");
        AddValidationNote($"False positive confirmed: {reason}");
    }

    public void Resolve(Guid resolvedBy, string resolution)
    {
        ResolutionStatus = ResolutionStatus.Resolved;
        ResolvedAt = DateTime.UtcNow;
        ResolvedBy = resolvedBy;
        AddResolutionNote($"Resolved: {resolution}");
    }

    public void Reopen(Guid reopenedBy, string reason)
    {
        if (ResolutionStatus == ResolutionStatus.Open)
            throw new InvalidOperationException("Result is already open");
            
        ResolutionStatus = ResolutionStatus.Reopened;
        ResolvedAt = null;
        ResolvedBy = null;
        AddResolutionNote($"Reopened: {reason}");
    }

    public void ShareResult(SharingLevel sharingLevel, Guid sharedBy)
    {
        if (ResolutionStatus == ResolutionStatus.FalsePositive)
            throw new InvalidOperationException("Cannot share false positive results");
            
        SharingLevel = sharingLevel;
        IsShared = true;
        SharedAt = DateTime.UtcNow;
        SharedBy = sharedBy;
    }

    public void RevokeSharing()
    {
        SharingLevel = SharingLevel.Private;
        IsShared = false;
        SharedAt = null;
        SharedBy = null;
    }

    public bool IsResolved()
    {
        return ResolutionStatus == ResolutionStatus.Resolved;
    }

    public bool IsFalsePositive()
    {
        return IsFalsePositiveFlag || ResolutionStatus == ResolutionStatus.FalsePositive;
    }

    // 新しいビジネスメソッド
    public void UpdateDetectionDuration(TimeSpan duration)
    {
        if (duration < TimeSpan.Zero)
            throw new ArgumentException("Detection duration cannot be negative", nameof(duration));
            
        DetectionDuration = duration;
    }

    public void UpdateAnomalyType(AnomalyType anomalyType, string? reason = null)
    {
        AnomalyType = anomalyType;
        
        if (!string.IsNullOrEmpty(reason))
        {
            AddValidationNote($"Anomaly type updated to {anomalyType}: {reason}");
        }
    }

    public void UpdateDetectionCondition(string condition)
    {
        DetectionCondition = ValidateDetectionCondition(condition);
        AddValidationNote($"Detection condition updated: {condition}");
    }

    public void ValidateResult(Guid validatedBy, bool isValid, string? notes = null)
    {
        IsValidated = true;
        
        if (!isValid)
        {
            IsFalsePositiveFlag = true;
        }
        
        if (!string.IsNullOrEmpty(notes))
        {
            AddValidationNote($"Validated by {validatedBy}: {(isValid ? "Valid" : "Invalid")} - {notes}");
        }
        else
        {
            AddValidationNote($"Validated by {validatedBy}: {(isValid ? "Valid" : "Invalid")}");
        }
    }

    public void AddValidationNote(string note)
    {
        if (string.IsNullOrWhiteSpace(note))
            return;
            
        ValidationNotes = string.IsNullOrEmpty(ValidationNotes) 
            ? $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {note}"
            : $"{ValidationNotes}\n[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {note}";
    }

    public bool HasValidationNotes()
    {
        return !string.IsNullOrEmpty(ValidationNotes);
    }

    public double GetDetectionDurationMs()
    {
        return DetectionDuration.TotalMilliseconds;
    }

    public bool IsHighPriority()
    {
        return AnomalyLevel >= AnomalyLevel.Critical;
    }

    public bool IsRecentDetection(TimeSpan threshold)
    {
        return DateTime.UtcNow - DetectedAt <= threshold;
    }

    public TimeSpan GetAge()
    {
        return DateTime.UtcNow - DetectedAt;
    }

    public TimeSpan? GetResolutionTime()
    {
        return ResolvedAt?.Subtract(DetectedAt);
    }

    private void AddResolutionNote(string note)
    {
        ResolutionNotes = string.IsNullOrEmpty(ResolutionNotes) 
            ? $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {note}"
            : $"{ResolutionNotes}\n[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {note}";
    }

    private static double ValidateConfidenceScore(double score)
    {
        if (score < 0.0 || score > 1.0)
            throw new ArgumentOutOfRangeException(nameof(score), "Confidence score must be between 0.0 and 1.0");
            
        return score;
    }

    private static string ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null or empty", nameof(description));
            
        if (description.Length > 1000)
            throw new ArgumentException("Description cannot exceed 1000 characters", nameof(description));
            
        return description.Trim();
    }

    private static string ValidateDetectionCondition(string condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
            return string.Empty;
            
        if (condition.Length > 500)
            throw new ArgumentException("Detection condition cannot exceed 500 characters", nameof(condition));
            
        return condition.Trim();
    }
}

public class DetectionInputData : ValueObject
{
    public double SignalValue { get; private set; }
    public DateTime Timestamp { get; private set; }
    public Dictionary<string, object> AdditionalData { get; private set; } = new();

    protected DetectionInputData() { }

    public DetectionInputData(double signalValue, DateTime timestamp, Dictionary<string, object>? additionalData = null)
    {
        SignalValue = signalValue;
        Timestamp = timestamp;
        AdditionalData = additionalData ?? new Dictionary<string, object>();
    }

    public T? GetAdditionalData<T>(string key)
    {
        if (AdditionalData.TryGetValue(key, out var value))
        {
            return (T)value;
        }
        return default(T);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return SignalValue;
        yield return Timestamp;
        yield return string.Join(",", AdditionalData.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
    }
}

public class DetectionDetails : ValueObject
{
    public DetectionType DetectionType { get; private set; }
    public string TriggerCondition { get; private set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; private set; } = new();
    public double ExecutionTimeMs { get; private set; }

    protected DetectionDetails() { }

    public DetectionDetails(
        DetectionType detectionType,
        string triggerCondition,
        Dictionary<string, object>? parameters = null,
        double executionTimeMs = 0)
    {
        DetectionType = detectionType;
        TriggerCondition = ValidateTriggerCondition(triggerCondition);
        Parameters = parameters ?? new Dictionary<string, object>();
        ExecutionTimeMs = executionTimeMs;
    }

    private static string ValidateTriggerCondition(string condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
            throw new ArgumentException("Trigger condition cannot be null or empty", nameof(condition));
            
        if (condition.Length > 500)
            throw new ArgumentException("Trigger condition cannot exceed 500 characters", nameof(condition));
            
        return condition.Trim();
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return DetectionType;
        yield return TriggerCondition;
        yield return string.Join(",", Parameters.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
        yield return ExecutionTimeMs;
    }
}


using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Values;

namespace AnomalyDetection.AnomalyDetection;

public class SafetyClassification : ValueObject
{
    public AsilLevel AsilLevel { get; private set; }
    public string? SafetyRequirementId { get; private set; }
    public string? SafetyGoalId { get; private set; }
    public string? HazardAnalysisId { get; private set; }

    protected SafetyClassification() { }

    public SafetyClassification(
        AsilLevel asilLevel, 
        string? safetyRequirementId = null, 
        string? safetyGoalId = null,
        string? hazardAnalysisId = null)
    {
        AsilLevel = asilLevel;
        SafetyRequirementId = ValidateId(safetyRequirementId, nameof(safetyRequirementId));
        SafetyGoalId = ValidateId(safetyGoalId, nameof(safetyGoalId));
        HazardAnalysisId = ValidateId(hazardAnalysisId, nameof(hazardAnalysisId));
        
        ValidateSafetyClassification();
    }

    public bool RequiresApproval()
    {
        return AsilLevel >= AsilLevel.B;
    }

    public bool RequiresFormalVerification()
    {
        return AsilLevel >= AsilLevel.C;
    }

    public bool RequiresIndependentReview()
    {
        return AsilLevel == AsilLevel.D;
    }

    public bool IsSafetyRelevant()
    {
        return AsilLevel > AsilLevel.QM;
    }

    public string GetSafetyLevelDescription()
    {
        return AsilLevel switch
        {
            AsilLevel.QM => "Quality Management - 非安全関連",
            AsilLevel.A => "ASIL A - 軽微な傷害のリスク",
            AsilLevel.B => "ASIL B - 軽度から中程度の傷害のリスク",
            AsilLevel.C => "ASIL C - 重篤な傷害のリスク",
            AsilLevel.D => "ASIL D - 生命に関わるリスク",
            _ => "Unknown ASIL Level"
        };
    }

    private static string? ValidateId(string? id, string paramName)
    {
        if (id != null && id.Length > 100)
            throw new ArgumentException($"{paramName} cannot exceed 100 characters", paramName);
            
        return id?.Trim();
    }

    private void ValidateSafetyClassification()
    {
        // ASIL B以上の場合は安全要求IDが必要
        if (AsilLevel >= AsilLevel.B && string.IsNullOrWhiteSpace(SafetyRequirementId))
        {
            throw new ArgumentException("Safety requirement ID is required for ASIL B and above");
        }
        
        // ASIL C以上の場合は安全目標IDも必要
        if (AsilLevel >= AsilLevel.C && string.IsNullOrWhiteSpace(SafetyGoalId))
        {
            throw new ArgumentException("Safety goal ID is required for ASIL C and above");
        }
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return AsilLevel;
        yield return SafetyRequirementId ?? string.Empty;
        yield return SafetyGoalId ?? string.Empty;
        yield return HazardAnalysisId ?? string.Empty;
    }

    public override string ToString()
    {
        return $"ASIL {AsilLevel}";
    }
}
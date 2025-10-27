using System;
using System.Collections.Generic;
using System.Linq;
using AnomalyDetection.MultiTenancy;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace AnomalyDetection.AnomalyDetection;

public class CanAnomalyDetectionLogic : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    
    // 値オブジェクト
    public DetectionLogicIdentity Identity { get; private set; }
    public DetectionLogicSpecification Specification { get; private set; }
    public LogicImplementation Implementation { get; private set; }
    public SafetyClassification Safety { get; private set; }
    
    // エンティティ
    private readonly List<DetectionParameter> _parameters = new();
    private readonly List<CanSignalMapping> _signalMappings = new();
    
    public IReadOnlyList<DetectionParameter> Parameters => _parameters.AsReadOnly();
    public IReadOnlyList<CanSignalMapping> SignalMappings => _signalMappings.AsReadOnly();
    
    // 属性
    public DetectionLogicStatus Status { get; private set; }
    public SharingLevel SharingLevel { get; private set; }
    public Guid? SourceLogicId { get; private set; }
    public Guid? VehiclePhaseId { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public string ApprovalNotes { get; private set; }
    
    // 実行統計
    public int ExecutionCount { get; private set; }
    public DateTime? LastExecutedAt { get; private set; }
    public double? LastExecutionTimeMs { get; private set; }

    protected CanAnomalyDetectionLogic() { }

    public CanAnomalyDetectionLogic(
        Guid id,
        Guid? tenantId,
        DetectionLogicIdentity identity,
        DetectionLogicSpecification specification,
        SafetyClassification safety) : base(id)
    {
        TenantId = tenantId;
        Identity = identity ?? throw new ArgumentNullException(nameof(identity));
        Specification = specification ?? throw new ArgumentNullException(nameof(specification));
        Safety = safety ?? throw new ArgumentNullException(nameof(safety));
        Status = DetectionLogicStatus.Draft;
        SharingLevel = SharingLevel.Private;
        ExecutionCount = 0;
    }

    // ビジネスメソッド
    public void UpdateImplementation(LogicImplementation newImplementation)
    {
        if (newImplementation == null)
            throw new ArgumentNullException(nameof(newImplementation));
        
        if (Status == DetectionLogicStatus.Approved)
            throw new InvalidOperationException("Cannot update implementation of approved logic without creating new version");
            
        Implementation = newImplementation;
        
        // 実装更新時は統計をリセット
        ExecutionCount = 0;
        LastExecutedAt = null;
        LastExecutionTimeMs = null;
    }

    public void AddParameter(DetectionParameter parameter)
    {
        if (parameter == null)
            throw new ArgumentNullException(nameof(parameter));
        
        if (_parameters.Any(p => p.Name == parameter.Name))
            throw new InvalidOperationException($"Parameter '{parameter.Name}' already exists");
            
        _parameters.Add(parameter);
    }

    public void UpdateParameter(string parameterName, string newValue)
    {
        var parameter = _parameters.FirstOrDefault(p => p.Name == parameterName);
        if (parameter == null)
            throw new InvalidOperationException($"Parameter '{parameterName}' not found");
            
        parameter.UpdateValue(newValue);
    }

    public void RemoveParameter(string parameterName)
    {
        var parameter = _parameters.FirstOrDefault(p => p.Name == parameterName);
        if (parameter != null)
        {
            _parameters.Remove(parameter);
        }
    }

    public void AddSignalMapping(CanSignalMapping mapping)
    {
        if (mapping == null)
            throw new ArgumentNullException(nameof(mapping));
        
        if (_signalMappings.Any(m => m.CanSignalId == mapping.CanSignalId))
            throw new InvalidOperationException("Signal is already mapped to this logic");
            
        _signalMappings.Add(mapping);
    }

    public void RemoveSignalMapping(Guid canSignalId)
    {
        var mapping = _signalMappings.FirstOrDefault(m => m.CanSignalId == canSignalId);
        if (mapping != null)
        {
            _signalMappings.Remove(mapping);
        }
    }

    public void UpdateSharingLevel(SharingLevel newSharingLevel)
    {
        if (Status == DetectionLogicStatus.Approved && Safety.RequiresApproval())
        {
            throw new InvalidOperationException("Cannot change sharing level of approved safety-critical logic without re-approval");
        }
        
        SharingLevel = newSharingLevel;
    }

    public void SubmitForApproval()
    {
        if (Status != DetectionLogicStatus.Draft)
            throw new InvalidOperationException("Only draft logic can be submitted for approval");
            
        if (Implementation == null)
            throw new InvalidOperationException("Implementation is required for approval");
            
        if (!_signalMappings.Any())
            throw new InvalidOperationException("At least one signal mapping is required");
            
        ValidateRequiredParameters();
        
        Status = DetectionLogicStatus.PendingApproval;
    }

    public void Approve(Guid approvedBy, string notes = null)
    {
        if (Status != DetectionLogicStatus.PendingApproval)
            throw new InvalidOperationException("Only pending logic can be approved");
            
        if (Safety.AsilLevel >= AsilLevel.C)
        {
            ValidateHighAsilRequirements();
        }
        
        Status = DetectionLogicStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        ApprovedBy = approvedBy;
        ApprovalNotes = notes;
    }

    public void Reject(string reason)
    {
        if (Status != DetectionLogicStatus.PendingApproval)
            throw new InvalidOperationException("Only pending logic can be rejected");
            
        Status = DetectionLogicStatus.Rejected;
        ApprovalNotes = reason;
    }

    public void Deprecate(string reason)
    {
        Status = DetectionLogicStatus.Deprecated;
        ApprovalNotes = string.IsNullOrEmpty(ApprovalNotes) 
            ? $"Deprecated: {reason}" 
            : $"{ApprovalNotes}; Deprecated: {reason}";
    }

    public void RecordExecution(double executionTimeMs)
    {
        if (Status != DetectionLogicStatus.Approved)
            throw new InvalidOperationException("Only approved logic can be executed");
            
        ExecutionCount++;
        LastExecutedAt = DateTime.UtcNow;
        LastExecutionTimeMs = executionTimeMs;
    }

    public bool CanExecute()
    {
        return Status == DetectionLogicStatus.Approved && 
               Implementation != null && 
               Implementation.IsExecutable();
    }

    public bool RequiresApproval()
    {
        return Safety.RequiresApproval();
    }

    public bool IsSharedWith(SharingLevel requestedLevel)
    {
        return SharingLevel >= requestedLevel;
    }

    public DetectionParameter GetParameter(string name)
    {
        return _parameters.FirstOrDefault(p => p.Name == name);
    }

    public bool HasRequiredParameters()
    {
        return _parameters.Where(p => p.IsRequired).All(p => !string.IsNullOrEmpty(p.Value));
    }

    public List<string> GetMissingRequiredParameters()
    {
        return _parameters
            .Where(p => p.IsRequired && string.IsNullOrEmpty(p.Value))
            .Select(p => p.Name)
            .ToList();
    }

    public double GetAverageExecutionTime()
    {
        return LastExecutionTimeMs ?? 0;
    }

    private void ValidateRequiredParameters()
    {
        var missingParams = GetMissingRequiredParameters();
        if (missingParams.Any())
        {
            throw new InvalidOperationException($"Required parameters are missing: {string.Join(", ", missingParams)}");
        }
    }

    private void ValidateHighAsilRequirements()
    {
        if (string.IsNullOrEmpty(Safety.SafetyRequirementId))
            throw new InvalidOperationException("Safety requirement ID is required for ASIL C/D");
            
        if (string.IsNullOrEmpty(Safety.SafetyGoalId))
            throw new InvalidOperationException("Safety goal ID is required for ASIL C/D");
    }
}
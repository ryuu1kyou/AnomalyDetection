using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Values;

namespace AnomalyDetection.Projects;

public class ProjectMilestone : Entity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public DateTime DueDate { get; private set; }
    public MilestoneStatus Status { get; private set; }
    public DateTime? CompletedDate { get; private set; }
    public Guid? CompletedBy { get; private set; }
    public int DisplayOrder { get; private set; }
    public MilestoneConfiguration Configuration { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    protected ProjectMilestone() { }

    public ProjectMilestone(
        string name,
        DateTime dueDate,
        string description = null,
        int displayOrder = 0,
        MilestoneConfiguration configuration = null)
    {
        Name = ValidateName(name);
        Description = ValidateDescription(description);
        DueDate = dueDate;
        Status = MilestoneStatus.NotStarted;
        DisplayOrder = displayOrder;
        Configuration = configuration ?? new MilestoneConfiguration();
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateName(string newName)
    {
        Name = ValidateName(newName);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDescription(string newDescription)
    {
        Description = ValidateDescription(newDescription);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDueDate(DateTime newDueDate)
    {
        if (Status == MilestoneStatus.Completed)
            throw new InvalidOperationException("Cannot update due date of completed milestone");
            
        DueDate = newDueDate;
        
        // 期日が過ぎている場合はステータスを更新
        if (newDueDate < DateTime.UtcNow && Status == MilestoneStatus.InProgress)
        {
            Status = MilestoneStatus.Delayed;
        }
        
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(MilestoneStatus newStatus, Guid? completedBy = null)
    {
        var oldStatus = Status;
        Status = newStatus;
        
        switch (newStatus)
        {
            case MilestoneStatus.Completed:
                CompletedDate = DateTime.UtcNow;
                CompletedBy = completedBy;
                break;
                
            case MilestoneStatus.InProgress:
                if (DueDate < DateTime.UtcNow)
                {
                    Status = MilestoneStatus.Delayed;
                }
                break;
                
            case MilestoneStatus.NotStarted:
            case MilestoneStatus.Delayed:
            case MilestoneStatus.Cancelled:
                if (oldStatus == MilestoneStatus.Completed)
                {
                    CompletedDate = null;
                    CompletedBy = null;
                }
                break;
        }
        
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDisplayOrder(int newOrder)
    {
        DisplayOrder = newOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateConfiguration(MilestoneConfiguration newConfiguration)
    {
        Configuration = newConfiguration ?? throw new ArgumentNullException(nameof(newConfiguration));
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsOverdue()
    {
        return DueDate < DateTime.UtcNow && Status != MilestoneStatus.Completed;
    }

    public bool IsCompleted()
    {
        return Status == MilestoneStatus.Completed;
    }

    public bool IsActive()
    {
        return Status == MilestoneStatus.InProgress;
    }

    public TimeSpan? GetTimeToDeadline()
    {
        if (Status == MilestoneStatus.Completed)
            return null;
            
        var remaining = DueDate - DateTime.UtcNow;
        return remaining;
    }

    public TimeSpan? GetCompletionTime()
    {
        return CompletedDate?.Subtract(CreatedAt);
    }

    public bool IsCritical()
    {
        return Configuration.IsCritical;
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Milestone name cannot be null or empty", nameof(name));
            
        if (name.Length > 100)
            throw new ArgumentException("Milestone name cannot exceed 100 characters", nameof(name));
            
        return name.Trim();
    }

    private static string ValidateDescription(string description)
    {
        if (description != null && description.Length > 1000)
            throw new ArgumentException("Description cannot exceed 1000 characters", nameof(description));
            
        return description?.Trim();
    }

    public override object[] GetKeys()
    {
        return new object[] { Name };
    }
}

public class MilestoneConfiguration : ValueObject
{
    public bool IsCritical { get; private set; }
    public bool RequiresApproval { get; private set; }
    public List<string> Dependencies { get; private set; }
    public Dictionary<string, object> CustomProperties { get; private set; }

    protected MilestoneConfiguration() { }

    public MilestoneConfiguration(
        bool isCritical = false,
        bool requiresApproval = false,
        List<string> dependencies = null,
        Dictionary<string, object> customProperties = null)
    {
        IsCritical = isCritical;
        RequiresApproval = requiresApproval;
        Dependencies = dependencies ?? new List<string>();
        CustomProperties = customProperties ?? new Dictionary<string, object>();
    }

    public bool HasDependencies()
    {
        return Dependencies.Any();
    }

    public void AddDependency(string dependency)
    {
        if (!string.IsNullOrWhiteSpace(dependency) && !Dependencies.Contains(dependency))
        {
            Dependencies.Add(dependency);
        }
    }

    public void RemoveDependency(string dependency)
    {
        Dependencies.Remove(dependency);
    }

    public T GetCustomProperty<T>(string key)
    {
        if (CustomProperties.TryGetValue(key, out var value))
        {
            return (T)value;
        }
        return default(T);
    }

    public void SetCustomProperty(string key, object value)
    {
        CustomProperties[key] = value;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return IsCritical;
        yield return RequiresApproval;
        yield return string.Join(",", Dependencies);
        yield return string.Join(",", CustomProperties.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
    }
}
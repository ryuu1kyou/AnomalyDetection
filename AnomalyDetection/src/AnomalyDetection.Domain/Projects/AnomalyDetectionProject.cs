using System;
using System.Collections.Generic;
using System.Linq;
using AnomalyDetection.CanSignals;
using AnomalyDetection.MultiTenancy;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace AnomalyDetection.Projects;

public class AnomalyDetectionProject : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    
    // プロジェクト基本情報
    public string ProjectCode { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public ProjectStatus Status { get; private set; }
    
    // 車両情報
    public string VehicleModel { get; private set; }
    public string ModelYear { get; private set; }
    public CanSystemType PrimarySystem { get; private set; }
    public OemCode OemCode { get; private set; }
    
    // プロジェクト期間
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public DateTime? ActualEndDate { get; private set; }
    
    // プロジェクト管理
    public Guid ProjectManagerId { get; private set; }
    public ProjectConfiguration Configuration { get; private set; }
    
    // マイルストーンとメンバー
    private readonly List<ProjectMilestone> _milestones = new();
    private readonly List<ProjectMember> _members = new();
    
    public IReadOnlyList<ProjectMilestone> Milestones => _milestones.AsReadOnly();
    public IReadOnlyList<ProjectMember> Members => _members.AsReadOnly();
    
    // 進捗統計
    public double ProgressPercentage { get; private set; }
    public int TotalTasks { get; private set; }
    public int CompletedTasks { get; private set; }
    public DateTime? LastProgressUpdate { get; private set; }

    protected AnomalyDetectionProject() { }

    public AnomalyDetectionProject(
        Guid id,
        Guid? tenantId,
        string projectCode,
        string name,
        string vehicleModel,
        string modelYear,
        CanSystemType primarySystem,
        OemCode oemCode,
        Guid projectManagerId,
        DateTime startDate,
        DateTime? endDate = null,
        string description = null) : base(id)
    {
        TenantId = tenantId;
        ProjectCode = ValidateProjectCode(projectCode);
        Name = ValidateName(name);
        VehicleModel = ValidateVehicleModel(vehicleModel);
        ModelYear = ValidateModelYear(modelYear);
        PrimarySystem = primarySystem;
        OemCode = oemCode ?? throw new ArgumentNullException(nameof(oemCode));
        ProjectManagerId = projectManagerId;
        StartDate = startDate;
        EndDate = ValidateEndDate(startDate, endDate);
        Description = ValidateDescription(description);
        Status = ProjectStatus.Planning;
        Configuration = new ProjectConfiguration();
        ProgressPercentage = 0.0;
        TotalTasks = 0;
        CompletedTasks = 0;
    }

    // ビジネスメソッド
    public void UpdateBasicInfo(string name, string description = null)
    {
        Name = ValidateName(name);
        Description = ValidateDescription(description);
    }

    public void UpdateVehicleInfo(string vehicleModel, string modelYear, CanSystemType primarySystem)
    {
        if (Status == ProjectStatus.Completed)
            throw new InvalidOperationException("Cannot update vehicle info of completed project");
            
        VehicleModel = ValidateVehicleModel(vehicleModel);
        ModelYear = ValidateModelYear(modelYear);
        PrimarySystem = primarySystem;
    }

    public void UpdateSchedule(DateTime startDate, DateTime? endDate)
    {
        if (Status == ProjectStatus.Completed)
            throw new InvalidOperationException("Cannot update schedule of completed project");
            
        StartDate = startDate;
        EndDate = ValidateEndDate(startDate, endDate);
    }

    public void UpdateProjectManager(Guid newProjectManagerId)
    {
        ProjectManagerId = newProjectManagerId;
    }

    public void UpdateConfiguration(ProjectConfiguration newConfiguration)
    {
        Configuration = newConfiguration ?? throw new ArgumentNullException(nameof(newConfiguration));
    }

    public void StartProject()
    {
        if (Status != ProjectStatus.Planning)
            throw new InvalidOperationException("Only planning projects can be started");
            
        if (!_members.Any())
            throw new InvalidOperationException("Project must have at least one member to start");
            
        Status = ProjectStatus.Active;
    }

    public void PutOnHold(string reason)
    {
        if (Status != ProjectStatus.Active)
            throw new InvalidOperationException("Only active projects can be put on hold");
            
        Status = ProjectStatus.OnHold;
        Configuration.AddNote($"Put on hold: {reason}");
    }

    public void ResumeProject()
    {
        if (Status != ProjectStatus.OnHold)
            throw new InvalidOperationException("Only on-hold projects can be resumed");
            
        Status = ProjectStatus.Active;
        Configuration.AddNote("Project resumed");
    }

    public void CompleteProject()
    {
        if (Status != ProjectStatus.Active)
            throw new InvalidOperationException("Only active projects can be completed");
            
        Status = ProjectStatus.Completed;
        ActualEndDate = DateTime.UtcNow;
        ProgressPercentage = 100.0;
    }

    public void CancelProject(string reason)
    {
        if (Status == ProjectStatus.Completed)
            throw new InvalidOperationException("Cannot cancel completed project");
            
        Status = ProjectStatus.Cancelled;
        ActualEndDate = DateTime.UtcNow;
        Configuration.AddNote($"Project cancelled: {reason}");
    }

    // マイルストーン管理
    public void AddMilestone(ProjectMilestone milestone)
    {
        if (milestone == null)
            throw new ArgumentNullException(nameof(milestone));
            
        if (_milestones.Any(m => m.Name == milestone.Name))
            throw new InvalidOperationException($"Milestone '{milestone.Name}' already exists");
            
        _milestones.Add(milestone);
        RecalculateProgress();
    }

    public void UpdateMilestone(string milestoneName, DateTime? newDueDate = null, MilestoneStatus? newStatus = null)
    {
        var milestone = _milestones.FirstOrDefault(m => m.Name == milestoneName);
        if (milestone == null)
            throw new InvalidOperationException($"Milestone '{milestoneName}' not found");
            
        if (newDueDate.HasValue)
            milestone.UpdateDueDate(newDueDate.Value);
            
        if (newStatus.HasValue)
            milestone.UpdateStatus(newStatus.Value);
            
        RecalculateProgress();
    }

    public void RemoveMilestone(string milestoneName)
    {
        var milestone = _milestones.FirstOrDefault(m => m.Name == milestoneName);
        if (milestone != null)
        {
            _milestones.Remove(milestone);
            RecalculateProgress();
        }
    }

    // メンバー管理
    public void AddMember(ProjectMember member)
    {
        if (member == null)
            throw new ArgumentNullException(nameof(member));
            
        if (_members.Any(m => m.UserId == member.UserId))
            throw new InvalidOperationException("User is already a member of this project");
            
        _members.Add(member);
    }

    public void UpdateMemberRole(Guid userId, ProjectRole newRole)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member == null)
            throw new InvalidOperationException("User is not a member of this project");
            
        member.UpdateRole(newRole);
    }

    public void RemoveMember(Guid userId)
    {
        if (userId == ProjectManagerId)
            throw new InvalidOperationException("Cannot remove project manager from project");
            
        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member != null)
        {
            _members.Remove(member);
        }
    }

    // 進捗管理
    public void UpdateProgress(int totalTasks, int completedTasks)
    {
        TotalTasks = Math.Max(0, totalTasks);
        CompletedTasks = Math.Max(0, Math.Min(completedTasks, totalTasks));
        ProgressPercentage = TotalTasks > 0 ? (double)CompletedTasks / TotalTasks * 100.0 : 0.0;
        LastProgressUpdate = DateTime.UtcNow;
    }

    public void RecalculateProgress()
    {
        if (_milestones.Any())
        {
            var completedMilestones = _milestones.Count(m => m.Status == MilestoneStatus.Completed);
            ProgressPercentage = (double)completedMilestones / _milestones.Count * 100.0;
        }
        
        LastProgressUpdate = DateTime.UtcNow;
    }

    // クエリメソッド
    public bool IsActive()
    {
        return Status == ProjectStatus.Active;
    }

    public bool IsCompleted()
    {
        return Status == ProjectStatus.Completed;
    }

    public bool IsOverdue()
    {
        return EndDate.HasValue && DateTime.UtcNow > EndDate.Value && Status != ProjectStatus.Completed;
    }

    public TimeSpan? GetRemainingTime()
    {
        if (!EndDate.HasValue || Status == ProjectStatus.Completed)
            return null;
            
        var remaining = EndDate.Value - DateTime.UtcNow;
        return remaining.TotalDays > 0 ? remaining : TimeSpan.Zero;
    }

    public TimeSpan GetProjectDuration()
    {
        var endDate = ActualEndDate ?? DateTime.UtcNow;
        return endDate - StartDate;
    }

    public List<ProjectMilestone> GetOverdueMilestones()
    {
        return _milestones
            .Where(m => m.IsOverdue() && m.Status != MilestoneStatus.Completed)
            .ToList();
    }

    public List<ProjectMember> GetActiveMembers()
    {
        return _members.Where(m => m.IsActive).ToList();
    }

    public bool HasMember(Guid userId)
    {
        return _members.Any(m => m.UserId == userId && m.IsActive);
    }

    // バリデーションメソッド
    private static string ValidateProjectCode(string projectCode)
    {
        if (string.IsNullOrWhiteSpace(projectCode))
            throw new ArgumentException("Project code cannot be null or empty", nameof(projectCode));
            
        if (projectCode.Length > 20)
            throw new ArgumentException("Project code cannot exceed 20 characters", nameof(projectCode));
            
        if (!System.Text.RegularExpressions.Regex.IsMatch(projectCode, @"^[A-Z0-9_-]+$"))
            throw new ArgumentException("Project code must contain only uppercase letters, numbers, underscores, and hyphens", nameof(projectCode));
            
        return projectCode.Trim();
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name cannot be null or empty", nameof(name));
            
        if (name.Length > 200)
            throw new ArgumentException("Project name cannot exceed 200 characters", nameof(name));
            
        return name.Trim();
    }

    private static string ValidateVehicleModel(string vehicleModel)
    {
        if (string.IsNullOrWhiteSpace(vehicleModel))
            throw new ArgumentException("Vehicle model cannot be null or empty", nameof(vehicleModel));
            
        if (vehicleModel.Length > 100)
            throw new ArgumentException("Vehicle model cannot exceed 100 characters", nameof(vehicleModel));
            
        return vehicleModel.Trim();
    }

    private static string ValidateModelYear(string modelYear)
    {
        if (string.IsNullOrWhiteSpace(modelYear))
            throw new ArgumentException("Model year cannot be null or empty", nameof(modelYear));
            
        if (!System.Text.RegularExpressions.Regex.IsMatch(modelYear, @"^\d{4}$"))
            throw new ArgumentException("Model year must be a 4-digit year", nameof(modelYear));
            
        var year = int.Parse(modelYear);
        var currentYear = DateTime.Now.Year;
        
        if (year < currentYear - 10 || year > currentYear + 10)
            throw new ArgumentException("Model year must be within reasonable range", nameof(modelYear));
            
        return modelYear;
    }

    private static DateTime? ValidateEndDate(DateTime startDate, DateTime? endDate)
    {
        if (endDate.HasValue && endDate.Value <= startDate)
            throw new ArgumentException("End date must be after start date", nameof(endDate));
            
        return endDate;
    }

    private static string ValidateDescription(string description)
    {
        if (description != null && description.Length > 2000)
            throw new ArgumentException("Description cannot exceed 2000 characters", nameof(description));
            
        return description?.Trim();
    }
}
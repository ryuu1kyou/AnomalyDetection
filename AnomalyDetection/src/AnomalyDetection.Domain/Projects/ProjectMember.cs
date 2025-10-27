using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Values;

namespace AnomalyDetection.Projects;

public class ProjectMember : Entity
{
    public Guid UserId { get; private set; }
    public ProjectRole Role { get; private set; }
    public DateTime JoinedDate { get; private set; }
    public DateTime? LeftDate { get; private set; }
    public bool IsActive { get; private set; }
    public string Notes { get; private set; }
    public MemberConfiguration Configuration { get; private set; }

    protected ProjectMember() { }

    public ProjectMember(
        Guid userId,
        ProjectRole role,
        string notes = null,
        MemberConfiguration configuration = null)
    {
        UserId = userId;
        Role = role;
        JoinedDate = DateTime.UtcNow;
        IsActive = true;
        Notes = ValidateNotes(notes);
        Configuration = configuration ?? new MemberConfiguration();
    }

    public void UpdateRole(ProjectRole newRole)
    {
        Role = newRole;
    }

    public void UpdateNotes(string newNotes)
    {
        Notes = ValidateNotes(newNotes);
    }

    public void UpdateConfiguration(MemberConfiguration newConfiguration)
    {
        Configuration = newConfiguration ?? throw new ArgumentNullException(nameof(newConfiguration));
    }

    public void Leave()
    {
        IsActive = false;
        LeftDate = DateTime.UtcNow;
    }

    public void Rejoin()
    {
        IsActive = true;
        LeftDate = null;
        JoinedDate = DateTime.UtcNow;
    }

    public TimeSpan GetMembershipDuration()
    {
        var endDate = LeftDate ?? DateTime.UtcNow;
        return endDate - JoinedDate;
    }

    public bool IsManager()
    {
        return Role == ProjectRole.Manager;
    }

    public bool IsLeader()
    {
        return Role == ProjectRole.Owner || Role == ProjectRole.Manager;
    }

    public bool CanManageProject()
    {
        return Role == ProjectRole.Owner || Role == ProjectRole.Manager;
    }

    public bool CanEditDetectionLogics()
    {
        return Role == ProjectRole.Owner || 
               Role == ProjectRole.Manager || 
               Role == ProjectRole.Engineer;
    }

    private static string ValidateNotes(string notes)
    {
        if (notes != null && notes.Length > 500)
            throw new ArgumentException("Notes cannot exceed 500 characters", nameof(notes));
            
        return notes?.Trim();
    }

    public override object[] GetKeys()
    {
        return new object[] { UserId };
    }
}

public class MemberConfiguration : ValueObject
{
    public List<string> Permissions { get; private set; }
    public Dictionary<string, object> Settings { get; private set; }
    public bool CanReceiveNotifications { get; private set; }
    public bool CanAccessReports { get; private set; }

    protected MemberConfiguration() { }

    public MemberConfiguration(
        List<string> permissions = null,
        Dictionary<string, object> settings = null,
        bool canReceiveNotifications = true,
        bool canAccessReports = true)
    {
        Permissions = permissions ?? new List<string>();
        Settings = settings ?? new Dictionary<string, object>();
        CanReceiveNotifications = canReceiveNotifications;
        CanAccessReports = canAccessReports;
    }

    public bool HasPermission(string permission)
    {
        return Permissions.Contains(permission);
    }

    public void AddPermission(string permission)
    {
        if (!string.IsNullOrWhiteSpace(permission) && !Permissions.Contains(permission))
        {
            Permissions.Add(permission);
        }
    }

    public void RemovePermission(string permission)
    {
        Permissions.Remove(permission);
    }

    public T GetSetting<T>(string key)
    {
        if (Settings.TryGetValue(key, out var value))
        {
            return (T)value;
        }
        return default(T);
    }

    public void SetSetting(string key, object value)
    {
        Settings[key] = value;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return string.Join(",", Permissions);
        yield return string.Join(",", Settings.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
        yield return CanReceiveNotifications;
        yield return CanAccessReports;
    }
}
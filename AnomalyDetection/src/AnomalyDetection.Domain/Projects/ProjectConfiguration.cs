using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Domain.Values;

namespace AnomalyDetection.Projects;

public class ProjectConfiguration : ValueObject
{
    public ProjectPriority Priority { get; private set; }
    public bool IsConfidential { get; private set; }
    public List<string> Tags { get; private set; } = default!;
    public Dictionary<string, object> CustomSettings { get; private set; } = default!;
    public List<string> Notes { get; private set; } = default!;
    public NotificationSettings NotificationSettings { get; private set; } = default!;

    protected ProjectConfiguration() { }

    public ProjectConfiguration(
        ProjectPriority priority = ProjectPriority.Normal,
        bool isConfidential = false,
        List<string>? tags = null,
        Dictionary<string, object>? customSettings = null,
        NotificationSettings? notificationSettings = null)
    {
        Priority = priority;
        IsConfidential = isConfidential;
        Tags = tags ?? new List<string>();
        CustomSettings = customSettings ?? new Dictionary<string, object>();
        Notes = new List<string>();
        NotificationSettings = notificationSettings ?? new NotificationSettings();
    }

    public void UpdatePriority(ProjectPriority newPriority)
    {
        Priority = newPriority;
    }

    public void SetConfidential(bool isConfidential)
    {
        IsConfidential = isConfidential;
    }

    public void AddTag(string tag)
    {
        if (!string.IsNullOrWhiteSpace(tag) && !Tags.Contains(tag))
        {
            Tags.Add(tag.Trim());
        }
    }

    public void RemoveTag(string tag)
    {
        Tags.Remove(tag);
    }

    public bool HasTag(string tag)
    {
        return Tags.Contains(tag);
    }

    public void SetCustomSetting(string key, object value)
    {
        CustomSettings[key] = value;
    }

    public T? GetCustomSetting<T>(string key)
    {
        if (CustomSettings.TryGetValue(key, out var value))
        {
            return (T)value;
        }
        return default;
    }

    public void AddNote(string note)
    {
        if (!string.IsNullOrWhiteSpace(note))
        {
            var timestampedNote = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {note.Trim()}";
            Notes.Add(timestampedNote);
        }
    }

    public void UpdateNotificationSettings(NotificationSettings newSettings)
    {
        NotificationSettings = newSettings ?? throw new ArgumentNullException(nameof(newSettings));
    }

    public bool IsHighPriority()
    {
        return Priority >= ProjectPriority.High;
    }

    public bool IsCritical()
    {
        return Priority == ProjectPriority.Critical;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Priority;
        yield return IsConfidential;
        yield return string.Join(",", Tags);
        yield return string.Join(",", CustomSettings.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
        yield return string.Join("|", Notes);
        yield return NotificationSettings;
    }
}

public class NotificationSettings : ValueObject
{
    public bool EnableMilestoneNotifications { get; private set; }
    public bool EnableProgressNotifications { get; private set; }
    public bool EnableOverdueNotifications { get; private set; }
    public int NotificationFrequencyHours { get; private set; }
    public List<string> NotificationChannels { get; private set; } = default!;

    protected NotificationSettings() { }

    public NotificationSettings(
        bool enableMilestoneNotifications = true,
        bool enableProgressNotifications = true,
        bool enableOverdueNotifications = true,
        int notificationFrequencyHours = 24,
        List<string>? notificationChannels = null)
    {
        EnableMilestoneNotifications = enableMilestoneNotifications;
        EnableProgressNotifications = enableProgressNotifications;
        EnableOverdueNotifications = enableOverdueNotifications;
        NotificationFrequencyHours = ValidateFrequency(notificationFrequencyHours);
        NotificationChannels = notificationChannels ?? new List<string> { "Email" };
    }

    public bool ShouldNotify(string notificationType)
    {
        return notificationType switch
        {
            "Milestone" => EnableMilestoneNotifications,
            "Progress" => EnableProgressNotifications,
            "Overdue" => EnableOverdueNotifications,
            _ => false
        };
    }

    public bool HasChannel(string channel)
    {
        return NotificationChannels.Contains(channel);
    }

    private static int ValidateFrequency(int hours)
    {
        if (hours < 1 || hours > 168) // 1 hour to 1 week
            throw new ArgumentOutOfRangeException(nameof(hours), "Notification frequency must be between 1 and 168 hours");
            
        return hours;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return EnableMilestoneNotifications;
        yield return EnableProgressNotifications;
        yield return EnableOverdueNotifications;
        yield return NotificationFrequencyHours;
        yield return string.Join(",", NotificationChannels);
    }
}

public enum ProjectPriority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Critical = 4
}
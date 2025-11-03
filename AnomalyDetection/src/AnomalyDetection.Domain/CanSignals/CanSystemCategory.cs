using System;
using System.Collections.Generic;
using System.Linq;
using AnomalyDetection.MultiTenancy;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Domain.Values;
using Volo.Abp.MultiTenancy;

namespace AnomalyDetection.CanSignals;

public class CanSystemCategory : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }

    public CanSystemType SystemType { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Icon { get; private set; }
    public string? Color { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }

    // カテゴリ固有の設定
    public SystemCategoryConfiguration Configuration { get; private set; } = new();

    // 統計情報
    public int SignalCount { get; private set; }
    public int ActiveSignalCount { get; private set; }
    public DateTime? LastSignalUpdate { get; private set; }

    protected CanSystemCategory() { }

    public CanSystemCategory(
        Guid id,
        Guid? tenantId,
        CanSystemType systemType,
        string name,
    string? description = null,
    string? icon = null,
    string? color = null,
        int displayOrder = 0) : base(id)
    {
        TenantId = tenantId;
        SystemType = systemType;
        Name = ValidateName(name);
        Description = ValidateDescription(description);
        Icon = ValidateIcon(icon);
        Color = ValidateColor(color);
        DisplayOrder = displayOrder;
        IsActive = true;
        Configuration = new SystemCategoryConfiguration();
        SignalCount = 0;
        ActiveSignalCount = 0;
    }

    // ビジネスメソッド
    public void UpdateBasicInfo(string name, string? description = null)
    {
        Name = ValidateName(name);
        Description = ValidateDescription(description);
    }

    public void UpdateVisualSettings(string? icon = null, string? color = null)
    {
        Icon = ValidateIcon(icon);
        Color = ValidateColor(color);
    }

    public void UpdateDisplayOrder(int newOrder)
    {
        DisplayOrder = newOrder;
    }

    public void UpdateConfiguration(SystemCategoryConfiguration newConfiguration)
    {
        Configuration = newConfiguration ?? throw new ArgumentNullException(nameof(newConfiguration));
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        if (ActiveSignalCount > 0)
            throw new InvalidOperationException("Cannot deactivate category with active signals");

        IsActive = false;
    }

    public void UpdateSignalStatistics(int totalSignals, int activeSignals)
    {
        SignalCount = Math.Max(0, totalSignals);
        ActiveSignalCount = Math.Max(0, activeSignals);
        LastSignalUpdate = DateTime.UtcNow;

        if (ActiveSignalCount > SignalCount)
            throw new ArgumentException("Active signal count cannot exceed total signal count");
    }

    public void IncrementSignalCount()
    {
        SignalCount++;
        ActiveSignalCount++;
        LastSignalUpdate = DateTime.UtcNow;
    }

    public void DecrementSignalCount(bool wasActive = true)
    {
        if (SignalCount > 0)
        {
            SignalCount--;
            if (wasActive && ActiveSignalCount > 0)
            {
                ActiveSignalCount--;
            }
            LastSignalUpdate = DateTime.UtcNow;
        }
    }

    public bool HasSignals()
    {
        return SignalCount > 0;
    }

    public bool HasActiveSignals()
    {
        return ActiveSignalCount > 0;
    }

    public double GetActiveSignalRatio()
    {
        return SignalCount > 0 ? (double)ActiveSignalCount / SignalCount : 0.0;
    }

    public bool IsHighPriority()
    {
        return Configuration.Priority == SystemPriority.High ||
               Configuration.Priority == SystemPriority.Critical;
    }

    public bool IsSafetyRelevant()
    {
        return Configuration.IsSafetyRelevant;
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be null or empty", nameof(name));

        if (name.Length > 100)
            throw new ArgumentException("Category name cannot exceed 100 characters", nameof(name));

        return name.Trim();
    }

    private static string? ValidateDescription(string? description)
    {
        if (description != null && description.Length > 1000)
            throw new ArgumentException("Description cannot exceed 1000 characters", nameof(description));

        return description?.Trim();
    }

    private static string? ValidateIcon(string? icon)
    {
        if (icon != null && icon.Length > 100)
            throw new ArgumentException("Icon cannot exceed 100 characters", nameof(icon));

        return icon?.Trim();
    }

    private static string? ValidateColor(string? color)
    {
        if (color != null)
        {
            if (color.Length > 20)
                throw new ArgumentException("Color cannot exceed 20 characters", nameof(color));

            // 基本的な色形式の検証（#RRGGBB または色名）
            if (!string.IsNullOrEmpty(color) &&
                !System.Text.RegularExpressions.Regex.IsMatch(color, @"^(#[0-9A-Fa-f]{6}|[a-zA-Z]+)$"))
            {
                throw new ArgumentException("Color must be in hex format (#RRGGBB) or a valid color name", nameof(color));
            }
        }

        return color?.Trim();
    }
}

public class SystemCategoryConfiguration : ValueObject
{
    public SystemPriority Priority { get; private set; }
    public bool IsSafetyRelevant { get; private set; }
    public bool RequiresRealTimeMonitoring { get; private set; }
    public int DefaultTimeoutMs { get; private set; }
    public int MaxSignalsPerCategory { get; private set; }
    public Dictionary<string, object> CustomSettings { get; private set; } = new();

    protected SystemCategoryConfiguration() { }

    public SystemCategoryConfiguration(
        SystemPriority priority = SystemPriority.Normal,
        bool isSafetyRelevant = false,
        bool requiresRealTimeMonitoring = false,
        int defaultTimeoutMs = 1000,
        int maxSignalsPerCategory = 1000,
        Dictionary<string, object> customSettings = null)
    {
        Priority = priority;
        IsSafetyRelevant = isSafetyRelevant;
        RequiresRealTimeMonitoring = requiresRealTimeMonitoring;
        DefaultTimeoutMs = ValidateTimeout(defaultTimeoutMs);
        MaxSignalsPerCategory = ValidateMaxSignals(maxSignalsPerCategory);
        CustomSettings = customSettings ?? new Dictionary<string, object>();
    }

    public bool IsHighPriority()
    {
        return Priority >= SystemPriority.High;
    }

    public bool IsCritical()
    {
        return Priority == SystemPriority.Critical;
    }

    public T GetCustomSetting<T>(string key)
    {
        if (CustomSettings.TryGetValue(key, out var value))
        {
            return (T)value;
        }
        return default(T);
    }

    public void SetCustomSetting(string key, object value)
    {
        CustomSettings[key] = value;
    }

    private static int ValidateTimeout(int timeoutMs)
    {
        if (timeoutMs < 0)
            throw new ArgumentOutOfRangeException(nameof(timeoutMs), "Timeout cannot be negative");

        if (timeoutMs > 60000) // 60 seconds max
            throw new ArgumentOutOfRangeException(nameof(timeoutMs), "Timeout cannot exceed 60 seconds");

        return timeoutMs;
    }

    private static int ValidateMaxSignals(int maxSignals)
    {
        if (maxSignals < 1)
            throw new ArgumentOutOfRangeException(nameof(maxSignals), "Max signals must be at least 1");

        if (maxSignals > 10000)
            throw new ArgumentOutOfRangeException(nameof(maxSignals), "Max signals cannot exceed 10,000");

        return maxSignals;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Priority;
        yield return IsSafetyRelevant;
        yield return RequiresRealTimeMonitoring;
        yield return DefaultTimeoutMs;
        yield return MaxSignalsPerCategory;
        yield return string.Join(",", CustomSettings.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
    }
}

public enum SystemPriority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Critical = 4
}

// 定義済みのシステムカテゴリ設定
public static class SystemCategoryDefaults
{
    public static readonly Dictionary<CanSystemType, (string Name, string Description, string Icon, string Color, SystemPriority Priority, bool IsSafetyRelevant)> DefaultCategories =
        new Dictionary<CanSystemType, (string, string, string, string, SystemPriority, bool)>
        {
            { CanSystemType.Engine, ("エンジン", "エンジン制御システム", "engine", "#FF6B35", SystemPriority.High, true) },
            { CanSystemType.Brake, ("ブレーキ", "ブレーキ制御システム", "brake", "#DC143C", SystemPriority.Critical, true) },
            { CanSystemType.Steering, ("ステアリング", "ステアリング制御システム", "steering", "#4169E1", SystemPriority.Critical, true) },
            { CanSystemType.Transmission, ("トランスミッション", "変速機制御システム", "transmission", "#FF8C00", SystemPriority.High, false) },
            { CanSystemType.Body, ("ボディ", "ボディ制御システム", "body", "#32CD32", SystemPriority.Normal, false) },
            { CanSystemType.Chassis, ("シャーシ", "シャーシ制御システム", "chassis", "#8B4513", SystemPriority.High, false) },
            { CanSystemType.HVAC, ("空調", "空調制御システム", "hvac", "#00CED1", SystemPriority.Low, false) },
            { CanSystemType.Lighting, ("照明", "照明制御システム", "lighting", "#FFD700", SystemPriority.Normal, false) },
            { CanSystemType.Infotainment, ("インフォテインメント", "情報娯楽システム", "infotainment", "#9370DB", SystemPriority.Low, false) },
            { CanSystemType.Safety, ("安全", "安全制御システム", "safety", "#FF0000", SystemPriority.Critical, true) },
            { CanSystemType.Powertrain, ("パワートレイン", "パワートレイン制御システム", "powertrain", "#FF4500", SystemPriority.High, true) },
            { CanSystemType.Gateway, ("ゲートウェイ", "通信ゲートウェイ", "gateway", "#708090", SystemPriority.High, false) },
            { CanSystemType.Battery, ("バッテリー", "バッテリー管理システム", "battery", "#228B22", SystemPriority.Critical, true) },
            { CanSystemType.Motor, ("モーター", "モーター制御システム", "motor", "#4682B4", SystemPriority.High, true) },
            { CanSystemType.Inverter, ("インバーター", "インバーター制御システム", "inverter", "#B22222", SystemPriority.High, true) },
            { CanSystemType.Charger, ("充電器", "充電制御システム", "charger", "#20B2AA", SystemPriority.Normal, false) },
            { CanSystemType.ADAS, ("ADAS", "先進運転支援システム", "adas", "#6A5ACD", SystemPriority.Critical, true) },
            { CanSystemType.Suspension, ("サスペンション", "サスペンション制御システム", "suspension", "#CD853F", SystemPriority.Normal, false) },
            { CanSystemType.Exhaust, ("排気", "排気制御システム", "exhaust", "#A0522D", SystemPriority.Normal, false) },
            { CanSystemType.Fuel, ("燃料", "燃料制御システム", "fuel", "#DAA520", SystemPriority.High, false) }
        };
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Volo.Abp.AspNetCore.SignalR;

namespace AnomalyDetection.Hubs;

/// <summary>
/// Real-time detection hub for anomaly notifications
/// Supports project-based subscriptions and broadcasting
/// </summary>
public class RealTimeDetectionHub : AbpHub
{
    public async Task SubscribeToProject(Guid projectId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"project_{projectId}");
        await Clients.Caller.SendAsync("Subscribed", new
        {
            Type = "Project",
            ProjectId = projectId,
            ConnectionId = Context.ConnectionId,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task UnsubscribeFromProject(Guid projectId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"project_{projectId}");
        await Clients.Caller.SendAsync("Unsubscribed", new
        {
            Type = "Project",
            ProjectId = projectId,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task SubscribeToSignal(Guid signalId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"signal_{signalId}");
        await Clients.Caller.SendAsync("Subscribed", new
        {
            Type = "Signal",
            SignalId = signalId,
            ConnectionId = Context.ConnectionId,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task SubscribeToAll()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "all_detections");
        await Clients.Caller.SendAsync("Subscribed", new
        {
            Type = "All",
            ConnectionId = Context.ConnectionId,
            Timestamp = DateTime.UtcNow
        });
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        await Clients.Caller.SendAsync("Connected", new
        {
            ConnectionId = Context.ConnectionId,
            Timestamp = DateTime.UtcNow,
            Message = "Connected to AnomalyDetection Hub"
        });
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Clean up - automatic group removal by SignalR
        await base.OnDisconnectedAsync(exception);
    }

    // Server-side methods for broadcasting (called from application services)
    public async Task BroadcastAnomalyDetected(RealTimeDetectionMessage message)
    {
        // Broadcast to all subscribers
        await Clients.Group("all_detections").SendAsync("AnomalyDetected", message);

        // Broadcast to project-specific group
        if (message.ProjectId != Guid.Empty)
        {
            await Clients.Group($"project_{message.ProjectId}").SendAsync("AnomalyDetected", message);
        }

        // Broadcast to signal-specific group
        if (message.SignalId != Guid.Empty)
        {
            await Clients.Group($"signal_{message.SignalId}").SendAsync("AnomalyDetected", message);
        }
    }

    public async Task BroadcastDetectionProgress(DetectionProgressMessage message)
    {
        if (message.ProjectId != Guid.Empty)
        {
            await Clients.Group($"project_{message.ProjectId}").SendAsync("DetectionProgress", message);
        }
    }

    public async Task BroadcastSystemAlert(SystemAlertMessage message)
    {
        await Clients.All.SendAsync("SystemAlert", message);
    }
}

/// <summary>
/// Real-time anomaly detection message
/// </summary>
public class RealTimeDetectionMessage
{
    public Guid DetectionId { get; set; }
    public Guid ProjectId { get; set; }
    public Guid SignalId { get; set; }
    public string SignalName { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; }
    public string AnomalyLevel { get; set; } = string.Empty; // Info, Warning, Critical
    public string DetectionType { get; set; } = string.Empty; // Timeout, Range, Deviation, etc.
    public double? ActualValue { get; set; }
    public double? ExpectedValue { get; set; }
    public double? Deviation { get; set; }
    public string? Description { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Detection progress notification
/// </summary>
public class DetectionProgressMessage
{
    public Guid ProjectId { get; set; }
    public int TotalSignals { get; set; }
    public int ProcessedSignals { get; set; }
    public int AnomaliesDetected { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EstimatedCompletion { get; set; }
    public string Status { get; set; } = string.Empty; // Running, Completed, Failed
}

/// <summary>
/// System-wide alert message
/// </summary>
public class SystemAlertMessage
{
    public Guid AlertId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Severity { get; set; } = string.Empty; // Info, Warning, Error, Critical
    public string Category { get; set; } = string.Empty; // Performance, Security, System
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object>? Details { get; set; }
}

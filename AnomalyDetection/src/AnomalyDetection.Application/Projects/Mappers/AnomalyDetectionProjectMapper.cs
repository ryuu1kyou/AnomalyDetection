using System;
using System.Linq;
using AnomalyDetection.Projects.Dtos;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;
using Volo.Abp.ObjectMapping;

namespace AnomalyDetection.Projects.Mappers;

[Mapper]
public partial class AnomalyDetectionProjectMapper : MapperBase<AnomalyDetectionProject, AnomalyDetectionProjectDto>
{
    public override AnomalyDetectionProjectDto Map(AnomalyDetectionProject source)
    {
        if (source == null) return null!;

        var dto = new AnomalyDetectionProjectDto
        {
            Id = source.Id,
            TenantId = source.TenantId,
            ProjectCode = source.ProjectCode,
            Name = source.Name,
            Description = source.Description ?? string.Empty,
            Status = source.Status,
            VehicleModel = source.VehicleModel,
            ModelYear = source.ModelYear,
            PrimarySystem = source.PrimarySystem,
            OemCode = source.OemCode,
            StartDate = source.StartDate,
            EndDate = source.EndDate,
            ActualEndDate = source.ActualEndDate,
            ProjectManagerId = source.ProjectManagerId,
            ProjectManagerName = string.Empty, // Set separately by app service

            // Configuration
            AutoProgressTracking = false, // Not in domain model
            RequireApprovalForChanges = false, // Not in domain model
            CustomSettings = source.Configuration.CustomSettings,
            ConfigurationNotes = string.Join("\n", source.Configuration.Notes),

            // Progress
            ProgressPercentage = source.ProgressPercentage,
            TotalTasks = source.TotalTasks,
            CompletedTasks = source.CompletedTasks,
            LastProgressUpdate = source.LastProgressUpdate,

            // Calculated properties
            IsActive = source.IsActive(),
            IsCompleted = source.IsCompleted(),
            IsOverdue = source.IsOverdue(),
            RemainingTime = source.GetRemainingTime(),
            ProjectDuration = source.GetProjectDuration(),
            OverdueMilestonesCount = source.GetOverdueMilestones().Count,
            ActiveMembersCount = source.GetActiveMembers().Count,

            // Audit fields
            CreationTime = source.CreationTime,
            CreatorId = source.CreatorId,
            LastModificationTime = source.LastModificationTime,
            LastModifierId = source.LastModifierId,
            DeletionTime = source.DeletionTime,
            DeleterId = source.DeleterId,
            IsDeleted = source.IsDeleted
        };

        return dto;
    }

    public override void Map(AnomalyDetectionProject source, AnomalyDetectionProjectDto destination)
    {
        if (source == null || destination == null) return;

        destination.Id = source.Id;
        destination.TenantId = source.TenantId;
        destination.ProjectCode = source.ProjectCode;
        destination.Name = source.Name;
        destination.Description = source.Description ?? string.Empty;
        destination.Status = source.Status;
        destination.VehicleModel = source.VehicleModel;
        destination.ModelYear = source.ModelYear;
        destination.PrimarySystem = source.PrimarySystem;
        destination.OemCode = source.OemCode;
        destination.StartDate = source.StartDate;
        destination.EndDate = source.EndDate;
        destination.ActualEndDate = source.ActualEndDate;
        destination.ProjectManagerId = source.ProjectManagerId;

        // Configuration
        destination.AutoProgressTracking = false; // Not in domain model
        destination.RequireApprovalForChanges = false; // Not in domain model
        destination.CustomSettings = source.Configuration.CustomSettings;
        destination.ConfigurationNotes = string.Join("\n", source.Configuration.Notes);

        // Progress
        destination.ProgressPercentage = source.ProgressPercentage;
        destination.TotalTasks = source.TotalTasks;
        destination.CompletedTasks = source.CompletedTasks;
        destination.LastProgressUpdate = source.LastProgressUpdate;

        // Calculated properties
        destination.IsActive = source.IsActive();
        destination.IsCompleted = source.IsCompleted();
        destination.IsOverdue = source.IsOverdue();
        destination.RemainingTime = source.GetRemainingTime();
        destination.ProjectDuration = source.GetProjectDuration();
        destination.OverdueMilestonesCount = source.GetOverdueMilestones().Count;
        destination.ActiveMembersCount = source.GetActiveMembers().Count;

        // Audit fields
        destination.CreationTime = source.CreationTime;
        destination.CreatorId = source.CreatorId;
        destination.LastModificationTime = source.LastModificationTime;
        destination.LastModifierId = source.LastModifierId;
        destination.DeletionTime = source.DeletionTime;
        destination.DeleterId = source.DeleterId;
        destination.IsDeleted = source.IsDeleted;
    }
}

using System;
using AnomalyDetection.Projects.Dtos;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;
using Volo.Abp.ObjectMapping;

namespace AnomalyDetection.Projects.Mappers;

[Mapper]
public partial class ProjectMilestoneMapper : MapperBase<ProjectMilestone, ProjectMilestoneDto>
{
    public override ProjectMilestoneDto Map(ProjectMilestone source)
    {
        if (source == null) return null!;

        return new ProjectMilestoneDto
        {
            Name = source.Name,
            Description = source.Description ?? string.Empty,
            DueDate = source.DueDate,
            Status = source.Status,
            CompletedDate = source.CompletedDate,
            CompletedBy = source.CompletedBy,
            CompletedByUserName = string.Empty, // Set separately by app service
            DisplayOrder = source.DisplayOrder,

            // Configuration
            IsCritical = source.Configuration.IsCritical,
            RequiresApproval = source.Configuration.RequiresApproval,
            Dependencies = source.Configuration.Dependencies,
            CustomProperties = source.Configuration.CustomProperties,

            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt,

            // Calculated properties
            IsOverdue = source.IsOverdue(),
            IsCompleted = source.IsCompleted(),
            IsActive = source.IsActive(),
            TimeToDeadline = source.GetTimeToDeadline(),
            CompletionTime = source.GetCompletionTime()
        };
    }

    public override void Map(ProjectMilestone source, ProjectMilestoneDto destination)
    {
        if (source == null || destination == null) return;

        destination.Name = source.Name;
        destination.Description = source.Description ?? string.Empty;
        destination.DueDate = source.DueDate;
        destination.Status = source.Status;
        destination.CompletedDate = source.CompletedDate;
        destination.CompletedBy = source.CompletedBy;
        destination.DisplayOrder = source.DisplayOrder;

        // Configuration
        destination.IsCritical = source.Configuration.IsCritical;
        destination.RequiresApproval = source.Configuration.RequiresApproval;
        destination.Dependencies = source.Configuration.Dependencies;
        destination.CustomProperties = source.Configuration.CustomProperties;

        destination.CreatedAt = source.CreatedAt;
        destination.UpdatedAt = source.UpdatedAt;

        // Calculated properties
        destination.IsOverdue = source.IsOverdue();
        destination.IsCompleted = source.IsCompleted();
        destination.IsActive = source.IsActive();
        destination.TimeToDeadline = source.GetTimeToDeadline();
        destination.CompletionTime = source.GetCompletionTime();
    }
}

using System;
using AnomalyDetection.Projects.Dtos;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;
using Volo.Abp.ObjectMapping;

namespace AnomalyDetection.Projects.Mappers;

[Mapper]
public partial class ProjectMemberMapper : MapperBase<ProjectMember, ProjectMemberDto>
{
    public override ProjectMemberDto Map(ProjectMember source)
    {
        if (source == null) return null!;

        return new ProjectMemberDto
        {
            UserId = source.UserId,
            UserName = string.Empty, // Set separately by app service
            Email = string.Empty, // Set separately by app service
            Role = source.Role,
            JoinedDate = source.JoinedDate,
            LeftDate = source.LeftDate,
            IsActive = source.IsActive,
            Notes = source.Notes ?? string.Empty,

            // Configuration
            Permissions = source.Configuration.Permissions,
            Settings = source.Configuration.Settings,
            CanReceiveNotifications = source.Configuration.CanReceiveNotifications,
            CanAccessReports = source.Configuration.CanAccessReports,

            // Calculated properties
            MembershipDuration = source.GetMembershipDuration(),
            IsManager = source.IsManager(),
            IsLeader = source.IsLeader(),
            CanManageProject = source.CanManageProject(),
            CanEditDetectionLogics = source.CanEditDetectionLogics()
        };
    }

    public override void Map(ProjectMember source, ProjectMemberDto destination)
    {
        if (source == null || destination == null) return;

        destination.UserId = source.UserId;
        destination.Role = source.Role;
        destination.JoinedDate = source.JoinedDate;
        destination.LeftDate = source.LeftDate;
        destination.IsActive = source.IsActive;
        destination.Notes = source.Notes ?? string.Empty;

        // Configuration
        destination.Permissions = source.Configuration.Permissions;
        destination.Settings = source.Configuration.Settings;
        destination.CanReceiveNotifications = source.Configuration.CanReceiveNotifications;
        destination.CanAccessReports = source.Configuration.CanAccessReports;

        // Calculated properties
        destination.MembershipDuration = source.GetMembershipDuration();
        destination.IsManager = source.IsManager();
        destination.IsLeader = source.IsLeader();
        destination.CanManageProject = source.CanManageProject();
        destination.CanEditDetectionLogics = source.CanEditDetectionLogics();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnomalyDetection.Projects.Dtos;
using AnomalyDetection.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace AnomalyDetection.Projects;

[Authorize(AnomalyDetectionPermissions.Projects.Default)]
public class AnomalyDetectionProjectAppService : ApplicationService, IAnomalyDetectionProjectAppService
{
    private readonly IRepository<AnomalyDetectionProject, Guid> _projectRepository;

    public AnomalyDetectionProjectAppService(
        IRepository<AnomalyDetectionProject, Guid> projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<PagedResultDto<AnomalyDetectionProjectDto>> GetListAsync(GetProjectsInput input)
    {
        var queryable = await _projectRepository.GetQueryableAsync();

        // Apply filters
        if (!string.IsNullOrEmpty(input.Filter))
        {
            queryable = queryable.Where(x =>
                x.Name.Contains(input.Filter) ||
                x.ProjectCode.Contains(input.Filter) ||
                x.Description.Contains(input.Filter));
        }

        if (!string.IsNullOrEmpty(input.ProjectCode))
        {
            queryable = queryable.Where(x => x.ProjectCode.Contains(input.ProjectCode));
        }

        if (input.Status.HasValue)
        {
            queryable = queryable.Where(x => x.Status == input.Status.Value);
        }

        if (input.ProjectManagerId.HasValue)
        {
            queryable = queryable.Where(x => x.ProjectManagerId == input.ProjectManagerId.Value);
        }

        if (input.StartDateFrom.HasValue)
        {
            queryable = queryable.Where(x => x.StartDate >= input.StartDateFrom.Value);
        }

        if (input.StartDateTo.HasValue)
        {
            queryable = queryable.Where(x => x.StartDate <= input.StartDateTo.Value);
        }

        // Apply sorting with type specification
        if (!string.IsNullOrEmpty(input.Sorting))
        {
            queryable = ApplySorting(queryable, input.Sorting);
        }
        else
        {
            queryable = queryable.OrderByDescending(x => x.CreationTime);
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        var items = await AsyncExecuter.ToListAsync(
            queryable.Skip(input.SkipCount).Take(input.MaxResultCount));

        var dtos = ObjectMapper.Map<List<AnomalyDetectionProject>, List<AnomalyDetectionProjectDto>>(items);

        return new PagedResultDto<AnomalyDetectionProjectDto>(totalCount, dtos);
    }

    private static IQueryable<AnomalyDetectionProject> ApplySorting(
        IQueryable<AnomalyDetectionProject> queryable,
        string sorting)
    {
        if (string.IsNullOrWhiteSpace(sorting))
        {
            return queryable.OrderByDescending(x => x.CreationTime);
        }

        // Simple sorting - you may need to extend this for more complex cases
        var parts = sorting.Split(' ');
        var propertyName = parts[0];
        var isDescending = parts.Length > 1 && parts[1].Equals("DESC", StringComparison.OrdinalIgnoreCase);

        return propertyName.ToLower() switch
        {
            "name" => isDescending ? queryable.OrderByDescending(x => x.Name) : queryable.OrderBy(x => x.Name),
            "startdate" => isDescending ? queryable.OrderByDescending(x => x.StartDate) : queryable.OrderBy(x => x.StartDate),
            "enddate" => isDescending ? queryable.OrderByDescending(x => x.EndDate) : queryable.OrderBy(x => x.EndDate),
            "status" => isDescending ? queryable.OrderByDescending(x => x.Status) : queryable.OrderBy(x => x.Status),
            _ => queryable.OrderByDescending(x => x.CreationTime)
        };
    }

    public async Task<AnomalyDetectionProjectDto> GetAsync(Guid id)
    {
        var project = await _projectRepository.GetAsync(id);
        return ObjectMapper.Map<AnomalyDetectionProject, AnomalyDetectionProjectDto>(project);
    }

    [Authorize(AnomalyDetectionPermissions.Projects.Create)]
    public async Task<AnomalyDetectionProjectDto> CreateAsync(CreateProjectDto input)
    {
        // Check for project code conflicts
        var existingProject = await _projectRepository.FirstOrDefaultAsync(x =>
            x.ProjectCode == input.ProjectCode && x.TenantId == CurrentTenant.Id);

        if (existingProject != null)
        {
            throw new BusinessException("Project:DuplicateProjectCode")
                .WithData("ProjectCode", input.ProjectCode);
        }

        var project = ObjectMapper.Map<CreateProjectDto, AnomalyDetectionProject>(input);

        project = await _projectRepository.InsertAsync(project, autoSave: true);
        return ObjectMapper.Map<AnomalyDetectionProject, AnomalyDetectionProjectDto>(project);
    }

    [Authorize(AnomalyDetectionPermissions.Projects.Edit)]
    public async Task<AnomalyDetectionProjectDto> UpdateAsync(Guid id, UpdateProjectDto input)
    {
        var project = await _projectRepository.GetAsync(id);

        // ProjectCode is immutable and cannot be changed after creation
        // No need to check for conflicts

        ObjectMapper.Map(input, project);

        project = await _projectRepository.UpdateAsync(project, autoSave: true);
        return ObjectMapper.Map<AnomalyDetectionProject, AnomalyDetectionProjectDto>(project);
    }

    [Authorize(AnomalyDetectionPermissions.Projects.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var canDelete = await CanDeleteAsync(id);
        if (!canDelete)
        {
            throw new BusinessException("Project:CannotDelete")
                .WithData("Id", id);
        }

        await _projectRepository.DeleteAsync(id);
    }

    public async Task<bool> CanDeleteAsync(Guid id)
    {
        var project = await _projectRepository.GetAsync(id);
        // Check if project can be deleted based on status and dependencies
        return project.Status == ProjectStatus.Planning || project.Status == ProjectStatus.Cancelled;
    }

    public async Task<ListResultDto<AnomalyDetectionProjectDto>> GetByStatusAsync(ProjectStatus status)
    {
        var projects = await _projectRepository.GetListAsync(x => x.Status == status);
        var dtos = ObjectMapper.Map<List<AnomalyDetectionProject>, List<AnomalyDetectionProjectDto>>(projects);
        return new ListResultDto<AnomalyDetectionProjectDto>(dtos);
    }

    public async Task<ListResultDto<AnomalyDetectionProjectDto>> GetByProjectManagerAsync(Guid projectManagerId)
    {
        var projects = await _projectRepository.GetListAsync(x => x.ProjectManagerId == projectManagerId);
        var dtos = ObjectMapper.Map<List<AnomalyDetectionProject>, List<AnomalyDetectionProjectDto>>(projects);
        return new ListResultDto<AnomalyDetectionProjectDto>(dtos);
    }

    public async Task<ListResultDto<AnomalyDetectionProjectDto>> GetByMemberAsync(Guid userId)
    {
        var queryable = await _projectRepository.GetQueryableAsync();
        var projects = await AsyncExecuter.ToListAsync(
            queryable.Where(x => x.Members.Any(m => m.UserId == userId && m.IsActive)));

        var dtos = ObjectMapper.Map<List<AnomalyDetectionProject>, List<AnomalyDetectionProjectDto>>(projects);
        return new ListResultDto<AnomalyDetectionProjectDto>(dtos);
    }

    public async Task<ListResultDto<AnomalyDetectionProjectDto>> GetActiveProjectsAsync()
    {
        var projects = await _projectRepository.GetListAsync(x => x.Status == ProjectStatus.Active);
        var dtos = ObjectMapper.Map<List<AnomalyDetectionProject>, List<AnomalyDetectionProjectDto>>(projects);
        return new ListResultDto<AnomalyDetectionProjectDto>(dtos);
    }

    public async Task<ListResultDto<AnomalyDetectionProjectDto>> GetOverdueProjectsAsync()
    {
        var projects = await _projectRepository.GetListAsync(x =>
            x.EndDate.HasValue && x.EndDate.Value < DateTime.UtcNow &&
            x.Status != ProjectStatus.Completed && x.Status != ProjectStatus.Cancelled);

        var dtos = ObjectMapper.Map<List<AnomalyDetectionProject>, List<AnomalyDetectionProjectDto>>(projects);
        return new ListResultDto<AnomalyDetectionProjectDto>(dtos);
    }

    [Authorize(AnomalyDetectionPermissions.Projects.ManageStatus)]
    public async Task StartProjectAsync(Guid id, StartProjectDto input)
    {
        var project = await _projectRepository.GetAsync(id);
        project.StartProject();
        await _projectRepository.UpdateAsync(project, autoSave: true);
    }

    [Authorize(AnomalyDetectionPermissions.Projects.ManageStatus)]
    public async Task PutOnHoldAsync(Guid id, PutProjectOnHoldDto input)
    {
        var project = await _projectRepository.GetAsync(id);
        project.PutOnHold(input.Reason);
        await _projectRepository.UpdateAsync(project, autoSave: true);
    }

    [Authorize(AnomalyDetectionPermissions.Projects.ManageStatus)]
    public async Task ResumeProjectAsync(Guid id, ResumeProjectDto input)
    {
        var project = await _projectRepository.GetAsync(id);
        project.ResumeProject();
        await _projectRepository.UpdateAsync(project, autoSave: true);
    }

    [Authorize(AnomalyDetectionPermissions.Projects.ManageStatus)]
    public async Task CompleteProjectAsync(Guid id, CompleteProjectDto input)
    {
        var project = await _projectRepository.GetAsync(id);
        project.CompleteProject();
        await _projectRepository.UpdateAsync(project, autoSave: true);
    }

    [Authorize(AnomalyDetectionPermissions.Projects.ManageStatus)]
    public async Task CancelProjectAsync(Guid id, CancelProjectDto input)
    {
        var project = await _projectRepository.GetAsync(id);
        project.CancelProject(input.Reason);
        await _projectRepository.UpdateAsync(project, autoSave: true);
    }

    [Authorize(AnomalyDetectionPermissions.Projects.Edit)]
    public async Task UpdateProgressAsync(Guid id, UpdateProjectProgressDto input)
    {
        var project = await _projectRepository.GetAsync(id);
        project.UpdateProgress(input.TotalTasks, input.CompletedTasks);
        if (!string.IsNullOrEmpty(input.Notes))
        {
            project.Configuration.AddNote(input.Notes);
        }
        await _projectRepository.UpdateAsync(project, autoSave: true);
    }

    [Authorize(AnomalyDetectionPermissions.Projects.Edit)]
    public async Task RecalculateProgressAsync(Guid id)
    {
        var project = await _projectRepository.GetAsync(id);
        project.RecalculateProgress();
        await _projectRepository.UpdateAsync(project, autoSave: true);
    }

    // Milestone Management

    [Authorize(AnomalyDetectionPermissions.Projects.ManageMilestones)]
    public async Task<ProjectMilestoneDto> AddMilestoneAsync(Guid projectId, CreateProjectMilestoneDto input)
    {
        var project = await _projectRepository.GetAsync(projectId);
        var milestone = new ProjectMilestone(
            input.Name,
            input.DueDate,
            input.Description,
            input.DisplayOrder);

        project.AddMilestone(milestone);
        await _projectRepository.UpdateAsync(project, autoSave: true);

        return ObjectMapper.Map<ProjectMilestone, ProjectMilestoneDto>(milestone);
    }

    [Authorize(AnomalyDetectionPermissions.Projects.ManageMilestones)]
    public async Task<ProjectMilestoneDto> UpdateMilestoneAsync(Guid projectId, string milestoneName, UpdateProjectMilestoneDto input)
    {
        var project = await _projectRepository.GetAsync(projectId);
        project.UpdateMilestone(milestoneName, input.DueDate, input.Status);

        var milestone = project.Milestones.FirstOrDefault(m => m.Name == milestoneName);
        if (milestone == null)
            throw new Volo.Abp.BusinessException("Milestone not found");

        if (!string.IsNullOrEmpty(input.Description))
        {
            milestone.UpdateDescription(input.Description);
        }

        await _projectRepository.UpdateAsync(project, autoSave: true);
        return ObjectMapper.Map<ProjectMilestone, ProjectMilestoneDto>(milestone);
    }

    [Authorize(AnomalyDetectionPermissions.Projects.ManageMilestones)]
    public async Task RemoveMilestoneAsync(Guid projectId, string milestoneName)
    {
        var project = await _projectRepository.GetAsync(projectId);
        project.RemoveMilestone(milestoneName);
        await _projectRepository.UpdateAsync(project, autoSave: true);
    }

    public async Task<ListResultDto<ProjectMilestoneDto>> GetMilestonesAsync(Guid projectId)
    {
        var project = await _projectRepository.GetAsync(projectId);
        var dtos = ObjectMapper.Map<List<ProjectMilestone>, List<ProjectMilestoneDto>>(project.Milestones.ToList());
        return new ListResultDto<ProjectMilestoneDto>(dtos);
    }

    public async Task<ListResultDto<ProjectMilestoneDto>> GetOverdueMilestonesAsync(Guid projectId)
    {
        var project = await _projectRepository.GetAsync(projectId);
        var overdueMilestones = project.Milestones
            .Where(m => m.DueDate < DateTime.UtcNow && m.Status != MilestoneStatus.Completed)
            .ToList();

        var dtos = ObjectMapper.Map<List<ProjectMilestone>, List<ProjectMilestoneDto>>(overdueMilestones);
        return new ListResultDto<ProjectMilestoneDto>(dtos);
    }

    [Authorize(AnomalyDetectionPermissions.Projects.ManageMilestones)]
    public async Task CompleteMilestoneAsync(Guid projectId, string milestoneName)
    {
        var project = await _projectRepository.GetAsync(projectId);
        var milestone = project.Milestones.FirstOrDefault(m => m.Name == milestoneName);
        if (milestone == null)
            throw new Volo.Abp.BusinessException("Milestone not found");

        milestone.UpdateStatus(MilestoneStatus.Completed, CurrentUser.Id);
        project.RecalculateProgress();
        await _projectRepository.UpdateAsync(project, autoSave: true);
    }

    // Member Management

    [Authorize(AnomalyDetectionPermissions.Projects.ManageMembers)]
    public async Task<ProjectMemberDto> AddMemberAsync(Guid projectId, CreateProjectMemberDto input)
    {
        var project = await _projectRepository.GetAsync(projectId);
        var member = new ProjectMember(input.UserId, input.Role, input.Notes);
        project.AddMember(member);

        await _projectRepository.UpdateAsync(project, autoSave: true);
        return ObjectMapper.Map<ProjectMember, ProjectMemberDto>(member);
    }

    [Authorize(AnomalyDetectionPermissions.Projects.ManageMembers)]
    public async Task<ProjectMemberDto> UpdateMemberAsync(Guid projectId, Guid userId, UpdateProjectMemberDto input)
    {
        var project = await _projectRepository.GetAsync(projectId);
        project.UpdateMemberRole(userId, input.Role);

        var member = project.Members.FirstOrDefault(m => m.UserId == userId);
        if (member == null)
            throw new Volo.Abp.BusinessException("Member not found");

        if (!string.IsNullOrEmpty(input.Notes))
        {
            member.UpdateNotes(input.Notes);
        }

        await _projectRepository.UpdateAsync(project, autoSave: true);
        return ObjectMapper.Map<ProjectMember, ProjectMemberDto>(member);
    }

    [Authorize(AnomalyDetectionPermissions.Projects.ManageMembers)]
    public async Task RemoveMemberAsync(Guid projectId, Guid userId)
    {
        var project = await _projectRepository.GetAsync(projectId);
        project.RemoveMember(userId);
        await _projectRepository.UpdateAsync(project, autoSave: true);
    }

    public async Task<ListResultDto<ProjectMemberDto>> GetMembersAsync(Guid projectId)
    {
        var project = await _projectRepository.GetAsync(projectId);
        var dtos = ObjectMapper.Map<List<ProjectMember>, List<ProjectMemberDto>>(project.Members.ToList());
        return new ListResultDto<ProjectMemberDto>(dtos);
    }

    public async Task<ListResultDto<ProjectMemberDto>> GetActiveMembersAsync(Guid projectId)
    {
        var project = await _projectRepository.GetAsync(projectId);
        var activeMembers = project.Members.Where(m => m.IsActive).ToList();

        var dtos = ObjectMapper.Map<List<ProjectMember>, List<ProjectMemberDto>>(activeMembers);
        return new ListResultDto<ProjectMemberDto>(dtos);
    }

    [Authorize(AnomalyDetectionPermissions.Projects.ManageMembers)]
    public async Task UpdateMemberRoleAsync(Guid projectId, Guid userId, ProjectRole newRole)
    {
        var project = await _projectRepository.GetAsync(projectId);
        project.UpdateMemberRole(userId, newRole);
        await _projectRepository.UpdateAsync(project, autoSave: true);
    }

    // Reporting and Analytics

    [Authorize(AnomalyDetectionPermissions.Projects.ViewReports)]
    public async Task<Dictionary<string, object>> GetDashboardDataAsync(Guid id)
    {
        var project = await _projectRepository.GetAsync(id);

        var daysRemaining = project.EndDate.HasValue ? (project.EndDate.Value - DateTime.UtcNow).Days : (int?)null;
        var creatorName = project.CreatorId.HasValue ? await GetUserNameAsync(project.CreatorId.Value) : null;

        return new Dictionary<string, object>
        {
            ["ProjectId"] = project.Id,
            ["ProjectName"] = project.Name,
            ["Status"] = project.Status.ToString(),
            ["Progress"] = project.ProgressPercentage,
            ["TotalMilestones"] = project.Milestones.Count,
            ["CompletedMilestones"] = project.Milestones.Count(m => m.Status == MilestoneStatus.Completed),
            ["OverdueMilestones"] = project.Milestones.Count(m => m.DueDate < DateTime.UtcNow && m.Status != MilestoneStatus.Completed),
            ["TotalMembers"] = project.Members.Count,
            ["ActiveMembers"] = project.Members.Count(m => m.IsActive),
            ["DaysRemaining"] = daysRemaining!,
            ["IsOverdue"] = project.EndDate.HasValue && project.EndDate.Value < DateTime.UtcNow && project.Status != ProjectStatus.Completed,
            ["CreatorName"] = creatorName!
        };
    }

    private Task<string?> GetUserNameAsync(Guid userId)
    {
        // TODO: Implement user name lookup
        return Task.FromResult<string?>(null);
    }

    [Authorize(AnomalyDetectionPermissions.Projects.ViewReports)]
    public async Task<Dictionary<string, object>> GetStatisticsAsync(Guid id)
    {
        var project = await _projectRepository.GetAsync(id);

        var duration = project.EndDate.HasValue
            ? (project.EndDate.Value - project.StartDate).Days
            : (int?)null;
        var actualDuration = project.Status == ProjectStatus.Completed
            ? (DateTime.UtcNow - project.StartDate).Days
            : (int?)null;

        return new Dictionary<string, object>
        {
            ["CreatedDate"] = project.CreationTime,
            ["LastModifiedDate"] = project.LastModificationTime as object ?? DBNull.Value,
            ["Duration"] = duration!,
            ["ActualDuration"] = actualDuration!
        };
    }

    [Authorize(AnomalyDetectionPermissions.Projects.ViewReports)]
    public async Task<byte[]> GenerateReportAsync(Guid id, string format)
    {
        // TODO: Implement report generation
        await Task.CompletedTask;
        throw new NotImplementedException("Report generation will be implemented in a future version");
    }

    [Authorize(AnomalyDetectionPermissions.Projects.ViewReports)]
    public async Task<byte[]> ExportAsync(Guid id, string format)
    {
        // TODO: Implement export functionality
        await Task.CompletedTask;
        throw new NotImplementedException("Export functionality will be implemented in a future version");
    }

    public async Task<List<Dictionary<string, object>>> GetTimelineAsync(Guid id)
    {
        var project = await _projectRepository.GetAsync(id);

        var timeline = new List<Dictionary<string, object>>();

        // Add project creation
        timeline.Add(new Dictionary<string, object>
        {
            ["Date"] = project.CreationTime,
            ["Event"] = "Project Created",
            ["Description"] = $"Project '{project.Name}' was created"
        });

        // Add milestones
        foreach (var milestone in project.Milestones.OrderBy(m => m.DueDate))
        {
            timeline.Add(new Dictionary<string, object>
            {
                ["Date"] = milestone.DueDate,
                ["Event"] = "Milestone",
                ["Description"] = $"Milestone: {milestone.Name}",
                ["Status"] = milestone.Status.ToString()
            });
        }

        return timeline.OrderBy(t => (DateTime)t["Date"]).ToList();
    }

    public async Task<double> GetHealthScoreAsync(Guid id)
    {
        var project = await _projectRepository.GetAsync(id);

        double score = 100.0;

        // Deduct points for overdue milestones
        var overdueMilestones = project.Milestones.Count(m =>
            m.DueDate < DateTime.UtcNow && m.Status != MilestoneStatus.Completed);
        score -= overdueMilestones * 10;

        // Deduct points if project is overdue
        if (project.EndDate.HasValue && project.EndDate.Value < DateTime.UtcNow &&
            project.Status != ProjectStatus.Completed)
        {
            score -= 20;
        }

        // Deduct points based on progress vs time elapsed
        if (project.EndDate.HasValue)
        {
            var totalDuration = (project.EndDate.Value - project.StartDate).TotalDays;
            var elapsedDuration = (DateTime.UtcNow - project.StartDate).TotalDays;
            var expectedProgress = Math.Min(100, (elapsedDuration / totalDuration) * 100);
            var actualProgress = project.ProgressPercentage;

            if (actualProgress < expectedProgress)
            {
                score -= (expectedProgress - actualProgress) * 0.5;
            }
        }

        return Math.Max(0, Math.Min(100, score));
    }
}
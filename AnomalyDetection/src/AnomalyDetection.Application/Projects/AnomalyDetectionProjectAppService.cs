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

        // Apply sorting
        if (!string.IsNullOrEmpty(input.Sorting))
        {
            queryable = queryable.OrderBy(input.Sorting);
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
        
        // Check for project code conflicts (excluding current project)
        var existingProject = await _projectRepository.FirstOrDefaultAsync(x => 
            x.ProjectCode == input.ProjectCode && x.Id != id && x.TenantId == CurrentTenant.Id);
        
        if (existingProject != null)
        {
            throw new BusinessException("Project:DuplicateProjectCode")
                .WithData("ProjectCode", input.ProjectCode);
        }

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
        project.Start(input.ActualStartDate);
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
        project.Resume(input.ResumeDate);
        await _projectRepository.UpdateAsync(project, autoSave: true);
    }

    [Authorize(AnomalyDetectionPermissions.Projects.ManageStatus)]
    public async Task CompleteProjectAsync(Guid id, CompleteProjectDto input)
    {
        var project = await _projectRepository.GetAsync(id);
        project.Complete(input.CompletionDate, input.CompletionNotes);
        await _projectRepository.UpdateAsync(project, autoSave: true);
    }

    [Authorize(AnomalyDetectionPermissions.Projects.ManageStatus)]
    public async Task CancelProjectAsync(Guid id, CancelProjectDto input)
    {
        var project = await _projectRepository.GetAsync(id);
        project.Cancel(input.CancellationReason);
        await _projectRepository.UpdateAsync(project, autoSave: true);
    }

    [Authorize(AnomalyDetectionPermissions.Projects.Edit)]
    public async Task UpdateProgressAsync(Guid id, UpdateProjectProgressDto input)
    {
        var project = await _projectRepository.GetAsync(id);
        project.UpdateProgress(input.ProgressPercentage, input.ProgressNotes);
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
        var milestone = project.AddMilestone(input.Name, input.Description, input.DueDate, input.DisplayOrder);
        
        await _projectRepository.UpdateAsync(project, autoSave: true);
        return ObjectMapper.Map<ProjectMilestone, ProjectMilestoneDto>(milestone);
    }

    [Authorize(AnomalyDetectionPermissions.Projects.ManageMilestones)]
    public async Task<ProjectMilestoneDto> UpdateMilestoneAsync(Guid projectId, string milestoneName, UpdateProjectMilestoneDto input)
    {
        var project = await _projectRepository.GetAsync(projectId);
        var milestone = project.UpdateMilestone(milestoneName, input.Description, input.DueDate, input.DisplayOrder);
        
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
        project.CompleteMilestone(milestoneName, CurrentUser.Id);
        await _projectRepository.UpdateAsync(project, autoSave: true);
    }

    // Member Management

    [Authorize(AnomalyDetectionPermissions.Projects.ManageMembers)]
    public async Task<ProjectMemberDto> AddMemberAsync(Guid projectId, CreateProjectMemberDto input)
    {
        var project = await _projectRepository.GetAsync(projectId);
        var member = project.AddMember(input.UserId, input.Role, input.Notes);
        
        await _projectRepository.UpdateAsync(project, autoSave: true);
        return ObjectMapper.Map<ProjectMember, ProjectMemberDto>(member);
    }

    [Authorize(AnomalyDetectionPermissions.Projects.ManageMembers)]
    public async Task<ProjectMemberDto> UpdateMemberAsync(Guid projectId, Guid userId, UpdateProjectMemberDto input)
    {
        var project = await _projectRepository.GetAsync(projectId);
        var member = project.UpdateMember(userId, input.Role, input.Notes);
        
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
        
        return new Dictionary<string, object>
        {
            ["ProjectId"] = project.Id,
            ["ProjectName"] = project.Name,
            ["Status"] = project.Status.ToString(),
            ["Progress"] = project.GetProgressPercentage(),
            ["TotalMilestones"] = project.Milestones.Count,
            ["CompletedMilestones"] = project.Milestones.Count(m => m.Status == MilestoneStatus.Completed),
            ["OverdueMilestones"] = project.Milestones.Count(m => m.DueDate < DateTime.UtcNow && m.Status != MilestoneStatus.Completed),
            ["TotalMembers"] = project.Members.Count,
            ["ActiveMembers"] = project.Members.Count(m => m.IsActive),
            ["DaysRemaining"] = project.EndDate.HasValue ? (project.EndDate.Value - DateTime.UtcNow).Days : (int?)null,
            ["IsOverdue"] = project.EndDate.HasValue && project.EndDate.Value < DateTime.UtcNow && project.Status != ProjectStatus.Completed
        };
    }

    [Authorize(AnomalyDetectionPermissions.Projects.ViewReports)]
    public async Task<Dictionary<string, object>> GetStatisticsAsync(Guid id)
    {
        var project = await _projectRepository.GetAsync(id);
        
        return new Dictionary<string, object>
        {
            ["CreatedDate"] = project.CreationTime,
            ["LastModifiedDate"] = project.LastModificationTime,
            ["Duration"] = project.EndDate.HasValue && project.StartDate.HasValue 
                ? (project.EndDate.Value - project.StartDate.Value).Days 
                : (int?)null,
            ["ActualDuration"] = project.Status == ProjectStatus.Completed && project.StartDate.HasValue
                ? (DateTime.UtcNow - project.StartDate.Value).Days
                : (int?)null
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
        if (project.StartDate.HasValue && project.EndDate.HasValue)
        {
            var totalDuration = (project.EndDate.Value - project.StartDate.Value).TotalDays;
            var elapsedDuration = (DateTime.UtcNow - project.StartDate.Value).TotalDays;
            var expectedProgress = Math.Min(100, (elapsedDuration / totalDuration) * 100);
            var actualProgress = project.GetProgressPercentage();
            
            if (actualProgress < expectedProgress)
            {
                score -= (expectedProgress - actualProgress) * 0.5;
            }
        }
        
        return Math.Max(0, Math.Min(100, score));
    }
}
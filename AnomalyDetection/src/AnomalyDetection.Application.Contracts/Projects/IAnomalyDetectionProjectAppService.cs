using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnomalyDetection.Projects;
using AnomalyDetection.Projects.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace AnomalyDetection.Projects;

public interface IAnomalyDetectionProjectAppService : IApplicationService
{
    /// <summary>
    /// Get a paginated list of projects with filtering and sorting
    /// </summary>
    Task<PagedResultDto<AnomalyDetectionProjectDto>> GetListAsync(GetProjectsInput input);
    
    /// <summary>
    /// Get a specific project by ID
    /// </summary>
    Task<AnomalyDetectionProjectDto> GetAsync(Guid id);
    
    /// <summary>
    /// Create a new project
    /// </summary>
    Task<AnomalyDetectionProjectDto> CreateAsync(CreateProjectDto input);
    
    /// <summary>
    /// Update an existing project
    /// </summary>
    Task<AnomalyDetectionProjectDto> UpdateAsync(Guid id, UpdateProjectDto input);
    
    /// <summary>
    /// Delete a project
    /// </summary>
    Task DeleteAsync(Guid id);
    
    /// <summary>
    /// Check if a project can be deleted
    /// </summary>
    Task<bool> CanDeleteAsync(Guid id);
    
    /// <summary>
    /// Get projects by status
    /// </summary>
    Task<ListResultDto<AnomalyDetectionProjectDto>> GetByStatusAsync(ProjectStatus status);
    
    /// <summary>
    /// Get projects by project manager
    /// </summary>
    Task<ListResultDto<AnomalyDetectionProjectDto>> GetByProjectManagerAsync(Guid projectManagerId);
    
    /// <summary>
    /// Get projects where user is a member
    /// </summary>
    Task<ListResultDto<AnomalyDetectionProjectDto>> GetByMemberAsync(Guid userId);
    
    /// <summary>
    /// Get active projects
    /// </summary>
    Task<ListResultDto<AnomalyDetectionProjectDto>> GetActiveProjectsAsync();
    
    /// <summary>
    /// Get overdue projects
    /// </summary>
    Task<ListResultDto<AnomalyDetectionProjectDto>> GetOverdueProjectsAsync();
    
    /// <summary>
    /// Start a project
    /// </summary>
    Task StartProjectAsync(Guid id, StartProjectDto input);
    
    /// <summary>
    /// Put project on hold
    /// </summary>
    Task PutOnHoldAsync(Guid id, PutProjectOnHoldDto input);
    
    /// <summary>
    /// Resume a project
    /// </summary>
    Task ResumeProjectAsync(Guid id, ResumeProjectDto input);
    
    /// <summary>
    /// Complete a project
    /// </summary>
    Task CompleteProjectAsync(Guid id, CompleteProjectDto input);
    
    /// <summary>
    /// Cancel a project
    /// </summary>
    Task CancelProjectAsync(Guid id, CancelProjectDto input);
    
    /// <summary>
    /// Update project progress
    /// </summary>
    Task UpdateProgressAsync(Guid id, UpdateProjectProgressDto input);
    
    /// <summary>
    /// Recalculate project progress based on milestones
    /// </summary>
    Task RecalculateProgressAsync(Guid id);
    
    // Milestone Management
    
    /// <summary>
    /// Add milestone to project
    /// </summary>
    Task<ProjectMilestoneDto> AddMilestoneAsync(Guid projectId, CreateProjectMilestoneDto input);
    
    /// <summary>
    /// Update project milestone
    /// </summary>
    Task<ProjectMilestoneDto> UpdateMilestoneAsync(Guid projectId, string milestoneName, UpdateProjectMilestoneDto input);
    
    /// <summary>
    /// Remove milestone from project
    /// </summary>
    Task RemoveMilestoneAsync(Guid projectId, string milestoneName);
    
    /// <summary>
    /// Get project milestones
    /// </summary>
    Task<ListResultDto<ProjectMilestoneDto>> GetMilestonesAsync(Guid projectId);
    
    /// <summary>
    /// Get overdue milestones for project
    /// </summary>
    Task<ListResultDto<ProjectMilestoneDto>> GetOverdueMilestonesAsync(Guid projectId);
    
    /// <summary>
    /// Complete milestone
    /// </summary>
    Task CompleteMilestoneAsync(Guid projectId, string milestoneName);
    
    // Member Management
    
    /// <summary>
    /// Add member to project
    /// </summary>
    Task<ProjectMemberDto> AddMemberAsync(Guid projectId, CreateProjectMemberDto input);
    
    /// <summary>
    /// Update project member
    /// </summary>
    Task<ProjectMemberDto> UpdateMemberAsync(Guid projectId, Guid userId, UpdateProjectMemberDto input);
    
    /// <summary>
    /// Remove member from project
    /// </summary>
    Task RemoveMemberAsync(Guid projectId, Guid userId);
    
    /// <summary>
    /// Get project members
    /// </summary>
    Task<ListResultDto<ProjectMemberDto>> GetMembersAsync(Guid projectId);
    
    /// <summary>
    /// Get active project members
    /// </summary>
    Task<ListResultDto<ProjectMemberDto>> GetActiveMembersAsync(Guid projectId);
    
    /// <summary>
    /// Update member role
    /// </summary>
    Task UpdateMemberRoleAsync(Guid projectId, Guid userId, ProjectRole newRole);
    
    // Reporting and Analytics
    
    /// <summary>
    /// Get project dashboard data
    /// </summary>
    Task<Dictionary<string, object>> GetDashboardDataAsync(Guid id);
    
    /// <summary>
    /// Get project statistics
    /// </summary>
    Task<Dictionary<string, object>> GetStatisticsAsync(Guid id);
    
    /// <summary>
    /// Generate project report
    /// </summary>
    Task<byte[]> GenerateReportAsync(Guid id, string format);
    
    /// <summary>
    /// Export project data
    /// </summary>
    Task<byte[]> ExportAsync(Guid id, string format);
    
    /// <summary>
    /// Get project timeline
    /// </summary>
    Task<List<Dictionary<string, object>>> GetTimelineAsync(Guid id);
    
    /// <summary>
    /// Get project health score
    /// </summary>
    Task<double> GetHealthScoreAsync(Guid id);
}
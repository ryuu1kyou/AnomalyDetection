using System.ComponentModel.DataAnnotations;

namespace AnomalyDetection.Projects.Dtos;

public class StartProjectDto
{
    [StringLength(500)]
    public string Notes { get; set; } = string.Empty;
}

public class PutProjectOnHoldDto
{
    [Required]
    [StringLength(1000)]
    public string Reason { get; set; } = string.Empty;
}

public class ResumeProjectDto
{
    [StringLength(500)]
    public string Notes { get; set; } = string.Empty;
}

public class CompleteProjectDto
{
    [StringLength(1000)]
    public string CompletionNotes { get; set; } = string.Empty;
}

public class CancelProjectDto
{
    [Required]
    [StringLength(1000)]
    public string Reason { get; set; } = string.Empty;
}

public class UpdateProjectProgressDto
{
    public int TotalTasks { get; set; }
    
    public int CompletedTasks { get; set; }
    
    [StringLength(500)]
    public string Notes { get; set; } = string.Empty;
}
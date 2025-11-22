using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace AnomalyDetection.KnowledgeBase;

public interface IKnowledgeBaseStatisticsExportAppService : IApplicationService
{
    Task<KnowledgeBaseStatisticsDto> GetStatisticsAsync();
    Task<ExportedFileDto> ExportAsync(KnowledgeBaseStatisticsExportDto input);
}

public class KnowledgeBaseStatisticsExportDto
{
    public string Format { get; set; } = "csv"; // csv|json|pdf|excel
    public bool IncludePopularArticles { get; set; } = true;
    public bool IncludeTopTags { get; set; } = true;
}

// NOTE: Types KnowledgeBaseStatisticsDto & KnowledgeArticleSummaryDto already defined in IKnowledgeBaseAppService.cs
// Reuse those definitions to avoid duplicate type errors.

public class ExportedFileDto
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public int RecordCount { get; set; }
    public DateTime ExportedAt { get; set; }
    public string Format { get; set; } = string.Empty;
    public byte[] FileData { get; set; } = Array.Empty<byte>();
}
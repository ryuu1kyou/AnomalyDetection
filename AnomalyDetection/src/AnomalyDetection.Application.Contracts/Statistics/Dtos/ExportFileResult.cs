using System;

namespace AnomalyDetection.Statistics.Dtos;

/// <summary>
/// Result of an export operation containing file data and metadata
/// </summary>
public class ExportFileResult
{
    /// <summary>
    /// The file data as byte array
    /// </summary>
    public byte[] FileData { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Content type (MIME type) of the file
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Suggested file name
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Number of records exported
    /// </summary>
    public int RecordCount { get; set; }

    /// <summary>
    /// Timestamp when the export was generated
    /// </summary>
    public DateTime ExportedAt { get; set; }

    /// <summary>
    /// Export format (csv, pdf, excel, json)
    /// </summary>
    public string Format { get; set; } = string.Empty;
}

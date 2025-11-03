using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace AnomalyDetection.Shared.Export;

/// <summary>
/// Common export service for CSV/PDF generation across all domains
/// </summary>
public class ExportService : DomainService
{
    private static readonly ConcurrentQueue<ExportAuditRecord> AuditTrail = new();
    private const int MaxAuditRecords = 512;

    public enum ExportFormat
    {
        Csv,
        Pdf,
        Excel,
        Json
    }

    /// <summary>
    /// Export data to CSV format
    /// </summary>
    public Task<byte[]> ExportToCsvAsync(
        IEnumerable<object> data,
        CsvExportOptions? options = null)
    {
        options ??= new CsvExportOptions();

        var items = data?.Where(d => d != null).ToList() ?? new List<object>();
        if (items.Count == 0)
        {
            return Task.FromResult(Array.Empty<byte>());
        }

        var properties = ResolveExportProperties(items, options.IncludedProperties, options.ExcludedProperties);

        var sb = new StringBuilder();

        if (options.IncludeHeader)
        {
            var headers = properties.Select(p =>
                options.CustomHeaders != null && options.CustomHeaders.TryGetValue(p.Name, out var header)
                    ? header
                    : p.Name);

            sb.AppendLine(string.Join(options.Delimiter, headers.Select(h => EscapeCsvValue(h, options.Delimiter))));
        }

        foreach (var item in items)
        {
            var values = properties.Select(p =>
            {
                var value = p.Getter(item);
                return FormatCsvValue(value, options);
            });

            sb.AppendLine(string.Join(options.Delimiter, values.Select(v => EscapeCsvValue(v, options.Delimiter))));
        }

        return Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    /// <summary>
    /// Export data to JSON format
    /// </summary>
    public Task<byte[]> ExportToJsonAsync<T>(
        IEnumerable<T> data,
        JsonExportOptions? options = null) where T : class
    {
        options ??= new JsonExportOptions();

        var json = System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = options.Indented,
            PropertyNamingPolicy = options.CamelCase ? System.Text.Json.JsonNamingPolicy.CamelCase : null
        });

        return Task.FromResult(Encoding.UTF8.GetBytes(json));
    }

    private Task<byte[]> ExportToExcelAsync(
        IReadOnlyList<object> items,
        ExportDetectionRequest request)
    {
        var options = request.CsvOptions ?? new CsvExportOptions();
        var properties = ResolveExportProperties(items, options.IncludedProperties, options.ExcludedProperties);

        var includeHeader = request.ExcelOptions?.IncludeHeader ?? options.IncludeHeader;
        var enableAutoFilter = request.ExcelOptions?.EnableAutoFilter ?? false;

        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            WriteZipEntry(archive, "[Content_Types].xml", BuildContentTypesXml());
            WriteZipEntry(archive, "_rels/.rels", BuildRootRelationshipsXml());
            WriteZipEntry(archive, "xl/workbook.xml", BuildWorkbookXml());
            WriteZipEntry(archive, "xl/_rels/workbook.xml.rels", BuildWorkbookRelationshipsXml());
            WriteZipEntry(archive, "xl/worksheets/sheet1.xml", BuildWorksheetXml(properties, items, options, includeHeader, enableAutoFilter));
        }

        return Task.FromResult(stream.ToArray());
    }

    /// <summary>
    /// Export detection results with metadata
    /// </summary>
    public async Task<ExportResult> ExportDetectionResultsAsync(
        ExportDetectionRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var materialized = request.Results?.Where(r => r != null).ToList() ?? new List<object>();
        var format = request.Format;
        byte[] data;
        string contentType;
        string fileExtension;

        switch (format)
        {
            case ExportFormat.Csv:
                data = await ExportToCsvAsync(materialized, request.CsvOptions);
                contentType = "text/csv";
                fileExtension = "csv";
                break;

            case ExportFormat.Json:
                data = await ExportToJsonAsync(materialized, request.JsonOptions);
                contentType = "application/json";
                fileExtension = "json";
                break;

            case ExportFormat.Pdf:
                data = await ExportToPdfAsync(materialized, request);
                contentType = "application/pdf";
                fileExtension = "pdf";
                break;

            case ExportFormat.Excel:
                data = await ExportToExcelAsync(materialized, request);
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                fileExtension = "xlsx";
                break;

            default:
                throw new NotSupportedException($"Export format {format} is not supported");
        }

        var metadata = new ExportMetadata
        {
            RecordCount = materialized.Count,
            ExportedAt = DateTime.UtcNow,
            Format = format.ToString(),
            GeneratedBy = string.IsNullOrWhiteSpace(request.GeneratedBy) ? "System" : request.GeneratedBy!,
            AdditionalProperties = request.AdditionalMetadata != null
                ? new Dictionary<string, string>(request.AdditionalMetadata, StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, string>()
        };

        var result = new ExportResult
        {
            Data = data,
            ContentType = contentType,
            FileName = $"{request.FileNamePrefix}_{DateTime.Now:yyyyMMdd_HHmmss}.{fileExtension}",
            Metadata = metadata
        };

        RecordExportAudit(result);

        return result;
    }

    /// <summary>
    /// Export to PDF (basic implementation - can be enhanced with library like QuestPDF)
    /// </summary>
    private Task<byte[]> ExportToPdfAsync(
        IReadOnlyList<object> items,
        ExportDetectionRequest request)
    {
        var options = request.CsvOptions ?? new CsvExportOptions();
        var properties = ResolveExportProperties(items, options.IncludedProperties, options.ExcludedProperties);

        var lines = new List<string>
        {
            $"{request.FileNamePrefix} Export",
            $"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC",
            $"Total Records: {items.Count}"
        };

        if (request.AdditionalMetadata != null)
        {
            foreach (var kvp in request.AdditionalMetadata)
            {
                lines.Add($"{kvp.Key}: {kvp.Value}");
            }
        }

        lines.Add(string.Empty);

        if (properties.Count > 0)
        {
            lines.Add(string.Join(" | ", properties.Select(p => p.Name)));

            foreach (var item in items)
            {
                var formatted = properties
                    .Select(p => FormatCsvValue(p.Getter(item), options))
                    .ToList();
                lines.Add(string.Join(" | ", formatted));
            }
        }

        return Task.FromResult(BuildSimplePdf(lines));
    }

    private void RecordExportAudit(ExportResult result)
    {
        var entry = new ExportAuditRecord
        {
            TimestampUtc = DateTime.UtcNow,
            Format = result.Metadata.Format,
            GeneratedBy = result.Metadata.GeneratedBy,
            SizeBytes = result.Data?.LongLength ?? 0
        };

        AuditTrail.Enqueue(entry);

        while (AuditTrail.Count > MaxAuditRecords && AuditTrail.TryDequeue(out _))
        {
            // Trim queue to constant size to avoid unbounded growth.
        }
    }

    private static void WriteZipEntry(ZipArchive archive, string entryName, string content)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Fastest);
        using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
        writer.Write(content);
    }

    private static string BuildContentTypesXml() =>
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\"><Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/><Default Extension=\"xml\" ContentType=\"application/xml\"/><Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/><Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/></Types>";

    private static string BuildRootRelationshipsXml() =>
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/></Relationships>";

    private static string BuildWorkbookXml() =>
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?><workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\"><sheets><sheet name=\"Sheet1\" sheetId=\"1\" r:id=\"rId1\"/></sheets></workbook>";

    private static string BuildWorkbookRelationshipsXml() =>
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/></Relationships>";

    private static string BuildWorksheetXml(
        IReadOnlyList<ExportProperty> properties,
        IReadOnlyList<object> items,
        CsvExportOptions options,
        bool includeHeader,
        bool enableAutoFilter)
    {
        var sb = new StringBuilder();
        sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.Append("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData>");

        var currentRow = 1;

        if (includeHeader && properties.Count > 0)
        {
            sb.Append($"<row r=\"{currentRow}\">");
            for (var column = 0; column < properties.Count; column++)
            {
                var cellRef = GetExcelColumnName(column + 1) + currentRow;
                var value = SecurityElement.Escape(properties[column].Name) ?? string.Empty;
                sb.Append($"<c r=\"{cellRef}\" t=\"str\"><v>{value}</v></c>");
            }
            sb.Append("</row>");
            currentRow++;
        }

        foreach (var item in items)
        {
            sb.Append($"<row r=\"{currentRow}\">");

            for (var column = 0; column < properties.Count; column++)
            {
                var cellRef = GetExcelColumnName(column + 1) + currentRow;
                var rawValue = properties[column].Getter(item);
                var formattedValue = SecurityElement.Escape(FormatCsvValue(rawValue, options)) ?? string.Empty;
                sb.Append($"<c r=\"{cellRef}\" t=\"str\"><v>{formattedValue}</v></c>");
            }

            sb.Append("</row>");
            currentRow++;
        }

        var lastRow = Math.Max(currentRow - 1, includeHeader ? 1 : 0);

        if (enableAutoFilter && includeHeader && properties.Count > 0 && lastRow > 1)
        {
            var lastColumn = GetExcelColumnName(properties.Count);
            sb.Append($"</sheetData><autoFilter ref=\"A1:{lastColumn}{lastRow}\"/></worksheet>");
            return sb.ToString();
        }

        sb.Append("</sheetData></worksheet>");
        return sb.ToString();
    }

    private static string GetExcelColumnName(int index)
    {
        var dividend = index;
        var columnName = new StringBuilder();

        while (dividend > 0)
        {
            var modulo = (dividend - 1) % 26;
            columnName.Insert(0, (char)('A' + modulo));
            dividend = (dividend - modulo) / 26;
        }

        return columnName.ToString();
    }

    private byte[] BuildSimplePdf(IEnumerable<string> lines)
    {
        var sanitizedLines = (lines ?? Array.Empty<string>())
            .Select(line => line ?? string.Empty)
            .ToList();

        using var stream = new MemoryStream();
        var encoding = Encoding.ASCII;

        void Write(string text)
        {
            var bytes = encoding.GetBytes(text);
            stream.Write(bytes, 0, bytes.Length);
        }

        Write("%PDF-1.4\n");

        var offsets = new List<long> { 0 };

        offsets.Add(stream.Position);
        Write("1 0 obj<< /Type /Catalog /Pages 2 0 R >>endobj\n");

        offsets.Add(stream.Position);
        Write("2 0 obj<< /Type /Pages /Kids [3 0 R] /Count 1 >>endobj\n");

        offsets.Add(stream.Position);
        Write("3 0 obj<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>endobj\n");

        var contentStream = BuildPdfContent(sanitizedLines);
        offsets.Add(stream.Position);
        Write($"4 0 obj<< /Length {contentStream.Length} >>stream\n");
        Write(contentStream);
        Write("\nendstream\nendobj\n");

        offsets.Add(stream.Position);
        Write("5 0 obj<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>endobj\n");

        var startxref = stream.Position;

        Write("xref\n");
        Write($"0 {offsets.Count}\n");
        Write("0000000000 65535 f \n");

        for (var i = 1; i < offsets.Count; i++)
        {
            Write($"{offsets[i]:0000000000} 00000 n \n");
        }

        Write($"trailer<< /Size {offsets.Count} /Root 1 0 R >>\n");
        Write("startxref\n");
        Write($"{startxref}\n");
        Write("%%EOF");

        return stream.ToArray();
    }

    private static string BuildPdfContent(IReadOnlyList<string> lines)
    {
        var builder = new StringBuilder();
        builder.AppendLine("BT");
        builder.AppendLine("/F1 10 Tf");
        builder.AppendLine("1 0 0 1 50 780 Tm");

        foreach (var line in lines)
        {
            builder.AppendLine($"({EscapePdfText(line)}) Tj");
            builder.AppendLine("0 -14 Td");
        }

        builder.Append("ET");
        return builder.ToString();
    }

    private static string EscapePdfText(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        return text
            .Replace("\\", "\\\\")
            .Replace("(", "\\(")
            .Replace(")", "\\)")
            .Replace("\r", string.Empty)
            .Replace("\n", " \\n ");
    }

    /// <summary>
    /// Escape CSV value
    /// </summary>
    private string EscapeCsvValue(string value, string delimiter)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Contains(delimiter) || value.Contains("\"") || value.Contains("\n"))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }

    /// <summary>
    /// Format CSV value based on type
    /// </summary>
    private static string FormatCsvValue(object? value, CsvExportOptions options)
    {
        if (value == null)
            return string.Empty;

        if (value is DateTime dt)
            return dt.ToString(options.DateTimeFormat);

        if (value is DateTimeOffset dto)
            return dto.ToString(options.DateTimeFormat);

        if (value is decimal || value is double || value is float)
            return string.Format(options.NumberFormat, value);

        if (value is bool b)
            return b ? "Yes" : "No";

        return value.ToString() ?? string.Empty;
    }

    private static List<ExportProperty> ResolveExportProperties(
        IReadOnlyList<object> items,
        List<string>? included,
        List<string>? excluded)
    {
        var includeSet = included != null && included.Count > 0
            ? new HashSet<string>(included, StringComparer.OrdinalIgnoreCase)
            : null;

        var excludeSet = excluded != null && excluded.Count > 0
            ? new HashSet<string>(excluded, StringComparer.OrdinalIgnoreCase)
            : null;

        var first = items.FirstOrDefault(i => i != null);
        if (first == null)
        {
            return new List<ExportProperty>();
        }

        bool ShouldInclude(string name)
        {
            if (includeSet != null && !includeSet.Contains(name))
            {
                return false;
            }

            if (excludeSet != null && excludeSet.Contains(name))
            {
                return false;
            }

            return true;
        }

        if (first is IDictionary<string, object>)
        {
            var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in items)
            {
                if (item is IDictionary<string, object> dict)
                {
                    foreach (var key in dict.Keys)
                    {
                        if (ShouldInclude(key))
                        {
                            keys.Add(key);
                        }
                    }
                }
            }

            IEnumerable<string> orderedKeys;

            if (includeSet != null && included != null && included.Count > 0)
            {
                orderedKeys = included.Where(ShouldInclude);
            }
            else
            {
                orderedKeys = keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase);
            }

            return orderedKeys
                .Select(key => new ExportProperty(key, row =>
                {
                    if (row is IDictionary<string, object> map && map.TryGetValue(key, out var val))
                    {
                        return val;
                    }

                    return null;
                }))
                .ToList();
        }

        var sampleType = first.GetType();
        var propertyInfos = sampleType.GetProperties()
            .Where(p => p.CanRead)
            .Where(p => ShouldInclude(p.Name))
            .OrderBy(p => p.MetadataToken)
            .ToList();

        return propertyInfos
            .Select(p => new ExportProperty(p.Name, row => row == null ? null : p.GetValue(row)))
            .ToList();
    }

    /// <summary>
    /// Get export statistics
    /// </summary>
    public async Task<ExportStatistics> GetExportStatisticsAsync(DateTime fromDate, DateTime toDate)
    {
        var upperBound = toDate == DateTime.MinValue ? DateTime.MaxValue : toDate;

        var relevant = AuditTrail
            .Where(record => record.TimestampUtc >= fromDate && record.TimestampUtc <= upperBound)
            .ToList();

        var exportsByFormat = relevant
            .GroupBy(r => r.Format)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

        var exportsByUser = relevant
            .GroupBy(r => r.GeneratedBy)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

        var averageSize = relevant.Count > 0
            ? (long)relevant.Average(r => r.SizeBytes)
            : 0;

        var statistics = new ExportStatistics
        {
            TotalExports = relevant.Count,
            ExportsByFormat = exportsByFormat,
            ExportsByUser = exportsByUser,
            AverageExportSize = averageSize,
            PeriodStart = fromDate,
            PeriodEnd = toDate
        };

        return await Task.FromResult(statistics);
    }
}

internal sealed class ExportAuditRecord
{
    public DateTime TimestampUtc { get; init; }
    public string Format { get; init; } = string.Empty;
    public string GeneratedBy { get; init; } = string.Empty;
    public long SizeBytes { get; init; }
}

internal sealed class ExportProperty
{
    public ExportProperty(string name, Func<object, object?> getter)
    {
        Name = name;
        Getter = getter;
    }

    public string Name { get; }
    public Func<object, object?> Getter { get; }
}

/// <summary>
/// CSV export options
/// </summary>
public class CsvExportOptions
{
    public string Delimiter { get; set; } = ",";
    public bool IncludeHeader { get; set; } = true;
    public string DateTimeFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";
    public string NumberFormat { get; set; } = "0.##";
    public List<string>? IncludedProperties { get; set; }
    public List<string>? ExcludedProperties { get; set; }
    public Dictionary<string, string>? CustomHeaders { get; set; }
}

/// <summary>
/// JSON export options
/// </summary>
public class JsonExportOptions
{
    public bool Indented { get; set; } = true;
    public bool CamelCase { get; set; } = true;
}

/// <summary>
/// Export detection request
/// </summary>
public class ExportDetectionRequest
{
    public IEnumerable<object> Results { get; set; } = new List<object>();
    public ExportService.ExportFormat Format { get; set; }
    public string FileNamePrefix { get; set; } = "detection_results";
    public CsvExportOptions? CsvOptions { get; set; }
    public JsonExportOptions? JsonOptions { get; set; }
    public ExcelExportOptions? ExcelOptions { get; set; }
    public string GeneratedBy { get; set; } = "System";
    public Dictionary<string, string>? AdditionalMetadata { get; set; }
}

/// <summary>
/// Export result
/// </summary>
public class ExportResult
{
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public ExportMetadata Metadata { get; set; } = new();
}

/// <summary>
/// Export metadata
/// </summary>
public class ExportMetadata
{
    public int RecordCount { get; set; }
    public DateTime ExportedAt { get; set; }
    public string Format { get; set; } = string.Empty;
    public string GeneratedBy { get; set; } = string.Empty;
    public Dictionary<string, string> AdditionalProperties { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// Excel export options
/// </summary>
public class ExcelExportOptions
{
    public bool IncludeHeader { get; set; } = true;
    public bool EnableAutoFilter { get; set; } = false;
}

/// <summary>
/// Export statistics
/// </summary>
public class ExportStatistics
{
    public int TotalExports { get; set; }
    public Dictionary<string, int> ExportsByFormat { get; set; } = new();
    public Dictionary<string, int> ExportsByUser { get; set; } = new();
    public long AverageExportSize { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}

using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Values;

namespace AnomalyDetection.AnomalyDetection;

public class LogicImplementation : ValueObject
{
    public ImplementationType Type { get; private set; }
    public string Content { get; private set; } = default!;
    public string? Language { get; private set; }
    public string? EntryPoint { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }

    protected LogicImplementation() { }

    public LogicImplementation(
        ImplementationType type,
        string content,
        string? language = null,
        string? entryPoint = null,
        string? createdBy = null)
    {
        Type = type;
        Content = ValidateContent(content);
        Language = ValidateLanguage(language);
        EntryPoint = ValidateEntryPoint(entryPoint);
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
        
        ValidateImplementation();
    }

    public bool IsExecutable()
    {
        return Type == ImplementationType.Script || 
               Type == ImplementationType.CompiledCode;
    }

    public bool RequiresCompilation()
    {
        return Type == ImplementationType.SourceCode;
    }

    public bool IsConfigurationBased()
    {
        return Type == ImplementationType.Configuration;
    }

    public int GetContentSize()
    {
        return Content?.Length ?? 0;
    }

    public bool IsLargeImplementation()
    {
        return GetContentSize() > 10000; // 10KB
    }

    private static string ValidateContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Implementation content cannot be null or empty", nameof(content));
            
        if (content.Length > 1000000) // 1MB limit
            throw new ArgumentException("Implementation content cannot exceed 1MB", nameof(content));
            
        return content;
    }

    private static string? ValidateLanguage(string? language)
    {
        if (language != null && language.Length > 50)
            throw new ArgumentException("Language cannot exceed 50 characters", nameof(language));
            
        return language?.Trim();
    }

    private static string? ValidateEntryPoint(string? entryPoint)
    {
        if (entryPoint != null && entryPoint.Length > 200)
            throw new ArgumentException("Entry point cannot exceed 200 characters", nameof(entryPoint));
            
        return entryPoint?.Trim();
    }

    private void ValidateImplementation()
    {
        switch (Type)
        {
            case ImplementationType.Script:
            case ImplementationType.SourceCode:
                if (string.IsNullOrWhiteSpace(Language))
                    throw new ArgumentException("Language is required for script and source code implementations");
                break;
                
            case ImplementationType.CompiledCode:
                if (string.IsNullOrWhiteSpace(EntryPoint))
                    throw new ArgumentException("Entry point is required for compiled code implementations");
                break;
        }
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Type;
        yield return Content;
        yield return Language ?? string.Empty;
        yield return EntryPoint ?? string.Empty;
        yield return CreatedAt;
        yield return CreatedBy ?? string.Empty;
    }
}

public enum ImplementationType
{
    Configuration = 1,  // JSON/XML設定ベース
    Script = 2,         // スクリプト言語（Python、JavaScript等）
    SourceCode = 3,     // ソースコード（C#、C++等）
    CompiledCode = 4,   // コンパイル済みバイナリ
    Template = 5        // テンプレートベース
}
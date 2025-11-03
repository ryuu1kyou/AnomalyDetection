using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace AnomalyDetection.KnowledgeBase;

/// <summary>
/// Comment/feedback left on knowledge base articles.
/// </summary>
public class KnowledgeArticleComment : FullAuditedEntity<Guid>
{
    public Guid KnowledgeArticleId { get; private set; }
    public Guid? AuthorUserId { get; private set; }
    public string? AuthorName { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public int Rating { get; private set; }

    protected KnowledgeArticleComment()
    {
    }

    public KnowledgeArticleComment(
        Guid id,
        Guid knowledgeArticleId,
        string content,
        int rating,
        Guid? authorUserId = null,
        string? authorName = null) : base(id)
    {
        KnowledgeArticleId = knowledgeArticleId;
        AuthorUserId = authorUserId;
        AuthorName = authorName;
        Content = ValidateContent(content);
        Rating = NormalizeRating(rating);
    }

    public void UpdateContent(string content)
    {
        Content = ValidateContent(content);
    }

    public int UpdateRating(int rating)
    {
        var normalized = NormalizeRating(rating);
        Rating = normalized;
        return normalized;
    }

    private static string ValidateContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Comment content cannot be empty.", nameof(content));
        }

        if (content.Length > 2000)
        {
            throw new ArgumentException("Comment content cannot exceed 2000 characters.", nameof(content));
        }

        return content.Trim();
    }

    private static int NormalizeRating(int rating)
    {
        if (rating < 0)
        {
            rating = 0;
        }

        if (rating > 5)
        {
            rating = 5;
        }

        return rating;
    }
}

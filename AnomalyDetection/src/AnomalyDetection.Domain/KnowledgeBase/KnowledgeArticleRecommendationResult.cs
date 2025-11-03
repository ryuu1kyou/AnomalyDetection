namespace AnomalyDetection.KnowledgeBase;

/// <summary>
/// Recommendation result with relevance score.
/// </summary>
public sealed class KnowledgeArticleRecommendationResult
{
    public KnowledgeArticle Article { get; }
    public double Score { get; }

    public KnowledgeArticleRecommendationResult(KnowledgeArticle article, double score)
    {
        Article = article;
        Score = score;
    }
}

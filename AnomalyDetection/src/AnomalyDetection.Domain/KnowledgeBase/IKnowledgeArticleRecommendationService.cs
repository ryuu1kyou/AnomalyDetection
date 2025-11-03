using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnomalyDetection.KnowledgeBase;

/// <summary>
/// Provides recommendation results for knowledge base articles.
/// </summary>
public interface IKnowledgeArticleRecommendationService
{
    Task<List<KnowledgeArticleRecommendationResult>> GetRecommendationsAsync(KnowledgeArticleRecommendationContext context);
}

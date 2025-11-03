using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnomalyDetection.Application.Monitoring
{
    public interface IMonitoringService : IDisposable
    {
        /// <summary>
        /// 異常検出実行を追跡
        /// </summary>
        void TrackDetectionExecution(string logicName, string signalName, double executionTimeMs, bool success);

        /// <summary>
        /// API リクエストを追跡
        /// </summary>
        void TrackApiRequest(string endpoint, string method, int statusCode, double responseTimeMs);

        /// <summary>
        /// 例外を追跡
        /// </summary>
        void TrackException(Exception exception, string context, Dictionary<string, string>? properties = null);

        /// <summary>
        /// カスタムメトリクスを追跡
        /// </summary>
        void TrackCustomMetric(string metricName, double value, Dictionary<string, string>? properties = null);

        /// <summary>
        /// 依存関係を追跡
        /// </summary>
        void TrackDependency(string dependencyName, string commandName, DateTimeOffset startTime, TimeSpan duration, bool success);

        /// <summary>
        /// アクティブセッション数を更新
        /// </summary>
        void UpdateActiveSessions(int count);

        /// <summary>
        /// データベース接続数を更新
        /// </summary>
        void UpdateDatabaseConnections(int count);

        /// <summary>
        /// キャッシュヒット率を更新
        /// </summary>
        void UpdateCacheHitRate(double hitRate);

        /// <summary>
        /// ヘルスメトリクスを取得
        /// </summary>
        Task<Dictionary<string, object>> GetHealthMetricsAsync();

        /// <summary>
        /// ビジネスメトリクスを追跡
        /// </summary>
        void TrackBusinessMetric(string eventName, Dictionary<string, string>? properties = null, Dictionary<string, double>? metrics = null);

        /// <summary>
        /// リアルタイム配信メトリクスを追跡
        /// </summary>
        void TrackRealTimeDelivery(string changeType, string targetGroup, TimeSpan? processingLatency, bool success);
    }
}
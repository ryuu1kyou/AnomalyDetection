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

        /// <summary>
        /// SignalR のアクティブ接続数を更新
        /// </summary>
        void UpdateSignalRConnections(int count);

        /// <summary>
        /// 検出結果生成カウンタをインクリメント
        /// </summary>
        void TrackDetectionResultCreated(string detectionLogicId, string canSignalId, double latencyMs);

        /// <summary>
        /// ブロードキャスト失敗を記録
        /// </summary>
        void TrackBroadcastFailure(string changeType, string targetGroup, Exception ex);

        /// <summary>
        /// ASIL レベル変更を追跡 (旧/新レベルと再レビュー強制有無)。
        /// </summary>
        /// <param name="oldLevel">旧 ASIL (enum int)</param>
        /// <param name="newLevel">新 ASIL (enum int)</param>
        /// <param name="reReviewTriggered">再レビューが発生したか</param>
        void TrackAsilLevelChange(int oldLevel, int newLevel, bool reReviewTriggered);
    }
}
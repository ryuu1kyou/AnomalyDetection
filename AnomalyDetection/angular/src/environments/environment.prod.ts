import { Environment } from '@abp/ng.core';

// Production configuration
// These values should be replaced with actual production URLs during deployment
const baseUrl = 'https://app.anomalydetection.example.com';
const apiUrl = 'https://api.anomalydetection.example.com';

const oAuthConfig = {
  issuer: apiUrl + '/',
  redirectUri: baseUrl,
  clientId: 'AnomalyDetection_App',
  responseType: 'code',
  scope: 'offline_access AnomalyDetection',
  requireHttps: true,
  showDebugInformation: false,
  strictDiscoveryDocumentValidation: true,
  sessionChecksEnabled: true,
};

export const environment: Environment = {
  production: true,
  application: {
    baseUrl,
    name: 'CAN Anomaly Detection System',
    logoUrl: '/assets/images/logo.png',
  },
  oAuthConfig,
  apis: {
    default: {
      url: apiUrl,
      rootNamespace: 'AnomalyDetection',
    },
    AbpAccountPublic: {
      url: oAuthConfig.issuer,
      rootNamespace: 'AbpAccountPublic',
    },
  },
  signalR: {
    detectionHubUrl: apiUrl + '/signalr-hubs/detection',
  },
  remoteEnv: {
    url: '/getEnvConfig',
    mergeStrategy: 'deepmerge',
  },
  localization: {
    defaultResourceName: 'AnomalyDetection',
  },
  // Performance optimization settings
  performance: {
    enableLazyLoading: true,
    enableAOT: true,
    enableServiceWorker: true,
  },
  // Monitoring and analytics
  monitoring: {
    enableErrorTracking: true,
    enablePerformanceMonitoring: true,
    sampleRate: 0.1, // 10% sampling for performance monitoring
  },
  // Feature flags
  features: {
    enableAdvancedAnalytics: true,
    enableOemTraceability: true,
    enableSimilarComparison: true,
    enableRealtimeUpdates: true,
  },
  // Cache settings
  cache: {
    enableHttpCache: true,
    cacheMaxAge: 3600, // 1 hour
  },
};

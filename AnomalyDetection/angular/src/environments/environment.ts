import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

const oAuthConfig = {
  issuer: 'http://localhost:5000/',
  redirectUri: baseUrl,
  clientId: 'AnomalyDetection_App',
  responseType: 'code',
  scope: 'openid profile email roles offline_access AnomalyDetection',
  requireHttps: false,
  showDebugInformation: true, // Enable debug logs for OAuth flow
  strictDiscoveryDocumentValidation: false,
  skipIssuerCheck: true, // Skip issuer validation in development
};

export const environment: Environment = {
  production: false,
  application: {
    baseUrl,
    name: 'AnomalyDetection',
  },
  oAuthConfig,
  localization: { defaultResourceName: 'AnomalyDetection' },
  apis: {
    default: {
      url: 'http://localhost:5000',
      rootNamespace: 'AnomalyDetection',
    },
    AbpAccountPublic: {
      url: oAuthConfig.issuer,
      rootNamespace: 'AbpAccountPublic',
    },
  },
  signalR: {
    detectionHubUrl: 'http://localhost:5000/signalr-hubs/detection',
  },
};

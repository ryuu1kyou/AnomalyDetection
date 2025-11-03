import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

const oAuthConfig = {
  issuer: 'https://localhost:44318/',
  redirectUri: baseUrl,
  clientId: 'AnomalyDetection_App',
  responseType: 'code',
  // Include standard OIDC scopes required by ABP along with API scope.
  scope: 'openid profile email roles offline_access AnomalyDetection',
  requireHttps: true,
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
      url: 'https://localhost:44318',
      rootNamespace: 'AnomalyDetection',
    },
    AbpAccountPublic: {
      url: oAuthConfig.issuer,
      rootNamespace: 'AbpAccountPublic',
    },
  },
  signalR: {
    detectionHubUrl: 'https://localhost:44318/signalr-hubs/detection',
  },
};

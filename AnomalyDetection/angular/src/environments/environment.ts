import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

const oAuthConfig = {
  issuer: 'https://localhost:44318/',
  redirectUri: baseUrl,
  clientId: 'AnomalyDetection_App',
  responseType: 'code',
  scope: 'offline_access AnomalyDetection',
  requireHttps: true,
};

export const environment = {
  production: false,
  application: {
    baseUrl,
    name: 'AnomalyDetection',
  },
  oAuthConfig,
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
} as Environment;

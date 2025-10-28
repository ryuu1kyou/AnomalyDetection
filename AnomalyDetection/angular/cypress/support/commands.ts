/// <reference types="cypress" />

// Custom commands for CAN Anomaly Detection System

declare global {
  namespace Cypress {
    interface Chainable {
      /**
       * Custom command to login with username and password
       */
      login(username: string, password: string): Chainable<void>;
      
      /**
       * Custom command to switch tenant
       */
      switchTenant(tenantName: string): Chainable<void>;
      
      /**
       * Custom command to create a CAN signal
       */
      createCanSignal(signalData: any): Chainable<void>;
      
      /**
       * Custom command to create a detection logic
       */
      createDetectionLogic(logicData: any): Chainable<void>;
      
      /**
       * Custom command to wait for API response
       */
      waitForApi(alias: string): Chainable<void>;
      
      /**
       * Custom command to search similar signals
       */
      searchSimilarSignals(searchData: any): Chainable<void>;
      
      /**
       * Custom command to compare test data
       */
      compareTestData(comparisonData: any): Chainable<void>;
      
      /**
       * Custom command to analyze anomaly patterns
       */
      analyzeAnomalyPatterns(analysisData: any): Chainable<void>;
      
      /**
       * Custom command to generate threshold recommendations
       */
      generateThresholdRecommendations(recommendationData: any): Chainable<void>;
      
      /**
       * Custom command to calculate accuracy metrics
       */
      calculateAccuracyMetrics(metricsData: any): Chainable<void>;
    }
  }
}

// Login command
Cypress.Commands.add('login', (username: string, password: string) => {
  cy.visit('/account/login');
  cy.get('[data-cy=username]').type(username);
  cy.get('[data-cy=password]').type(password);
  cy.get('[data-cy=login-button]').click();
  cy.url().should('not.include', '/account/login');
});

// Switch tenant command
Cypress.Commands.add('switchTenant', (tenantName: string) => {
  cy.get('[data-cy=tenant-selector]').click();
  cy.get(`[data-cy=tenant-option-${tenantName}]`).click();
  cy.get('[data-cy=current-tenant]').should('contain', tenantName);
});

// Create CAN signal command
Cypress.Commands.add('createCanSignal', (signalData: any) => {
  cy.visit('/can-signals');
  cy.get('[data-cy=create-signal-button]').click();
  
  cy.get('[data-cy=signal-name]').type(signalData.signalName);
  cy.get('[data-cy=can-id]').type(signalData.canId);
  cy.get('[data-cy=description]').type(signalData.description);
  cy.get('[data-cy=system-type]').select(signalData.systemType);
  
  if (signalData.startBit !== undefined) {
    cy.get('[data-cy=start-bit]').clear().type(signalData.startBit.toString());
  }
  if (signalData.length !== undefined) {
    cy.get('[data-cy=length]').clear().type(signalData.length.toString());
  }
  if (signalData.minValue !== undefined) {
    cy.get('[data-cy=min-value]').clear().type(signalData.minValue.toString());
  }
  if (signalData.maxValue !== undefined) {
    cy.get('[data-cy=max-value]').clear().type(signalData.maxValue.toString());
  }
  
  cy.get('[data-cy=save-button]').click();
  cy.get('[data-cy=success-message]').should('be.visible');
});

// Create detection logic command
Cypress.Commands.add('createDetectionLogic', (logicData: any) => {
  cy.visit('/detection-logics');
  cy.get('[data-cy=create-logic-button]').click();
  
  cy.get('[data-cy=logic-name]').type(logicData.name);
  cy.get('[data-cy=logic-description]').type(logicData.description);
  cy.get('[data-cy=detection-type]').select(logicData.detectionType);
  cy.get('[data-cy=system-type]').select(logicData.systemType);
  
  if (logicData.logicContent) {
    cy.get('[data-cy=logic-content]').type(logicData.logicContent);
  }
  
  cy.get('[data-cy=save-button]').click();
  cy.get('[data-cy=success-message]').should('be.visible');
});

// Wait for API response command
Cypress.Commands.add('waitForApi', (alias: string) => {
  cy.wait(`@${alias}`).then((interception) => {
    expect(interception.response?.statusCode).to.be.oneOf([200, 201, 204]);
  });
});

// Search similar signals command
Cypress.Commands.add('searchSimilarSignals', (searchData: any) => {
  cy.get('[data-cy=reference-signal-selector]').click();
  cy.waitForApi('getCanSignals');
  cy.get('[data-cy=signal-option]').contains(searchData.referenceSignal).click();
  
  if (searchData.threshold) {
    cy.get('[data-cy=similarity-threshold]').clear().type(searchData.threshold.toString());
  }
  
  if (searchData.scope) {
    cy.get('[data-cy=search-scope]').select(searchData.scope);
  }
  
  cy.get('[data-cy=search-similar-signals-button]').click();
  cy.waitForApi('searchSimilarSignals');
});

// Compare test data command
Cypress.Commands.add('compareTestData', (comparisonData: any) => {
  cy.get('[data-cy=baseline-signal-selector]').click();
  cy.waitForApi('getCanSignals');
  cy.get('[data-cy=signal-option]').contains(comparisonData.baselineSignal).click();
  
  cy.get('[data-cy=comparison-signal-selector]').click();
  cy.get('[data-cy=signal-option]').contains(comparisonData.comparisonSignal).click();
  
  if (comparisonData.timeRange) {
    cy.get('[data-cy=comparison-time-range]').select(comparisonData.timeRange);
  }
  
  cy.get('[data-cy=compare-test-data-button]').click();
  cy.waitForApi('compareTestData');
});

// Analyze anomaly patterns command
Cypress.Commands.add('analyzeAnomalyPatterns', (analysisData: any) => {
  if (analysisData.timeRange) {
    cy.get('[data-cy=analysis-time-range]').select(analysisData.timeRange);
  }
  
  if (analysisData.scope) {
    cy.get('[data-cy=analysis-scope]').select(analysisData.scope);
  }
  
  cy.get('[data-cy=analyze-patterns-button]').click();
  cy.waitForApi('analyzePattern');
});

// Generate threshold recommendations command
Cypress.Commands.add('generateThresholdRecommendations', (recommendationData: any) => {
  cy.get('[data-cy=target-logic-selector]').click();
  cy.waitForApi('getDetectionLogics');
  cy.get('[data-cy=logic-option]').contains(recommendationData.logicId).click();
  
  if (recommendationData.period) {
    cy.get('[data-cy=analysis-period]').select(recommendationData.period);
  }
  
  if (recommendationData.goal) {
    cy.get('[data-cy=optimization-goal]').select(recommendationData.goal);
  }
  
  cy.get('[data-cy=generate-recommendations-button]').click();
  cy.waitForApi('getThresholdRecommendations');
});

// Calculate accuracy metrics command
Cypress.Commands.add('calculateAccuracyMetrics', (metricsData: any) => {
  if (metricsData.timeRange) {
    cy.get('[data-cy=metrics-time-range]').select(metricsData.timeRange);
  }
  
  if (metricsData.scope) {
    cy.get('[data-cy=metrics-scope]').select(metricsData.scope);
  }
  
  cy.get('[data-cy=calculate-accuracy-button]').click();
  cy.waitForApi('getDetectionAccuracy');
});

export {};
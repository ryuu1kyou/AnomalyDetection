describe('Similar Pattern Search Flow', () => {
  beforeEach(() => {
    // Intercept API calls for Similar Pattern Search
    cy.intercept('POST', '/api/app/similar-pattern-search/search-signals', { fixture: 'similar-signals-result.json' }).as('searchSimilarSignals');
    cy.intercept('POST', '/api/app/similar-pattern-search/compare-test-data', { fixture: 'test-data-comparison.json' }).as('compareTestData');
    cy.intercept('POST', '/api/app/similar-pattern-search/calculate-similarity', { fixture: 'similarity-calculation.json' }).as('calculateSimilarity');
    cy.intercept('GET', '/api/app/can-signal*', { fixture: 'can-signals.json' }).as('getCanSignals');
    cy.intercept('GET', '/api/app/detection-logic*', { fixture: 'detection-logics.json' }).as('getDetectionLogics');
    cy.intercept('GET', '/api/app/anomaly-detection-result*', { fixture: 'detection-results.json' }).as('getDetectionResults');
    
    // Login as detection engineer
    cy.login('engineer', 'engineer123');
    
    // Switch to Toyota tenant
    cy.switchTenant('Toyota');
  });

  describe('Similar Signal Search', () => {
    it('should display similar signal search interface', () => {
      cy.visit('/similar-pattern-search');
      
      // Check if search interface is displayed
      cy.get('[data-cy=similar-search-container]').should('be.visible');
      cy.get('[data-cy=search-criteria-form]').should('be.visible');
      cy.get('[data-cy=search-results-section]').should('be.visible');
      
      // Check search criteria fields
      cy.get('[data-cy=reference-signal-selector]').should('be.visible');
      cy.get('[data-cy=similarity-threshold]').should('be.visible');
      cy.get('[data-cy=search-scope]').should('be.visible');
      cy.get('[data-cy=comparison-attributes]').should('be.visible');
    });

    it('should search for similar CAN signals', () => {
      cy.visit('/similar-pattern-search');
      
      // Select reference signal
      cy.get('[data-cy=reference-signal-selector]').click();
      cy.waitForApi('getCanSignals');
      cy.get('[data-cy=signal-option]').first().click();
      
      // Set similarity threshold
      cy.get('[data-cy=similarity-threshold]').clear().type('0.7');
      
      // Select search scope
      cy.get('[data-cy=search-scope]').select('Industry');
      
      // Select comparison attributes
      cy.get('[data-cy=compare-can-id]').check();
      cy.get('[data-cy=compare-signal-name]').check();
      cy.get('[data-cy=compare-system-type]').check();
      cy.get('[data-cy=compare-value-range]').check();
      cy.get('[data-cy=compare-data-length]').check();
      
      // Execute search
      cy.get('[data-cy=search-similar-signals-button]').click();
      cy.waitForApi('searchSimilarSignals');
      
      // Verify search results
      cy.get('[data-cy=search-results]').should('be.visible');
      cy.get('[data-cy=similar-signal-result]').should('have.length.greaterThan', 0);
      
      // Check result details
      cy.get('[data-cy=similar-signal-result]').first().within(() => {
        cy.get('[data-cy=signal-name]').should('be.visible');
        cy.get('[data-cy=similarity-score]').should('be.visible');
        cy.get('[data-cy=recommendation-level]').should('be.visible');
        cy.get('[data-cy=matched-attributes]').should('be.visible');
      });
    });

    it('should display detailed similarity breakdown', () => {
      cy.visit('/similar-pattern-search');
      
      // Perform search first
      cy.searchSimilarSignals({
        referenceSignal: 'EngineRPM',
        threshold: 0.8,
        scope: 'OEM'
      });
      
      // Click on first result to view details
      cy.get('[data-cy=view-similarity-details-button]').first().click();
      
      // Verify similarity breakdown modal
      cy.get('[data-cy=similarity-breakdown-modal]').should('be.visible');
      cy.get('[data-cy=overall-similarity-score]').should('be.visible');
      
      // Check individual attribute scores
      cy.get('[data-cy=can-id-similarity]').should('be.visible');
      cy.get('[data-cy=signal-name-similarity]').should('be.visible');
      cy.get('[data-cy=system-type-similarity]').should('be.visible');
      cy.get('[data-cy=value-range-similarity]').should('be.visible');
      cy.get('[data-cy=data-length-similarity]').should('be.visible');
      cy.get('[data-cy=cycle-time-similarity]').should('be.visible');
      
      // Check attribute differences
      cy.get('[data-cy=attribute-differences]').should('be.visible');
      cy.get('[data-cy=matched-attributes-list]').should('be.visible');
      cy.get('[data-cy=different-attributes-list]').should('be.visible');
      
      // Check recommendation details
      cy.get('[data-cy=recommendation-explanation]').should('be.visible');
      cy.get('[data-cy=usage-suggestions]').should('be.visible');
    });

    it('should filter search results by recommendation level', () => {
      cy.visit('/similar-pattern-search');
      
      // Perform search
      cy.searchSimilarSignals({
        referenceSignal: 'EngineRPM',
        threshold: 0.5,
        scope: 'Industry'
      });
      
      // Filter by Highly Recommended
      cy.get('[data-cy=recommendation-filter]').select('Highly');
      
      // Verify all results are highly recommended
      cy.get('[data-cy=recommendation-level]').each($element => {
        cy.wrap($element).should('contain', 'Highly');
      });
      
      // Filter by High
      cy.get('[data-cy=recommendation-filter]').select('High');
      
      // Verify all results are high recommendation
      cy.get('[data-cy=recommendation-level]').each($element => {
        cy.wrap($element).should('contain', 'High');
      });
      
      // Reset filter
      cy.get('[data-cy=reset-recommendation-filter]').click();
    });

    it('should sort search results by similarity score', () => {
      cy.visit('/similar-pattern-search');
      
      // Perform search
      cy.searchSimilarSignals({
        referenceSignal: 'EngineRPM',
        threshold: 0.3,
        scope: 'Industry'
      });
      
      // Sort by similarity score descending (default)
      cy.get('[data-cy=sort-by-similarity]').click();
      
      // Verify results are sorted by similarity score
      cy.get('[data-cy=similarity-score]').then($scores => {
        const scores = Array.from($scores).map(el => parseFloat(el.textContent || '0'));
        const sortedScores = [...scores].sort((a, b) => b - a);
        expect(scores).to.deep.equal(sortedScores);
      });
      
      // Sort by similarity score ascending
      cy.get('[data-cy=sort-by-similarity]').click();
      
      // Verify results are sorted ascending
      cy.get('[data-cy=similarity-score]').then($scores => {
        const scores = Array.from($scores).map(el => parseFloat(el.textContent || '0'));
        const sortedScores = [...scores].sort((a, b) => a - b);
        expect(scores).to.deep.equal(sortedScores);
      });
    });

    it('should export search results', () => {
      cy.visit('/similar-pattern-search');
      
      // Perform search
      cy.searchSimilarSignals({
        referenceSignal: 'EngineRPM',
        threshold: 0.6,
        scope: 'OEM'
      });
      
      // Intercept export request
      cy.intercept('GET', '/api/app/similar-pattern-search/export-results*', { fixture: 'similarity-search-export.csv' }).as('exportResults');
      
      // Export results
      cy.get('[data-cy=export-results-button]').click();
      cy.get('[data-cy=export-csv-button]').click();
      
      cy.waitForApi('exportResults');
      
      // Verify export success
      cy.get('[data-cy=export-success-message]').should('be.visible');
    });

    it('should save search criteria as template', () => {
      cy.visit('/similar-pattern-search');
      
      // Configure search criteria
      cy.get('[data-cy=reference-signal-selector]').click();
      cy.waitForApi('getCanSignals');
      cy.get('[data-cy=signal-option]').first().click();
      
      cy.get('[data-cy=similarity-threshold]').clear().type('0.8');
      cy.get('[data-cy=search-scope]').select('Industry');
      
      // Select specific attributes
      cy.get('[data-cy=compare-can-id]').check();
      cy.get('[data-cy=compare-system-type]').check();
      cy.get('[data-cy=compare-value-range]').check();
      
      // Save as template
      cy.get('[data-cy=save-template-button]').click();
      cy.get('[data-cy=template-name]').type('Engine Signal Search Template');
      cy.get('[data-cy=template-description]').type('Standard template for engine-related signal searches');
      cy.get('[data-cy=confirm-save-template]').click();
      
      // Verify template saved
      cy.get('[data-cy=success-message]').should('be.visible');
      cy.get('[data-cy=success-message]').should('contain', 'Template saved successfully');
      
      // Verify template appears in dropdown
      cy.get('[data-cy=load-template-dropdown]').click();
      cy.get('[data-cy=template-option]').should('contain', 'Engine Signal Search Template');
    });

    it('should load saved search template', () => {
      cy.visit('/similar-pattern-search');
      
      // Load existing template
      cy.get('[data-cy=load-template-dropdown]').select('Engine Signal Search Template');
      
      // Verify template loaded
      cy.get('[data-cy=similarity-threshold]').should('have.value', '0.8');
      cy.get('[data-cy=search-scope]').should('have.value', 'Industry');
      cy.get('[data-cy=compare-can-id]').should('be.checked');
      cy.get('[data-cy=compare-system-type]').should('be.checked');
      cy.get('[data-cy=compare-value-range]').should('be.checked');
    });
  });

  describe('Test Data Comparison', () => {
    it('should display test data comparison interface', () => {
      cy.visit('/similar-pattern-search/test-data-comparison');
      
      // Check if comparison interface is displayed
      cy.get('[data-cy=test-data-comparison-container]').should('be.visible');
      cy.get('[data-cy=baseline-data-section]').should('be.visible');
      cy.get('[data-cy=comparison-data-section]').should('be.visible');
      cy.get('[data-cy=comparison-results-section]').should('be.visible');
    });

    it('should compare test data between similar signals', () => {
      cy.visit('/similar-pattern-search/test-data-comparison');
      
      // Select baseline signal
      cy.get('[data-cy=baseline-signal-selector]').click();
      cy.waitForApi('getCanSignals');
      cy.get('[data-cy=signal-option]').first().click();
      
      // Select comparison signal
      cy.get('[data-cy=comparison-signal-selector]').click();
      cy.get('[data-cy=signal-option]').eq(1).click();
      
      // Set comparison parameters
      cy.get('[data-cy=comparison-time-range]').select('LastWeek');
      cy.get('[data-cy=comparison-metrics]').within(() => {
        cy.get('[data-cy=compare-thresholds]').check();
        cy.get('[data-cy=compare-conditions]').check();
        cy.get('[data-cy=compare-results]').check();
      });
      
      // Execute comparison
      cy.get('[data-cy=compare-test-data-button]').click();
      cy.waitForApi('compareTestData');
      
      // Verify comparison results
      cy.get('[data-cy=comparison-results]').should('be.visible');
      cy.get('[data-cy=threshold-differences]').should('be.visible');
      cy.get('[data-cy=condition-differences]').should('be.visible');
      cy.get('[data-cy=result-differences]').should('be.visible');
      
      // Check statistical summary
      cy.get('[data-cy=statistical-summary]').should('be.visible');
      cy.get('[data-cy=correlation-coefficient]').should('be.visible');
      cy.get('[data-cy=variance-analysis]').should('be.visible');
    });

    it('should display threshold difference analysis', () => {
      cy.visit('/similar-pattern-search/test-data-comparison');
      
      // Perform comparison
      cy.compareTestData({
        baselineSignal: 'EngineRPM',
        comparisonSignal: 'EngineRPM_Toyota',
        timeRange: 'LastMonth'
      });
      
      // Check threshold differences section
      cy.get('[data-cy=threshold-differences]').should('be.visible');
      cy.get('[data-cy=threshold-difference-chart]').should('be.visible');
      
      // Verify threshold comparison details
      cy.get('[data-cy=upper-threshold-diff]').should('be.visible');
      cy.get('[data-cy=lower-threshold-diff]').should('be.visible');
      cy.get('[data-cy=threshold-impact-analysis]').should('be.visible');
      
      // Check recommendations
      cy.get('[data-cy=threshold-recommendations]').should('be.visible');
      cy.get('[data-cy=recommendation-item]').should('have.length.greaterThan', 0);
    });

    it('should display condition difference analysis', () => {
      cy.visit('/similar-pattern-search/test-data-comparison');
      
      // Perform comparison
      cy.compareTestData({
        baselineSignal: 'EngineRPM',
        comparisonSignal: 'EngineRPM_Toyota',
        timeRange: 'LastMonth'
      });
      
      // Check condition differences section
      cy.get('[data-cy=condition-differences]').should('be.visible');
      cy.get('[data-cy=condition-difference-table]').should('be.visible');
      
      // Verify condition comparison details
      cy.get('[data-cy=detection-condition-diff]').should('be.visible');
      cy.get('[data-cy=trigger-condition-diff]').should('be.visible');
      cy.get('[data-cy=condition-impact-level]').should('be.visible');
      
      // Check condition recommendations
      cy.get('[data-cy=condition-recommendations]').should('be.visible');
    });

    it('should display result difference analysis', () => {
      cy.visit('/similar-pattern-search/test-data-comparison');
      
      // Perform comparison
      cy.compareTestData({
        baselineSignal: 'EngineRPM',
        comparisonSignal: 'EngineRPM_Toyota',
        timeRange: 'LastMonth'
      });
      
      // Check result differences section
      cy.get('[data-cy=result-differences]').should('be.visible');
      cy.get('[data-cy=result-difference-chart]').should('be.visible');
      
      // Verify result comparison metrics
      cy.get('[data-cy=detection-accuracy-diff]').should('be.visible');
      cy.get('[data-cy=false-positive-rate-diff]').should('be.visible');
      cy.get('[data-cy=false-negative-rate-diff]').should('be.visible');
      
      // Check performance impact
      cy.get('[data-cy=performance-impact]').should('be.visible');
      cy.get('[data-cy=execution-time-diff]').should('be.visible');
    });

    it('should generate comparison recommendations', () => {
      cy.visit('/similar-pattern-search/test-data-comparison');
      
      // Perform comparison
      cy.compareTestData({
        baselineSignal: 'EngineRPM',
        comparisonSignal: 'EngineRPM_Toyota',
        timeRange: 'LastMonth'
      });
      
      // Check recommendations section
      cy.get('[data-cy=comparison-recommendations]').should('be.visible');
      cy.get('[data-cy=recommendation-list]').should('be.visible');
      
      // Verify recommendation details
      cy.get('[data-cy=recommendation-item]').each($item => {
        cy.wrap($item).within(() => {
          cy.get('[data-cy=recommendation-type]').should('be.visible');
          cy.get('[data-cy=recommendation-priority]').should('be.visible');
          cy.get('[data-cy=recommendation-description]').should('be.visible');
          cy.get('[data-cy=implementation-steps]').should('be.visible');
        });
      });
      
      // Check actionable recommendations
      cy.get('[data-cy=apply-recommendation-button]').should('be.visible');
      cy.get('[data-cy=save-recommendation-button]').should('be.visible');
    });

    it('should export comparison report', () => {
      cy.visit('/similar-pattern-search/test-data-comparison');
      
      // Perform comparison
      cy.compareTestData({
        baselineSignal: 'EngineRPM',
        comparisonSignal: 'EngineRPM_Toyota',
        timeRange: 'LastMonth'
      });
      
      // Intercept export request
      cy.intercept('GET', '/api/app/similar-pattern-search/export-comparison*', { fixture: 'comparison-report.pdf' }).as('exportComparison');
      
      // Export comparison report
      cy.get('[data-cy=export-comparison-button]').click();
      cy.get('[data-cy=export-pdf-button]').click();
      
      cy.waitForApi('exportComparison');
      
      // Verify export success
      cy.get('[data-cy=export-success-message]').should('be.visible');
    });
  });

  describe('Data Visualization', () => {
    it('should display similarity score visualization', () => {
      cy.visit('/similar-pattern-search/visualization');
      
      // Load visualization data
      cy.get('[data-cy=load-visualization-data]').click();
      cy.waitForApi('searchSimilarSignals');
      
      // Check visualization components
      cy.get('[data-cy=similarity-heatmap]').should('be.visible');
      cy.get('[data-cy=similarity-scatter-plot]').should('be.visible');
      cy.get('[data-cy=attribute-radar-chart]').should('be.visible');
      
      // Verify interactive features
      cy.get('[data-cy=zoom-controls]').should('be.visible');
      cy.get('[data-cy=filter-controls]').should('be.visible');
      cy.get('[data-cy=legend]').should('be.visible');
    });

    it('should display comparison trend charts', () => {
      cy.visit('/similar-pattern-search/visualization');
      
      // Select comparison mode
      cy.get('[data-cy=visualization-mode]').select('Comparison');
      
      // Load comparison data
      cy.get('[data-cy=load-comparison-data]').click();
      cy.waitForApi('compareTestData');
      
      // Check trend charts
      cy.get('[data-cy=threshold-trend-chart]').should('be.visible');
      cy.get('[data-cy=accuracy-trend-chart]').should('be.visible');
      cy.get('[data-cy=performance-trend-chart]').should('be.visible');
      
      // Verify chart interactions
      cy.get('[data-cy=time-range-selector]').should('be.visible');
      cy.get('[data-cy=metric-selector]').should('be.visible');
    });

    it('should filter visualization by signal attributes', () => {
      cy.visit('/similar-pattern-search/visualization');
      
      // Load data
      cy.get('[data-cy=load-visualization-data]').click();
      cy.waitForApi('searchSimilarSignals');
      
      // Apply system type filter
      cy.get('[data-cy=system-type-filter]').select('Engine');
      
      // Verify filtered visualization
      cy.get('[data-cy=filtered-data-points]').should('be.visible');
      cy.get('[data-cy=filter-summary]').should('contain', 'Engine');
      
      // Apply similarity threshold filter
      cy.get('[data-cy=similarity-threshold-filter]').type('0.8');
      
      // Verify updated visualization
      cy.get('[data-cy=high-similarity-points]').should('be.visible');
    });
  });

  describe('Integration with Detection Logic', () => {
    it('should search similar patterns from detection logic page', () => {
      // Start from detection logics page
      cy.visit('/detection-logics');
      cy.waitForApi('getDetectionLogics');
      
      // Click similar patterns button on first logic
      cy.get('[data-cy=find-similar-patterns-button]').first().click();
      
      // Should navigate to similar pattern search with logic pre-selected
      cy.url().should('include', '/similar-pattern-search');
      cy.get('[data-cy=reference-logic-display]').should('be.visible');
      
      // Execute search automatically
      cy.waitForApi('searchSimilarSignals');
      
      // Verify results are displayed
      cy.get('[data-cy=search-results]').should('be.visible');
    });

    it('should apply similar pattern recommendations to detection logic', () => {
      cy.visit('/similar-pattern-search');
      
      // Perform search and get recommendations
      cy.searchSimilarSignals({
        referenceSignal: 'EngineRPM',
        threshold: 0.8,
        scope: 'Industry'
      });
      
      // Select a high-similarity result
      cy.get('[data-cy=similar-signal-result]').first().within(() => {
        cy.get('[data-cy=apply-pattern-button]').click();
      });
      
      // Choose application target
      cy.get('[data-cy=target-logic-selector]').click();
      cy.waitForApi('getDetectionLogics');
      cy.get('[data-cy=logic-option]').first().click();
      
      // Select parameters to apply
      cy.get('[data-cy=apply-thresholds]').check();
      cy.get('[data-cy=apply-conditions]').check();
      
      // Confirm application
      cy.get('[data-cy=confirm-apply-button]').click();
      
      // Verify success message
      cy.get('[data-cy=success-message]').should('be.visible');
      cy.get('[data-cy=success-message]').should('contain', 'Pattern applied successfully');
    });
  });
});
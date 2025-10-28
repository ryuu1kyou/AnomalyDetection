describe('Anomaly Analysis Flow', () => {
  beforeEach(() => {
    // Intercept API calls for Anomaly Analysis
    cy.intercept('POST', '/api/app/anomaly-analysis/analyze-pattern', { fixture: 'pattern-analysis-result.json' }).as('analyzePattern');
    cy.intercept('POST', '/api/app/anomaly-analysis/threshold-recommendations', { fixture: 'threshold-recommendations.json' }).as('getThresholdRecommendations');
    cy.intercept('POST', '/api/app/anomaly-analysis/detection-accuracy', { fixture: 'detection-accuracy-metrics.json' }).as('getDetectionAccuracy');
    cy.intercept('GET', '/api/app/anomaly-detection-result*', { fixture: 'detection-results.json' }).as('getDetectionResults');
    cy.intercept('GET', '/api/app/detection-logic*', { fixture: 'detection-logics.json' }).as('getDetectionLogics');
    cy.intercept('GET', '/api/app/can-signal*', { fixture: 'can-signals.json' }).as('getCanSignals');
    
    // Login as test engineer
    cy.login('testengineer', 'test123');
    
    // Switch to Toyota tenant
    cy.switchTenant('Toyota');
  });

  describe('Pattern Analysis', () => {
    it('should display pattern analysis dashboard', () => {
      cy.visit('/anomaly-analysis');
      
      // Check if dashboard components are displayed
      cy.get('[data-cy=analysis-dashboard]').should('be.visible');
      cy.get('[data-cy=pattern-analysis-section]').should('be.visible');
      cy.get('[data-cy=threshold-recommendations-section]').should('be.visible');
      cy.get('[data-cy=accuracy-metrics-section]').should('be.visible');
      
      // Check summary cards
      cy.get('[data-cy=total-anomalies-count]').should('be.visible');
      cy.get('[data-cy=detection-accuracy-summary]').should('be.visible');
      cy.get('[data-cy=active-logics-count]').should('be.visible');
      cy.get('[data-cy=optimization-opportunities]').should('be.visible');
    });

    it('should analyze anomaly patterns', () => {
      cy.visit('/anomaly-analysis/pattern-analysis');
      
      // Set analysis parameters
      cy.get('[data-cy=analysis-time-range]').select('LastMonth');
      cy.get('[data-cy=analysis-scope]').select('AllSystems');
      
      // Select detection logics to analyze
      cy.get('[data-cy=logic-selector]').click();
      cy.waitForApi('getDetectionLogics');
      cy.get('[data-cy=logic-option]').first().check();
      cy.get('[data-cy=logic-option]').eq(1).check();
      cy.get('[data-cy=apply-logic-selection]').click();
      
      // Execute pattern analysis
      cy.get('[data-cy=analyze-patterns-button]').click();
      cy.waitForApi('analyzePattern');
      
      // Verify analysis results
      cy.get('[data-cy=pattern-analysis-results]').should('be.visible');
      cy.get('[data-cy=anomaly-type-distribution]').should('be.visible');
      cy.get('[data-cy=anomaly-level-distribution]').should('be.visible');
      cy.get('[data-cy=frequency-patterns]').should('be.visible');
      
      // Check distribution charts
      cy.get('[data-cy=type-distribution-chart]').should('be.visible');
      cy.get('[data-cy=level-distribution-chart]').should('be.visible');
      cy.get('[data-cy=frequency-timeline-chart]').should('be.visible');
    });

    it('should display anomaly type distribution analysis', () => {
      cy.visit('/anomaly-analysis/pattern-analysis');
      
      // Execute analysis
      cy.analyzeAnomalyPatterns({
        timeRange: 'LastWeek',
        scope: 'Engine'
      });
      
      // Check anomaly type distribution
      cy.get('[data-cy=anomaly-type-distribution]').should('be.visible');
      cy.get('[data-cy=type-distribution-chart]').should('be.visible');
      
      // Verify type breakdown
      cy.get('[data-cy=out-of-range-count]').should('be.visible');
      cy.get('[data-cy=rate-of-change-count]').should('be.visible');
      cy.get('[data-cy=pattern-deviation-count]').should('be.visible');
      cy.get('[data-cy=timeout-count]').should('be.visible');
      
      // Check percentage calculations
      cy.get('[data-cy=type-percentage]').each($element => {
        const percentage = parseFloat($element.text());
        expect(percentage).to.be.at.least(0).and.at.most(100);
      });
      
      // Verify drill-down functionality
      cy.get('[data-cy=type-drill-down-button]').first().click();
      cy.get('[data-cy=type-detail-modal]').should('be.visible');
      cy.get('[data-cy=type-specific-analysis]').should('be.visible');
    });

    it('should display anomaly level distribution analysis', () => {
      cy.visit('/anomaly-analysis/pattern-analysis');
      
      // Execute analysis
      cy.analyzeAnomalyPatterns({
        timeRange: 'LastMonth',
        scope: 'AllSystems'
      });
      
      // Check anomaly level distribution
      cy.get('[data-cy=anomaly-level-distribution]').should('be.visible');
      cy.get('[data-cy=level-distribution-chart]').should('be.visible');
      
      // Verify level breakdown
      cy.get('[data-cy=info-level-count]').should('be.visible');
      cy.get('[data-cy=warning-level-count]').should('be.visible');
      cy.get('[data-cy=error-level-count]').should('be.visible');
      cy.get('[data-cy=critical-level-count]').should('be.visible');
      
      // Check severity trend
      cy.get('[data-cy=severity-trend-chart]').should('be.visible');
      cy.get('[data-cy=escalation-analysis]').should('be.visible');
    });

    it('should display frequency pattern analysis', () => {
      cy.visit('/anomaly-analysis/pattern-analysis');
      
      // Execute analysis
      cy.analyzeAnomalyPatterns({
        timeRange: 'LastMonth',
        scope: 'AllSystems'
      });
      
      // Check frequency patterns
      cy.get('[data-cy=frequency-patterns]').should('be.visible');
      cy.get('[data-cy=frequency-timeline-chart]').should('be.visible');
      
      // Verify frequency metrics
      cy.get('[data-cy=peak-frequency-time]').should('be.visible');
      cy.get('[data-cy=average-frequency]').should('be.visible');
      cy.get('[data-cy=frequency-variance]').should('be.visible');
      
      // Check pattern identification
      cy.get('[data-cy=recurring-patterns]').should('be.visible');
      cy.get('[data-cy=anomaly-clusters]').should('be.visible');
      cy.get('[data-cy=seasonal-patterns]').should('be.visible');
    });

    it('should analyze anomaly correlations', () => {
      cy.visit('/anomaly-analysis/pattern-analysis');
      
      // Execute analysis
      cy.analyzeAnomalyPatterns({
        timeRange: 'LastMonth',
        scope: 'AllSystems'
      });
      
      // Check correlation analysis
      cy.get('[data-cy=anomaly-correlations]').should('be.visible');
      cy.get('[data-cy=correlation-matrix]').should('be.visible');
      cy.get('[data-cy=correlation-heatmap]').should('be.visible');
      
      // Verify correlation details
      cy.get('[data-cy=strong-correlations]').should('be.visible');
      cy.get('[data-cy=correlation-coefficient]').should('be.visible');
      cy.get('[data-cy=correlation-significance]').should('be.visible');
      
      // Check cross-signal correlations
      cy.get('[data-cy=cross-signal-correlations]').should('be.visible');
      cy.get('[data-cy=system-level-correlations]').should('be.visible');
    });

    it('should filter pattern analysis by system type', () => {
      cy.visit('/anomaly-analysis/pattern-analysis');
      
      // Apply system type filter
      cy.get('[data-cy=system-type-filter]').select('Engine');
      
      // Execute analysis
      cy.get('[data-cy=analyze-patterns-button]').click();
      cy.waitForApi('analyzePattern');
      
      // Verify filtered results
      cy.get('[data-cy=filter-summary]').should('contain', 'Engine');
      cy.get('[data-cy=filtered-anomaly-count]').should('be.visible');
      
      // Check that results are system-specific
      cy.get('[data-cy=system-specific-patterns]').should('be.visible');
      cy.get('[data-cy=engine-anomaly-types]').should('be.visible');
    });

    it('should export pattern analysis report', () => {
      cy.visit('/anomaly-analysis/pattern-analysis');
      
      // Execute analysis
      cy.analyzeAnomalyPatterns({
        timeRange: 'LastMonth',
        scope: 'AllSystems'
      });
      
      // Intercept export request
      cy.intercept('GET', '/api/app/anomaly-analysis/export-pattern-analysis*', { fixture: 'pattern-analysis-report.pdf' }).as('exportPatternAnalysis');
      
      // Export analysis report
      cy.get('[data-cy=export-analysis-button]').click();
      cy.get('[data-cy=export-pdf-button]').click();
      
      cy.waitForApi('exportPatternAnalysis');
      
      // Verify export success
      cy.get('[data-cy=export-success-message]').should('be.visible');
    });
  });

  describe('Threshold Recommendations', () => {
    it('should display threshold recommendations interface', () => {
      cy.visit('/anomaly-analysis/threshold-recommendations');
      
      // Check interface components
      cy.get('[data-cy=threshold-recommendations-container]').should('be.visible');
      cy.get('[data-cy=logic-selection-section]').should('be.visible');
      cy.get('[data-cy=current-performance-section]').should('be.visible');
      cy.get('[data-cy=recommendations-section]').should('be.visible');
    });

    it('should generate threshold recommendations', () => {
      cy.visit('/anomaly-analysis/threshold-recommendations');
      
      // Select detection logic
      cy.get('[data-cy=target-logic-selector]').click();
      cy.waitForApi('getDetectionLogics');
      cy.get('[data-cy=logic-option]').first().click();
      
      // Set analysis parameters
      cy.get('[data-cy=analysis-period]').select('LastMonth');
      cy.get('[data-cy=optimization-goal]').select('BalancedAccuracy');
      
      // Generate recommendations
      cy.get('[data-cy=generate-recommendations-button]').click();
      cy.waitForApi('getThresholdRecommendations');
      
      // Verify recommendations
      cy.get('[data-cy=threshold-recommendations-results]').should('be.visible');
      cy.get('[data-cy=current-performance-metrics]').should('be.visible');
      cy.get('[data-cy=recommended-thresholds]').should('be.visible');
      cy.get('[data-cy=expected-improvements]').should('be.visible');
      
      // Check recommendation details
      cy.get('[data-cy=recommendation-item]').should('have.length.greaterThan', 0);
      cy.get('[data-cy=recommendation-item]').first().within(() => {
        cy.get('[data-cy=threshold-parameter]').should('be.visible');
        cy.get('[data-cy=current-value]').should('be.visible');
        cy.get('[data-cy=recommended-value]').should('be.visible');
        cy.get('[data-cy=expected-impact]').should('be.visible');
      });
    });

    it('should display current performance analysis', () => {
      cy.visit('/anomaly-analysis/threshold-recommendations');
      
      // Select logic and generate recommendations
      cy.generateThresholdRecommendations({
        logicId: 'engine-rpm-logic',
        period: 'LastWeek',
        goal: 'HighPrecision'
      });
      
      // Check current performance section
      cy.get('[data-cy=current-performance-metrics]').should('be.visible');
      cy.get('[data-cy=current-precision]').should('be.visible');
      cy.get('[data-cy=current-recall]').should('be.visible');
      cy.get('[data-cy=current-f1-score]').should('be.visible');
      cy.get('[data-cy=current-false-positive-rate]').should('be.visible');
      
      // Check performance trends
      cy.get('[data-cy=performance-trend-chart]').should('be.visible');
      cy.get('[data-cy=threshold-sensitivity-analysis]').should('be.visible');
    });

    it('should display optimization metrics', () => {
      cy.visit('/anomaly-analysis/threshold-recommendations');
      
      // Generate recommendations
      cy.generateThresholdRecommendations({
        logicId: 'engine-rpm-logic',
        period: 'LastMonth',
        goal: 'BalancedAccuracy'
      });
      
      // Check optimization metrics
      cy.get('[data-cy=optimization-metrics]').should('be.visible');
      cy.get('[data-cy=precision-improvement]').should('be.visible');
      cy.get('[data-cy=recall-improvement]').should('be.visible');
      cy.get('[data-cy=f1-score-improvement]').should('be.visible');
      
      // Verify improvement calculations
      cy.get('[data-cy=improvement-percentage]').each($element => {
        const improvement = parseFloat($element.text());
        expect(improvement).to.be.a('number');
      });
      
      // Check confidence intervals
      cy.get('[data-cy=confidence-interval]').should('be.visible');
      cy.get('[data-cy=statistical-significance]').should('be.visible');
    });

    it('should simulate threshold changes', () => {
      cy.visit('/anomaly-analysis/threshold-recommendations');
      
      // Generate recommendations
      cy.generateThresholdRecommendations({
        logicId: 'engine-rpm-logic',
        period: 'LastMonth',
        goal: 'BalancedAccuracy'
      });
      
      // Open threshold simulator
      cy.get('[data-cy=simulate-thresholds-button]').click();
      cy.get('[data-cy=threshold-simulator-modal]').should('be.visible');
      
      // Adjust threshold values
      cy.get('[data-cy=upper-threshold-slider]').invoke('val', 8500).trigger('input');
      cy.get('[data-cy=lower-threshold-slider]').invoke('val', 500).trigger('input');
      
      // Run simulation
      cy.get('[data-cy=run-simulation-button]').click();
      
      // Verify simulation results
      cy.get('[data-cy=simulation-results]').should('be.visible');
      cy.get('[data-cy=simulated-precision]').should('be.visible');
      cy.get('[data-cy=simulated-recall]').should('be.visible');
      cy.get('[data-cy=impact-visualization]').should('be.visible');
    });

    it('should apply recommended thresholds', () => {
      cy.visit('/anomaly-analysis/threshold-recommendations');
      
      // Generate recommendations
      cy.generateThresholdRecommendations({
        logicId: 'engine-rpm-logic',
        period: 'LastMonth',
        goal: 'HighRecall'
      });
      
      // Select recommendations to apply
      cy.get('[data-cy=recommendation-item]').first().within(() => {
        cy.get('[data-cy=apply-recommendation-checkbox]').check();
      });
      
      cy.get('[data-cy=recommendation-item]').eq(1).within(() => {
        cy.get('[data-cy=apply-recommendation-checkbox]').check();
      });
      
      // Apply selected recommendations
      cy.get('[data-cy=apply-recommendations-button]').click();
      
      // Confirm application
      cy.get('[data-cy=confirm-apply-modal]').should('be.visible');
      cy.get('[data-cy=application-summary]').should('be.visible');
      cy.get('[data-cy=confirm-apply-button]').click();
      
      // Verify success
      cy.get('[data-cy=success-message]').should('be.visible');
      cy.get('[data-cy=success-message]').should('contain', 'Thresholds updated successfully');
    });

    it('should schedule threshold optimization', () => {
      cy.visit('/anomaly-analysis/threshold-recommendations');
      
      // Generate recommendations
      cy.generateThresholdRecommendations({
        logicId: 'engine-rpm-logic',
        period: 'LastMonth',
        goal: 'BalancedAccuracy'
      });
      
      // Schedule automatic optimization
      cy.get('[data-cy=schedule-optimization-button]').click();
      cy.get('[data-cy=optimization-schedule-modal]').should('be.visible');
      
      // Set schedule parameters
      cy.get('[data-cy=optimization-frequency]').select('Weekly');
      cy.get('[data-cy=optimization-time]').type('02:00');
      cy.get('[data-cy=auto-apply-threshold]').type('0.8');
      
      // Enable notifications
      cy.get('[data-cy=enable-notifications]').check();
      cy.get('[data-cy=notification-email]').type('engineer@toyota.com');
      
      // Save schedule
      cy.get('[data-cy=save-schedule-button]').click();
      
      // Verify schedule saved
      cy.get('[data-cy=success-message]').should('be.visible');
      cy.get('[data-cy=success-message]').should('contain', 'Optimization schedule saved');
    });
  });

  describe('Detection Accuracy Metrics', () => {
    it('should display accuracy metrics dashboard', () => {
      cy.visit('/anomaly-analysis/accuracy-metrics');
      
      // Check dashboard components
      cy.get('[data-cy=accuracy-metrics-dashboard]').should('be.visible');
      cy.get('[data-cy=overall-accuracy-section]').should('be.visible');
      cy.get('[data-cy=accuracy-by-type-section]').should('be.visible');
      cy.get('[data-cy=accuracy-by-time-section]').should('be.visible');
      cy.get('[data-cy=confusion-matrix-section]').should('be.visible');
    });

    it('should calculate detection accuracy metrics', () => {
      cy.visit('/anomaly-analysis/accuracy-metrics');
      
      // Set calculation parameters
      cy.get('[data-cy=metrics-time-range]').select('LastMonth');
      cy.get('[data-cy=metrics-scope]').select('AllLogics');
      
      // Select specific logics (optional)
      cy.get('[data-cy=logic-filter]').click();
      cy.waitForApi('getDetectionLogics');
      cy.get('[data-cy=logic-checkbox]').first().check();
      cy.get('[data-cy=logic-checkbox]').eq(1).check();
      cy.get('[data-cy=apply-logic-filter]').click();
      
      // Calculate metrics
      cy.get('[data-cy=calculate-accuracy-button]').click();
      cy.waitForApi('getDetectionAccuracy');
      
      // Verify accuracy metrics
      cy.get('[data-cy=accuracy-metrics-results]').should('be.visible');
      cy.get('[data-cy=overall-precision]').should('be.visible');
      cy.get('[data-cy=overall-recall]').should('be.visible');
      cy.get('[data-cy=overall-f1-score]').should('be.visible');
      cy.get('[data-cy=overall-accuracy]').should('be.visible');
      
      // Check metric values are valid
      cy.get('[data-cy=precision-value]').should('contain.text', '%');
      cy.get('[data-cy=recall-value]').should('contain.text', '%');
      cy.get('[data-cy=f1-score-value]').should('contain.text', '%');
    });

    it('should display accuracy by anomaly type', () => {
      cy.visit('/anomaly-analysis/accuracy-metrics');
      
      // Calculate metrics
      cy.calculateAccuracyMetrics({
        timeRange: 'LastWeek',
        scope: 'AllLogics'
      });
      
      // Check accuracy by type section
      cy.get('[data-cy=accuracy-by-type]').should('be.visible');
      cy.get('[data-cy=type-accuracy-chart]').should('be.visible');
      
      // Verify type-specific metrics
      cy.get('[data-cy=out-of-range-accuracy]').should('be.visible');
      cy.get('[data-cy=rate-of-change-accuracy]').should('be.visible');
      cy.get('[data-cy=pattern-deviation-accuracy]').should('be.visible');
      cy.get('[data-cy=timeout-accuracy]').should('be.visible');
      
      // Check detailed breakdown
      cy.get('[data-cy=type-precision-breakdown]').should('be.visible');
      cy.get('[data-cy=type-recall-breakdown]').should('be.visible');
    });

    it('should display accuracy by time range', () => {
      cy.visit('/anomaly-analysis/accuracy-metrics');
      
      // Calculate metrics
      cy.calculateAccuracyMetrics({
        timeRange: 'LastMonth',
        scope: 'AllLogics'
      });
      
      // Check accuracy by time section
      cy.get('[data-cy=accuracy-by-time]').should('be.visible');
      cy.get('[data-cy=time-accuracy-chart]').should('be.visible');
      
      // Verify time-based metrics
      cy.get('[data-cy=daily-accuracy-trend]').should('be.visible');
      cy.get('[data-cy=weekly-accuracy-average]').should('be.visible');
      cy.get('[data-cy=accuracy-variance]').should('be.visible');
      
      // Check trend analysis
      cy.get('[data-cy=accuracy-trend-direction]').should('be.visible');
      cy.get('[data-cy=performance-stability]').should('be.visible');
    });

    it('should display confusion matrix', () => {
      cy.visit('/anomaly-analysis/accuracy-metrics');
      
      // Calculate metrics
      cy.calculateAccuracyMetrics({
        timeRange: 'LastMonth',
        scope: 'AllLogics'
      });
      
      // Check confusion matrix section
      cy.get('[data-cy=confusion-matrix]').should('be.visible');
      cy.get('[data-cy=confusion-matrix-heatmap]').should('be.visible');
      
      // Verify matrix values
      cy.get('[data-cy=true-positives]').should('be.visible');
      cy.get('[data-cy=false-positives]').should('be.visible');
      cy.get('[data-cy=true-negatives]').should('be.visible');
      cy.get('[data-cy=false-negatives]').should('be.visible');
      
      // Check derived metrics
      cy.get('[data-cy=false-positive-rate]').should('be.visible');
      cy.get('[data-cy=false-negative-rate]').should('be.visible');
      cy.get('[data-cy=specificity]').should('be.visible');
      cy.get('[data-cy=sensitivity]').should('be.visible');
    });

    it('should compare accuracy across detection logics', () => {
      cy.visit('/anomaly-analysis/accuracy-metrics');
      
      // Enable comparison mode
      cy.get('[data-cy=comparison-mode]').check();
      
      // Select logics to compare
      cy.get('[data-cy=comparison-logic-selector]').click();
      cy.waitForApi('getDetectionLogics');
      cy.get('[data-cy=logic-comparison-option]').first().check();
      cy.get('[data-cy=logic-comparison-option]').eq(1).check();
      cy.get('[data-cy=logic-comparison-option]').eq(2).check();
      cy.get('[data-cy=apply-comparison-selection]').click();
      
      // Calculate comparison metrics
      cy.get('[data-cy=calculate-comparison-button]').click();
      cy.waitForApi('getDetectionAccuracy');
      
      // Verify comparison results
      cy.get('[data-cy=logic-comparison-table]').should('be.visible');
      cy.get('[data-cy=comparison-chart]').should('be.visible');
      
      // Check ranking
      cy.get('[data-cy=logic-ranking]').should('be.visible');
      cy.get('[data-cy=best-performing-logic]').should('be.visible');
      cy.get('[data-cy=improvement-opportunities]').should('be.visible');
    });

    it('should export accuracy metrics report', () => {
      cy.visit('/anomaly-analysis/accuracy-metrics');
      
      // Calculate metrics
      cy.calculateAccuracyMetrics({
        timeRange: 'LastMonth',
        scope: 'AllLogics'
      });
      
      // Intercept export request
      cy.intercept('GET', '/api/app/anomaly-analysis/export-accuracy-metrics*', { fixture: 'accuracy-metrics-report.pdf' }).as('exportAccuracyMetrics');
      
      // Export metrics report
      cy.get('[data-cy=export-metrics-button]').click();
      cy.get('[data-cy=export-pdf-button]').click();
      
      cy.waitForApi('exportAccuracyMetrics');
      
      // Verify export success
      cy.get('[data-cy=export-success-message]').should('be.visible');
    });
  });

  describe('Integration and Workflow', () => {
    it('should navigate between analysis modules', () => {
      cy.visit('/anomaly-analysis');
      
      // Navigate to pattern analysis
      cy.get('[data-cy=pattern-analysis-link]').click();
      cy.url().should('include', '/pattern-analysis');
      
      // Navigate to threshold recommendations
      cy.get('[data-cy=threshold-recommendations-link]').click();
      cy.url().should('include', '/threshold-recommendations');
      
      // Navigate to accuracy metrics
      cy.get('[data-cy=accuracy-metrics-link]').click();
      cy.url().should('include', '/accuracy-metrics');
      
      // Return to dashboard
      cy.get('[data-cy=analysis-dashboard-link]').click();
      cy.url().should('include', '/anomaly-analysis');
    });

    it('should create analysis workflow from detection results', () => {
      // Start from detection results page
      cy.visit('/detection-results');
      cy.waitForApi('getDetectionResults');
      
      // Click analyze patterns button
      cy.get('[data-cy=analyze-patterns-button]').click();
      
      // Should navigate to pattern analysis with results pre-filtered
      cy.url().should('include', '/anomaly-analysis/pattern-analysis');
      cy.get('[data-cy=pre-filtered-results]').should('be.visible');
      
      // Execute analysis automatically
      cy.waitForApi('analyzePattern');
      
      // Verify analysis results
      cy.get('[data-cy=pattern-analysis-results]').should('be.visible');
    });

    it('should optimize detection logic from analysis results', () => {
      cy.visit('/anomaly-analysis/threshold-recommendations');
      
      // Generate recommendations
      cy.generateThresholdRecommendations({
        logicId: 'engine-rpm-logic',
        period: 'LastMonth',
        goal: 'BalancedAccuracy'
      });
      
      // Apply recommendations
      cy.get('[data-cy=recommendation-item]').first().within(() => {
        cy.get('[data-cy=apply-recommendation-checkbox]').check();
      });
      
      cy.get('[data-cy=apply-recommendations-button]').click();
      cy.get('[data-cy=confirm-apply-button]').click();
      
      // Navigate to detection logic to verify changes
      cy.get('[data-cy=view-updated-logic-button]').click();
      cy.url().should('include', '/detection-logics/');
      
      // Verify threshold values were updated
      cy.get('[data-cy=logic-parameters]').should('be.visible');
      cy.get('[data-cy=updated-threshold-indicator]').should('be.visible');
    });

    it('should schedule automated analysis reports', () => {
      cy.visit('/anomaly-analysis');
      
      // Open report scheduling
      cy.get('[data-cy=schedule-reports-button]').click();
      cy.get('[data-cy=report-schedule-modal]').should('be.visible');
      
      // Configure report schedule
      cy.get('[data-cy=report-frequency]').select('Weekly');
      cy.get('[data-cy=report-day]').select('Monday');
      cy.get('[data-cy=report-time]').type('08:00');
      
      // Select report types
      cy.get('[data-cy=include-pattern-analysis]').check();
      cy.get('[data-cy=include-accuracy-metrics]').check();
      cy.get('[data-cy=include-recommendations]').check();
      
      // Set recipients
      cy.get('[data-cy=report-recipients]').type('team@toyota.com, manager@toyota.com');
      
      // Save schedule
      cy.get('[data-cy=save-report-schedule]').click();
      
      // Verify schedule saved
      cy.get('[data-cy=success-message]').should('be.visible');
      cy.get('[data-cy=success-message]').should('contain', 'Report schedule saved');
    });
  });
});
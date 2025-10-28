describe('OEM Traceability Flow', () => {
  beforeEach(() => {
    // Intercept API calls for OEM Traceability
    cy.intercept('GET', '/api/app/oem-traceability/customizations*', { fixture: 'oem-customizations.json' }).as('getCustomizations');
    cy.intercept('POST', '/api/app/oem-traceability/customizations', { fixture: 'oem-customization-created.json' }).as('createCustomization');
    cy.intercept('PUT', '/api/app/oem-traceability/customizations/*/submit', {}).as('submitCustomization');
    cy.intercept('PUT', '/api/app/oem-traceability/customizations/*/approve', {}).as('approveCustomization');
    cy.intercept('PUT', '/api/app/oem-traceability/customizations/*/reject', {}).as('rejectCustomization');
    cy.intercept('GET', '/api/app/oem-traceability/approvals*', { fixture: 'oem-approvals.json' }).as('getApprovals');
    cy.intercept('GET', '/api/app/oem-traceability/trace/*/*', { fixture: 'traceability-result.json' }).as('getTraceability');
    cy.intercept('GET', '/api/app/can-signal*', { fixture: 'can-signals.json' }).as('getCanSignals');
    cy.intercept('GET', '/api/app/detection-logic*', { fixture: 'detection-logics.json' }).as('getDetectionLogics');
    
    // Login as OEM admin user
    cy.login('oemadmin', 'admin123');
    
    // Switch to Toyota tenant
    cy.switchTenant('Toyota');
  });

  describe('OEM Customization Management', () => {
    it('should display OEM traceability dashboard', () => {
      cy.visit('/oem-traceability');
      
      // Wait for data to load
      cy.waitForApi('getCustomizations');
      cy.waitForApi('getApprovals');
      
      // Check if dashboard components are displayed
      cy.get('[data-cy=traceability-dashboard]').should('be.visible');
      cy.get('[data-cy=customizations-summary]').should('be.visible');
      cy.get('[data-cy=approvals-summary]').should('be.visible');
      cy.get('[data-cy=recent-activities]').should('be.visible');
      
      // Check summary cards
      cy.get('[data-cy=pending-customizations-count]').should('be.visible');
      cy.get('[data-cy=approved-customizations-count]').should('be.visible');
      cy.get('[data-cy=pending-approvals-count]').should('be.visible');
    });

    it('should create a new CAN signal customization', () => {
      cy.visit('/oem-traceability/customizations');
      cy.waitForApi('getCustomizations');
      
      // Click create customization button
      cy.get('[data-cy=create-customization-button]').click();
      
      // Select entity type
      cy.get('[data-cy=entity-type]').select('CanSignal');
      
      // Select specific CAN signal
      cy.get('[data-cy=entity-selector]').click();
      cy.waitForApi('getCanSignals');
      cy.get('[data-cy=entity-option]').first().click();
      
      // Select customization type
      cy.get('[data-cy=customization-type]').select('ParameterModification');
      
      // Enter customization reason
      cy.get('[data-cy=customization-reason]').type('Adjust signal range for Toyota specific requirements');
      
      // Modify parameters
      cy.get('[data-cy=custom-parameters-section]').should('be.visible');
      cy.get('[data-cy=add-parameter-button]').click();
      
      cy.get('[data-cy=parameter-key]').type('MaxValue');
      cy.get('[data-cy=parameter-value]').type('9000');
      cy.get('[data-cy=save-parameter-button]').click();
      
      // Save customization
      cy.get('[data-cy=save-customization-button]').click();
      cy.waitForApi('createCustomization');
      
      // Verify success message
      cy.get('[data-cy=success-message]').should('be.visible');
      cy.get('[data-cy=success-message]').should('contain', 'Customization created successfully');
      
      // Verify customization appears in list
      cy.visit('/oem-traceability/customizations');
      cy.waitForApi('getCustomizations');
      cy.get('[data-cy=customizations-table]').should('contain', 'ParameterModification');
    });

    it('should create a detection logic customization', () => {
      cy.visit('/oem-traceability/customizations');
      cy.waitForApi('getCustomizations');
      
      // Click create customization button
      cy.get('[data-cy=create-customization-button]').click();
      
      // Select detection logic entity type
      cy.get('[data-cy=entity-type]').select('DetectionLogic');
      
      // Select specific detection logic
      cy.get('[data-cy=entity-selector]').click();
      cy.waitForApi('getDetectionLogics');
      cy.get('[data-cy=entity-option]').first().click();
      
      // Select customization type
      cy.get('[data-cy=customization-type]').select('ThresholdAdjustment');
      
      // Enter customization reason
      cy.get('[data-cy=customization-reason]').type('Adjust detection thresholds for Toyota vehicle characteristics');
      
      // Modify threshold parameters
      cy.get('[data-cy=add-parameter-button]').click();
      cy.get('[data-cy=parameter-key]').type('UpperThreshold');
      cy.get('[data-cy=parameter-value]').type('8500');
      cy.get('[data-cy=save-parameter-button]').click();
      
      cy.get('[data-cy=add-parameter-button]').click();
      cy.get('[data-cy=parameter-key]').type('LowerThreshold');
      cy.get('[data-cy=parameter-value]').type('500');
      cy.get('[data-cy=save-parameter-button]').click();
      
      // Save customization
      cy.get('[data-cy=save-customization-button]').click();
      cy.waitForApi('createCustomization');
      
      // Verify success message
      cy.get('[data-cy=success-message]').should('be.visible');
      cy.get('[data-cy=success-message]').should('contain', 'Customization created successfully');
    });

    it('should submit customization for approval', () => {
      cy.visit('/oem-traceability/customizations');
      cy.waitForApi('getCustomizations');
      
      // Find a draft customization and submit for approval
      cy.get('[data-cy=customization-row]').contains('Draft').parent().within(() => {
        cy.get('[data-cy=submit-approval-button]').click();
      });
      
      // Enter submission notes
      cy.get('[data-cy=submission-notes]').type('Ready for review - all parameters validated');
      
      // Confirm submission
      cy.get('[data-cy=confirm-submit-button]').click();
      cy.waitForApi('submitCustomization');
      
      // Verify success message
      cy.get('[data-cy=success-message]').should('be.visible');
      cy.get('[data-cy=success-message]').should('contain', 'Customization submitted for approval');
      
      // Verify status changed to Pending Approval
      cy.get('[data-cy=customizations-table]').should('contain', 'Pending Approval');
    });

    it('should filter customizations by status', () => {
      cy.visit('/oem-traceability/customizations');
      cy.waitForApi('getCustomizations');
      
      // Filter by Draft status
      cy.get('[data-cy=status-filter]').select('Draft');
      
      // Verify all displayed customizations are Draft
      cy.get('[data-cy=customization-status]').each($element => {
        cy.wrap($element).should('contain', 'Draft');
      });
      
      // Filter by Approved status
      cy.get('[data-cy=status-filter]').select('Approved');
      
      // Verify all displayed customizations are Approved
      cy.get('[data-cy=customization-status]').each($element => {
        cy.wrap($element).should('contain', 'Approved');
      });
      
      // Reset filter
      cy.get('[data-cy=reset-filters-button]').click();
    });

    it('should filter customizations by entity type', () => {
      cy.visit('/oem-traceability/customizations');
      cy.waitForApi('getCustomizations');
      
      // Filter by CanSignal entity type
      cy.get('[data-cy=entity-type-filter]').select('CanSignal');
      
      // Verify all displayed customizations are for CAN signals
      cy.get('[data-cy=customization-entity-type]').each($element => {
        cy.wrap($element).should('contain', 'CanSignal');
      });
      
      // Filter by DetectionLogic entity type
      cy.get('[data-cy=entity-type-filter]').select('DetectionLogic');
      
      // Verify all displayed customizations are for detection logics
      cy.get('[data-cy=customization-entity-type]').each($element => {
        cy.wrap($element).should('contain', 'DetectionLogic');
      });
    });

    it('should view customization details', () => {
      cy.visit('/oem-traceability/customizations');
      cy.waitForApi('getCustomizations');
      
      // Click on first customization to view details
      cy.get('[data-cy=view-customization-button]').first().click();
      
      // Verify customization details page
      cy.url().should('include', '/oem-traceability/customizations/');
      cy.get('[data-cy=customization-details]').should('be.visible');
      
      // Check if all customization properties are displayed
      cy.get('[data-cy=entity-info]').should('be.visible');
      cy.get('[data-cy=customization-type-display]').should('be.visible');
      cy.get('[data-cy=customization-reason-display]').should('be.visible');
      cy.get('[data-cy=custom-parameters-display]').should('be.visible');
      cy.get('[data-cy=original-parameters-display]').should('be.visible');
      cy.get('[data-cy=status-history]').should('be.visible');
    });
  });

  describe('Approval Workflow Management', () => {
    it('should display pending approvals', () => {
      cy.visit('/oem-traceability/approvals');
      cy.waitForApi('getApprovals');
      
      // Check if approvals table is displayed
      cy.get('[data-cy=approvals-table]').should('be.visible');
      
      // Check table headers
      cy.get('[data-cy=entity-type-header]').should('contain', 'Entity Type');
      cy.get('[data-cy=approval-type-header]').should('contain', 'Approval Type');
      cy.get('[data-cy=requested-by-header]').should('contain', 'Requested By');
      cy.get('[data-cy=requested-at-header]').should('contain', 'Requested At');
      cy.get('[data-cy=priority-header]').should('contain', 'Priority');
      cy.get('[data-cy=due-date-header]').should('contain', 'Due Date');
    });

    it('should approve a customization request', () => {
      cy.visit('/oem-traceability/approvals');
      cy.waitForApi('getApprovals');
      
      // Find a pending approval and approve it
      cy.get('[data-cy=approval-row]').contains('Pending').parent().within(() => {
        cy.get('[data-cy=approve-button]').click();
      });
      
      // Enter approval notes
      cy.get('[data-cy=approval-notes]').type('Customization meets all requirements and safety standards');
      
      // Confirm approval
      cy.get('[data-cy=confirm-approve-button]').click();
      cy.waitForApi('approveCustomization');
      
      // Verify success message
      cy.get('[data-cy=success-message]').should('be.visible');
      cy.get('[data-cy=success-message]').should('contain', 'Customization approved successfully');
    });

    it('should reject a customization request', () => {
      cy.visit('/oem-traceability/approvals');
      cy.waitForApi('getApprovals');
      
      // Find a pending approval and reject it
      cy.get('[data-cy=approval-row]').contains('Pending').parent().within(() => {
        cy.get('[data-cy=reject-button]').click();
      });
      
      // Enter rejection reason
      cy.get('[data-cy=rejection-reason]').type('Parameters exceed safety limits - requires revision');
      
      // Confirm rejection
      cy.get('[data-cy=confirm-reject-button]').click();
      cy.waitForApi('rejectCustomization');
      
      // Verify success message
      cy.get('[data-cy=success-message]').should('be.visible');
      cy.get('[data-cy=success-message]').should('contain', 'Customization rejected');
    });

    it('should filter approvals by priority', () => {
      cy.visit('/oem-traceability/approvals');
      cy.waitForApi('getApprovals');
      
      // Filter by High priority
      cy.get('[data-cy=priority-filter]').select('High');
      
      // Verify all displayed approvals are High priority
      cy.get('[data-cy=approval-priority]').each($element => {
        cy.wrap($element).should('contain', 'High');
      });
      
      // Filter by Critical priority
      cy.get('[data-cy=priority-filter]').select('Critical');
      
      // Verify all displayed approvals are Critical priority
      cy.get('[data-cy=approval-priority]').each($element => {
        cy.wrap($element).should('contain', 'Critical');
      });
    });

    it('should identify overdue approvals', () => {
      cy.visit('/oem-traceability/approvals');
      cy.waitForApi('getApprovals');
      
      // Check for overdue indicator
      cy.get('[data-cy=overdue-indicator]').should('be.visible');
      
      // Filter by overdue approvals
      cy.get('[data-cy=show-overdue-only]').check();
      
      // Verify all displayed approvals are overdue
      cy.get('[data-cy=approval-row]').each($row => {
        cy.wrap($row).find('[data-cy=overdue-indicator]').should('be.visible');
      });
    });

    it('should update approval due date', () => {
      cy.visit('/oem-traceability/approvals');
      cy.waitForApi('getApprovals');
      
      // Find an approval and update due date
      cy.get('[data-cy=approval-row]').first().within(() => {
        cy.get('[data-cy=update-due-date-button]').click();
      });
      
      // Set new due date
      const futureDate = new Date();
      futureDate.setDate(futureDate.getDate() + 7);
      const dateString = futureDate.toISOString().split('T')[0];
      
      cy.get('[data-cy=new-due-date]').type(dateString);
      cy.get('[data-cy=due-date-reason]').type('Extended due to additional review requirements');
      
      // Confirm update
      cy.get('[data-cy=confirm-update-button]').click();
      
      // Verify success message
      cy.get('[data-cy=success-message]').should('be.visible');
      cy.get('[data-cy=success-message]').should('contain', 'Due date updated successfully');
    });
  });

  describe('Traceability Tracking', () => {
    it('should trace CAN signal customizations', () => {
      cy.visit('/oem-traceability/trace');
      
      // Select entity type
      cy.get('[data-cy=trace-entity-type]').select('CanSignal');
      
      // Select specific CAN signal
      cy.get('[data-cy=trace-entity-selector]').click();
      cy.waitForApi('getCanSignals');
      cy.get('[data-cy=trace-entity-option]').first().click();
      
      // Execute trace
      cy.get('[data-cy=execute-trace-button]').click();
      cy.waitForApi('getTraceability');
      
      // Verify traceability results
      cy.get('[data-cy=traceability-results]').should('be.visible');
      cy.get('[data-cy=customization-history]').should('be.visible');
      cy.get('[data-cy=approval-history]').should('be.visible');
      cy.get('[data-cy=change-timeline]').should('be.visible');
      
      // Check timeline entries
      cy.get('[data-cy=timeline-entry]').should('have.length.greaterThan', 0);
      cy.get('[data-cy=timeline-entry]').first().should('contain', 'Created');
    });

    it('should trace detection logic customizations', () => {
      cy.visit('/oem-traceability/trace');
      
      // Select detection logic entity type
      cy.get('[data-cy=trace-entity-type]').select('DetectionLogic');
      
      // Select specific detection logic
      cy.get('[data-cy=trace-entity-selector]').click();
      cy.waitForApi('getDetectionLogics');
      cy.get('[data-cy=trace-entity-option]').first().click();
      
      // Execute trace
      cy.get('[data-cy=execute-trace-button]').click();
      cy.waitForApi('getTraceability');
      
      // Verify traceability results
      cy.get('[data-cy=traceability-results]').should('be.visible');
      cy.get('[data-cy=customization-history]').should('be.visible');
      
      // Check for parameter changes
      cy.get('[data-cy=parameter-changes]').should('be.visible');
      cy.get('[data-cy=threshold-modifications]').should('be.visible');
    });

    it('should export traceability report', () => {
      cy.visit('/oem-traceability/trace');
      
      // Select entity and execute trace
      cy.get('[data-cy=trace-entity-type]').select('CanSignal');
      cy.get('[data-cy=trace-entity-selector]').click();
      cy.waitForApi('getCanSignals');
      cy.get('[data-cy=trace-entity-option]').first().click();
      cy.get('[data-cy=execute-trace-button]').click();
      cy.waitForApi('getTraceability');
      
      // Intercept export request
      cy.intercept('GET', '/api/app/oem-traceability/export-trace*', { fixture: 'traceability-export.pdf' }).as('exportTrace');
      
      // Export traceability report
      cy.get('[data-cy=export-trace-button]').click();
      cy.get('[data-cy=export-pdf-button]').click();
      
      cy.waitForApi('exportTrace');
      
      // Verify export success
      cy.get('[data-cy=export-success-message]').should('be.visible');
    });

    it('should filter traceability by date range', () => {
      cy.visit('/oem-traceability/trace');
      
      // Select entity and execute initial trace
      cy.get('[data-cy=trace-entity-type]').select('CanSignal');
      cy.get('[data-cy=trace-entity-selector]').click();
      cy.waitForApi('getCanSignals');
      cy.get('[data-cy=trace-entity-option]').first().click();
      cy.get('[data-cy=execute-trace-button]').click();
      cy.waitForApi('getTraceability');
      
      // Apply date range filter
      const startDate = new Date();
      startDate.setMonth(startDate.getMonth() - 1);
      const endDate = new Date();
      
      cy.get('[data-cy=date-range-start]').type(startDate.toISOString().split('T')[0]);
      cy.get('[data-cy=date-range-end]').type(endDate.toISOString().split('T')[0]);
      cy.get('[data-cy=apply-date-filter-button]').click();
      
      // Verify filtered results
      cy.get('[data-cy=timeline-entry]').each($entry => {
        cy.wrap($entry).find('[data-cy=entry-date]').should('be.visible');
      });
    });

    it('should display customization impact analysis', () => {
      cy.visit('/oem-traceability/trace');
      
      // Select entity and execute trace
      cy.get('[data-cy=trace-entity-type]').select('DetectionLogic');
      cy.get('[data-cy=trace-entity-selector]').click();
      cy.waitForApi('getDetectionLogics');
      cy.get('[data-cy=trace-entity-option]').first().click();
      cy.get('[data-cy=execute-trace-button]').click();
      cy.waitForApi('getTraceability');
      
      // Check impact analysis section
      cy.get('[data-cy=impact-analysis]').should('be.visible');
      cy.get('[data-cy=affected-projects]').should('be.visible');
      cy.get('[data-cy=dependent-logics]').should('be.visible');
      cy.get('[data-cy=risk-assessment]').should('be.visible');
      
      // Verify impact metrics
      cy.get('[data-cy=impact-score]').should('be.visible');
      cy.get('[data-cy=affected-systems-count]').should('be.visible');
      cy.get('[data-cy=safety-impact-level]').should('be.visible');
    });
  });

  describe('Integration with Other Modules', () => {
    it('should navigate from CAN signal to traceability', () => {
      // Start from CAN signals page
      cy.visit('/can-signals');
      cy.waitForApi('getCanSignals');
      
      // Click traceability button on first signal
      cy.get('[data-cy=signal-traceability-button]').first().click();
      
      // Should navigate to traceability page with signal pre-selected
      cy.url().should('include', '/oem-traceability/trace');
      cy.get('[data-cy=trace-entity-type]').should('have.value', 'CanSignal');
      cy.get('[data-cy=selected-entity-name]').should('be.visible');
    });

    it('should navigate from detection logic to traceability', () => {
      // Start from detection logics page
      cy.visit('/detection-logics');
      cy.waitForApi('getDetectionLogics');
      
      // Click traceability button on first logic
      cy.get('[data-cy=logic-traceability-button]').first().click();
      
      // Should navigate to traceability page with logic pre-selected
      cy.url().should('include', '/oem-traceability/trace');
      cy.get('[data-cy=trace-entity-type]').should('have.value', 'DetectionLogic');
      cy.get('[data-cy=selected-entity-name]').should('be.visible');
    });

    it('should create customization from signal details page', () => {
      // Navigate to signal details
      cy.visit('/can-signals');
      cy.waitForApi('getCanSignals');
      cy.get('[data-cy=view-signal-button]').first().click();
      
      // Click customize button
      cy.get('[data-cy=customize-signal-button]').click();
      
      // Should navigate to customization creation with signal pre-selected
      cy.url().should('include', '/oem-traceability/customizations/create');
      cy.get('[data-cy=entity-type]').should('have.value', 'CanSignal');
      cy.get('[data-cy=selected-entity-display]').should('be.visible');
    });
  });
});
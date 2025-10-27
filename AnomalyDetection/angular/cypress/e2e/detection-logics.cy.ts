describe('Detection Logic Management', () => {
  beforeEach(() => {
    // Intercept API calls
    cy.intercept('GET', '/api/app/detection-logic*', { fixture: 'detection-logics.json' }).as('getDetectionLogics');
    cy.intercept('POST', '/api/app/detection-logic', { fixture: 'detection-logic-created.json' }).as('createDetectionLogic');
    cy.intercept('PUT', '/api/app/detection-logic/*', { fixture: 'detection-logic-updated.json' }).as('updateDetectionLogic');
    cy.intercept('DELETE', '/api/app/detection-logic/*', {}).as('deleteDetectionLogic');
    cy.intercept('POST', '/api/app/detection-logic/*/submit-approval', {}).as('submitForApproval');
    cy.intercept('POST', '/api/app/detection-logic/*/approve', {}).as('approveLogic');
    cy.intercept('POST', '/api/app/detection-logic/*/test-execution', { fixture: 'test-execution-result.json' }).as('testExecution');
    
    // Login as admin user
    cy.login('admin', 'admin123');
    
    // Switch to Toyota tenant
    cy.switchTenant('Toyota');
  });

  it('should display detection logics list', () => {
    cy.visit('/detection-logics');
    
    // Wait for logics to load
    cy.waitForApi('getDetectionLogics');
    
    // Check if table is displayed
    cy.get('[data-cy=detection-logics-table]').should('be.visible');
    
    // Check if logics are displayed
    cy.get('[data-cy=logic-row]').should('have.length.greaterThan', 0);
    
    // Check table headers
    cy.get('[data-cy=logic-name-header]').should('contain', 'Logic Name');
    cy.get('[data-cy=detection-type-header]').should('contain', 'Detection Type');
    cy.get('[data-cy=status-header]').should('contain', 'Status');
    cy.get('[data-cy=asil-level-header]').should('contain', 'ASIL Level');
  });

  it('should create a new detection logic', () => {
    const newLogic = {
      name: 'TestRangeCheck',
      description: 'Test range check logic for E2E testing',
      detectionType: 'OutOfRange',
      systemType: 'Engine',
      logicContent: '{"min": 0, "max": 8000}'
    };

    cy.createDetectionLogic(newLogic);
    
    // Verify logic was created
    cy.waitForApi('createDetectionLogic');
    cy.url().should('include', '/detection-logics');
    
    // Check if logic appears in the list
    cy.get('[data-cy=detection-logics-table]').should('contain', newLogic.name);
  });

  it('should edit an existing detection logic', () => {
    cy.visit('/detection-logics');
    cy.waitForApi('getDetectionLogics');
    
    // Click edit button on first logic
    cy.get('[data-cy=edit-logic-button]').first().click();
    
    // Update logic description
    cy.get('[data-cy=logic-description]').clear().type('Updated description for E2E test');
    
    // Save changes
    cy.get('[data-cy=save-button]').click();
    cy.waitForApi('updateDetectionLogic');
    
    // Verify success message
    cy.get('[data-cy=success-message]').should('be.visible');
    cy.get('[data-cy=success-message]').should('contain', 'Logic updated successfully');
  });

  it('should submit logic for approval', () => {
    cy.visit('/detection-logics');
    cy.waitForApi('getDetectionLogics');
    
    // Find a draft logic and submit for approval
    cy.get('[data-cy=logic-row]').contains('Draft').parent().within(() => {
      cy.get('[data-cy=submit-approval-button]').click();
    });
    
    // Confirm submission
    cy.get('[data-cy=confirm-submit-button]').click();
    cy.waitForApi('submitForApproval');
    
    // Verify success message
    cy.get('[data-cy=success-message]').should('be.visible');
    cy.get('[data-cy=success-message]').should('contain', 'Submitted for approval');
  });

  it('should approve a detection logic', () => {
    cy.visit('/detection-logics');
    cy.waitForApi('getDetectionLogics');
    
    // Find a pending approval logic and approve it
    cy.get('[data-cy=logic-row]').contains('Pending Approval').parent().within(() => {
      cy.get('[data-cy=approve-button]').click();
    });
    
    // Enter approval notes
    cy.get('[data-cy=approval-notes]').type('Approved for production use');
    cy.get('[data-cy=confirm-approve-button]').click();
    cy.waitForApi('approveLogic');
    
    // Verify success message
    cy.get('[data-cy=success-message]').should('be.visible');
    cy.get('[data-cy=success-message]').should('contain', 'Logic approved successfully');
  });

  it('should test logic execution', () => {
    cy.visit('/detection-logics');
    cy.waitForApi('getDetectionLogics');
    
    // Find an approved logic and test it
    cy.get('[data-cy=logic-row]').contains('Approved').parent().within(() => {
      cy.get('[data-cy=test-execution-button]').click();
    });
    
    // Enter test data
    cy.get('[data-cy=test-input-value]').type('5000');
    cy.get('[data-cy=test-timestamp]').type('2024-01-01T12:00:00');
    
    // Execute test
    cy.get('[data-cy=execute-test-button]').click();
    cy.waitForApi('testExecution');
    
    // Verify test results are displayed
    cy.get('[data-cy=test-results]').should('be.visible');
    cy.get('[data-cy=test-result-status]').should('contain', 'Normal');
    cy.get('[data-cy=test-confidence]').should('be.visible');
  });

  it('should filter logics by detection type', () => {
    cy.visit('/detection-logics');
    cy.waitForApi('getDetectionLogics');
    
    // Filter by OutOfRange detection type
    cy.get('[data-cy=detection-type-filter]').select('OutOfRange');
    
    // Verify all displayed logics are OutOfRange type
    cy.get('[data-cy=logic-detection-type]').each($element => {
      cy.wrap($element).should('contain', 'OutOfRange');
    });
    
    // Reset filter
    cy.get('[data-cy=reset-filters-button]').click();
  });

  it('should filter logics by status', () => {
    cy.visit('/detection-logics');
    cy.waitForApi('getDetectionLogics');
    
    // Filter by Approved status
    cy.get('[data-cy=status-filter]').select('Approved');
    
    // Verify all displayed logics are Approved
    cy.get('[data-cy=logic-status]').each($element => {
      cy.wrap($element).should('contain', 'Approved');
    });
  });

  it('should filter logics by ASIL level', () => {
    cy.visit('/detection-logics');
    cy.waitForApi('getDetectionLogics');
    
    // Filter by ASIL B level
    cy.get('[data-cy=asil-level-filter]').select('B');
    
    // Verify all displayed logics are ASIL B
    cy.get('[data-cy=logic-asil-level]').each($element => {
      cy.wrap($element).should('contain', 'B');
    });
  });

  it('should validate detection logic form', () => {
    cy.visit('/detection-logics/create');
    
    // Try to save without required fields
    cy.get('[data-cy=save-button]').click();
    
    // Check validation errors
    cy.get('[data-cy=logic-name-error]').should('be.visible');
    cy.get('[data-cy=detection-type-error]').should('be.visible');
    
    // Fill required fields
    cy.get('[data-cy=logic-name]').type('ValidLogic');
    cy.get('[data-cy=detection-type]').select('OutOfRange');
    
    // Validation errors should disappear
    cy.get('[data-cy=logic-name-error]').should('not.exist');
    cy.get('[data-cy=detection-type-error]').should('not.exist');
  });

  it('should display logic execution statistics', () => {
    cy.visit('/detection-logics');
    cy.waitForApi('getDetectionLogics');
    
    // Click on first logic to view details
    cy.get('[data-cy=view-logic-button]').first().click();
    
    // Verify logic details page
    cy.url().should('include', '/detection-logics/');
    cy.get('[data-cy=logic-details]').should('be.visible');
    
    // Check execution statistics
    cy.get('[data-cy=execution-count]').should('be.visible');
    cy.get('[data-cy=average-execution-time]').should('be.visible');
    cy.get('[data-cy=last-executed-at]').should('be.visible');
  });

  it('should manage logic parameters', () => {
    cy.visit('/detection-logics/create');
    
    // Fill basic logic information
    cy.get('[data-cy=logic-name]').type('ParameterTestLogic');
    cy.get('[data-cy=detection-type]').select('OutOfRange');
    
    // Add parameters
    cy.get('[data-cy=add-parameter-button]').click();
    
    cy.get('[data-cy=parameter-name]').type('MinThreshold');
    cy.get('[data-cy=parameter-type]').select('Double');
    cy.get('[data-cy=parameter-default-value]').type('0.0');
    cy.get('[data-cy=parameter-description]').type('Minimum threshold value');
    
    cy.get('[data-cy=save-parameter-button]').click();
    
    // Verify parameter was added
    cy.get('[data-cy=parameter-list]').should('contain', 'MinThreshold');
    
    // Add another parameter
    cy.get('[data-cy=add-parameter-button]').click();
    
    cy.get('[data-cy=parameter-name]').type('MaxThreshold');
    cy.get('[data-cy=parameter-type]').select('Double');
    cy.get('[data-cy=parameter-default-value]').type('100.0');
    cy.get('[data-cy=parameter-description]').type('Maximum threshold value');
    
    cy.get('[data-cy=save-parameter-button]').click();
    
    // Verify both parameters are listed
    cy.get('[data-cy=parameter-list]').should('contain', 'MinThreshold');
    cy.get('[data-cy=parameter-list]').should('contain', 'MaxThreshold');
  });

  it('should clone an existing logic', () => {
    cy.visit('/detection-logics');
    cy.waitForApi('getDetectionLogics');
    
    // Intercept clone request
    cy.intercept('POST', '/api/app/detection-logic/*/clone', { fixture: 'detection-logic-cloned.json' }).as('cloneLogic');
    
    // Click clone button on first logic
    cy.get('[data-cy=clone-logic-button]').first().click();
    
    // Enter new name for cloned logic
    cy.get('[data-cy=clone-name]').type('ClonedLogic');
    cy.get('[data-cy=confirm-clone-button]').click();
    
    cy.waitForApi('cloneLogic');
    
    // Verify success message
    cy.get('[data-cy=success-message]').should('be.visible');
    cy.get('[data-cy=success-message]').should('contain', 'Logic cloned successfully');
  });

  it('should handle logic sharing levels', () => {
    cy.visit('/detection-logics/create');
    
    // Fill basic information
    cy.get('[data-cy=logic-name]').type('SharedLogic');
    cy.get('[data-cy=detection-type]').select('OutOfRange');
    
    // Set sharing level
    cy.get('[data-cy=sharing-level]').select('Industry');
    
    // Verify sharing level warning
    cy.get('[data-cy=sharing-warning]').should('be.visible');
    cy.get('[data-cy=sharing-warning]').should('contain', 'This logic will be shared with industry partners');
    
    // Change to private
    cy.get('[data-cy=sharing-level]').select('Private');
    cy.get('[data-cy=sharing-warning]').should('not.exist');
  });
});
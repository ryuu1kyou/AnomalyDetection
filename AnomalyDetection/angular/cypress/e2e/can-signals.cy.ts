describe('CAN Signal Management', () => {
  beforeEach(() => {
    // Intercept API calls
    cy.intercept('GET', '/api/app/can-signal*', { fixture: 'can-signals.json' }).as('getCanSignals');
    cy.intercept('POST', '/api/app/can-signal', { fixture: 'can-signal-created.json' }).as('createCanSignal');
    cy.intercept('PUT', '/api/app/can-signal/*', { fixture: 'can-signal-updated.json' }).as('updateCanSignal');
    cy.intercept('DELETE', '/api/app/can-signal/*', {}).as('deleteCanSignal');
    
    // Login as admin user
    cy.login('admin', 'admin123');
    
    // Switch to Toyota tenant
    cy.switchTenant('Toyota');
  });

  it('should display CAN signals list', () => {
    cy.visit('/can-signals');
    
    // Wait for signals to load
    cy.waitForApi('getCanSignals');
    
    // Check if table is displayed
    cy.get('[data-cy=can-signals-table]').should('be.visible');
    
    // Check if signals are displayed
    cy.get('[data-cy=signal-row]').should('have.length.greaterThan', 0);
    
    // Check table headers
    cy.get('[data-cy=signal-name-header]').should('contain', 'Signal Name');
    cy.get('[data-cy=can-id-header]').should('contain', 'CAN ID');
    cy.get('[data-cy=system-type-header]').should('contain', 'System Type');
    cy.get('[data-cy=status-header]').should('contain', 'Status');
  });

  it('should create a new CAN signal', () => {
    const newSignal = {
      signalName: 'TestEngineRPM',
      canId: '0x123',
      description: 'Test engine RPM signal for E2E testing',
      systemType: 'Engine',
      startBit: 0,
      length: 16,
      minValue: 0,
      maxValue: 8000
    };

    cy.createCanSignal(newSignal);
    
    // Verify signal was created
    cy.waitForApi('createCanSignal');
    cy.url().should('include', '/can-signals');
    
    // Check if signal appears in the list
    cy.get('[data-cy=can-signals-table]').should('contain', newSignal.signalName);
  });

  it('should edit an existing CAN signal', () => {
    cy.visit('/can-signals');
    cy.waitForApi('getCanSignals');
    
    // Click edit button on first signal
    cy.get('[data-cy=edit-signal-button]').first().click();
    
    // Update signal description
    cy.get('[data-cy=description]').clear().type('Updated description for E2E test');
    
    // Save changes
    cy.get('[data-cy=save-button]').click();
    cy.waitForApi('updateCanSignal');
    
    // Verify success message
    cy.get('[data-cy=success-message]').should('be.visible');
    cy.get('[data-cy=success-message]').should('contain', 'Signal updated successfully');
  });

  it('should delete a CAN signal', () => {
    cy.visit('/can-signals');
    cy.waitForApi('getCanSignals');
    
    // Get initial count of signals
    cy.get('[data-cy=signal-row]').then($rows => {
      const initialCount = $rows.length;
      
      // Click delete button on first signal
      cy.get('[data-cy=delete-signal-button]').first().click();
      
      // Confirm deletion in dialog
      cy.get('[data-cy=confirm-delete-button]').click();
      cy.waitForApi('deleteCanSignal');
      
      // Verify signal was removed
      cy.get('[data-cy=success-message]').should('be.visible');
      cy.get('[data-cy=signal-row]').should('have.length', initialCount - 1);
    });
  });

  it('should search and filter CAN signals', () => {
    cy.visit('/can-signals');
    cy.waitForApi('getCanSignals');
    
    // Test search functionality
    cy.get('[data-cy=search-input]').type('Engine');
    cy.get('[data-cy=search-button]').click();
    
    // Verify filtered results
    cy.get('[data-cy=signal-row]').each($row => {
      cy.wrap($row).should('contain.text', 'Engine');
    });
    
    // Clear search
    cy.get('[data-cy=clear-search-button]').click();
    cy.get('[data-cy=search-input]').should('have.value', '');
  });

  it('should filter by system type', () => {
    cy.visit('/can-signals');
    cy.waitForApi('getCanSignals');
    
    // Filter by Engine system type
    cy.get('[data-cy=system-type-filter]').select('Engine');
    
    // Verify all displayed signals are Engine type
    cy.get('[data-cy=signal-system-type]').each($element => {
      cy.wrap($element).should('contain', 'Engine');
    });
    
    // Reset filter
    cy.get('[data-cy=reset-filters-button]').click();
  });

  it('should validate CAN signal form', () => {
    cy.visit('/can-signals/create');
    
    // Try to save without required fields
    cy.get('[data-cy=save-button]').click();
    
    // Check validation errors
    cy.get('[data-cy=signal-name-error]').should('be.visible');
    cy.get('[data-cy=can-id-error]').should('be.visible');
    
    // Fill required fields
    cy.get('[data-cy=signal-name]').type('ValidSignal');
    cy.get('[data-cy=can-id]').type('0x999');
    
    // Validation errors should disappear
    cy.get('[data-cy=signal-name-error]').should('not.exist');
    cy.get('[data-cy=can-id-error]').should('not.exist');
  });

  it('should handle CAN ID conflicts', () => {
    cy.visit('/can-signals/create');
    
    // Try to create signal with existing CAN ID
    cy.get('[data-cy=signal-name]').type('ConflictSignal');
    cy.get('[data-cy=can-id]').type('0x123'); // Assuming this ID already exists
    cy.get('[data-cy=description]').type('Signal with conflicting CAN ID');
    cy.get('[data-cy=system-type]').select('Engine');
    
    // Intercept conflict response
    cy.intercept('POST', '/api/app/can-signal', {
      statusCode: 400,
      body: { error: { message: 'CAN ID already exists' } }
    }).as('createConflictSignal');
    
    cy.get('[data-cy=save-button]').click();
    cy.waitForApi('createConflictSignal');
    
    // Check error message
    cy.get('[data-cy=error-message]').should('be.visible');
    cy.get('[data-cy=error-message]').should('contain', 'CAN ID already exists');
  });

  it('should display signal details', () => {
    cy.visit('/can-signals');
    cy.waitForApi('getCanSignals');
    
    // Click on first signal to view details
    cy.get('[data-cy=view-signal-button]').first().click();
    
    // Verify signal details page
    cy.url().should('include', '/can-signals/');
    cy.get('[data-cy=signal-details]').should('be.visible');
    
    // Check if all signal properties are displayed
    cy.get('[data-cy=signal-name-display]').should('be.visible');
    cy.get('[data-cy=can-id-display]').should('be.visible');
    cy.get('[data-cy=system-type-display]').should('be.visible');
    cy.get('[data-cy=specification-display]').should('be.visible');
  });

  it('should export CAN signals', () => {
    cy.visit('/can-signals');
    cy.waitForApi('getCanSignals');
    
    // Intercept export request
    cy.intercept('GET', '/api/app/can-signal/export*', { fixture: 'can-signals-export.csv' }).as('exportSignals');
    
    // Click export button
    cy.get('[data-cy=export-button]').click();
    cy.get('[data-cy=export-csv-button]').click();
    
    cy.waitForApi('exportSignals');
    
    // Verify download was initiated (this is browser-dependent)
    cy.get('[data-cy=export-success-message]').should('be.visible');
  });

  it('should handle pagination', () => {
    cy.visit('/can-signals');
    cy.waitForApi('getCanSignals');
    
    // Check if pagination is visible (assuming more than 10 signals)
    cy.get('[data-cy=pagination]').should('be.visible');
    
    // Go to next page
    cy.get('[data-cy=next-page-button]').click();
    
    // Verify URL contains page parameter
    cy.url().should('include', 'page=2');
    
    // Go back to first page
    cy.get('[data-cy=first-page-button]').click();
    cy.url().should('include', 'page=1');
  });
});
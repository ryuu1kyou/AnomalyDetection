describe('Multi-Tenancy Functionality', () => {
  beforeEach(() => {
    // Intercept API calls
    cy.intercept('GET', '/api/app/tenant*', { fixture: 'tenants.json' }).as('getTenants');
    cy.intercept('POST', '/api/app/tenant/switch/*', { statusCode: 200 }).as('switchTenant');
    cy.intercept('GET', '/api/app/tenant/*/data-isolation', { fixture: 'data-isolation-result.json' }).as('verifyDataIsolation');
    cy.intercept('GET', '/api/app/can-signal*', { fixture: 'toyota-can-signals.json' }).as('getToyotaSignals');
    cy.intercept('GET', '/api/app/can-signal*', { fixture: 'honda-can-signals.json' }).as('getHondaSignals');
    
    // Login as admin user
    cy.login('admin', 'admin123');
  });

  it('should display available tenants in selector', () => {
    cy.visit('/dashboard');
    
    // Wait for tenants to load
    cy.waitForApi('getTenants');
    
    // Check tenant selector is visible
    cy.get('[data-cy=tenant-selector]').should('be.visible');
    
    // Click to open tenant dropdown
    cy.get('[data-cy=tenant-selector]').click();
    
    // Verify tenant options are displayed
    cy.get('[data-cy=tenant-option-Toyota]').should('be.visible');
    cy.get('[data-cy=tenant-option-Honda]').should('be.visible');
    cy.get('[data-cy=tenant-option-Nissan]').should('be.visible');
    
    // Check current tenant is displayed
    cy.get('[data-cy=current-tenant]').should('be.visible');
  });

  it('should switch between tenants successfully', () => {
    cy.visit('/dashboard');
    cy.waitForApi('getTenants');
    
    // Switch to Toyota tenant
    cy.switchTenant('Toyota');
    cy.waitForApi('switchTenant');
    
    // Verify current tenant changed
    cy.get('[data-cy=current-tenant]').should('contain', 'Toyota');
    
    // Switch to Honda tenant
    cy.switchTenant('Honda');
    cy.waitForApi('switchTenant');
    
    // Verify current tenant changed
    cy.get('[data-cy=current-tenant]').should('contain', 'Honda');
  });

  it('should show different data for different tenants', () => {
    cy.visit('/can-signals');
    
    // Start with Toyota tenant
    cy.switchTenant('Toyota');
    cy.waitForApi('switchTenant');
    
    // Intercept Toyota-specific data
    cy.intercept('GET', '/api/app/can-signal*', { fixture: 'toyota-can-signals.json' }).as('getToyotaSignals');
    
    // Reload page to get Toyota data
    cy.reload();
    cy.waitForApi('getToyotaSignals');
    
    // Verify Toyota-specific signals
    cy.get('[data-cy=can-signals-table]').should('contain', 'Toyota_EngineRPM');
    cy.get('[data-cy=can-signals-table]').should('not.contain', 'Honda_EngineRPM');
    
    // Switch to Honda tenant
    cy.switchTenant('Honda');
    cy.waitForApi('switchTenant');
    
    // Intercept Honda-specific data
    cy.intercept('GET', '/api/app/can-signal*', { fixture: 'honda-can-signals.json' }).as('getHondaSignals');
    
    // Reload page to get Honda data
    cy.reload();
    cy.waitForApi('getHondaSignals');
    
    // Verify Honda-specific signals
    cy.get('[data-cy=can-signals-table]').should('contain', 'Honda_EngineRPM');
    cy.get('[data-cy=can-signals-table]').should('not.contain', 'Toyota_EngineRPM');
  });

  it('should maintain tenant context across navigation', () => {
    cy.visit('/dashboard');
    cy.waitForApi('getTenants');
    
    // Switch to Toyota tenant
    cy.switchTenant('Toyota');
    cy.waitForApi('switchTenant');
    
    // Navigate to different pages
    cy.visit('/can-signals');
    cy.get('[data-cy=current-tenant]').should('contain', 'Toyota');
    
    cy.visit('/detection-logics');
    cy.get('[data-cy=current-tenant]').should('contain', 'Toyota');
    
    cy.visit('/detection-results');
    cy.get('[data-cy=current-tenant]').should('contain', 'Toyota');
    
    // Tenant should remain consistent across all pages
  });

  it('should verify data isolation between tenants', () => {
    cy.visit('/admin/data-isolation');
    
    // Switch to Toyota tenant
    cy.switchTenant('Toyota');
    cy.waitForApi('switchTenant');
    
    // Trigger data isolation verification
    cy.get('[data-cy=verify-isolation-button]').click();
    cy.waitForApi('verifyDataIsolation');
    
    // Check isolation results
    cy.get('[data-cy=isolation-results]').should('be.visible');
    cy.get('[data-cy=isolation-status]').should('contain', 'Data isolation verified');
    cy.get('[data-cy=tenant-data-count]').should('be.visible');
    
    // Switch to Honda and verify again
    cy.switchTenant('Honda');
    cy.waitForApi('switchTenant');
    
    cy.get('[data-cy=verify-isolation-button]').click();
    cy.waitForApi('verifyDataIsolation');
    
    // Results should be different for Honda
    cy.get('[data-cy=isolation-results]').should('be.visible');
    cy.get('[data-cy=isolation-status]').should('contain', 'Data isolation verified');
  });

  it('should handle tenant switching errors gracefully', () => {
    cy.visit('/dashboard');
    cy.waitForApi('getTenants');
    
    // Intercept switch request with error
    cy.intercept('POST', '/api/app/tenant/switch/*', {
      statusCode: 403,
      body: { error: { message: 'Access denied to this tenant' } }
    }).as('switchTenantError');
    
    // Try to switch to restricted tenant
    cy.get('[data-cy=tenant-selector]').click();
    cy.get('[data-cy=tenant-option-RestrictedTenant]').click();
    
    cy.waitForApi('switchTenantError');
    
    // Verify error message is displayed
    cy.get('[data-cy=error-message]').should('be.visible');
    cy.get('[data-cy=error-message]').should('contain', 'Access denied');
    
    // Verify tenant didn't change
    cy.get('[data-cy=current-tenant]').should('not.contain', 'RestrictedTenant');
  });

  it('should show tenant-specific features', () => {
    cy.visit('/dashboard');
    cy.waitForApi('getTenants');
    
    // Switch to Toyota tenant (premium features)
    cy.switchTenant('Toyota');
    cy.waitForApi('switchTenant');
    
    // Check premium features are available
    cy.get('[data-cy=advanced-analytics-menu]').should('be.visible');
    cy.get('[data-cy=export-features]').should('be.visible');
    
    // Switch to basic tenant
    cy.switchTenant('BasicTenant');
    cy.waitForApi('switchTenant');
    
    // Check premium features are hidden
    cy.get('[data-cy=advanced-analytics-menu]').should('not.exist');
    cy.get('[data-cy=export-features]').should('not.exist');
  });

  it('should display tenant information in header', () => {
    cy.visit('/dashboard');
    cy.waitForApi('getTenants');
    
    // Switch to Toyota tenant
    cy.switchTenant('Toyota');
    cy.waitForApi('switchTenant');
    
    // Check tenant info in header
    cy.get('[data-cy=tenant-info]').should('be.visible');
    cy.get('[data-cy=tenant-name]').should('contain', 'Toyota');
    cy.get('[data-cy=tenant-oem-code]').should('contain', 'TOYOTA');
    
    // Check tenant logo/icon if present
    cy.get('[data-cy=tenant-logo]').should('be.visible');
  });

  it('should handle tenant data loading states', () => {
    cy.visit('/can-signals');
    
    // Intercept with delay to test loading state
    cy.intercept('GET', '/api/app/can-signal*', { 
      delay: 2000,
      fixture: 'can-signals.json' 
    }).as('getSignalsWithDelay');
    
    // Switch tenant to trigger data reload
    cy.switchTenant('Toyota');
    cy.waitForApi('switchTenant');
    
    // Check loading indicator is shown
    cy.get('[data-cy=loading-indicator]').should('be.visible');
    
    // Wait for data to load
    cy.waitForApi('getSignalsWithDelay');
    
    // Loading indicator should disappear
    cy.get('[data-cy=loading-indicator]').should('not.exist');
    cy.get('[data-cy=can-signals-table]').should('be.visible');
  });

  it('should preserve user preferences per tenant', () => {
    cy.visit('/can-signals');
    cy.waitForApi('getTenants');
    
    // Switch to Toyota tenant
    cy.switchTenant('Toyota');
    cy.waitForApi('switchTenant');
    
    // Set some preferences (e.g., table page size)
    cy.get('[data-cy=page-size-selector]').select('25');
    cy.get('[data-cy=sort-column]').click(); // Sort by name
    
    // Switch to Honda tenant
    cy.switchTenant('Honda');
    cy.waitForApi('switchTenant');
    
    // Set different preferences
    cy.get('[data-cy=page-size-selector]').select('50');
    
    // Switch back to Toyota
    cy.switchTenant('Toyota');
    cy.waitForApi('switchTenant');
    
    // Verify Toyota preferences are restored
    cy.get('[data-cy=page-size-selector]').should('have.value', '25');
  });

  it('should show tenant statistics in dashboard', () => {
    cy.visit('/dashboard');
    cy.waitForApi('getTenants');
    
    // Intercept tenant statistics
    cy.intercept('GET', '/api/app/tenant/*/statistics', { fixture: 'tenant-statistics.json' }).as('getTenantStats');
    
    // Switch to Toyota tenant
    cy.switchTenant('Toyota');
    cy.waitForApi('switchTenant');
    cy.waitForApi('getTenantStats');
    
    // Check tenant-specific statistics
    cy.get('[data-cy=tenant-stats]').should('be.visible');
    cy.get('[data-cy=signal-count]').should('be.visible');
    cy.get('[data-cy=logic-count]').should('be.visible');
    cy.get('[data-cy=result-count]').should('be.visible');
    cy.get('[data-cy=last-activity]').should('be.visible');
  });
});
// Cypress E2E placeholder for Req4 template create-from flow
// NOTE: Adjust selectors once integrated into nav/menu.

describe('Detection Templates Create From Template Flow', () => {
  it('loads list, opens create form, validates and submits', () => {
    // Visit list
    cy.visit('/detection-templates');
    cy.contains('template', { matchCase: false });

    // Navigate to create (assumes button or link exists)
    cy.contains(/create/i).click({ force: true });

    // Wait template params load (simplistic heuristic)
    cy.get('form').should('exist');

    // Optionally adjust a numeric parameter if present
    cy.get('input').first().then($el => {
      if ($el.attr('type') !== 'hidden') {
        cy.wrap($el).clear().type('123');
      }
    });

    // Validate parameters if button present
    cy.contains(/validate|検証/i).then(($el: JQuery<HTMLElement>) => {
      if ($el && $el.length > 0) {
        cy.wrap($el).click({ force: true });
      }
    });

    // Submit (create) – fuzzy match
    cy.contains(/create|生成|作成/i).click({ force: true });

    // Redirect expectation
    cy.url().should('include', '/detection-logics');
  });
});

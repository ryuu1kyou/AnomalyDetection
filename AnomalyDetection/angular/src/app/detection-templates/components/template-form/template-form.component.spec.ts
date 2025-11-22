import { TestBed } from '@angular/core/testing';
import { TemplateFormComponent } from './template-form.component';
import { DetectionTemplatesService } from '../../services/detection-templates.service';
import { of } from 'rxjs';
import { expect } from 'chai';

describe('TemplateFormComponent', () => {
  let component: TemplateFormComponent;
  let svc: jasmine.SpyObj<DetectionTemplatesService>;

  beforeEach(() => {
    svc = jasmine.createSpyObj('DetectionTemplatesService', ['getByType', 'createFromTemplate', 'validateParameters']);
    svc.getByType.and.returnValue(of({
      templateType: 1,
      name: 'Timeout Template',
      parameters: [
        { name: 'timeoutMs', type: 'number', required: true, min: 10, max: 10000, defaultValue: 500 } as any
      ]
    }));
    svc.validateParameters.and.returnValue(of({ isValid: true, errors: [], warnings: [] }));
    svc.createFromTemplate.and.returnValue(of({ id: 'new-id' }));
    TestBed.configureTestingModule({
      imports: [TemplateFormComponent],
      providers: [{ provide: DetectionTemplatesService, useValue: svc }]
    });
    const fixture = TestBed.createComponent(TemplateFormComponent);
    component = fixture.componentInstance;
    // simulate query param load
    component['template']!.set({
      templateType: 1,
      name: 'Timeout Template',
      parameters: [
        { name: 'timeoutMs', type: 'number', required: true, min: 10, max: 10000, defaultValue: 500 } as any
      ]
    } as any);
    fixture.detectChanges();
  });

  it('should build form validators', () => {
    expect(component.form).to.exist;
  });

  it('should validate parameters successfully', () => {
    component.validate();
    expect(svc.validateParameters.calls.count()).to.be.greaterThan(0);
  });
});

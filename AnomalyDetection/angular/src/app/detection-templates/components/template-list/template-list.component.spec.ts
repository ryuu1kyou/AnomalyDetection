import { TemplateListComponent } from './template-list.component';
import { DetectionTemplatesService } from '../../services/detection-templates.service';
import { of } from 'rxjs';
import { TestBed } from '@angular/core/testing';
import { expect } from 'chai';

describe('TemplateListComponent', () => {
  let component: TemplateListComponent;
  let svcSpy: jasmine.SpyObj<DetectionTemplatesService>;

  beforeEach(() => {
    svcSpy = jasmine.createSpyObj('DetectionTemplatesService', ['getAvailable']);
    svcSpy.getAvailable.and.returnValue(of([]));

    TestBed.configureTestingModule({
      imports: [TemplateListComponent],
      providers: [{ provide: DetectionTemplatesService, useValue: svcSpy }]
    });

    const fixture = TestBed.createComponent(TemplateListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).to.not.be.undefined;
  });

  it('loads templates on init', () => {
    component.ngOnInit();
    expect(svcSpy.getAvailable.calls.count()).to.be.greaterThan(0);
  });
});

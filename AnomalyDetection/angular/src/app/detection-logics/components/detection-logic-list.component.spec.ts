import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { Router } from '@angular/router';
import { of } from 'rxjs';

import { DetectionLogicListComponent } from './detection-logic-list.component';
import { DetectionLogicService } from '../services/detection-logic.service';
import { 
  CanAnomalyDetectionLogicDto, 
  DetectionType, 
  DetectionLogicStatus, 
  SharingLevel, 
  AsilLevel,
  CanSystemType 
} from '../models/detection-logic.model';

describe('DetectionLogicListComponent', () => {
  let component: DetectionLogicListComponent;
  let fixture: ComponentFixture<DetectionLogicListComponent>;
  let mockDetectionLogicService: jasmine.SpyObj<DetectionLogicService>;
  let mockRouter: jasmine.SpyObj<Router>;

  const mockDetectionLogics: CanAnomalyDetectionLogicDto[] = [
    {
      id: '1',
      name: 'Engine RPM Range Check',
      description: 'Checks if engine RPM is within normal range',
      detectionType: DetectionType.OutOfRange,
      status: DetectionLogicStatus.Approved,
      sharingLevel: SharingLevel.Private,
      asilLevel: AsilLevel.B,
      systemType: CanSystemType.Engine,
      logicContent: '{"min": 0, "max": 8000}',
      oemCode: 'TOYOTA',
      version: '1.0',
      executionCount: 1500,
      lastExecutedAt: new Date(),
      averageExecutionTime: 2.5,
      approvedBy: 'admin',
      approvedAt: new Date(),
      approvalNotes: 'Approved for production use',
      parameters: [],
      signalMappings: [],
      creationTime: new Date(),
      lastModificationTime: new Date()
    },
    {
      id: '2',
      name: 'Brake Pressure Timeout',
      description: 'Detects brake pressure signal timeout',
      detectionType: DetectionType.Timeout,
      status: DetectionLogicStatus.Draft,
      sharingLevel: SharingLevel.OemPartner,
      asilLevel: AsilLevel.C,
      systemType: CanSystemType.Brake,
      logicContent: '{"timeout": 200}',
      oemCode: 'TOYOTA',
      version: '1.0',
      executionCount: 0,
      lastExecutedAt: null,
      averageExecutionTime: 0,
      approvedBy: null,
      approvedAt: null,
      approvalNotes: null,
      parameters: [],
      signalMappings: [],
      creationTime: new Date(),
      lastModificationTime: new Date()
    }
  ];

  beforeEach(async () => {
    const detectionLogicServiceSpy = jasmine.createSpyObj('DetectionLogicService', [
      'getDetectionLogics',
      'deleteDetectionLogic',
      'submitForApproval',
      'approveLogic',
      'rejectLogic',
      'testExecution'
    ]);
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      declarations: [DetectionLogicListComponent],
      imports: [
        MatTableModule,
        MatPaginatorModule,
        MatSortModule,
        MatFormFieldModule,
        MatInputModule,
        MatButtonModule,
        MatIconModule,
        MatChipsModule,
        NoopAnimationsModule
      ],
      providers: [
        { provide: DetectionLogicService, useValue: detectionLogicServiceSpy },
        { provide: Router, useValue: routerSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(DetectionLogicListComponent);
    component = fixture.componentInstance;
    mockDetectionLogicService = TestBed.inject(DetectionLogicService) as jasmine.SpyObj<DetectionLogicService>;
    mockRouter = TestBed.inject(Router) as jasmine.SpyObj<Router>;

    // Setup service responses
    mockDetectionLogicService.getDetectionLogics.and.returnValue(of({
      items: mockDetectionLogics,
      totalCount: mockDetectionLogics.length
    }));
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load detection logics on init', () => {
    component.ngOnInit();
    
    expect(mockDetectionLogicService.getDetectionLogics).toHaveBeenCalled();
    expect(component.dataSource.data).toEqual(mockDetectionLogics);
  });

  it('should display detection logics in table', () => {
    component.ngOnInit();
    fixture.detectChanges();

    const compiled = fixture.nativeElement;
    expect(compiled.textContent).toContain('Engine RPM Range Check');
    expect(compiled.textContent).toContain('Brake Pressure Timeout');
  });

  it('should navigate to create logic page', () => {
    component.createLogic();

    expect(mockRouter.navigate).toHaveBeenCalledWith(['/detection-logics/create']);
  });

  it('should navigate to edit logic page', () => {
    const logic = mockDetectionLogics[0];
    
    component.editLogic(logic);

    expect(mockRouter.navigate).toHaveBeenCalledWith(['/detection-logics/edit', logic.id]);
  });

  it('should delete logic after confirmation', () => {
    const logic = mockDetectionLogics[0];
    mockDetectionLogicService.deleteDetectionLogic.and.returnValue(of(void 0));
    spyOn(window, 'confirm').and.returnValue(true);

    component.deleteLogic(logic);

    expect(window.confirm).toHaveBeenCalled();
    expect(mockDetectionLogicService.deleteDetectionLogic).toHaveBeenCalledWith(logic.id);
  });

  it('should submit logic for approval', () => {
    const logic = mockDetectionLogics[1]; // Draft status
    mockDetectionLogicService.submitForApproval.and.returnValue(of(void 0));

    component.submitForApproval(logic);

    expect(mockDetectionLogicService.submitForApproval).toHaveBeenCalledWith(logic.id);
  });

  it('should approve logic', () => {
    const logic = mockDetectionLogics[1];
    mockDetectionLogicService.approveLogic.and.returnValue(of(void 0));
    spyOn(window, 'prompt').and.returnValue('Approved for testing');

    component.approveLogic(logic);

    expect(window.prompt).toHaveBeenCalled();
    expect(mockDetectionLogicService.approveLogic).toHaveBeenCalledWith(logic.id, 'Approved for testing');
  });

  it('should reject logic', () => {
    const logic = mockDetectionLogics[1];
    mockDetectionLogicService.rejectLogic.and.returnValue(of(void 0));
    spyOn(window, 'prompt').and.returnValue('Needs more testing');

    component.rejectLogic(logic);

    expect(window.prompt).toHaveBeenCalled();
    expect(mockDetectionLogicService.rejectLogic).toHaveBeenCalledWith(logic.id, 'Needs more testing');
  });

  it('should test logic execution', () => {
    const logic = mockDetectionLogics[0];
    const testData = { inputValue: 5000, timestamp: new Date() };
    mockDetectionLogicService.testExecution.and.returnValue(of({ result: 'normal', confidence: 0.95 }));

    component.testExecution(logic, testData);

    expect(mockDetectionLogicService.testExecution).toHaveBeenCalledWith(logic.id, testData);
  });

  it('should show status badge with correct color', () => {
    component.ngOnInit();
    fixture.detectChanges();

    const statusBadges = fixture.nativeElement.querySelectorAll('.status-badge');
    expect(statusBadges.length).toBeGreaterThan(0);
  });

  it('should show ASIL level badge', () => {
    component.ngOnInit();
    fixture.detectChanges();

    const asilBadges = fixture.nativeElement.querySelectorAll('.asil-badge');
    expect(asilBadges.length).toBeGreaterThan(0);
  });

  it('should show sharing level indicator', () => {
    component.ngOnInit();
    fixture.detectChanges();

    const sharingIndicators = fixture.nativeElement.querySelectorAll('.sharing-level');
    expect(sharingIndicators.length).toBeGreaterThan(0);
  });

  it('should filter by detection type', () => {
    component.filterByDetectionType(DetectionType.OutOfRange);

    expect(mockDetectionLogicService.getDetectionLogics).toHaveBeenCalledWith({
      skipCount: 0,
      maxResultCount: 10,
      detectionType: DetectionType.OutOfRange
    });
  });

  it('should filter by status', () => {
    component.filterByStatus(DetectionLogicStatus.Approved);

    expect(mockDetectionLogicService.getDetectionLogics).toHaveBeenCalledWith({
      skipCount: 0,
      maxResultCount: 10,
      status: DetectionLogicStatus.Approved
    });
  });

  it('should filter by ASIL level', () => {
    component.filterByAsilLevel(AsilLevel.B);

    expect(mockDetectionLogicService.getDetectionLogics).toHaveBeenCalledWith({
      skipCount: 0,
      maxResultCount: 10,
      asilLevel: AsilLevel.B
    });
  });

  it('should handle pagination changes', () => {
    const pageEvent = { pageIndex: 1, pageSize: 10, length: 20 };
    
    component.onPageChange(pageEvent);

    expect(mockDetectionLogicService.getDetectionLogics).toHaveBeenCalledWith({
      skipCount: 10,
      maxResultCount: 10
    });
  });

  it('should show execution statistics', () => {
    component.ngOnInit();
    fixture.detectChanges();

    const compiled = fixture.nativeElement;
    expect(compiled.textContent).toContain('1500'); // execution count
    expect(compiled.textContent).toContain('2.5'); // average execution time
  });

  it('should handle empty results', () => {
    mockDetectionLogicService.getDetectionLogics.and.returnValue(of({
      items: [],
      totalCount: 0
    }));

    component.ngOnInit();

    expect(component.dataSource.data).toEqual([]);
  });

  it('should show action buttons based on status', () => {
    component.ngOnInit();
    fixture.detectChanges();

    const actionButtons = fixture.nativeElement.querySelectorAll('.action-button');
    expect(actionButtons.length).toBeGreaterThan(0);
  });
});
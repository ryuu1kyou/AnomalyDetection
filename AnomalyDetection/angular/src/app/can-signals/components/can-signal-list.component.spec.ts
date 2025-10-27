import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { Router } from '@angular/router';
import { of } from 'rxjs';

import { CanSignalListComponent } from './can-signal-list.component';
import { CanSignalService } from '../services/can-signal.service';
import { CanSignalDto, CanSystemType, SignalDataType, SignalByteOrder } from '../models/can-signal.model';

describe('CanSignalListComponent', () => {
  let component: CanSignalListComponent;
  let fixture: ComponentFixture<CanSignalListComponent>;
  let mockCanSignalService: jasmine.SpyObj<CanSignalService>;
  let mockRouter: jasmine.SpyObj<Router>;

  const mockCanSignals: CanSignalDto[] = [
    {
      id: '1',
      signalName: 'EngineRPM',
      canId: '0x123',
      description: 'Engine RPM signal',
      systemType: CanSystemType.Engine,
      startBit: 0,
      length: 16,
      dataType: SignalDataType.Unsigned,
      minValue: 0,
      maxValue: 8000,
      factor: 1.0,
      offset: 0.0,
      unit: 'rpm',
      cycleTime: 100,
      timeoutTime: 500,
      byteOrder: SignalByteOrder.Motorola,
      oemCode: 'TOYOTA',
      isStandard: false,
      status: 'Active',
      version: '1.0',
      effectiveDate: new Date(),
      sourceDocument: 'CAN_Spec_v1.0.xlsx',
      notes: 'Primary engine RPM signal',
      creationTime: new Date(),
      lastModificationTime: new Date()
    },
    {
      id: '2',
      signalName: 'BrakePressure',
      canId: '0x456',
      description: 'Brake pressure signal',
      systemType: CanSystemType.Brake,
      startBit: 0,
      length: 12,
      dataType: SignalDataType.Unsigned,
      minValue: 0,
      maxValue: 200,
      factor: 0.1,
      offset: 0.0,
      unit: 'bar',
      cycleTime: 50,
      timeoutTime: 200,
      byteOrder: SignalByteOrder.Intel,
      oemCode: 'TOYOTA',
      isStandard: true,
      status: 'Active',
      version: '1.0',
      effectiveDate: new Date(),
      sourceDocument: 'CAN_Spec_v1.0.xlsx',
      notes: 'Standard brake pressure signal',
      creationTime: new Date(),
      lastModificationTime: new Date()
    }
  ];

  beforeEach(async () => {
    const canSignalServiceSpy = jasmine.createSpyObj('CanSignalService', [
      'getCanSignals',
      'deleteCanSignal',
      'searchCanSignals'
    ]);
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      declarations: [CanSignalListComponent],
      imports: [
        MatTableModule,
        MatPaginatorModule,
        MatSortModule,
        MatFormFieldModule,
        MatInputModule,
        MatButtonModule,
        MatIconModule,
        NoopAnimationsModule
      ],
      providers: [
        { provide: CanSignalService, useValue: canSignalServiceSpy },
        { provide: Router, useValue: routerSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CanSignalListComponent);
    component = fixture.componentInstance;
    mockCanSignalService = TestBed.inject(CanSignalService) as jasmine.SpyObj<CanSignalService>;
    mockRouter = TestBed.inject(Router) as jasmine.SpyObj<Router>;

    // Setup service responses
    mockCanSignalService.getCanSignals.and.returnValue(of({
      items: mockCanSignals,
      totalCount: mockCanSignals.length
    }));
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load CAN signals on init', () => {
    component.ngOnInit();
    
    expect(mockCanSignalService.getCanSignals).toHaveBeenCalled();
    expect(component.dataSource.data).toEqual(mockCanSignals);
  });

  it('should display CAN signals in table', () => {
    component.ngOnInit();
    fixture.detectChanges();

    const compiled = fixture.nativeElement;
    expect(compiled.textContent).toContain('EngineRPM');
    expect(compiled.textContent).toContain('BrakePressure');
    expect(compiled.textContent).toContain('0x123');
    expect(compiled.textContent).toContain('0x456');
  });

  it('should filter signals when search is performed', () => {
    mockCanSignalService.searchCanSignals.and.returnValue(of({
      items: [mockCanSignals[0]],
      totalCount: 1
    }));

    component.applyFilter('Engine');

    expect(mockCanSignalService.searchCanSignals).toHaveBeenCalledWith('Engine');
  });

  it('should navigate to create signal page', () => {
    component.createSignal();

    expect(mockRouter.navigate).toHaveBeenCalledWith(['/can-signals/create']);
  });

  it('should navigate to edit signal page', () => {
    const signal = mockCanSignals[0];
    
    component.editSignal(signal);

    expect(mockRouter.navigate).toHaveBeenCalledWith(['/can-signals/edit', signal.id]);
  });

  it('should delete signal after confirmation', () => {
    const signal = mockCanSignals[0];
    mockCanSignalService.deleteCanSignal.and.returnValue(of(void 0));
    spyOn(window, 'confirm').and.returnValue(true);

    component.deleteSignal(signal);

    expect(window.confirm).toHaveBeenCalled();
    expect(mockCanSignalService.deleteCanSignal).toHaveBeenCalledWith(signal.id);
  });

  it('should not delete signal if not confirmed', () => {
    const signal = mockCanSignals[0];
    spyOn(window, 'confirm').and.returnValue(false);

    component.deleteSignal(signal);

    expect(window.confirm).toHaveBeenCalled();
    expect(mockCanSignalService.deleteCanSignal).not.toHaveBeenCalled();
  });

  it('should handle pagination changes', () => {
    const pageEvent = { pageIndex: 1, pageSize: 10, length: 20 };
    
    component.onPageChange(pageEvent);

    expect(mockCanSignalService.getCanSignals).toHaveBeenCalledWith({
      skipCount: 10,
      maxResultCount: 10,
      sorting: undefined,
      filter: undefined
    });
  });

  it('should handle sorting changes', () => {
    const sortEvent = { active: 'signalName', direction: 'asc' };
    
    component.onSortChange(sortEvent);

    expect(mockCanSignalService.getCanSignals).toHaveBeenCalledWith({
      skipCount: 0,
      maxResultCount: 10,
      sorting: 'signalName asc',
      filter: undefined
    });
  });

  it('should show system type badge with correct color', () => {
    component.ngOnInit();
    fixture.detectChanges();

    const badges = fixture.nativeElement.querySelectorAll('.system-type-badge');
    expect(badges.length).toBeGreaterThan(0);
  });

  it('should show standard signal indicator', () => {
    component.ngOnInit();
    fixture.detectChanges();

    const standardIndicators = fixture.nativeElement.querySelectorAll('.standard-indicator');
    expect(standardIndicators.length).toBeGreaterThan(0);
  });

  it('should handle empty search results', () => {
    mockCanSignalService.searchCanSignals.and.returnValue(of({
      items: [],
      totalCount: 0
    }));

    component.applyFilter('NonExistent');

    expect(component.dataSource.data).toEqual([]);
  });

  it('should clear filter when search is empty', () => {
    component.applyFilter('');

    expect(mockCanSignalService.getCanSignals).toHaveBeenCalled();
  });
});
import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { DetectionTemplatesService } from './detection-templates.service';
import { expect } from 'chai';

describe('DetectionTemplatesService', () => {
  let service: DetectionTemplatesService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule]
    });
    service = TestBed.inject(DetectionTemplatesService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should request available templates', () => {
    service.getAvailable().subscribe(list => {
      expect(list).to.be.an('array');
    });
    const req = httpMock.expectOne('/api/detection-templates/available');
    expect(req.request.method).to.equal('GET');
    req.flush([]);
  });
});

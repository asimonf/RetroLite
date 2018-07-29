import { TestBed, inject } from '@angular/core/testing';

import { RetroLiteApiService } from './retro-lite-api.service';

describe('RetroLiteApiService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [RetroLiteApiService]
    });
  });

  it('should be created', inject([RetroLiteApiService], (service: RetroLiteApiService) => {
    expect(service).toBeTruthy();
  }));
});

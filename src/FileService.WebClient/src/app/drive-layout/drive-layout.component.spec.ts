import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DriveLayoutComponent } from './drive-layout.component';

describe('DriveLayoutComponent', () => {
  let component: DriveLayoutComponent;
  let fixture: ComponentFixture<DriveLayoutComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DriveLayoutComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(DriveLayoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

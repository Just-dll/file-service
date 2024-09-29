import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FolderPageComponent } from './folder-page.component';

describe('FolderComponent', () => {
  let component: FolderPageComponent;
  let fixture: ComponentFixture<FolderPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FolderPageComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(FolderPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

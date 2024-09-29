import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ShareFolderDialogComponent } from './share-folder-dialog.component';

describe('ShareFolderDialogComponent', () => {
  let component: ShareFolderDialogComponent;
  let fixture: ComponentFixture<ShareFolderDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ShareFolderDialogComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(ShareFolderDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

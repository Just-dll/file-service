import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FileShortModel, FolderModel } from '../folder-page/models.folder';
import { FolderPageComponent } from '../folder-page/folder-page.component';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';

@Component({
  selector: 'app-my-drive',
  templateUrl: '../folder-page/folder-page.component.html',
  styleUrl: '../folder-page/folder-page.component.scss'
})
export class MyDriveComponent extends FolderPageComponent implements OnInit {
  
  constructor(http: HttpClient, route: ActivatedRoute, fb: FormBuilder, 
    dialog: MatDialog, router: Router ) {
    super(http, route, dialog, router);
  }
  
  override ngOnInit(): void {
    //this.getFolderBreadCrumbs();
    this.getMyFolderContent();
  }

  getMyFolderContent() {
    this.isLoading = true;
    const apiUrl = `/api/Folder/`;
    this.http.get<FolderModel>(apiUrl).subscribe(
      (data) => {
        this.isLoading = false;
        this.folderContent = data;
        this.folderId = data.id;
        console.log('Folder content:', this.folderContent);
      },
      (error) => {
        console.error('Error fetching folder content:', error);
      }
    );
  }

}

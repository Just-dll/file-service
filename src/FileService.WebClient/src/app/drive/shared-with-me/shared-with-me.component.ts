import { Component } from '@angular/core';
import { FolderModel } from '../folder-page/models.folder';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-shared-with-me',
  templateUrl: './shared-with-me.component.html',
  styleUrl: './shared-with-me.component.scss'
})
export class SharedWithMeComponent {
  sharedFolders!: FolderModel[];

  constructor(private http: HttpClient) {}
  
  ngOnInit(): void {
    this.getMyFolderContent();
  }

  getMyFolderContent() {
    const apiUrl = `api/Access/`;
    this.http.get<FolderModel[]>(apiUrl).subscribe(
      (data) => {
        this.sharedFolders = data;
        console.log('Folder content:', this.sharedFolders);
      },
      (error) => {
        console.error('Error fetching folder content:', error);
      }
    );
  }
}

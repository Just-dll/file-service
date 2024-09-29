import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';
import { AccessModel, FileShortModel, FolderModel, FolderShortModel } from './models.folder';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { ShareFolderDialogComponent } from './dialogs/share-folder-dialog/share-folder-dialog.component';
import { MatDialog } from '@angular/material/dialog';
import { CreateFolderDialogComponent } from './dialogs/create-folder-dialog/create-folder-dialog.component';

@Component({
  selector: 'app-folder-page',
  templateUrl: './folder-page.component.html',
  styleUrls: ['./folder-page.component.scss']
})
export class FolderPageComponent implements OnInit {
  folderId!: number;
  folderContent!: FolderModel;
  folderBreadcrumbs!: FolderShortModel[];
  accessors!: AccessModel[];
  isLoading : boolean = false;
  previewFileUrl: string | null = null;
  previewFileType: string | null = null;
  previewFileName: string = '';
  isPreviewOpen: boolean = false;

  constructor(
    protected http: HttpClient,
    protected route: ActivatedRoute,  // Inject FormBuilder service
    private dialog: MatDialog 
  ) {
  }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const idParam = params.get('id');
      if (idParam) {
        this.folderId = +idParam;
        this.getFolderBreadCrumbs();
        this.getFolderContent();
        this.getFolderAccessors();
        this.resetPreviewState();
      } else {
        console.error('No folder ID found in the route parameters.');
      }
    });
  }

  getFolderBreadCrumbs() {
    this.http.get<FolderShortModel[]>(`/api/folder/${this.folderId}/path`).subscribe(
      (data: FolderShortModel[]) => {
        this.folderBreadcrumbs = data;
      },
      (error) => {
        console.error('Error fetching breadcrumb data', error);
      }
    );
  }

  onFilePreview(event: { fileUrl: string, fileType: string, fileName: string }) {
    this.previewFileUrl = event.fileUrl;
    this.previewFileType = event.fileType;
    this.previewFileName = event.fileName;
    this.isPreviewOpen = true;
  }

  closePreview(): void {
    this.previewFileUrl = null;
    this.previewFileType = null;
    this.isPreviewOpen = false;
  }

  private resetPreviewState(): void {
    this.previewFileUrl = null;
    this.previewFileType = null;
    this.previewFileName = '';
    this.isPreviewOpen = false;
  }
  
  handleFolderAction(event: { action: string, folderId: number }) {
    const { action, folderId } = event;
  
    switch (action) {
      case 'share':
        this.onShareFolderClick(folderId);
        break;
      case 'delete':
        this.onDeleteClick(folderId);
        break;
      case 'download':
        this.onDownloadClick();
        break;
      default:
        console.error(`Unknown action: ${action}`);
    }
  }

  getFolderContent(): void {
    // Send the GET request to the API
    this.isLoading = true;
    const apiUrl = `/api/Folder/${this.folderId}`;
    this.http.get<FolderModel>(apiUrl).subscribe(
      (data) => {
        this.folderContent = data;
        this.isLoading = false;
        console.log('Folder content:', this.folderContent);
      },
      (error) => {
        console.error('Error fetching folder content:', error);
      }
    );
  }

  getFolderAccessors(): void {
    const apiUrl = `/api/Folder/${this.folderId}/Access`;
    this.http.get<AccessModel[]>(apiUrl).subscribe(
      (data) => {
        this.accessors = data;
        console.log('Folder accessors:', this.accessors);
      },
      (error) => {
        console.error('Error fetching folder accessors:', error);
      }
    );
  }

  onShareFolderClick(folderId: number): void {
    const dialogRef = this.dialog.open(ShareFolderDialogComponent, {
      width: '400px',
      data: { folderId }  // Pass the folderId to the dialog
    });
  
    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        const { email, accessPermissions } = result;
        this.shareFolder(folderId, email, accessPermissions);
      }
    });
  }
  
  // Function to handle the API call for sharing the folder
  shareFolder(folderId: number, email: string, permission: number) {
    const apiUrl = `/api/Folder/${folderId}/Access`;
    const payload = { email, permission };
  
    this.http.post(apiUrl, payload).subscribe(
      () => {
        console.log('Folder shared successfully');
      },
      (error) => {
        console.error('Error sharing folder:', error);
      }
    );
  }

  onDownloadClick(): void {
    console.log('Download button clicked');
    // Add your download logic here
  }

  onDeleteClick(folderId: number): void {
    // Send the DELETE request to the API
    const apiUrl = `api/Folder/${folderId}`;
    this.http.delete(apiUrl).subscribe(
      () => {
        console.log(`Folder with ID ${folderId} deleted successfully.`);
        // Remove the folder from the frontend
        this.folderContent.innerFolders = this.folderContent.innerFolders.filter(folder => folder.id !== folderId);
      },
      (error) => {
        console.error('Error deleting folder:', error);
      }
    );
  }

  onUploadClick() {
    console.log(":ssp")
    const fileInput = document.querySelector<HTMLInputElement>('#fileInput');
    if (fileInput) {
      fileInput.click();
    }
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const files = Array.from(input.files);  // Convert FileList to an array of files
      const filesToUpload: File[] = [];
  
      // Filter out files that already exist in the folder
      files.forEach((file) => {
        if (!this.folderContent.files.find(fsm => fsm.name === file.name)) {
          filesToUpload.push(file);
        } else {
          console.error(`File ${file.name} already exists on drive`);
        }
      });
  
      // Upload files if there are any left after filtering
      if (filesToUpload.length > 0) {
        this.uploadFiles(filesToUpload);
      }
    }
  }
  
  // Method to upload multiple files to the API
  uploadFiles(files: File[]) {
    const formData = new FormData();
    files.forEach(file => formData.append('files', file));  // Add each file to FormData
  
    const headers = new HttpHeaders({
      'enctype': 'multipart/form-data'
    });
  
    this.http.post(`/api/Folder/${this.folderContent.id}/File`, formData, { headers }).subscribe(
      (response) => {
        const uploadedFiles = (response as any).uploadedFiles as FileShortModel[];
        console.log('Files uploaded successfully', uploadedFiles);
  
        // Update folderContent with uploaded files
        this.folderContent.files.push(...uploadedFiles);
      },
      (error) => {
        console.error('File upload failed', error);
      }
    );
  }

  onDeleteFileClick(fileId: number) {
    const apiUrl = `api/Folder/${this.folderId}/File/${fileId}`;
    this.http.delete(apiUrl).subscribe(
      () => {
        console.log(`File with ID ${fileId} deleted successfully.`);
        this.folderContent.files = this.folderContent.files.filter(file => file.id != fileId);
      },
      (error) => {
        console.error('Error deleting folder:', error);
      }
    );
  }

  onAddFolderClick(folderId: number): void {
    const dialogRef = this.dialog.open(CreateFolderDialogComponent, {
      width: '400px',
      data: { folderId }  // Send folder ID for potential future use
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        const folderName = result.folderName;  // Folder name returned from the dialog
        this.createFolder(folderId, folderName);
      }
    });
  }

  createFolder(folderId: number, name: string) {
      const newFolder = {
        name: name
      };

    const apiUrl = `api/Folder?folderId=${folderId}`;
  
      // Make the POST request to the backend API
      this.http.post(apiUrl, newFolder).subscribe(
        (response) => {
          const createdFolder = response as FolderModel;
          console.log('Folder created successfully', createdFolder);
  
          // Update the UI by adding the newly created folder to the list
          this.folderContent.innerFolders.push(createdFolder);
        },
        (error) => {
          console.error('Error creating folder:', error);
        }
      );
  }
}

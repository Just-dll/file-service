import { Component, EventEmitter, Input, Output } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FileShortModel } from '../models.folder';

@Component({
  selector: 'app-file',
  templateUrl: './file.component.html',
  styleUrls: ['./file.component.scss']
})
export class FileComponent {
  @Input() file!: FileShortModel;
  @Input() parentFolderId!: number;

  @Output() deleteClicked = new EventEmitter<number>();
  @Output() previewClicked = new EventEmitter<{ fileUrl: string, fileType: string, fileName: string }>();

  constructor(private http: HttpClient) {}

  onDownloadClick(): void {
    const downloadUrl = `/api/Folder/${this.parentFolderId}/File/${this.file.id}/download`;
  
    this.http.get(downloadUrl, { observe: 'response', responseType: 'blob' }).subscribe(
      (response) => {
        const contentType = response.headers.get('Content-Type') || 'application/octet-stream';
        const contentDisposition = response.headers.get('Content-Disposition');
  
        let fileName = this.file.name;  // Default to file name from the model
  
        // Try to extract the filename from the Content-Disposition header if it exists
        if (contentDisposition) {
          const fileNameMatch = contentDisposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
  
          if (fileNameMatch && fileNameMatch[1]) {
            fileName = decodeURIComponent(fileNameMatch[1]
              .replace(/['"]/g, '')  // Remove any surrounding quotes
              .replace(/UTF-8''/g, '') // Remove UTF-8 encoding specifier
              .replace(/_/g, ' ')      // Replace underscores with spaces
              .trim());                // Trim any extra spaces
          }
        }
  
        if (response.body) {
          const blob = new Blob([response.body], { type: contentType });
          const url = URL.createObjectURL(blob);
  
          // Create a temporary anchor element to trigger the download
          const anchor = document.createElement('a');
          anchor.href = url;
          anchor.download = fileName;
          anchor.click();
  
          // Clean up the URL object to release memory
          URL.revokeObjectURL(url);
        } else {
          console.error('No data received for the file download.');
        }
      }, 
      (error) => {
        console.error('Error fetching file for download:', error);
      }
    );
  }
  

  onDeleteClick(): void {
    this.deleteClicked.emit(this.file.id);
  }

  // In app-file component, update this method
  onPreviewClick(): void {
    const fileUrl = `/api/Folder/${this.parentFolderId}/File/${this.file.id}/preview`;
  
    this.http.get(fileUrl, { observe: 'response', responseType: 'blob' }).subscribe(
      (response) => {
        const contentType = response.headers.get('Content-Type') || 'application/octet-stream';
        
        if (response.body) {
          const blob = new Blob([response.body], { type: contentType });
          const url = URL.createObjectURL(blob);

          this.previewClicked.emit({
            fileUrl: url,
            fileType: contentType,
            fileName: this.file.name
          });
        } else {
          console.error('No data received for the file preview.');
        }
      }, 
      (error) => {
        console.error('Error fetching file for preview:', error);
      }
    );
  }
  

}

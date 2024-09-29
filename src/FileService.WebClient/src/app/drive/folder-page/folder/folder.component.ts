import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FolderShortModel } from '../models.folder';
import { HttpClient } from '@angular/common/http';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-folder',
  templateUrl: './folder.component.html',
  styleUrls: ['./folder.component.scss']
})
export class FolderComponent {
  @Input() folder!: FolderShortModel;
  @Output() deleteClicked = new EventEmitter<number>();
  @Output() shareFolderClicked = new EventEmitter<number>();
  
  constructor(private http : HttpClient, private snackBar: MatSnackBar) { }
  onShareClick(event: MouseEvent) {
    event.stopPropagation(); // Prevents the routerLink from being triggered
    this.shareFolderClicked.emit(this.folder.id);
  }

  onDownloadClick(event: MouseEvent) {
    event.stopPropagation(); // Prevents the routerLink from being triggered
    const downloadUrl = `/api/Folder/${this.folder.id}/download`;

    this.http.get(downloadUrl, { observe: 'response', responseType: 'blob' }).subscribe(
      (response) => {
        const contentType = response.headers.get('Content-Type') || 'application/octet-stream';
        const contentDisposition = response.headers.get('Content-Disposition');
  
        let fileName = this.folder.name;  // Default to file name from the model
  
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
        if (error.status === 404) {
          // If a 404 error occurs, show a snackbar message
          this.snackBar.open('The folder must contain at least one file.', 'Close', {
            duration: 3000, // The snackbar will be visible for 3 seconds
            verticalPosition: 'bottom',
            horizontalPosition: 'right'
          });
        } else {
          console.error('Error fetching file for download:', error);
        }
      }
    );

  }

  onDeleteClick(event: MouseEvent) {
    event.stopPropagation(); // Prevents the routerLink from being triggered
    this.deleteClicked.emit(this.folder.id);
  }
}
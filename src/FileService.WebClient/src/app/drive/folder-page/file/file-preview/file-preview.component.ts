import { Component, Input, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-file-preview',
  templateUrl: './file-preview.component.html',
  styleUrls: ['./file-preview.component.scss']
})
export class FilePreviewComponent implements OnInit, OnChanges {
  @Input() fileUrl: string | null = null;
  @Input() fileType: string | null = null;
  @Input() fileName: string = '';

  fileContent: string | null = null;
  isZoomed: boolean = false; // Track whether the image is zoomed

  constructor(private http: HttpClient) {}

  ngOnInit(): void {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['fileUrl'] && this.fileUrl) {
      this.loadFile();
    }
  }

  closePreview(): void {
    this.fileUrl = null;
    this.fileContent = null;
  }

  private loadFile(): void {
    if (this.fileType === 'text/plain' && this.fileUrl) {
      this.loadTextFile(this.fileUrl);
    }
  }

  private loadTextFile(url: string): void {
    this.http.get(url, { responseType: 'text' }).subscribe({
      next: (data) => this.fileContent = data,
      error: () => this.fileContent = 'Unable to load the file content.'
    });
  }

  // Determine if the file can be previewed
  isPreviewAvailable(): boolean {
    return this.fileType?.startsWith('image') || this.fileType === 'text/plain';
  }

  // Method for downloading the file
  downloadFile(): void {
    if (this.fileUrl) {
      const link = document.createElement('a');
      link.href = this.fileUrl;
      link.download = this.fileName;
      link.click();
    }
  }

  // Method for printing the file
  printFile(): void {
    if (this.isPreviewAvailable() && this.fileUrl) {
      const printWindow = window.open(this.fileUrl, '_blank');
      printWindow?.print();
    }
  }

  // Toggle zoom state for the image
  toggleZoom(): void {
    this.isZoomed = !this.isZoomed;
  }
}

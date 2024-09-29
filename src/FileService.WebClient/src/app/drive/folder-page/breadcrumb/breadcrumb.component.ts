import { Component, Input, Output, EventEmitter } from '@angular/core';
import { FolderShortModel } from '../models.folder';

@Component({
  selector: 'app-breadcrumb',
  templateUrl: './breadcrumb.component.html',
  styleUrls: ['./breadcrumb.component.scss']
})
export class BreadcrumbComponent {
  @Input() breadcrumbs!: FolderShortModel[];
  
  @Output() folderAction = new EventEmitter<{ action: string, folderId: number }>();

  isMenuOpen = false;

  toggleMenu() {
    this.isMenuOpen = !this.isMenuOpen;
  }

  onShareClick(folderId: number) {
    this.folderAction.emit({ action: 'share', folderId });
    this.isMenuOpen = false;
  }

  onDeleteClick(folderId: number) {
    this.folderAction.emit({ action: 'delete', folderId });
    this.isMenuOpen = false;
  }

  onDownloadClick(folderId: number) {
    this.folderAction.emit({ action: 'download', folderId });
    this.isMenuOpen = false;
  }
}

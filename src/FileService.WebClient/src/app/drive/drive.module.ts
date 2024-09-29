import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DriveComponent } from './drive.component';
import { DriveRoutingModule } from './drive-routing.module';
import { SidebarComponent } from "../global-components/sidebar/sidebar.component";
import { FolderPageComponent } from './folder-page/folder-page.component';
import { RouterLink } from '@angular/router';
import { MyDriveComponent } from './my-drive/my-drive.component';
import { DriveLayoutComponent } from '../drive-layout/drive-layout.component';
import { SharedWithMeComponent } from './shared-with-me/shared-with-me.component';
import { ReactiveFormsModule } from '@angular/forms';
import { BreadcrumbComponent } from './folder-page/breadcrumb/breadcrumb.component';
import { FolderComponent } from './folder-page/folder/folder.component';
import { FileComponent } from './folder-page/file/file.component';
import { MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatButtonModule } from '@angular/material/button';
import { ShareFolderDialogComponent } from './folder-page/dialogs/share-folder-dialog/share-folder-dialog.component';
import { FilePreviewComponent } from './folder-page/file/file-preview/file-preview.component';
import { CreateFolderDialogComponent } from './folder-page/dialogs/create-folder-dialog/create-folder-dialog.component';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatMenuModule } from '@angular/material/menu';
import { MatIconModule } from '@angular/material/icon';

@NgModule({
  declarations: [
    DriveComponent,
    FolderPageComponent,
    MyDriveComponent,
    SharedWithMeComponent,
    BreadcrumbComponent,
    FolderComponent,
    FileComponent,
    ShareFolderDialogComponent,
    FilePreviewComponent,
    CreateFolderDialogComponent
  ],
  imports: [
    CommonModule,
    DriveRoutingModule,
    SidebarComponent,
    RouterLink,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatCheckboxModule,
    MatButtonModule,
    MatMenuModule,
    MatIconModule,
    MatSnackBarModule,
  ]
})
export class DriveModule { }

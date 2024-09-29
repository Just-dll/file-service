import { RouterModule, Routes } from '@angular/router';
import { DriveComponent } from './drive.component';
import { NgModule } from '@angular/core';
import { FolderPageComponent } from './folder-page/folder-page.component';
import { DriveLayoutComponent } from '../drive-layout/drive-layout.component';
import { MyDriveComponent } from './my-drive/my-drive.component';
import { SharedWithMeComponent } from './shared-with-me/shared-with-me.component';

const routes: Routes = [
    { 
      path: '', 
      redirectTo: 'my-drive',
      pathMatch: 'full'
    },
    {
      path: 'my-drive',
      component: MyDriveComponent
    },
    {
      path: 'folder/:id',
      component: FolderPageComponent
    },
    {
      path: 'shared',
      component: SharedWithMeComponent
    }
  ]


@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})

export class DriveRoutingModule { }
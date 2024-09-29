import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';
import { HomeComponent } from './home/home.component'
import { DriveLayoutComponent } from './drive-layout/drive-layout.component';
import { AuthGuard } from './auth.guard';

const routes: Routes = [
  {
    path: '',
    component: HomeComponent
  },
  {
    path: 'drive',
    component: DriveLayoutComponent,
    canActivate: [AuthGuard],
    loadChildren: () => import('./drive/drive.module').then(m => m.DriveModule),
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes), HttpClientModule],
  exports: [RouterModule],
})
export class AppRoutingModule { }

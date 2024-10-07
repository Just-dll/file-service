import { NgModule, isDevMode } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { FormsModule } from '@angular/forms';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { HomeComponent } from './home/home.component';
import { NavbarComponent } from "./global-components/navbar/navbar.component";
import { ServiceWorkerModule } from '@angular/service-worker';
import { FooterComponent } from "./global-components/footer/footer.component";
import { SidebarComponent } from "./global-components/sidebar/sidebar.component";
import { DriveLayoutComponent } from './drive-layout/drive-layout.component';
import { environment } from '../environment/environment.development';
import { OAuthModule, OAuthService } from 'angular-oauth2-oidc';
import { CsrfHeaderInterceptor } from './csrf-header.interceptor';
import { AuthInterceptor } from './auth.interceptor';
import { PageNotFoundComponent } from './page-not-found/page-not-found.component';

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    DriveLayoutComponent,
    PageNotFoundComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    CommonModule,
    NavbarComponent,
    ServiceWorkerModule.register('ngsw-worker.js', {
        enabled: !isDevMode(),
        // Register the ServiceWorker as soon as the application is stable
        // or after 30 seconds (whichever comes first).
        registrationStrategy: 'registerWhenStable:30000'
    }),
    FooterComponent,
    SidebarComponent,
    OAuthModule.forRoot()
  ],
  providers: [      
    OAuthService,
    {
      provide: HTTP_INTERCEPTORS,
      useClass: CsrfHeaderInterceptor,
      multi: true,
    },
    /*{
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    },*/
    provideAnimationsAsync('noop'),
  ],
  
  bootstrap: [AppComponent]
})
export class AppModule { }

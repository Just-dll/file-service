import { Component, HostListener } from '@angular/core';
import { RouterLink } from '@angular/router';
import { environment } from '../../../environment/environment.development';
import { CommonModule } from '@angular/common';
import { AuthenticationService } from '../../authentication.service';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, CommonModule],
  standalone: true,
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss'],
})
export class NavbarComponent {
  loginUrl: string | undefined;
  previousScrollPosition: number = 0;
  isNavbarHidden: boolean = false;
  isNavbarOpen: boolean = false;
  
  public username$ = this.auth.getUsername();
  public authenticated$ = this.auth.getIsAuthenticated();
  public anonymous$ = this.auth.getIsAnonymous();
  public logoutUrl$ = this.auth.getLogoutUrl();

  constructor(private auth: AuthenticationService) {
    auth.getSession();
    //this.loginUrl = `${environment.identityUrl}/Account/Login`;
  }
  
 /* @HostListener('window:scroll', [])
  onWindowScroll() {
    const currentScrollPosition = window.pageYOffset || document.documentElement.scrollTop;

    if (currentScrollPosition > this.previousScrollPosition) {
      // Scrolling down
      this.isNavbarHidden = true;
    } else {
      // Scrolling up
      this.isNavbarHidden = false;
    }

    this.previousScrollPosition = currentScrollPosition;
  }
 */
  toggleNavbar() {
    this.isNavbarOpen = !this.isNavbarOpen;
  }
}

import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, GuardResult, MaybeAsync, Router, RouterStateSnapshot } from '@angular/router';
import { AuthenticationService } from './authentication.service';
import { map } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

  constructor(private authService : AuthenticationService, private router: Router) { }

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): MaybeAsync<GuardResult> {
      return this.authService.getIsAuthenticated().pipe(
        map(isLoggedIn => {
          if(isLoggedIn) {
            return true;
          }
          else {
            this.router.navigate(['/bff/login'])
            return false;
          }
        })
      )
  }
  
}

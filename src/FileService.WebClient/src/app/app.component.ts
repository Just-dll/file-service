import { Component } from '@angular/core';
import { AuthenticationService } from './authentication.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
})
export class AppComponent {
  title = 'namename';

  constructor() {}

  ngOnInit() {
    // Attempt to silently log in the user if they are not authenticated
    //this.authService.checkAndAttemptSilentLogin();
  }
}

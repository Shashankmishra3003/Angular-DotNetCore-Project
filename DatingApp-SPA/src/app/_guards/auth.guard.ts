import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';

@Injectable({
  providedIn: 'root'
})
// Check if the user is logged in, so only boolean return type is needed, removed everything else
export class AuthGuard implements CanActivate {
  constructor(private authService: AuthService,
              private router: Router,
              private alertify: AlertifyService
              ) {}

  canActivate(): boolean {
    if (this.authService.loggedIn()) {
      return true;
    }
    this.alertify.error('You shall not pass!!!');
    this.router.navigate(['/home']);
    return false;
  }
}

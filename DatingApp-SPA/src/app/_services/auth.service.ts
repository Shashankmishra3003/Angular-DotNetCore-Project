import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { JwtHelperService } from '@auth0/angular-jwt';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  baseUrl = environment.apiUrl + 'auth/';
  jwtHelper = new JwtHelperService();
  decodedToken: any;
  currentUser: User;
  photoUrl = new BehaviorSubject<string>('../../assets/user.png');
  currentPhotoUrl = this.photoUrl.asObservable(); // we cna update this value from any component
constructor(private http: HttpClient) { }

// Called when a user logs In and we update the photoUrl
// using Behaviour Subject
changeMemberPhoto(photoUrl: string) {
  this.photoUrl.next(photoUrl);
}

// stores token and user photo in Local storage
login(model: any) {
  return this.http.post(this.baseUrl + 'login', model)
  .pipe(
    map((response: any) => {
      const user = response;
      if (user) {
        localStorage.setItem('token', user.token);
        localStorage.setItem('user',JSON.stringify(user.user));
        // decoding the token to extract the user name
        this.decodedToken = this.jwtHelper.decodeToken(user.token);
        this.currentUser = user.user;
        // Passing the photoUrl to Observable
        this.changeMemberPhoto(this.currentUser.photoUrl);
      }
    })
  );
}

register(model: any){
  return this.http.post(this.baseUrl + 'register', model);
}

// Validating the Token using Auth0
loggedIn() {
  const token = localStorage.getItem('token');
  return !this.jwtHelper.isTokenExpired(token);
}
}

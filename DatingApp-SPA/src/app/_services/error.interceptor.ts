import { Injectable } from '@angular/core';
import {
  HttpInterceptor,
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpErrorResponse,
  HTTP_INTERCEPTORS,
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  intercept(
    req: HttpRequest<any>,      // request
    next: HttpHandler           // what happens next, catching what happends next
  ): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
        catchError(error => {
         // error is the HTTPErrorResponse, we extract everything and write appropriate conditions
            if (error.status === 401) {
                return throwError(error.statusText);
            }
            if (error instanceof HttpErrorResponse) {
                const applicaitonError = error.headers.get('Application-Error');
                if (applicaitonError) {
                    return throwError(applicaitonError);
                }
                const serverError = error.error;
                let modalStateError = '';
                // tslint:disable-next-line: max-line-length
                if (serverError.errors && typeof serverError.errors === 'object') {
                // If the error object exists,for custom errors declared in the model class in the API
                    for (const key in serverError.errors) {
                        if (serverError.errors[key]) {
                            modalStateError += serverError.errors[key] + '\n';
                        }
                    }
                }
                return throwError(modalStateError || serverError || 'Server Error');
                // If there is no object error then we throw the string error
                // Password validation may return 2 errors so it is an object of arrays, username returns only 1 string of error
                // If we have an uncaptured error in the API then we return 'Server Error'
            }
        })
    );
  }
}

// We register a new interecptor provider and add this to our Provider array.
// Angular has an http array of interceptor providers and we add our provider to it.
// We add this Provider in the app.modules provider
export const ErrorInterceptorProvider = {
    provide : HTTP_INTERCEPTORS,
    useClass: ErrorInterceptor,  // The class we have written
    multi: true
};

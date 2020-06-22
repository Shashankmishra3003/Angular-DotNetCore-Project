import { Injectable } from '@angular/core';
import * as alertify from 'alertifyjs'; // a new file typings.d.ts and new typesroot is added in tsconfig

@Injectable({
  providedIn: 'root',
})
export class AlertifyService {
  constructor() {}

  // callback is for the operation to be performed when the ok button on message is clicked
  confirm(message: string, okCallback: () => any) {
    alertify.confirm(message, (event: any) => {
      if (event) {
        okCallback();
      } else {
      }
    });
  }

  success(message: string) {
    alertify.success(message);
  }
  error(message: string) {
    alertify.error(message);
  }
  warning(message: string) {
    alertify.warning(message);
  }
  message(message: string) {
    alertify.message(message);
  }
}

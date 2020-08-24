import { Injectable } from '@angular/core';
import * as alertify from 'alertifyjs';

@Injectable({
  providedIn: 'root'
})
export class AlertifyService {

constructor() {
  alertify.set('notifier', 'position', 'bottom-center');
}

  confirm(message: string, okCallback: () => any, title?: string ) {
    alertify.confirm(title , message, (event: any) => {
      if (event) {
        okCallback();
      } else {}
    }, () => {
      alertify.error('Canceled');
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

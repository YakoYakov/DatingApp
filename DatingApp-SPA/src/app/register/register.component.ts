import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';
import { FormGroup, FormControl, Validators, FormBuilder } from '@angular/forms';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  @Output() cancelRegister = new EventEmitter();

  model: any = {};
  registerForm: FormGroup;

  constructor(private authService: AuthService,
              private alertify: AlertifyService,
              private fb: FormBuilder) { }

  ngOnInit() {
    this.createRegisterForm();
  }

  createRegisterForm() {
    this.registerForm = this.fb.group({
      username: ['', [ Validators.required, Validators.minLength(3), Validators.maxLength(50)]],
      password: ['', [ Validators.required, Validators.minLength(3), Validators.maxLength(50)]],
      confirmPassword: ['', Validators.required]
    }, {validator: this.comparePasswordsFieldsValidator});
  }

  private comparePasswordsFieldsValidator(g: FormGroup) {
    // tslint:disable-next-line: object-literal-key-quotes
    return g.get('password').value === g.get('confirmPassword').value ? null : { 'mismatch': true };
  }

  register() {
    // this.authService.register(this.model).subscribe(() => {
    //   this.alertify.success('Registration is successful');
    // }, error => {
    //   this.alertify.error(error);
    // });
    console.log(this.registerForm.value);
  }

  cancel() {
    this.cancelRegister.emit(false);
    this.alertify.warning('cancelled!');
  }

}

import { Component } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators,
  ReactiveFormsModule,
} from '@angular/forms';

import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth/auth.service';
import { AgentService } from '../../../core/api/agent';
import { ILoginRequest } from '../../../core/models/auth';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { PasswordModule } from 'primeng/password';
import { MessageModule } from 'primeng/message';
import { TranslateService } from '@ngx-translate/core';
import { TranslateModule } from '@ngx-translate/core';

const EMAIL_MAX = 255;

@Component({
  standalone: true,
  selector: 'lnk-login',
  imports: [
    ReactiveFormsModule,
    RouterModule,
    InputTextModule,
    ButtonModule,
    PasswordModule,
    MessageModule,
    TranslateModule
],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})
export class LoginComponent {
  loginForm: FormGroup;
  returnUrl = '/pact';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private agent: AgentService,
    private router: Router,
    private route: ActivatedRoute,
    private translate: TranslateService
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email, Validators.maxLength(EMAIL_MAX)]],
      password: ['', [Validators.required, Validators.minLength(6)]],
    });

    this.returnUrl =
      this.route.snapshot.queryParams['returnUrl'] || '/pact';
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    const loginRequest: ILoginRequest = {
      email: this.loginForm.value.email,
      password: this.loginForm.value.password,
    };

    this.agent.Users.login(loginRequest).subscribe({
      next: (tokenResponse) => {
        console.log('Login successful:', tokenResponse);

        this.authService.login(tokenResponse);

        this.router.navigate([this.returnUrl]);
      },
      error: (error) => {
        console.error('Login failed:', error);
      },
    });
  }

  get email() {
    return this.loginForm.get('email');
  }

  get password() {
    return this.loginForm.get('password');
  }
}

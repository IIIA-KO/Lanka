import { Component, inject } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';

import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth/auth.service';
import { AgentService } from '../../../core/api/agent';
import { ILoginRequest } from '../../../core/models/auth';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { PasswordModule } from 'primeng/password';
import { MessageModule } from 'primeng/message';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

const EMAIL_MAX = 255;

@Component({
  standalone: true,
  selector: 'app-login',
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
  public loginForm: FormGroup;
  public returnUrl = '/pact';

  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly agent = inject(AgentService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly translate = inject(TranslateService);

  constructor() {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email, Validators.maxLength(EMAIL_MAX)]],
      password: ['', [Validators.required, Validators.minLength(6)]],
    });

    this.returnUrl =
      this.route.snapshot.queryParams['returnUrl'] || '/pact';
  }

  public get email(): AbstractControl | null {
    return this.loginForm.get('email');
  }

  public get password(): AbstractControl | null {
    return this.loginForm.get('password');
  }

  public onSubmit(): void {
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
        console.warn('[LoginComponent] Login successful');

        this.authService.login(tokenResponse);

        this.router.navigate([this.returnUrl]);
      },
      error: (error) => {
        console.error('[LoginComponent] Login failed:', error);
      },
    });
  }
}

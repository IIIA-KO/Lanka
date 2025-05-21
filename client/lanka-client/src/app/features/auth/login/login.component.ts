import { Component } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators,
  ReactiveFormsModule,
} from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth/auth.service';
import { AgentService } from '../../../core/api/agent';

import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { PasswordModule } from 'primeng/password';
import { MessageModule } from 'primeng/message';

const EMAIL_MAX = 255;

@Component({
  standalone: true,
  selector: 'lnk-login',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    InputTextModule,
    ButtonModule,
    PasswordModule,
    MessageModule,
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})
export class LoginComponent {
  form: FormGroup;
  errorMessage = '';

  constructor(
    private authService: AuthService,
    private formBuilder: FormBuilder,
    private router: Router,
    private agent: AgentService
  ) {
    this.form = this.formBuilder.group({
      email: [
        '',
        [
          Validators.required,
          Validators.email,
          Validators.maxLength(EMAIL_MAX),
        ],
      ],
      password: [
        '',
        [
          Validators.required,
          Validators.minLength(8),
          Validators.maxLength(16),
          Validators.pattern(/[A-Z]/), // uppercase
          Validators.pattern(/[a-z]/), // lowercase
          Validators.pattern(/[0-9]/), // number
          Validators.pattern(/[!?*.$]/), // special
        ],
      ],
    });
  }

  submit() {
    if (this.form.invalid) return;

    this.agent.Users.login(this.form.value).subscribe({
      next: (response: any) => {
        this.authService.login(response);
        this.router.navigate(['/']);
      },
    });
  }
}

import { Component } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators,
  ReactiveFormsModule,
} from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AgentService } from '../../../core/api/agent';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';

const NAME_MAX = 100;
const EMAIL_MAX = 255;

@Component({
  standalone: true,
  selector: 'lnk-register',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    InputTextModule,
    PasswordModule,
    ButtonModule,
    MessageModule,
  ],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css',
})
export class RegisterComponent {
  form: FormGroup;

  constructor(
    private formBuilder: FormBuilder,
    private router: Router,
    private agent: AgentService
  ) {
    this.form = this.formBuilder.group({
      firstName: [
        '',
        [
          Validators.required,
          Validators.minLength(2),
          Validators.maxLength(NAME_MAX),
        ],
      ],
      lastName: [
        '',
        [
          Validators.required,
          Validators.minLength(2),
          Validators.maxLength(NAME_MAX),
        ],
      ],
      birthDate: ['', [Validators.required]],
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

    this.agent.Users.register(this.form.value).subscribe({
      next: () => {
        this.router.navigate(['/login']);
      },
    });
  }
}

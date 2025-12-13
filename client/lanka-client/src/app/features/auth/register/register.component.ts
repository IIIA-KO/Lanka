import { Component, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FriendlyHttpError } from '../../../core/services/friendly-error.service';
import { ApiErrorExtractorService } from '../../../core/services/api-error-extractor.service';
import {
  FormBuilder,
  FormGroup,
  Validators,
  ReactiveFormsModule,
} from '@angular/forms';
import { Router, RouterModule } from '@angular/router';

import { AgentService } from '../../../core/api/agent';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

const NAME_MAX = 100;
const EMAIL_MAX = 255;

@Component({
  standalone: true,
  selector: 'app-register',
  imports: [
    ReactiveFormsModule,
    RouterModule,
    InputTextModule,
    PasswordModule,
    ButtonModule,
    MessageModule,
    TranslateModule
],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css',
})
export class RegisterComponent {
  public form: FormGroup;
  public errorMessage: string | null = null;

  private readonly formBuilder = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly agent = inject(AgentService);
  private readonly translate = inject(TranslateService);
  private readonly errorExtractor = inject(ApiErrorExtractorService);

  constructor() {
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

  public submit(): void {
    if (this.form.invalid) return;

    this.agent.Users.register(this.form.value).subscribe({
      next: () => {
        this.router.navigate(['/login']);
      },
      error: (error: HttpErrorResponse | FriendlyHttpError) => {
        console.error('[Register Component] Registration failed:', error);
        this.errorMessage = this.errorExtractor.extractErrorMessage(error);
      }
    });
  }
}


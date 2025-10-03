import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { catchError, of } from 'rxjs';

// PrimeNG Modules
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TextareaModule } from 'primeng/textarea';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageModule } from 'primeng/message';
import { DividerModule } from 'primeng/divider';
import { TagModule } from 'primeng/tag';

import { PactsAgent } from '../../../core/api/pacts.agent';
import { IPact, ICreatePactRequest, IEditPactRequest } from '../../../core/models/campaigns';
import { SnackbarService } from '../../../core/services/snackbar/snackbar.service';

@Component({
  standalone: true,
  selector: 'app-pact',
  imports: [
    CommonModule,
    CurrencyPipe,
    ReactiveFormsModule,
    ButtonModule,
    CardModule,
    TextareaModule,
    ProgressSpinnerModule,
    MessageModule,
    DividerModule,
    TagModule
  ],
  templateUrl: './pact.component.html',
  styleUrls: ['./pact.component.css']
})
export class PactComponent implements OnInit {
  public pact: IPact | null = null;
  public pactForm!: FormGroup;
  public loading = false;
  public isEditing = false;
  public error: string | null = null;

  private readonly fb = inject(FormBuilder);
  private readonly pactsAgent = inject(PactsAgent);
  private readonly router = inject(Router);
  private readonly snackbarService = inject(SnackbarService);

  public ngOnInit(): void {
    this.initializeForm();
    this.loadPact();
  }

  public loadPact(): void {
    this.loading = true;
    this.error = null; // Clear any previous errors
    
    this.pactsAgent.getBloggerPact().pipe(
      catchError((error) => {
        // No pact exists yet - this is normal for new users, don't show as error
        if (error.status === 404 || error.status === 204) {
          return of(null);
        }
        
        // For other errors, let the interceptor handle user notifications
        // Only set component error state for display purposes
        this.error = 'Unable to load your partnership agreement';
        return of(null);
      })
    ).subscribe({
      next: (pact: IPact | null) => {
        this.pact = pact;
        if (pact) {
          this.pactForm.patchValue({
            content: pact.content
          });
        }
      },
      complete: () => {
        this.loading = false;
      }
    });
  }

  public startEditing(): void {
    this.isEditing = true;
    this.error = null; // Clear any errors when starting to edit
    
    // For first-time users, populate with sample content
    this.initializeFormWithSample();
  }

  public cancelEditing(): void {
    this.isEditing = false;
    this.error = null; // Clear any errors when canceling
    
    if (this.pact) {
      this.pactForm.patchValue({
        content: this.pact.content
      });
    } else {
      this.pactForm.reset();
    }
  }

  public onSubmit(): void {
    if (this.pactForm.valid) {
      this.loading = true;
      
      if (this.pact) {
        // Edit existing pact
        const request: IEditPactRequest = {
          pactId: this.pact.id,
          content: this.pactForm.value.content
        };
        
        this.pactsAgent.editPact(request).pipe(
          catchError(() => {
            // Let the error interceptor handle the user notification
            return of(null);
          })
        ).subscribe({
          next: (result) => {
            if (result) {
              this.pact = result;
              this.isEditing = false;
              this.snackbarService.showSuccess('Pact updated successfully');
            }
          },
          complete: () => {
            this.loading = false;
          }
        });
      } else {
        // Create new pact
        const request: ICreatePactRequest = {
          content: this.pactForm.value.content
        };
        
        this.pactsAgent.createPact(request).pipe(
          catchError(() => {
            // Let the error interceptor handle the user notification
            return of(null);
          })
        ).subscribe({
          next: (result) => {
            if (result) {
              this.snackbarService.showSuccess('Pact created successfully');
              this.loadPact(); // Reload to get the full pact object
              this.isEditing = false;
            }
          },
          complete: () => {
            this.loading = false;
          }
        });
      }
    }
  }

  public navigateToOffers(): void {
    this.router.navigate(['/offers']);
  }

  /**
   * Resets the form content to the sample template
   */
  public resetToSample(): void {
    this.pactForm.patchValue({
      content: this.getSamplePactContent()
    });
    
    // Mark the field as touched to show validation
    this.pactForm.get('content')?.markAsTouched();
  }

  public getFieldError(fieldName: string): string | null {
    const field = this.pactForm.get(fieldName);
    if (field && field.touched && field.errors) {
      if (field.errors['required']) {
        return 'Content is required';
      }
      if (field.errors['minlength']) {
        return `Content must be at least ${field.errors['minlength'].requiredLength} characters`;
      }
      if (field.errors['maxlength']) {
        return `Content must not exceed ${field.errors['maxlength'].requiredLength} characters`;
      }
    }
    return null;
  }

  /**
   * Provides sample pact content for first-time users
   */
  public getSamplePactContent(): string {
    return `Partnership Terms & Collaboration Guidelines

🤝 About Me
I am a passionate content creator specializing in [your niche - fashion/tech/lifestyle/etc.]. I believe in authentic partnerships that provide genuine value to my audience while supporting brand growth.

📋 Collaboration Scope
• Content Types: Instagram posts, Stories, Reels, YouTube videos
• Posting Schedule: Flexible timeline with 3-5 business days notice
• Content Approval: Draft review process before final publication
• Usage Rights: Limited to agreed campaign duration unless otherwise specified

💰 Investment & Deliverables
• Pricing varies based on content type, reach, and campaign complexity
• Package deals available for multi-platform campaigns
• Additional revisions beyond initial scope may incur extra charges
• Payment terms: 50% upfront, 50% upon content delivery

📈 Performance & Analytics
• Detailed performance reports provided within 7 days of campaign completion
• Engagement metrics, reach, and audience insights included
• Long-term partnership discounts available for returning clients

🎯 Brand Alignment
I only partner with brands that align with my values and audience interests. This ensures authentic content that resonates with my community and drives meaningful results for your business.

📞 Let's Connect
Ready to create something amazing together? I'm excited to discuss how we can bring your brand vision to life through engaging, authentic content.

Note: These are general guidelines. Specific terms will be customized for each partnership based on campaign requirements and objectives.`;
  }

  /**
   * Initializes form with sample content for first-time users
   */
  private initializeFormWithSample(): void {
    if (!this.pact && this.isEditing) {
      this.pactForm.patchValue({
        content: this.getSamplePactContent()
      });
    }
  }

  private initializeForm(): void {
    this.pactForm = this.fb.group({
      content: ['', [Validators.required, Validators.minLength(50), Validators.maxLength(2000)]]
    });
  }
}
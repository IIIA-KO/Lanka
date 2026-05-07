import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { catchError, of, switchMap, map, Observable, Subject, takeUntil } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';

// PrimeNG Modules
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TextareaModule } from 'primeng/textarea';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageModule } from 'primeng/message';
import { DividerModule } from 'primeng/divider';
import { TagModule } from 'primeng/tag';
import { SelectButtonModule } from 'primeng/selectbutton';
import { TooltipModule } from 'primeng/tooltip';
import { TranslateModule } from '@ngx-translate/core';

import { PactsAgent } from '../../../core/api/pacts.agent';
import { IPact, ICreatePactRequest, IEditPactRequest } from '../../../core/models/campaigns';
import { SnackbarService } from '../../../core/services/snackbar/snackbar.service';
import { BloggersAgent } from '../../../core/api/bloggers.agent';
import { MarkdownService } from '../../../core/services/markdown.service';

@Component({
  standalone: true,
  selector: 'app-pact',
  imports: [
    CommonModule,
    CurrencyPipe,
    ReactiveFormsModule,
    FormsModule,
    ButtonModule,
    CardModule,
    TextareaModule,
    ProgressSpinnerModule,
    MessageModule,
    DividerModule,
    TagModule,
    SelectButtonModule,
    TooltipModule,
    TranslateModule
  ],
  templateUrl: './pact.component.html',
  styleUrls: ['./pact.component.css']
})
export class PactComponent implements OnInit, OnDestroy {
  private static readonly MaxContentLength = 4000;

  public pact: IPact | null = null;
  public pactForm!: FormGroup;
  public loading = false;
  public isEditing = false;
  public error: string | null = null;
  
  public editModeOptions = [
    { label: 'Write', value: 'write', icon: 'pi pi-pencil' },
    { label: 'Preview', value: 'preview', icon: 'pi pi-eye' }
  ];
  public activeEditMode = 'write';

  private readonly fb = inject(FormBuilder);
  private readonly pactsAgent = inject(PactsAgent);
  private readonly bloggersAgent = inject(BloggersAgent);
  private readonly router = inject(Router);
  private readonly snackbarService = inject(SnackbarService);
  private readonly markdownService = inject(MarkdownService);
  private bloggerId: string | null = null;
  private readonly destroy$ = new Subject<void>();

  public ngOnInit(): void {
    this.initializeForm();
    this.loadPact();
  }

  public ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  public loadPact(): void {
    this.loading = true;
    this.error = null; // Clear any previous errors
    
    this.ensureBloggerId().pipe(
      takeUntil(this.destroy$),
      switchMap((bloggerId) => {
        if (!bloggerId) {
          return of(null);
        }

        return this.pactsAgent.getPactByBloggerId(bloggerId).pipe(
          catchError((error: HttpErrorResponse) => {
            if (error.status === 404 || error.status === 204) {
              return of(null);
            }

            this.error = 'Unable to load your partnership agreement';
            return of(null);
          })
        );
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
    if (this.pactForm.invalid) {
      this.pactForm.markAllAsTouched();
      return;
    }

    if (this.pactForm.valid) {
      this.loading = true;
      
      if (this.pact) {
        // Edit existing pact
        const request: IEditPactRequest = {
          pactId: this.pact.id,
          content: this.pactForm.value.content
        };
        
        this.pactsAgent.editPact(request).pipe(
          takeUntil(this.destroy$),
          catchError(() => {
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
          takeUntil(this.destroy$),
          catchError(() => {
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

  public navigateToCreateOffer(): void {
    this.router.navigate(['/offers/create']);
  }

  public navigateToOfferEdit(offerId: string): void {
    this.router.navigate(['/offers', offerId, 'edit']);
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
    return `# Partnership Agreement

This pact describes how I usually work with brands on sponsored Instagram content. Specific campaign details, dates, deliverables, and price are confirmed separately in each offer or campaign.

## Collaboration Fit

I accept campaigns that match my audience, content style, and personal standards. I may decline products or briefs that feel misleading, low quality, unsafe, or unrelated to my community.

## Campaign Workflow

1. The client shares the campaign goal, product details, key message, required mentions, and deadline.
2. I confirm the scope, publication date, and deliverables before work begins.
3. If a draft review is included, I send the concept or preview before publishing.
4. After publishing, I provide the final content link and a short work report.

## Deliverables

Each offer defines the exact deliverable, such as a Reel, Story set, feed post, mention, or highlight. Unless agreed otherwise:

* one deliverable includes one publication on my Instagram profile;
* minor edits are included before publication;
* major brief changes after approval may require a new campaign or extra fee;
* publication timing may shift slightly because of platform issues or product delivery delays.

## Content Review

The client may request changes that keep the content accurate and aligned with the approved brief. I keep final editorial control over tone, wording, visuals, and presentation so the content stays natural for my audience.

## Usage Rights

The client may repost the published content organically with credit to my profile. Paid advertising, whitelisting, editing the content, or using it outside social media requires written approval and separate terms.

## Payment

Payment is made through Lanka according to the campaign price and currency. The campaign is considered ready for payment after the agreed content is delivered and the work report is submitted.

## Cancellation

Either side can cancel before work starts. If the campaign is cancelled after content planning, filming, or editing has begun, we should agree on fair compensation for completed work.

## Communication

Please keep campaign requirements, deadlines, and feedback in Lanka chat so both sides have a clear record of the agreement.`;
  }

  public getParsedContent(content: string): string {
    return this.markdownService.parse(content);
  }

  public insertMarkdown(prefix: string, suffix = ''): void {
    const textarea = document.getElementById('content') as HTMLTextAreaElement;
    if (!textarea) return;

    const start = textarea.selectionStart;
    const end = textarea.selectionEnd;
    const text = this.pactForm.get('content')?.value || '';
    const before = text.substring(0, start);
    const selection = text.substring(start, end);
    const after = text.substring(end);

    const newText = `${before}${prefix}${selection}${suffix}${after}`;
    
    this.pactForm.patchValue({ content: newText });
    
    // Restore focus and selection
    setTimeout(() => {
      textarea.focus();
      const newCursorPos = start + prefix.length + selection.length + suffix.length;
      textarea.setSelectionRange(newCursorPos, newCursorPos);
    });
  }

  private ensureBloggerId(): Observable<string | null> {
    if (this.bloggerId) {
      return of(this.bloggerId);
    }

    return this.bloggersAgent.getProfile().pipe(
      map((profile) => {
        if (profile?.id) {
          this.bloggerId = profile.id;
          return profile.id;
        }

        this.error = 'Unable to resolve your blogger account.';
        return null;
      }),
      catchError(() => {
        this.error = 'Unable to load your blogger profile.';
        return of(null);
      })
    );
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
      content: ['', [
        Validators.required,
        Validators.minLength(50),
        Validators.maxLength(PactComponent.MaxContentLength),
      ]]
    });
  }
}

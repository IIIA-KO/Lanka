import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { catchError, of, Subject, takeUntil } from 'rxjs';

// PrimeNG Modules
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TextareaModule } from 'primeng/textarea';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageModule } from 'primeng/message';
import { RatingModule } from 'primeng/rating';
import { TagModule } from 'primeng/tag';
import { DialogModule } from 'primeng/dialog';
import { SelectModule } from 'primeng/select';

import { ReviewsAgent } from '../../../core/api/reviews.agent';
import { CampaignsAgent } from '../../../core/api/campaigns.agent';
import { AuthService } from '../../../core/services/auth/auth.service';
import { IReview, ICreateReviewRequest, IEditReviewRequest, CampaignStatus } from '../../../core/models/campaigns';
import { SnackbarService } from '../../../core/services/snackbar/snackbar.service';

@Component({
  standalone: true,
  selector: 'app-reviews',
  imports: [
    DatePipe,
    FormsModule,
    ReactiveFormsModule,
    ButtonModule,
    CardModule,
    TextareaModule,
    ProgressSpinnerModule,
    MessageModule,
    RatingModule,
    TagModule,
    DialogModule,
    SelectModule
  ],
  templateUrl: './reviews.component.html',
  styleUrls: ['./reviews.component.css']
})
export class ReviewsComponent implements OnInit, OnDestroy {
  public reviews: IReview[] = [];
  public loading = false;
  public loadingCampaigns = false;
  public error: string | null = null;

  // Dialog state
  public showCreateDialog = false;
  public showEditDialog = false;

  // Forms
  public createReviewForm!: FormGroup;
  public editReviewForm!: FormGroup;
  public editingReview: IReview | null = null;

  // Real completed campaigns for the create dialog
  public availableCampaigns: { label: string; value: string }[] = [];

  private currentUserId: string | null = null;
  private readonly destroy$ = new Subject<void>();

  private readonly fb = inject(FormBuilder);
  private readonly reviewsAgent = inject(ReviewsAgent);
  private readonly campaignsAgent = inject(CampaignsAgent);
  private readonly authService = inject(AuthService);
  private readonly snackbarService = inject(SnackbarService);

  public ngOnInit(): void {
    this.currentUserId = this.authService.getUserIdFromToken()?.toLowerCase() ?? null;
    this.initializeForms();
    this.loadReviews();
    this.loadCompletedCampaigns();
  }

  public ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  public openCreateDialog(): void {
    this.createReviewForm.reset();
    this.createReviewForm.patchValue({ rating: 1 });
    this.showCreateDialog = true;
  }

  public openEditDialog(review: IReview): void {
    this.editingReview = review;
    this.editReviewForm.patchValue({
      rating: review.rating,
      comment: review.comment
    });
    this.showEditDialog = true;
  }

  public closeCreateDialog(): void {
    this.showCreateDialog = false;
    this.createReviewForm.reset();
  }

  public closeEditDialog(): void {
    this.showEditDialog = false;
    this.editReviewForm.reset();
    this.editingReview = null;
  }

  public onCreateReview(): void {
    if (this.createReviewForm.valid) {
      this.loading = true;
      const request: ICreateReviewRequest = this.createReviewForm.value;

      this.reviewsAgent.createReview(request).pipe(
        takeUntil(this.destroy$),
        catchError(error => {
          this.snackbarService.showError('Error creating review: ' + error.message);
          return of(null);
        })
      ).subscribe({
        next: (result) => {
          if (result) {
            this.snackbarService.showSuccess('Review created successfully');
            this.closeCreateDialog();
            this.loadReviews();
          }
        },
        complete: () => {
          this.loading = false;
        }
      });
    }
  }

  public onEditReview(): void {
    if (this.editReviewForm.valid && this.editingReview) {
      this.loading = true;
      const request: IEditReviewRequest = {
        reviewId: this.editingReview.id,
        rating: this.editReviewForm.value.rating,
        comment: this.editReviewForm.value.comment
      };

      this.reviewsAgent.editReview(request).pipe(
        takeUntil(this.destroy$),
        catchError(error => {
          this.snackbarService.showError('Error editing review: ' + error.message);
          return of(null);
        })
      ).subscribe({
        next: (result) => {
          if (result) {
            this.snackbarService.showSuccess('Review updated successfully');
            this.closeEditDialog();
            this.loadReviews();
          }
        },
        complete: () => {
          this.loading = false;
        }
      });
    }
  }

  public deleteReview(review: IReview): void {
    if (confirm('Are you sure you want to delete this review? This action cannot be undone.')) {
      this.loading = true;
      this.reviewsAgent.deleteReview(review.id).pipe(
        takeUntil(this.destroy$),
        catchError(error => {
          this.snackbarService.showError('Error deleting review: ' + error.message);
          return of(null);
        })
      ).subscribe({
        next: () => {
          this.snackbarService.showSuccess('Review deleted successfully');
          this.loadReviews();
        },
        complete: () => {
          this.loading = false;
        }
      });
    }
  }

  public getFieldError(form: FormGroup, fieldName: string): string | null {
    const field = form.get(fieldName);
    if (field && field.touched && field.errors) {
      if (field.errors['required']) {
        return 'This field is required';
      }
      if (field.errors['minlength']) {
        return `Minimum length is ${field.errors['minlength'].requiredLength} characters`;
      }
      if (field.errors['maxlength']) {
        return `Maximum length is ${field.errors['maxlength'].requiredLength} characters`;
      }
      if (field.errors['min']) {
        return 'Rating must be at least 1';
      }
      if (field.errors['max']) {
        return 'Rating must not exceed 5';
      }
    }
    return null;
  }

  public getRatingArray(rating: number): boolean[] {
    return Array(5).fill(false).map((_, i) => i < rating);
  }

  public onReviewAction(action: string, review: IReview): void {
    switch (action) {
      case 'edit':
        this.openEditDialog(review);
        break;
      case 'delete':
        this.deleteReview(review);
        break;
    }
  }

  private initializeForms(): void {
    this.createReviewForm = this.fb.group({
      campaignId: ['', [Validators.required]],
      rating: [1, [Validators.required, Validators.min(1), Validators.max(5)]],
      comment: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(500)]]
    });

    this.editReviewForm = this.fb.group({
      rating: [1, [Validators.required, Validators.min(1), Validators.max(5)]],
      comment: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(500)]]
    });
  }

  private loadReviews(): void {
    if (!this.currentUserId) {
      this.error = 'Unable to identify current user.';
      return;
    }

    this.loading = true;
    this.reviewsAgent.getBloggerReviews(this.currentUserId).pipe(
      takeUntil(this.destroy$),
      catchError(error => {
        this.error = 'Error loading reviews: ' + error.message;
        return of([]);
      })
    ).subscribe({
      next: (reviews: IReview[]) => {
        this.reviews = reviews;
      },
      complete: () => {
        this.loading = false;
      }
    });
  }

  private loadCompletedCampaigns(): void {
    if (!this.currentUserId) return;

    this.loadingCampaigns = true;
    this.campaignsAgent.getBloggerCampaigns(this.currentUserId).pipe(
      takeUntil(this.destroy$),
      catchError(() => of([]))
    ).subscribe({
      next: (campaigns) => {
        this.availableCampaigns = campaigns
          .filter(c => c.status === CampaignStatus.Completed)
          .map(c => ({ label: c.name, value: c.id }));
        this.loadingCampaigns = false;
      }
    });
  }
}
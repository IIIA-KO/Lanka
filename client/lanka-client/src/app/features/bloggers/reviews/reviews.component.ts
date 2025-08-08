import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable, catchError, of } from 'rxjs';

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
import { IReview, ICreateReviewRequest, IEditReviewRequest } from '../../../core/models/campaigns';
import { SnackbarService } from '../../../core/services/snackbar/snackbar.service';

@Component({
  standalone: true,
  selector: 'app-reviews',
  imports: [
    CommonModule,
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
export class ReviewsComponent implements OnInit {
  reviews: IReview[] = [];
  loading = false;
  error: string | null = null;
  
  // Dialog state
  showCreateDialog = false;
  showEditDialog = false;
  
  // Forms
  createReviewForm!: FormGroup;
  editReviewForm!: FormGroup;
  editingReview: IReview | null = null;
  
  // Mock campaign data for demonstration
  availableCampaigns = [
    { label: 'Summer Collection Campaign', value: 'campaign-1' },
    { label: 'Tech Product Review', value: 'campaign-2' },
    { label: 'Fitness Brand Partnership', value: 'campaign-3' }
  ];

  constructor(
    private fb: FormBuilder,
    private reviewsAgent: ReviewsAgent,
    private router: Router,
    private snackbarService: SnackbarService
  ) {}

  ngOnInit(): void {
    this.initializeForms();
    this.loadReviews();
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
    this.loading = true;
    this.reviewsAgent.getBloggerReviews().pipe(
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

  openCreateDialog(): void {
    this.createReviewForm.reset();
    this.createReviewForm.patchValue({ rating: 1 });
    this.showCreateDialog = true;
  }

  openEditDialog(review: IReview): void {
    this.editingReview = review;
    this.editReviewForm.patchValue({
      rating: review.rating,
      comment: review.comment
    });
    this.showEditDialog = true;
  }

  closeCreateDialog(): void {
    this.showCreateDialog = false;
    this.createReviewForm.reset();
  }

  closeEditDialog(): void {
    this.showEditDialog = false;
    this.editReviewForm.reset();
    this.editingReview = null;
  }

  onCreateReview(): void {
    if (this.createReviewForm.valid) {
      this.loading = true;
      const request: ICreateReviewRequest = this.createReviewForm.value;
      
      this.reviewsAgent.createReview(request).pipe(
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

  onEditReview(): void {
    if (this.editReviewForm.valid && this.editingReview) {
      this.loading = true;
      const request: IEditReviewRequest = {
        reviewId: this.editingReview.id,
        rating: this.editReviewForm.value.rating,
        comment: this.editReviewForm.value.comment
      };
      
      this.reviewsAgent.editReview(request).pipe(
        catchError(error => {
          this.snackbarService.showError('Error updating review: ' + error.message);
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

  deleteReview(review: IReview): void {
    if (confirm('Are you sure you want to delete this review? This action cannot be undone.')) {
      this.loading = true;
      this.reviewsAgent.deleteReview(review.id).pipe(
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

  getFieldError(form: FormGroup, fieldName: string): string | null {
    const field = form.get(fieldName);
    if (field && field.touched && field.errors) {
      if (field.errors['required']) {
        return `${fieldName.charAt(0).toUpperCase() + fieldName.slice(1)} is required`;
      }
      if (field.errors['minlength']) {
        return `${fieldName.charAt(0).toUpperCase() + fieldName.slice(1)} must be at least ${field.errors['minlength'].requiredLength} characters`;
      }
      if (field.errors['maxlength']) {
        return `${fieldName.charAt(0).toUpperCase() + fieldName.slice(1)} must not exceed ${field.errors['maxlength'].requiredLength} characters`;
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

  getRatingArray(rating: number): boolean[] {
    return Array(5).fill(false).map((_, i) => i < rating);
  }

  onReviewAction(action: string, review: IReview): void {
    switch (action) {
      case 'edit':
        this.openEditDialog(review);
        break;
      case 'delete':
        this.deleteReview(review);
        break;
    }
  }
} 
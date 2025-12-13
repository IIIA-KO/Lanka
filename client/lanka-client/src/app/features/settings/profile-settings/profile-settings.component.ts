import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { DividerModule } from 'primeng/divider';
import { IBloggerProfile } from '../../../core/models/blogger';
import { AgentService } from '../../../core/api/agent';
import { UpdateBloggerProfileRequest } from '../../../core/api/bloggers.agent';
import { SnackbarService } from '../../../core/services/snackbar/snackbar.service';
import { AuthService } from '../../../core/services/auth/auth.service';
import { ImageCroppedEvent, ImageCropperComponent } from 'ngx-image-cropper';
import { FileUploadModule } from 'primeng/fileupload';
import { DialogModule } from 'primeng/dialog';
import { SelectModule } from 'primeng/select';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-profile-settings',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    CardModule,
    ButtonModule,
    InputTextModule,
    TextareaModule,
    DividerModule,
    ImageCropperComponent,
    FileUploadModule,
    DialogModule,
    SelectModule,
    TranslateModule
  ],
  templateUrl: './profile-settings.component.html',
  styleUrl: './profile-settings.component.css'
})
export class ProfileSettingsComponent implements OnInit {
  public profileForm!: FormGroup;
  public profile!: IBloggerProfile;
  public loading = false;
  // Account deletion properties
  public isDeletingAccount = false;
  public showDeleteConfirmation = false;

  // Avatar/photo properties
  public photoPreview: string | ArrayBuffer | null = null;
  public photoFile: File | null = null;
  public cropping = false;
  public imageChangedEvent: Event | null = null;
  public croppedImage: string | null = null;
  public uploading = false;
  public readonly categoryOptions: { label: string; value: string }[] = [
    'None',
    'Cooking and Food',
    'Fashion and Style',
    'Clothing and Footwear',
    'Horticulture',
    'Animals',
    'Cryptocurrency',
    'Technology',
    'Travel',
    'Education',
    'Fitness',
    'Art',
    'Photography',
    'Music',
    'Sports',
    'Health and Wellness',
    'Gaming',
    'Parenting',
    'DIY and Crafts',
    'Literature',
    'Science',
    'History',
    'News',
    'Politics',
    'Finance',
    'Environment',
    'Real Estate',
    'Automobiles',
    'Movies and TV',
    'Comedy',
    'Home Decor',
    'Relationships',
    'Self Improvement',
    'Entrepreneurship',
    'Legal Advice',
    'Marketing',
    'Mental Health',
    'Personal Development',
    'Religion and Spirituality',
    'Social Media'
  ].map(category => ({ label: category, value: category }));

  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(AgentService);
  private readonly snackbar = inject(SnackbarService);
  private readonly auth = inject(AuthService);
  private readonly translate = inject(TranslateService);

  public get photoUri(): string | null {
    return this.profile?.profilePhotoUri ?? null;
  }

  public ngOnInit(): void {
    this.profile = this.route.snapshot.data['profile'];
    this.initForm();
  }

  public onSubmit(): void {
    if (this.profileForm.invalid) {
      return;
    }

    this.loading = true;
    const formValue = this.profileForm.getRawValue();

    const updateRequest: UpdateBloggerProfileRequest = {
      firstName: formValue.firstName,
      lastName: formValue.lastName,
      birthDate: formValue.birthDate,
      bio: formValue.bio ?? '',
      category: formValue.category
    };

    this.api.Bloggers.updateProfile(updateRequest).subscribe({
      next: (updatedProfile) => {
        this.loading = false;
        this.snackbar.showSuccess('SETTINGS.PROFILE_UPDATED');
        this.profile = updatedProfile;
        this.profileForm.patchValue({
          bio: updatedProfile.bio ?? '',
          category: updatedProfile.category ?? 'None'
        });
        this.profileForm.markAsPristine();
      },
      error: (error) => {
        this.loading = false;
        console.error('Failed to update profile', error);
        this.snackbar.showError('SETTINGS.PROFILE_UPDATE_FAILED');
      }
    });
  }

  public toggleDeleteConfirmation(): void {
    this.showDeleteConfirmation = !this.showDeleteConfirmation;
  }

  public cancelAccountDeletion(): void {
    this.showDeleteConfirmation = false;
    this.isDeletingAccount = false;
  }

  public confirmAccountDeletion(): void {
    if (this.isDeletingAccount) return;

    this.isDeletingAccount = true;
    this.api.Users.deleteAccount().subscribe({
      next: () => {
        this.snackbar.showSuccess('SETTINGS.ACCOUNT_DELETED');
        this.auth.logout();
      },
      error: (error) => {
        this.isDeletingAccount = false;
        this.showDeleteConfirmation = false;
        this.snackbar.showError('SETTINGS.ACCOUNT_DELETE_FAILED');
        console.error('Delete account error', error);
      }
    });
  }

  // Avatar Handling Methods
  public onFileChange(event: Event): void {
    const target = event.target as HTMLInputElement;
    if (target.files && target.files[0]) {
      this.imageChangedEvent = event;
      this.cropping = true;
    }
  }

  public imageCropped(event: ImageCroppedEvent): void {
    if (event.base64) {
      this.croppedImage = event.base64;
      this.photoPreview = event.base64;
      this.photoFile = this.base64ToFile(event.base64, 'profile.jpg');
    } else if (event.blob) {
      this.photoFile = new File([event.blob], 'profile.jpg', { type: 'image/jpeg' });
      const reader = new FileReader();
      reader.onload = () => {
        this.photoPreview = reader.result;
        this.croppedImage = reader.result as string;
      };
      reader.readAsDataURL(event.blob);
    }
  }

  public imageLoaded(): void {
    // Image loaded successfully
  }

  public cropperReady(): void {
    // Cropper ready
  }

  public loadImageFailed(): void {
    this.snackbar.showError('SETTINGS.IMAGE_LOAD_FAILED');
  }

  public cancelCrop(): void {
    this.cropping = false;
    this.resetPhotoState();
    const fileInput = document.querySelector('input[type="file"]') as HTMLInputElement;
    if (fileInput) fileInput.value = '';
  }

  public uploadPhoto(): void {
    if (!this.photoFile) return;

    if (this.photoFile.size > 5 * 1024 * 1024) {
      this.snackbar.showError('SETTINGS.FILE_TOO_LARGE');
      return;
    }

    this.uploading = true;
    this.api.Bloggers.uploadProfilePhoto(this.photoFile).subscribe({
      next: (response) => {
         this.uploading = false;
         this.cropping = false;
         this.resetPhotoState();
         this.snackbar.showSuccess('SETTINGS.PHOTO_UPDATED');
         if (typeof response === 'string') {
             this.profile.profilePhotoUri = response;
         } else {
            this.refreshProfile();
         }
      },
      error: (err) => {
        this.uploading = false;
        this.snackbar.showError('SETTINGS.UPLOAD_FAILED');
        console.error('Upload error', err);
      }
    });
  }

  public removePhoto(): void {
      if (!confirm(this.translate.instant('SETTINGS.CONFIRM_REMOVE_PHOTO'))) return;
      
      this.uploading = true;
      this.api.Bloggers.deleteProfilePhoto().subscribe({
          next: () => {
              this.uploading = false;
              this.profile.profilePhotoUri = undefined;
              this.snackbar.showSuccess('SETTINGS.PHOTO_REMOVED');
          },
          error: () => {
              this.uploading = false;
              this.snackbar.showError('SETTINGS.REMOVE_FAILED');
          }
      });
  }

  private base64ToFile(dataurl: string, filename: string): File {
    try {
      const arr = dataurl.split(',');
      const mimeMatch = arr[0].match(/:(.*?);/);
      const mime = mimeMatch ? mimeMatch[1] : 'image/jpeg';
      const bstr = atob(arr[1]);
      let n = bstr.length;
      const u8arr = new Uint8Array(n);
      while (n--) {
        u8arr[n] = bstr.charCodeAt(n);
      }
      return new File([u8arr], filename, { type: mime });
    } catch (error) {
       console.error('Error converting base64', error);
       throw new Error('Failed to convert image');
    }
  }

  private resetPhotoState(): void {
    this.photoFile = null;
    this.photoPreview = null;
    this.croppedImage = null;
    this.imageChangedEvent = null;
  }

  private refreshProfile(): void {
      this.api.Bloggers.getProfile().subscribe({
          next: (profile) => this.profile = profile,
          error: () => this.snackbar.showError('SETTINGS.REFRESH_FAILED')
      });
  }

  private initForm(): void {
    let birthDateStr = '';
    if (this.profile?.birthDate) {
      const date = new Date(this.profile.birthDate);
      birthDateStr = date.toISOString().split('T')[0];
    }

    this.profileForm = this.fb.group({
      firstName: [this.profile?.firstName || '', [Validators.required]],
      lastName: [this.profile?.lastName || '', [Validators.required]],
      birthDate: [{ value: birthDateStr, disabled: true }, [Validators.required]],
      bio: [this.profile?.bio || '', [Validators.maxLength(500)]],
      category: [this.profile?.category || 'None', [Validators.required, Validators.maxLength(100)]]
    });
  }
}

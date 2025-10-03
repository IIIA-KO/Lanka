import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { AgentService } from '../../../../core/api/agent';
import { AuthService } from '../../../../core/services/auth/auth.service';
import { IBloggerProfile } from '../../../../core/models/blogger';
import {
  ImageCroppedEvent,
  ImageCropperComponent,
} from 'ngx-image-cropper';

@Component({
  imports: [
    FormsModule,
    ReactiveFormsModule,
    ImageCropperComponent
],
  selector: 'app-edit-profile',
  templateUrl: './edit-profile.component.html',
  styleUrl: './edit-profile.component.css',
})
export class EditProfileComponent implements OnInit {
  public profile: IBloggerProfile | null = null;
  public editForm: FormGroup;

  // Avatar/photo logic
  public photoPreview: string | ArrayBuffer | null = null;
  public photoFile: File | null = null;
  public cropping = false;
  public imageChangedEvent: Event | null = null;
  public croppedImage: string | null = null;
  public uploading = false;

  // Danger zone (account deletion)
  public deleteCountdown = 0;
  public confirmingDelete = false;
  public deletingAccount = false;
  public deleteError: string | null = null;

  private readonly api = inject(AgentService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly auth = inject(AuthService);
  private deleteTimer: ReturnType<typeof setInterval> | null = null;

  constructor() {
    const nav = this.router.getCurrentNavigation();
    if (nav?.extras?.state?.['profile']) {
      this.profile = nav.extras.state['profile'];
      console.warn('[EditProfileComponent] Profile from navigation state:', this.profile);
    }

    this.editForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.maxLength(50)]],
      lastName: ['', [Validators.required, Validators.maxLength(50)]],
      birthDate: ['', [Validators.required]],
      bio: ['', [Validators.maxLength(500)]],
    });
  }

  public get firstName() {
    return this.editForm.get('firstName');
  }

  public get lastName() {
    return this.editForm.get('lastName');
  }

  public get birthDate() {
    return this.editForm.get('birthDate');
  }

  public get bio() {
    return this.editForm.get('bio');
  }

  // Avatar/photo logic
  public get photoUri(): string | null {
    return this.profile?.profilePhotoUri ?? null;
  }

  public ngOnInit(): void {
    if (!this.profile) {
      this.profile = this.route.snapshot.data['profile'] ?? null;
      console.warn('[EditProfileComponent] Profile from route data:', this.profile);
    }

    if (this.profile) {
      this.editForm.patchValue({
        firstName: this.profile.firstName,
        lastName: this.profile.lastName,
        birthDate: this.profile.birthDate,
        bio: this.profile.bio,
      });
    }
  }

  public onSubmit(): void {
    if (this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      return;
    }
    this.api.Bloggers.updateProfile(this.editForm.value).subscribe({
      next: () => {
        this.router.navigate(['/profile']);
      },
    });
  }

  public onCancel(): void {
    this.router.navigate(['/profile']);
  }

  public onFileChange(event: Event): void {
    const target = event.target as HTMLInputElement;
    console.warn('[EditProfileComponent] File selected:', target.files?.[0]);
    if (target.files && target.files[0]) {
      this.imageChangedEvent = event;
      this.cropping = true;
      console.warn('[EditProfileComponent] Cropping mode activated');
    }
  }

  // Add this method to handle image loading
  public imageLoaded(): void {
    console.warn('[EditProfileComponent] Image loaded successfully');
    // Image loaded successfully
  }

  // Add this method to handle crop ready
  public cropperReady(): void {
    console.warn('[EditProfileComponent] Cropper ready');
    // Cropper ready
  }

  // Add this method to handle load failed
  public loadImageFailed(): void {
    console.warn('[EditProfileComponent] Image load failed');
    alert('Failed to load image. Please try again.');
  }

  public imageCropped(event: ImageCroppedEvent): void {
    console.warn('[EditProfileComponent] Image cropped event:', event);

    if (event.base64) {
      this.croppedImage = event.base64;
      this.photoPreview = event.base64;

      // Convert to File object
      this.photoFile = this.base64ToFile(event.base64, 'profile.jpg');
      console.warn('[EditProfileComponent] Cropped image set, photoFile:', this.photoFile);
    } else if (event.blob) {
      // Alternative: use blob if base64 is not available
      this.photoFile = new File([event.blob], 'profile.jpg', {
        type: 'image/jpeg',
      });

      // Convert blob to base64 for preview
      const reader = new FileReader();
      reader.onload = () => {
        this.photoPreview = reader.result;
        this.croppedImage = reader.result as string;
      };
      reader.readAsDataURL(event.blob);
      console.warn('[EditProfileComponent] Cropped image set from blob, photoFile:', this.photoFile);
    }
  }

  public uploadPhoto(): void {
    console.warn('[EditProfileComponent] Upload photo clicked, photoFile:', this.photoFile);

    if (!this.photoFile) {
      alert('No photo file to upload!');
      return;
    }

    // Validate file size (e.g., max 5MB)
    if (this.photoFile.size > 5 * 1024 * 1024) {
      alert('File size too large. Please select a smaller image.');
      return;
    }

    // Debug: Log file details
    console.warn('[EditProfileComponent] File details:', {
      name: this.photoFile.name,
      size: this.photoFile.size,
      type: this.photoFile.type,
    });

    this.uploading = true;

    this.api.Bloggers.uploadProfilePhoto(this.photoFile).subscribe({
      next: (response) => {
        console.warn('[EditProfileComponent] Upload successful:', response);
        this.cropping = false;
        this.resetPhotoState();
        this.uploading = false;

        // Refresh profile data
        this.refreshProfile();
      },
      error: (err) => {
        console.error('[EditProfileComponent] Upload error:', err);
        this.uploading = false;
        alert('Upload failed: ' + (err?.message || 'Unknown error'));
      },
    });
  }

  public removePhoto(): void {
    if (!confirm('Are you sure you want to remove your profile photo?')) {
      return;
    }

    this.uploading = true;

    this.api.Bloggers.deleteProfilePhoto().subscribe({
      next: () => {
        console.warn('[EditProfileComponent] Photo removed successfully');
        this.uploading = false;
        this.refreshProfile();
      },
      error: (err) => {
        console.error('[EditProfileComponent] Remove photo error:', err);
        this.uploading = false;
        alert('Failed to remove photo');
      },
    });
  }

  public cancelCrop(): void {
    this.cropping = false;
    this.resetPhotoState();

    // Reset file input
    const fileInput = document.querySelector(
      'input[type="file"]'
    ) as HTMLInputElement;
    if (fileInput) {
      fileInput.value = '';
    }
  }

  public startDeleteCountdown(): void {
    if (this.deletingAccount) return;
    this.deleteError = null;
    this.confirmingDelete = true;
    this.deleteCountdown = 5;

    this.clearDeleteTimer();
    this.deleteTimer = setInterval(() => {
      this.deleteCountdown -= 1;
      if (this.deleteCountdown <= 0) {
        this.clearDeleteTimer();
        this.performDelete();
      }
    }, 1000);
  }

  public cancelDeleteCountdown(): void {
    this.clearDeleteTimer();
    this.confirmingDelete = false;
    this.deleteCountdown = 0;
  }

  public performDelete(): void {
    this.deletingAccount = true;
    this.api.Users.deleteAccount().subscribe({
      next: () => {
        // Ensure timers are cleared and user is logged out
        this.clearDeleteTimer();
        this.auth.logout();
      },
      error: (err) => {
        this.deletingAccount = false;
        this.confirmingDelete = false;
        this.deleteCountdown = 0;
        this.deleteError = err?.message || 'Failed to delete account. Please try again later.';
      },
    });
  }

  private clearDeleteTimer(): void {
    if (this.deleteTimer) {
      clearInterval(this.deleteTimer);
      this.deleteTimer = null;
    }
  }

  private base64ToFile(dataurl: string, filename: string): File {
    try {
      const arr = dataurl.split(',');
      const mimeMatch = arr[0].match(/:(.*?);/);
      const mime = mimeMatch ? mimeMatch[1] : 'image/jpeg';
      const bstr = atob(arr[1]);
      const n = bstr.length;
      const u8arr = new Uint8Array(n);

      for (let i = 0; i < n; i++) {
        u8arr[i] = bstr.charCodeAt(i);
      }

      return new File([u8arr], filename, { type: mime });
    } catch (error) {
      console.error('[EditProfileComponent] Error converting base64 to file:', error);
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
      next: (profile: IBloggerProfile) => {
        this.profile = profile;
        console.warn('[EditProfileComponent] Profile refreshed:', profile);
      },
      error: (err) => {
        console.error('[EditProfileComponent] Error refreshing profile:', err);
      },
    });
  }
}

import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { IBloggerProfile } from '../../../../core/models/blogger';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AgentService } from '../../../../core/api/agent';
import {
  ImageCropperComponent,
  ImageCroppedEvent,
  LoadedImage,
} from 'ngx-image-cropper';
import { ActivatedRoute } from '@angular/router';

@Component({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    ImageCropperComponent,
  ],
  selector: 'app-edit-profile',
  templateUrl: './edit-profile.component.html',
  styleUrl: './edit-profile.component.css',
})
export class EditProfileComponent implements OnInit {
  profile: IBloggerProfile | null = null;
  editForm: FormGroup;

  // Avatar/photo logic
  photoPreview: string | ArrayBuffer | null = null;
  photoFile: File | null = null;
  cropping = false;
  imageChangedEvent: any = '';
  croppedImage: string | null = null;
  uploading = false;

  constructor(
    private api: AgentService,
    private router: Router,
    private fb: FormBuilder,
    private route: ActivatedRoute
  ) {
    const nav = this.router.getCurrentNavigation();
    if (nav?.extras?.state?.['profile']) {
      this.profile = nav.extras.state['profile'];
      console.log('Profile from navigation state:', this.profile);
    }

    this.editForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.maxLength(50)]],
      lastName: ['', [Validators.required, Validators.maxLength(50)]],
      birthDate: ['', [Validators.required]],
      bio: ['', [Validators.maxLength(500)]],
    });
  }

  ngOnInit(): void {
    if (!this.profile) {
      this.profile = this.route.snapshot.data['profile'] ?? null;
      console.log('Profile from route data:', this.profile);
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

  onSubmit(): void {
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

  onCancel(): void {
    this.router.navigate(['/profile']);
  }

  get firstName() {
    return this.editForm.get('firstName');
  }
  get lastName() {
    return this.editForm.get('lastName');
  }
  get birthDate() {
    return this.editForm.get('birthDate');
  }
  get bio() {
    return this.editForm.get('bio');
  }

  // Avatar/photo logic
  get photoUri(): string | null {
    return this.profile?.profilePhotoUri ?? null;
  }

  onFileChange(event: any): void {
    console.log('File selected:', event.target.files[0]);
    if (event.target.files && event.target.files[0]) {
      this.imageChangedEvent = event;
      this.cropping = true;
      console.log('Cropping mode activated');
    }
  }

  // Add this method to handle image loading
  imageLoaded(image: LoadedImage) {
    console.log('Image loaded successfully');
    // Image loaded successfully
  }

  // Add this method to handle crop ready
  cropperReady() {
    console.log('Cropper ready');
    // Cropper ready
  }

  // Add this method to handle load failed
  loadImageFailed() {
    console.log('Image load failed');
    alert('Failed to load image. Please try again.');
  }

  imageCropped(event: ImageCroppedEvent) {
    console.log('Image cropped event:', event);

    if (event.base64) {
      this.croppedImage = event.base64;
      this.photoPreview = event.base64;

      // Convert to File object
      this.photoFile = this.base64ToFile(event.base64, 'profile.jpg');
      console.log('Cropped image set, photoFile:', this.photoFile);
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
      console.log('Cropped image set from blob, photoFile:', this.photoFile);
    }
  }

  base64ToFile(dataurl: string, filename: string): File {
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
      console.error('Error converting base64 to file:', error);
      throw new Error('Failed to convert image');
    }
  }

  uploadPhoto() {
    console.log('Upload photo clicked, photoFile:', this.photoFile);

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
    console.log('File details:', {
      name: this.photoFile.name,
      size: this.photoFile.size,
      type: this.photoFile.type,
    });

    this.uploading = true;

    this.api.Bloggers.uploadProfilePhoto(this.photoFile).subscribe({
      next: (response) => {
        console.log('Upload successful:', response);
        this.cropping = false;
        this.resetPhotoState();
        this.uploading = false;

        // Refresh profile data
        this.refreshProfile();
      },
      error: (err) => {
        console.error('Upload error:', err);
        this.uploading = false;
        alert('Upload failed: ' + (err?.message || 'Unknown error'));
      },
    });
  }

  removePhoto() {
    if (!confirm('Are you sure you want to remove your profile photo?')) {
      return;
    }

    this.uploading = true;

    this.api.Bloggers.deleteProfilePhoto().subscribe({
      next: () => {
        console.log('Photo removed successfully');
        this.uploading = false;
        this.refreshProfile();
      },
      error: (err) => {
        console.error('Remove photo error:', err);
        this.uploading = false;
        alert('Failed to remove photo');
      },
    });
  }

  cancelCrop() {
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

  private resetPhotoState() {
    this.photoFile = null;
    this.photoPreview = null;
    this.croppedImage = null;
    this.imageChangedEvent = '';
  }

  private refreshProfile() {
    this.api.Bloggers.getProfile().subscribe({
      next: (profile: any) => {
        this.profile = profile;
        console.log('Profile refreshed:', profile);
      },
      error: (err) => {
        console.error('Error refreshing profile:', err);
      },
    });
  }
}

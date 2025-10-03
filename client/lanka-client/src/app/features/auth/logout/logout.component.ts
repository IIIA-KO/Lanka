import { Component, OnInit, inject } from '@angular/core';
import { AuthService } from '../../../core/services/auth/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-logout',
  standalone: true,
  template: '',
})
export class LogoutComponent implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  public ngOnInit(): void {
    this.auth.clearTokens();
    this.router.navigate(['/auth/login']);
  }
}

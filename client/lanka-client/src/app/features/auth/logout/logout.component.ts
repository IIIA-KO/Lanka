import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../../core/services/auth/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'lnk-logout',
  standalone: true,
  template: '',
})
export class LogoutComponent implements OnInit {
  constructor(private auth: AuthService, private router: Router) {}

  ngOnInit() {
    this.auth.clear();
    this.router.navigate(['/auth/login']);
  }
}

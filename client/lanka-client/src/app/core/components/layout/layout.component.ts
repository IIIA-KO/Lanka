import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from '../navbar/navbar.component';
import { AuthService } from '../../services/auth/auth.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'lnk-layout',
  imports: [RouterOutlet, CommonModule, NavbarComponent],
  templateUrl: './layout.component.html',
  styleUrl: './layout.component.css',
})
export class LayoutComponent {
  isAuthenticated$!: Observable<boolean>;

  constructor(private auth: AuthService) {
    this.isAuthenticated$ = this.auth.isAuthenticated$;
  }
}

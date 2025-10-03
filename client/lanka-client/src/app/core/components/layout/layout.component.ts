import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from '../navbar/navbar.component';
import { AuthService } from '../../services/auth/auth.service';
import { LoadingComponent } from '../../../shared/components/loading/loading.component';

@Component({
  selector: 'app-layout',
  imports: [RouterOutlet, CommonModule, NavbarComponent, LoadingComponent],
  templateUrl: './layout.component.html',
  styleUrl: './layout.component.css',
})
export class LayoutComponent {
  public isAuthenticated$ = this.auth.isAuthenticated$;
  private readonly auth = inject(AuthService);
}

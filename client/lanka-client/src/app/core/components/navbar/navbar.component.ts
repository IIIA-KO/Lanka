import { PanelMenuModule } from 'primeng/panelmenu';
import { Component, HostBinding } from '@angular/core';
import { RouterModule, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';

interface MenuItem {
  title: string;
  icon: string;
  path: string;
}

@Component({
  selector: 'lnk-navbar',
  imports: [PanelMenuModule, CommonModule, RouterModule, ConfirmDialogModule],
  standalone: true,
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css',
  providers: [ConfirmationService],
})
export class NavbarComponent {
  open = false;

  menu: MenuItem[] = [
    { title: 'Profile', icon: '/icons/profile-icon.svg', path: '/profile' },
    { title: 'Search', icon: '/icons/search-icon.svg', path: '/search' },
    { title: 'Statistics', icon: '/icons/statistics-icon.svg', path: '/statistics' },
    { title: 'Calendar', icon: '/icons/calendar-icon.svg', path: '/campaigns' },
    { title: 'Notifications', icon: '/icons/notifications-icon.svg', path: '/notifications' },
    { title: 'Pacts', icon: '/icons/pact-icon.svg', path: '/pact' },
  ];

  constructor(
    private router: Router,
    private confirmationService: ConfirmationService
  ) {}

  confirmLogout(): void{
    this.confirmationService.confirm({
      message: 'Are you sure you want to log out?',
      header: 'Confirm Logout',
      icon: 'pi pi-exclamation-circle',
      acceptButtonStyleClass: 'p-button-secondary',
      rejectButtonStyleClass: 'my-reject-btn ',
      accept: () => {
        this.router.navigate(['/logout']);
      },
      reject: () => {
        // User chose not to log out
      },
    });
  }
}

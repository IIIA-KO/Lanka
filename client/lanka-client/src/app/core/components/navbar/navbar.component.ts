import { PanelMenuModule } from 'primeng/panelmenu';
import { Component, HostBinding } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

interface MenuItem {
  title: string;
  icon: string;
  path: string;
}

@Component({
  selector: 'lnk-navbar',
  imports: [PanelMenuModule, CommonModule, RouterModule],
  standalone: true,
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css',
})
export class NavbarComponent {
  open = false;

  menu: MenuItem[] = [
    { title: 'Profile', icon: '/icons/profile-icon.svg', path: '/profile' },
    { title: 'Search', icon: '/icons/search-icon.svg', path: '/search' },
    { title: 'Statistics', icon: '/icons/statistics-icon.svg', path: '/statistics' },
    { title: 'Calendar', icon: '/icons/calendar-icon.svg', path: '/campaugns' },
    { title: 'Notifications', icon: '/icons/notifications-icon.svg', path: '/notifications' },
    { title: 'Terms of cooperation', icon: '/icons/termsOfcooperation-icon.svg', path: '/dashboard' },
  ];
}

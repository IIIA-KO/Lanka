import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'lnk-auth-layout',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './auth.layout.component.html',
  styleUrls: ['./auth.layout.component.css']
})
export class AuthLayoutComponent {}

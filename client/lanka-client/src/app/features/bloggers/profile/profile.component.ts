import { OnInit } from '@angular/core';
import { IBloggerProfile } from '../../../core/models/blogger';
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ActivatedRoute } from '@angular/router';
import { IAgeRatio } from '../../../core/models/analytics/analytics.audience';
import { AgentService } from '../../../core/api/agent';
@Component({
  imports: [CommonModule, RouterModule],
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
  profile: IBloggerProfile | null = null;
  ageRatio: IAgeRatio | null = null;
  error: string | null = null;

  constructor(private route: ActivatedRoute, private api: AgentService) {}

  ngOnInit(): void {
    this.profile = this.route.snapshot.data['profile'] ?? null;
    this.api.Analytics.getAudienceAgeRatio().subscribe({
      next: (data: IAgeRatio) => {
        this.ageRatio = data;
      },
      error: (err: any) => {
        this.error = err.message || 'Failed to load age ratio data';
      }
    });
  }
}

import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CampaignsAgent } from '../../core/api/campaigns.agent';
import { ICampaign, CampaignStatus } from '../../core/models/campaigns';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';

interface CalendarDay {
  date: Date;
  isCurrentMonth: boolean;
  isToday: boolean;
  events: ICampaign[];
}

interface CalendarMonth {
  name: string;
  year: number;
  days: CalendarDay[];
}

@Component({
  selector: 'app-calendar',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    CardModule,
    ButtonModule,
    SelectModule,
    InputTextModule,
    TagModule,
    TooltipModule
  ],
  templateUrl: './calendar.component.html',
  styleUrl: './calendar.component.css'
})
export class CalendarComponent implements OnInit {
  public campaigns: ICampaign[] = [];
  public filteredCampaigns: ICampaign[] = [];
  public months: CalendarMonth[] = [];
  public viewMode: 'grid' | 'list' = 'grid';
  public loading = false;

  // Filters
  public searchQuery = '';
  public statusFilter: CampaignStatus | 'All' = 'All';
  public statusOptions = [
    { label: 'All Statuses', value: 'All' },
    { label: 'Pending', value: CampaignStatus.Pending },
    { label: 'Confirmed', value: CampaignStatus.Confirmed },
    { label: 'In Progress', value: CampaignStatus.Confirmed }, // Mapping Confirmed to In Progress for UI
    { label: 'Done', value: CampaignStatus.Done },
    { label: 'Completed', value: CampaignStatus.Completed },
    { label: 'Cancelled', value: CampaignStatus.Cancelled },
    { label: 'Rejected', value: CampaignStatus.Rejected }
  ];

  private readonly campaignsApi = inject(CampaignsAgent);

  public ngOnInit(): void {
    this.loadCampaigns();
  }

  public loadCampaigns(): void {
    this.loading = true;
    this.campaignsApi.getCampaigns().subscribe({
      next: (data) => {
        this.campaigns = data;
        this.applyFilters();
        this.generateCalendar();
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load campaigns', err);
        this.loading = false;
      }
    });
  }

  public applyFilters(): void {
    this.filteredCampaigns = this.campaigns.filter(c => {
      const matchesSearch = !this.searchQuery || 
        c.name.toLowerCase().includes(this.searchQuery.toLowerCase()) ||
        c.description.toLowerCase().includes(this.searchQuery.toLowerCase());
      
      const matchesStatus = this.statusFilter === 'All' || c.status === this.statusFilter;

      return matchesSearch && matchesStatus;
    });

    if (this.viewMode === 'grid') {
      this.generateCalendar(); // Regenerate grid with filtered events
    }
  }

  public toggleView(mode: 'grid' | 'list'): void {
    this.viewMode = mode;
  }

  public getStatusSeverity(status: string): 'success' | 'info' | 'warning' | 'danger' | 'secondary' | 'contrast' | undefined {
    switch (status) {
      case CampaignStatus.Pending: return 'warning';
      case CampaignStatus.Confirmed: return 'info';
      case CampaignStatus.Done: return 'contrast'; // Changed from 'primary' to 'contrast'
      case CampaignStatus.Completed: return 'success';
      case CampaignStatus.Rejected: return 'danger';
      case CampaignStatus.Cancelled: return 'danger';
      default: return 'secondary';
    }
  }

  private generateCalendar(): void {
    const today = new Date();
    this.months = [];

    // Generate 3 months: Current, Next, Next+1
    for (let i = 0; i < 3; i++) {
      const monthDate = new Date(today.getFullYear(), today.getMonth() + i, 1);
      this.months.push(this.generateMonth(monthDate));
    }
  }

  private generateMonth(date: Date): CalendarMonth {
    const year = date.getFullYear();
    const month = date.getMonth();
    const daysInMonth = new Date(year, month + 1, 0).getDate();
    const firstDayOfWeek = new Date(year, month, 1).getDay(); // 0 = Sunday

    const days: CalendarDay[] = [];

    // Add empty slots for days before the first of the month (if starting week on Sunday)
    // Adjust logic if week starts on Monday
    for (let i = 0; i < firstDayOfWeek; i++) {
      days.push({
        date: new Date(year, month, -firstDayOfWeek + i + 1), // Previous month days
        isCurrentMonth: false,
        isToday: false,
        events: []
      });
    }

    // Add days of the month
    for (let i = 1; i <= daysInMonth; i++) {
      const currentDay = new Date(year, month, i);
      const isToday = currentDay.toDateString() === new Date().toDateString();
      
      // Find events for this day
      // Using expectedCompletionDate or scheduledOnUtc
      const events = this.filteredCampaigns.filter(c => {
        const eventDate = new Date(c.expectedCompletionDate); // Or scheduledOnUtc
        return eventDate.toDateString() === currentDay.toDateString();
      });

      days.push({
        date: currentDay,
        isCurrentMonth: true,
        isToday: isToday,
        events: events
      });
    }

    return {
      name: date.toLocaleString('default', { month: 'long' }),
      year: year,
      days: days
    };
  }
}

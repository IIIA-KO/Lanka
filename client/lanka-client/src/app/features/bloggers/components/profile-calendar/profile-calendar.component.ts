import { Component, Input, OnInit, OnChanges, SimpleChanges, inject } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule, DatePipe } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CampaignsAgent } from '../../../../core/api/campaigns.agent';
import { ICampaign, CampaignStatus } from '../../../../core/models/campaigns';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';

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
  selector: 'app-profile-calendar',
  standalone: true,
  imports: [CommonModule, TranslateModule, TooltipModule, DialogModule, TagModule, ButtonModule],
  providers: [DatePipe],
  templateUrl: './profile-calendar.component.html',
  styleUrl: './profile-calendar.component.css'
})
export class ProfileCalendarComponent implements OnInit, OnChanges {
  @Input() public bloggerId!: string;
  @Input() public isOwnProfile = false;
  @Input() public filterRole: 'all' | 'creator' | 'client' = 'all';

  public campaigns: ICampaign[] = [];
  public months: CalendarMonth[] = [];
  public loading = false;
  public weekdays = ['ПН', 'ВТ', 'СР', 'ЧТ', 'ПТ', 'СБ', 'НД'];
  public currentYear = new Date().getFullYear();
  public baseDate = new Date();

  private readonly campaignsApi = inject(CampaignsAgent);
  private readonly translate = inject(TranslateService);
  private readonly router = inject(Router);
  private readonly datePipe = inject(DatePipe);

  public ngOnInit(): void {
    // Determine language for weekdays if needed, or rely on a generic approach.
    // The user screenshot showed Ukrainian "ПН ВТ СР...". We can use translate service or fixed arrays.
    this.translate.onLangChange.subscribe(() => {
      this.updateWeekdays();
      this.generateCalendar(); // Regenerate names
    });
    this.updateWeekdays();

    if (this.bloggerId) {
      this.loadCampaigns();
    }
  }

  public ngOnChanges(changes: SimpleChanges): void {
    if (changes['filterRole'] && !changes['filterRole'].firstChange) {
      if (this.bloggerId) {
        this.loadCampaigns();
      }
    }
  }

  public onDayClick(day: CalendarDay): void {
    if (!this.isOwnProfile || day.events.length === 0) return;
    this.router.navigate(['/campaigns'], {
      queryParams: { view: 'calendar', d: day.date.toISOString() }
    });
  }

  public getTooltipText(day: CalendarDay): string {
    if (day.events.length === 0) return '';

    if (this.isOwnProfile) {
        return day.events.map(e => {
            const time = this.datePipe.transform(e.scheduledOnUtc, 'shortTime');
            const roleStr = e.creatorId?.toLowerCase() === this.bloggerId.toLowerCase()
              ? this.translate.instant('CAMPAIGNS.ROLE_CREATOR')
              : this.translate.instant('CAMPAIGNS.ROLE_CLIENT');
            return `${time} - ${e.name} (${roleStr}, ${e.status})`;
        }).join('\n');
    } else {
        return this.translate.instant('PROFILE.CALENDAR.CAMPAIGNS_COUNT', { count: day.events.length });
    }
  }

  public nextMonth(): void {
    this.baseDate = new Date(this.baseDate.getFullYear(), this.baseDate.getMonth() + 1, 1);
    this.loadCampaigns();
  }

  public prevMonth(): void {
    this.baseDate = new Date(this.baseDate.getFullYear(), this.baseDate.getMonth() - 1, 1);
    this.loadCampaigns();
  }

  private updateWeekdays(): void {
    const days = this.translate.instant('PROFILE.CALENDAR.WEEKDAYS');
    this.weekdays = Array.isArray(days) ? days : ['MO', 'TU', 'WE', 'TH', 'FR', 'SA', 'SU'];
  }

  private loadCampaigns(): void {
    this.loading = this.months.length === 0;
    const startDate = new Date(this.baseDate.getFullYear(), this.baseDate.getMonth(), 1).toISOString();
    const endDate = new Date(this.baseDate.getFullYear(), this.baseDate.getMonth() + 3, 0, 23, 59, 59).toISOString();

    const request$ = this.campaignsApi.getBloggerCampaigns(this.bloggerId, startDate, endDate);

    request$.subscribe({
      next: (campaigns) => {
        // Exclude Rejected, Cancelled, and maybe Pending for public visibility?
        // Pending campaigns might not be fully scheduled. Let's show everything except Rejected/Cancelled, but for public profile, maybe only Confirmed/Done/Completed?
        // Let's be safe and show everything not cancelled/rejected.
        // Filter cancelled/rejected first
        let activeCampaigns = campaigns.filter(c => 
          c.status !== CampaignStatus.Rejected && c.status !== CampaignStatus.Cancelled
        );

        // Apply role filter if specified
        if (this.filterRole !== 'all') {
          activeCampaigns = activeCampaigns.filter(c => 
            this.filterRole === 'creator'
              ? c.creatorId?.toLowerCase() === this.bloggerId.toLowerCase()
              : c.clientId?.toLowerCase() === this.bloggerId.toLowerCase()
          );
        }

        this.campaigns = activeCampaigns;
        this.generateCalendar();
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load campaigns for calendar', err);
        this.loading = false;
      }
    });
  }

  private generateCalendar(): void {
    this.currentYear = this.baseDate.getFullYear();
    this.months = [];

    // Let's generate 3 months rolling window: Current month, +1, +2
    for (let i = 0; i < 3; i++) {
      const monthDate = new Date(this.baseDate.getFullYear(), this.baseDate.getMonth() + i, 1);
      this.months.push(this.generateMonth(monthDate));
    }
  }

  private generateMonth(date: Date): CalendarMonth {
    const year = date.getFullYear();
    const month = date.getMonth();
    const daysInMonth = new Date(year, month + 1, 0).getDate();
    
    // JS getDay() is 0 for Sunday. We want Monday to be 0 for our UI.
    let firstDayOfWeek = new Date(year, month, 1).getDay() - 1;
    if (firstDayOfWeek === -1) firstDayOfWeek = 6; // Sunday becomes 6

    const days: CalendarDay[] = [];

    // Empty slots before 1st of month
    for (let i = 0; i < firstDayOfWeek; i++) {
        const prevDate = new Date(year, month, -firstDayOfWeek + i + 1);
        days.push({
            date: prevDate,
            isCurrentMonth: false,
            isToday: false,
            events: []
        });
    }

    // Actual days
    for (let i = 1; i <= daysInMonth; i++) {
      const currentDay = new Date(year, month, i);
      const isToday = currentDay.toDateString() === new Date().toDateString();
      
      const events = this.campaigns.filter(c => {
        const eventDate = new Date(c.scheduledOnUtc);
        return eventDate.toDateString() === currentDay.toDateString();
      });

      days.push({
        date: currentDay,
        isCurrentMonth: true,
        isToday: isToday,
        events: events
      });
    }
    
    // Fill remaining to complete the 7-day grid row
    const remainder = days.length % 7;
    if (remainder !== 0) {
        for (let i = 1; i <= 7 - remainder; i++) {
            const nextDate = new Date(year, month + 1, i);
            days.push({
                date: nextDate,
                isCurrentMonth: false,
                isToday: false,
                events: []
            });
        }
    }

    const lang = this.translate.currentLang || this.translate.defaultLang || 'en';
    const locale = lang === 'uk' ? 'uk-UA' : 'en-US';
    const monthName = date.toLocaleString(locale, { month: 'long' });

    return {
      name: monthName.toUpperCase(),
      year: year,
      days: days
    };
  }

}

import { Component, Input } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { DividerModule } from 'primeng/divider';
import { ICampaignReport } from '../../../../../core/models/campaigns';

@Component({
  standalone: true,
  selector: 'app-campaign-report',
  imports: [CommonModule, DatePipe, TranslateModule, DividerModule],
  templateUrl: './campaign-report.component.html',
  styleUrls: ['./campaign-report.component.css'],
})
export class CampaignReportComponent {
  @Input() public report!: ICampaignReport;
}

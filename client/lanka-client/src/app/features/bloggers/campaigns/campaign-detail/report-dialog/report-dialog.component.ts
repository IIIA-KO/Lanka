import { Component, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { ChipModule } from 'primeng/chip';
import { TranslateModule } from '@ngx-translate/core';
import { IMarkCampaignAsDoneRequest, ICampaignReport } from '../../../../../core/models/campaigns';

@Component({
  standalone: true,
  selector: 'app-report-dialog',
  imports: [
    CommonModule,
    FormsModule,
    DialogModule,
    ButtonModule,
    InputTextModule,
    TextareaModule,
    ChipModule,
    TranslateModule,
  ],
  templateUrl: './report-dialog.component.html',
  styleUrls: ['./report-dialog.component.css'],
})
export class ReportDialogComponent {
  @Output() public submitted = new EventEmitter<IMarkCampaignAsDoneRequest>();
  @Output() public cancelled = new EventEmitter<void>();

  public visible = false;
  public submitting = false;
  public isEditMode = false;

  public contentDelivered = '';
  public approach = '';
  public notes = '';
  public newLink = '';
  public postPermalinks: string[] = [];

  public get isValid(): boolean {
    return this.contentDelivered.trim().length >= 10 && this.approach.trim().length >= 10;
  }

  public open(): void {
    this.reset();
    this.isEditMode = false;
    this.visible = true;
  }

  public openForEdit(report: ICampaignReport): void {
    this.reset();
    this.isEditMode = true;
    this.contentDelivered = report.contentDelivered;
    this.approach = report.approach;
    this.notes = report.notes ?? '';
    this.postPermalinks = [...(report.postPermalinks ?? [])];
    this.visible = true;
  }

  public close(): void {
    this.visible = false;
    this.cancelled.emit();
  }

  public addLink(): void {
    const link = this.newLink.trim();
    if (link && !this.postPermalinks.includes(link)) {
      this.postPermalinks.push(link);
    }
    this.newLink = '';
  }

  public removeLink(link: string): void {
    this.postPermalinks = this.postPermalinks.filter(l => l !== link);
  }

  public onLinkKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      event.preventDefault();
      this.addLink();
    }
  }

  public submit(): void {
    if (!this.isValid) { return; }
    this.submitting = true;
    this.submitted.emit({
      contentDelivered: this.contentDelivered.trim(),
      approach: this.approach.trim(),
      notes: this.notes.trim() || undefined,
      postPermalinks: this.postPermalinks,
    });
  }

  public resetSubmitting(): void {
    this.submitting = false;
  }

  private reset(): void {
    this.contentDelivered = '';
    this.approach = '';
    this.notes = '';
    this.newLink = '';
    this.postPermalinks = [];
    this.submitting = false;
  }
}

import { DatePipe, NgClass } from '@angular/common';
import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subject, forkJoin, takeUntil } from 'rxjs';

import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';

import { BloggersAgent } from '../../core/api/bloggers.agent';
import { ChatAgent, IChatThread } from '../../core/api/chat.agent';
import { SnackbarService } from '../../core/services/snackbar/snackbar.service';
import { ChatThreadComponent } from './chat-thread/chat-thread.component';

@Component({
  standalone: true,
  selector: 'app-chats',
  imports: [
    DatePipe,
    NgClass,
    RouterLink,
    TranslateModule,
    ButtonModule,
    ProgressSpinnerModule,
    TagModule,
    TooltipModule,
    ChatThreadComponent,
  ],
  templateUrl: './chats.component.html',
  styleUrl: './chats.component.css',
})
export class ChatsComponent implements OnInit, OnDestroy {
  public threads: IChatThread[] = [];
  public currentBloggerId: string | null = null;
  public selectedThreadId: string | null = null;
  public loading = true;

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly chatAgent = inject(ChatAgent);
  private readonly bloggersAgent = inject(BloggersAgent);
  private readonly snackbar = inject(SnackbarService);
  private readonly translate = inject(TranslateService);
  private readonly destroy$ = new Subject<void>();

  public ngOnInit(): void {
    this.route.paramMap
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        this.selectedThreadId = params.get('threadId');
        this.ensureSelectedThread();
      });

    this.load();
  }

  public ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  public selectThread(thread: IChatThread): void {
    this.selectedThreadId = thread.id;
    void this.router.navigate(['/chats', thread.id]);
  }

  public refresh(): void {
    this.load();
  }

  public selectedThread(): IChatThread | null {
    return this.threads.find(thread => thread.id === this.selectedThreadId) ?? null;
  }

  public participantName(thread: IChatThread): string {
    return `${thread.otherParticipantFirstName ?? ''} ${thread.otherParticipantLastName ?? ''}`.trim()
      || this.translate.instant('CHAT.UNKNOWN_USER');
  }

  public threadContext(thread: IChatThread): string {
    if (thread.offerName) {
      return this.translate.instant('CHAT.OFFER_CONTEXT', { name: thread.offerName });
    }

    if (thread.campaignName) {
      return this.translate.instant('CHAT.CAMPAIGN_CONTEXT', { name: thread.campaignName });
    }

    return this.translate.instant('CHAT.DIRECT_CONTEXT');
  }

  public preview(thread: IChatThread): string {
    if (!thread.lastMessageContent) {
      return this.translate.instant('CHAT.NO_MESSAGES');
    }

    if (thread.lastMessageIsSystem) {
      return this.translate.instant(`CHAT.SYSTEM.${thread.lastMessageContent}`);
    }

    return thread.lastMessageContent;
  }

  public profileRoute(thread: IChatThread): unknown[] {
    return ['/bloggers', thread.otherParticipantId];
  }

  private load(): void {
    this.loading = true;
    forkJoin({
      profile: this.bloggersAgent.getProfile(),
      threads: this.chatAgent.getThreads(),
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: ({ profile, threads }) => {
          this.currentBloggerId = profile.id;
          this.threads = threads;
          this.loading = false;
          this.ensureSelectedThread();
        },
        error: () => {
          this.loading = false;
          this.snackbar.showError(this.translate.instant('CHAT.ERROR_LOAD_THREADS'));
        },
      });
  }

  private ensureSelectedThread(): void {
    if (this.loading || this.threads.length === 0) {
      return;
    }

    const selectedExists = this.threads.some(thread => thread.id === this.selectedThreadId);
    if (!selectedExists) {
      this.selectedThreadId = this.threads[0].id;
      void this.router.navigate(['/chats', this.selectedThreadId], { replaceUrl: true });
    }
  }
}

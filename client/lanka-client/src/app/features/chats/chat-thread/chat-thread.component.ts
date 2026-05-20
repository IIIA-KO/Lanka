import { DatePipe } from '@angular/common';
import { AfterViewInit, Component, ElementRef, Input, OnChanges, OnDestroy, OnInit, SimpleChanges, ViewChild, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subject, catchError, finalize, forkJoin, of, takeUntil } from 'rxjs';

import { ButtonModule } from 'primeng/button';
import { TextareaModule } from 'primeng/textarea';
import { TooltipModule } from 'primeng/tooltip';

import { ChatAgent, IChatMessage, IChatThread } from '../../../core/api/chat.agent';
import { AuthService } from '../../../core/services/auth/auth.service';
import { NotificationsService } from '../../../core/services/notifications.service';
import { SignalRService } from '../../../core/services/signalr.service';
import { SnackbarService } from '../../../core/services/snackbar/snackbar.service';

@Component({
  standalone: true,
  selector: 'app-chat-thread',
  imports: [DatePipe, FormsModule, RouterLink, TranslateModule, ButtonModule, TextareaModule, TooltipModule],
  templateUrl: './chat-thread.component.html',
  styleUrl: './chat-thread.component.css',
})
export class ChatThreadComponent implements OnInit, OnChanges, AfterViewInit, OnDestroy {
  @Input({ required: true }) public threadId!: string;
  @Input({ required: true }) public currentBloggerId!: string;
  @Input() public thread: IChatThread | null = null;

  @ViewChild('messageList') private messageList?: ElementRef<HTMLDivElement>;

  public messages: IChatMessage[] = [];
  public draft = '';
  public loading = true;
  public loadingOlder = false;
  public hasMore = true;
  public sending = false;
  public editingMessageId: string | null = null;
  public editDraft = '';

  private viewReady = false;
  private initialized = false;
  private readonly destroy$ = new Subject<void>();
  private readonly chatAgent = inject(ChatAgent);
  private readonly authService = inject(AuthService);
  private readonly notificationsService = inject(NotificationsService);
  private readonly signalRService = inject(SignalRService);
  private readonly snackbarService = inject(SnackbarService);
  private readonly translate = inject(TranslateService);

  public ngOnInit(): void {
    this.initialized = true;
    this.notificationsService.setActiveChatThread(this.threadId);
    this.loadInitialMessages();
    this.connectRealtime();
  }

  public ngOnChanges(changes: SimpleChanges): void {
    if (!this.initialized || !changes['threadId']) {
      return;
    }

    const previousThreadId = changes['threadId'].previousValue as string | undefined;
    if (previousThreadId) {
      void this.signalRService.leaveChat(previousThreadId);
    }

    this.messages = [];
    this.draft = '';
    this.editingMessageId = null;
    this.editDraft = '';
    this.hasMore = true;
    this.notificationsService.setActiveChatThread(this.threadId);
    this.loadInitialMessages();
    void this.signalRService.joinChat(this.threadId);
  }

  public ngAfterViewInit(): void {
    this.viewReady = true;
  }

  public ngOnDestroy(): void {
    void this.signalRService.leaveChat(this.threadId);
    this.notificationsService.setActiveChatThread(null);
    this.destroy$.next();
    this.destroy$.complete();
  }

  public isOwn(message: IChatMessage): boolean {
    return !!message.senderBloggerId && message.senderBloggerId.toLowerCase() === this.currentBloggerId.toLowerCase();
  }

  public canModify(message: IChatMessage): boolean {
    return this.isOwn(message) && !message.isSystem && !message.isDeleted;
  }

  public authorName(message: IChatMessage): string {
    const name = `${message.senderFirstName ?? ''} ${message.senderLastName ?? ''}`.trim();
    return name || this.translate.instant('CHAT.UNKNOWN_USER');
  }

  public systemText(message: IChatMessage): string {
    return this.translate.instant(`CHAT.SYSTEM.${message.content}`);
  }

  public participantName(): string {
    const name = `${this.thread?.otherParticipantFirstName ?? ''} ${this.thread?.otherParticipantLastName ?? ''}`.trim();
    return name || this.translate.instant('CHAT.UNKNOWN_USER');
  }

  public threadContext(): string {
    if (this.thread?.offerName) {
      return this.translate.instant('CHAT.OFFER_CONTEXT', { name: this.thread.offerName });
    }

    if (this.thread?.campaignName) {
      return this.translate.instant('CHAT.CAMPAIGN_CONTEXT', { name: this.thread.campaignName });
    }

    return this.translate.instant('CHAT.DIRECT_CONTEXT');
  }

  public profileRoute(): unknown[] {
    return ['/bloggers', this.thread?.otherParticipantId];
  }

  public onScroll(): void {
    const element = this.messageList?.nativeElement;
    if (!element || element.scrollTop > 48 || this.loadingOlder || !this.hasMore || this.messages.length === 0) {
      return;
    }

    this.loadOlderMessages();
  }

  public onComposerKeydown(event: KeyboardEvent): void {
    if (event.key !== 'Enter' || event.shiftKey) {
      return;
    }

    event.preventDefault();
    this.send();
  }

  public send(): void {
    const content = this.draft.trim();
    if (!content || this.sending) {
      return;
    }

    this.sending = true;
    this.chatAgent.sendMessage(this.threadId, content)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => { this.sending = false; })
      )
      .subscribe({
        next: message => {
          this.draft = '';
          this.upsertMessage(message);
          this.scrollToBottom();
        },
        error: () => this.snackbarService.showError(this.translate.instant('CHAT.ERROR_SEND')),
      });
  }

  public startEdit(message: IChatMessage): void {
    this.editingMessageId = message.id;
    this.editDraft = message.content;
  }

  public cancelEdit(): void {
    this.editingMessageId = null;
    this.editDraft = '';
  }

  public saveEdit(message: IChatMessage): void {
    const content = this.editDraft.trim();
    if (!content) {
      return;
    }

    this.chatAgent.editMessage(this.threadId, message.id, content)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: updated => {
          this.upsertMessage(updated);
          this.cancelEdit();
        },
        error: () => this.snackbarService.showError(this.translate.instant('CHAT.ERROR_EDIT')),
      });
  }

  public delete(message: IChatMessage): void {
    if (!window.confirm(this.translate.instant('CHAT.DELETE_CONFIRM'))) {
      return;
    }

    this.chatAgent.deleteMessage(this.threadId, message.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => this.applyDeleted(message.id),
        error: () => this.snackbarService.showError(this.translate.instant('CHAT.ERROR_DELETE')),
      });
  }

  private loadInitialMessages(): void {
    this.loading = true;
    forkJoin({
      messages: this.chatAgent.getMessages(this.threadId, undefined, 30).pipe(catchError(() => of([] as IChatMessage[]))),
      markRead: this.chatAgent.markRead(this.threadId).pipe(catchError(() => of(undefined))),
    })
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => { this.loading = false; })
      )
      .subscribe(({ messages }) => {
        this.messages = [...messages].reverse();
        this.hasMore = messages.length === 30;
        this.scrollToBottom();
      });
  }

  private loadOlderMessages(): void {
    const first = this.messages[0];
    if (!first) {
      return;
    }

    const element = this.messageList?.nativeElement;
    const previousHeight = element?.scrollHeight ?? 0;
    this.loadingOlder = true;

    this.chatAgent.getMessages(this.threadId, first.createdAtUtc, 30)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => { this.loadingOlder = false; })
      )
      .subscribe({
        next: messages => {
          const older = [...messages].reverse();
          this.messages = [...older, ...this.messages];
          this.hasMore = messages.length === 30;

          queueMicrotask(() => {
            if (!element) {
              return;
            }

            element.scrollTop = element.scrollHeight - previousHeight;
          });
        },
        error: () => this.snackbarService.showError(this.translate.instant('CHAT.ERROR_LOAD')),
      });
  }

  private connectRealtime(): void {
    const token = this.authService.getToken();
    if (!token) {
      return;
    }

    this.signalRService.startConnection(token)
      .then(() => this.signalRService.joinChat(this.threadId))
      .catch(() => undefined);

    this.signalRService.connectionState
      .pipe(takeUntil(this.destroy$))
      .subscribe(state => {
        if (state === 'connected') {
          void this.signalRService.joinChat(this.threadId);
        }
      });

    this.signalRService.chatMessageSent$
      .pipe(takeUntil(this.destroy$))
      .subscribe(message => {
        if (message.threadId !== this.threadId) {
          return;
        }

        this.upsertMessage(message);
        this.scrollToBottom();
        if (!this.isOwn(message)) {
          this.markRead();
        }
      });

    this.signalRService.chatMessageEdited$
      .pipe(takeUntil(this.destroy$))
      .subscribe(message => {
        if (message.threadId === this.threadId) {
          this.upsertMessage(message);
        }
      });

    this.signalRService.chatMessageDeleted$
      .pipe(takeUntil(this.destroy$))
      .subscribe(event => {
        if (event.threadId === this.threadId) {
          this.applyDeleted(event.messageId);
        }
      });

    this.signalRService.chatMessagesRead$
      .pipe(takeUntil(this.destroy$))
      .subscribe(event => {
        if (event.threadId !== this.threadId || event.readerBloggerId.toLowerCase() === this.currentBloggerId.toLowerCase()) {
          return;
        }

        this.messages = this.messages.map(message =>
          this.isOwn(message) && !message.readAtUtc ? { ...message, readAtUtc: event.readAtUtc } : message
        );
      });
  }

  private markRead(): void {
    this.chatAgent.markRead(this.threadId)
      .pipe(takeUntil(this.destroy$), catchError(() => of(undefined)))
      .subscribe();
  }

  private upsertMessage(message: IChatMessage): void {
    const index = this.messages.findIndex(existing => existing.id === message.id);
    if (index >= 0) {
      this.messages[index] = message;
      this.messages = [...this.messages];
      return;
    }

    this.messages = [...this.messages, message].sort(
      (left, right) => new Date(left.createdAtUtc).getTime() - new Date(right.createdAtUtc).getTime()
    );
  }

  private applyDeleted(messageId: string): void {
    this.messages = this.messages.map(message =>
      message.id === messageId ? { ...message, isDeleted: true, content: '' } : message
    );
  }

  private scrollToBottom(): void {
    if (!this.viewReady) {
      setTimeout(() => this.scrollToBottom(), 0);
      return;
    }

    queueMicrotask(() => {
      const element = this.messageList?.nativeElement;
      if (element) {
        element.scrollTop = element.scrollHeight;
      }
    });
  }
}

import { Injectable, inject } from '@angular/core';
import { UsersAgent } from './users.agent';
import { BloggersAgent } from './bloggers.agent';
import { AnalyticsAgent } from './analytics.agent';

@Injectable({ providedIn: 'root' })
export class AgentService {
  public readonly Users = inject(UsersAgent);
  public readonly Bloggers = inject(BloggersAgent);
  public readonly Analytics = inject(AnalyticsAgent);
}

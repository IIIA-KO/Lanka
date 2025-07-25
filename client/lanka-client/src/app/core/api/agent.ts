import { Injectable } from '@angular/core';
import { UsersAgent } from './users.agent';
import { BloggersAgent } from './bloggers.agent';
import { AnalyticsAgent } from './analytics.agent';
@Injectable({ providedIn: 'root' })
export class AgentService {
  constructor(
    public Users: UsersAgent,
    public Bloggers: BloggersAgent,
    public Analytics: AnalyticsAgent
  ) {}
}

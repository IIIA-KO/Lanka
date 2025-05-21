import { Injectable } from '@angular/core';
import { UsersAgent } from './users.agent';

@Injectable({ providedIn: 'root' })
export class AgentService {
  constructor(public Users: UsersAgent) {}
}

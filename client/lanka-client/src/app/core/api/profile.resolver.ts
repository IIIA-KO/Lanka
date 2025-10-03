import { Injectable, inject } from '@angular/core';
import { Resolve } from '@angular/router';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { BloggersAgent } from './bloggers.agent';
import { IBloggerProfile } from '../models/blogger';

@Injectable({ providedIn: 'root' })
export class ProfileResolver implements Resolve<IBloggerProfile | null> {
  private readonly bloggersAgent = inject(BloggersAgent);

  public resolve(): Observable<IBloggerProfile | null> {
    return this.bloggersAgent.getProfile().pipe(
      catchError(() => of(null))
    );
  }
} 
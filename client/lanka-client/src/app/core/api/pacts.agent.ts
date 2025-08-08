import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import { IPact, ICreatePactRequest, IEditPactRequest } from '../models/campaigns';
import { AuthService } from '../services/auth/auth.service';
import { JwtService } from '../services/jwt/jwt.service';

const BASE_URL = environment.apiUrl;

@Injectable({
  providedIn: 'root',
})
export class PactsAgent {
  constructor(
    private http: HttpClient,
    private authService: AuthService,
    private jwtService: JwtService
  ) {}

  private handleError(error: any) {
    const message = error.error?.message || error.message || 'Unknown error';
    return throwError(() => new Error(message));
  }

  createPact(request: ICreatePactRequest): Observable<string> {
    return this.http
      .post<string>(`${BASE_URL}/pacts`, request)
      .pipe(catchError(this.handleError));
  }

  getBloggerPact(): Observable<IPact> {
    const bloggerId = this.getBloggerId();
    if (!bloggerId) {
      return throwError(() => new Error('User not authenticated'));
    }
    
    return this.http
      .get<IPact>(`${BASE_URL}/pacts/${bloggerId}`)
      .pipe(catchError(this.handleError));
  }

  editPact(request: IEditPactRequest): Observable<IPact> {
    return this.http
      .put<IPact>(`${BASE_URL}/pacts/${request.pactId}`, request)
      .pipe(catchError(this.handleError));
  }

  /**
   * Gets the current user's blogger ID from the JWT token
   */
  private getBloggerId(): string | null {
    const token = this.authService.getToken();
    if (!token) {
      return null;
    }
    
    return this.jwtService.getUserIdFromToken(token);
  }
}

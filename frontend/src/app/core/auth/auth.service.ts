import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { map, Observable, tap, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../api/api-response';
import { AuthResponse, CurrentUser, LoginRequest } from './auth.types';

const STORAGE_KEY = 'jewelry-auth';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  // Signal-based state per CLAUDE.md §11
  private readonly _user = signal<CurrentUser | null>(this.loadFromStorage());
  readonly user = this._user.asReadonly();
  readonly isAuthenticated = computed(() => this._user() !== null);

  login(req: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<ApiResponse<AuthResponse>>(`${environment.apiBaseUrl}/auth/login`, req)
      .pipe(
        map((res) => {
          if (!res.success || !res.data) {
            throw res.errors ?? [{ field: 'auth', message: 'Login failed' }];
          }
          return res.data;
        }),
        tap((auth) => this.persist(auth)),
      );
  }

  logout(): void {
    localStorage.removeItem(STORAGE_KEY);
    this._user.set(null);
    this.router.navigate(['/login']);
  }

  getAccessToken(): string | null {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;
    try {
      return (JSON.parse(raw) as AuthResponse).accessToken ?? null;
    } catch {
      return null;
    }
  }

  private persist(auth: AuthResponse): void {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(auth));
    this._user.set({
      userId: auth.userId,
      email: auth.email,
      fullName: auth.fullName,
      role: auth.role,
    });
  }

  private loadFromStorage(): CurrentUser | null {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;
    try {
      const a = JSON.parse(raw) as AuthResponse;
      if (new Date(a.refreshTokenExpiresAt) < new Date()) {
        localStorage.removeItem(STORAGE_KEY);
        return null;
      }
      return {
        userId: a.userId,
        email: a.email,
        fullName: a.fullName,
        role: a.role,
      };
    } catch {
      return null;
    }
  }
}

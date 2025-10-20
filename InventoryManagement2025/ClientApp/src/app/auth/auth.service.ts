import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, map, tap } from 'rxjs';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  phoneNumber?: string | null;
  department?: string | null;
}

export interface AuthResponse {
  accessToken: string;
  expiresAt: string;
  user: UserProfile;
}

export interface UserProfile {
  userId: number;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  department?: string | null;
  phoneNumber?: string | null;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenKey = 'inventory.jwt';
  private readonly userKey = 'inventory.user';
  private readonly baseUrl = '/api/auth';

  private readonly currentUserSubject = new BehaviorSubject<UserProfile | null>(this.loadStoredUser());

  readonly currentUser$ = this.currentUserSubject.asObservable();
  readonly isAuthenticated$ = this.currentUser$.pipe(map((user) => !!user));

  constructor(private readonly http: HttpClient) {}

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/login`, request)
      .pipe(tap((response) => this.persistAuth(response)));
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/register`, request)
      .pipe(tap((response) => this.persistAuth(response)));
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
    this.currentUserSubject.next(null);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  private persistAuth(response: AuthResponse): void {
    localStorage.setItem(this.tokenKey, response.accessToken);
    localStorage.setItem(this.userKey, JSON.stringify(response.user));
    this.currentUserSubject.next(response.user);
  }

  private loadStoredUser(): UserProfile | null {
    const userRaw = localStorage.getItem(this.userKey);
    if (!userRaw) {
      return null;
    }

    try {
      return JSON.parse(userRaw) as UserProfile;
    } catch (error) {
      localStorage.removeItem(this.userKey);
      return null;
    }
  }
}

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';

export interface LoginRequest {
  userId: string;
}

export interface UserDto {
  id: string;
  fullName: string;
  roleCode: string;
  roleName: string;
  departmentCode: string;
  departmentName: string;
}

export interface LoginResponse {
  success: boolean;
  message: string;
  token: string;
  user: UserDto;
}

// Keycloak config — phải khớp với realm-export.json
const KC_URL      = 'http://localhost:8080';
const KC_REALM    = 'vbdh-realm';
const KC_CLIENT   = 'vbdh-client';
const REDIRECT_URI = `${window.location.origin}/callback`;

@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = '/api/auth';
  private _user$ = new BehaviorSubject<UserDto | null>(this.getUser());
  readonly user$ = this._user$.asObservable();

  constructor(private http: HttpClient) {}

  // ── Keycloak Authorization Code Flow ──────────────────────────

  /** Redirect trình duyệt sang trang login của Keycloak */
  redirectToKeycloak(): void {
    const state = crypto.randomUUID();
    const nonce = crypto.randomUUID();
    sessionStorage.setItem('kc_state', state);
    sessionStorage.setItem('kc_nonce', nonce);

    const params = new URLSearchParams({
      client_id:     KC_CLIENT,
      redirect_uri:  REDIRECT_URI,
      response_type: 'code',
      scope:         'openid profile email',
      state,
      nonce,
    });

    window.location.href = `${KC_URL}/realms/${KC_REALM}/protocol/openid-connect/auth?${params}`;
  }

  /** Đổi authorization code lấy token (gọi từ /callback) */
  exchangeCode(code: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/keycloak-callback`, { code, redirectUri: REDIRECT_URI });
  }

  /** Refresh token Keycloak */
  keycloakRefresh(refreshToken: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/keycloak-refresh`, { refreshToken });
  }

  // ── Login nội bộ ───────────────────────────────────────────────

  login(userId: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, { userId });
  }

  logout(): Observable<any> {
    return this.http.post(`${this.apiUrl}/logout`, {});
  }

  // ── Token storage ──────────────────────────────────────────────

  getToken(): string | null        { return localStorage.getItem('token'); }
  setToken(t: string): void        { localStorage.setItem('token', t); }
  getRefreshToken(): string | null { return localStorage.getItem('refresh_token'); }
  setRefreshToken(t: string): void { localStorage.setItem('refresh_token', t); }
  getIdToken(): string | null      { return localStorage.getItem('id_token'); }
  setIdToken(t: string): void      { localStorage.setItem('id_token', t); }

  getUser(): UserDto | null {
    const u = localStorage.getItem('user');
    return u ? JSON.parse(u) : null;
  }
  setUser(u: UserDto): void {
    localStorage.setItem('user', JSON.stringify(u));
    this._user$.next(u);
  }

  /** Decode JWT payload từ Keycloak và lưu user */
  setKeycloakUser(accessToken: string): void {
    try {
      const pad = (s: string) => s + '='.repeat((4 - s.length % 4) % 4);
      const payload = JSON.parse(atob(pad(accessToken.split('.')[1].replace(/-/g, '+').replace(/_/g, '/'))));
      this.setUser({
        id:             payload.sub,
        fullName:       payload.name || payload.preferred_username || '',
        roleCode:       payload.user_role_code || '',
        roleName:       payload.user_role_code || '',
        departmentCode: payload.organize_code  || '',
        departmentName: payload.organize_name  || '',
      });
      localStorage.setItem('auth_type', 'keycloak');
    } catch { console.error('Failed to decode Keycloak token'); }
  }

  isKeycloakAuth(): boolean { return localStorage.getItem('auth_type') === 'keycloak'; }

  clearAuth(): void {
    ['token', 'refresh_token', 'id_token', 'user', 'auth_type'].forEach(k => localStorage.removeItem(k));
    this._user$.next(null);
  }

  isAuthenticated(): boolean { return !!this.getToken(); }
}

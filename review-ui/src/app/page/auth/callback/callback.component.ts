import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-callback',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="w-full h-screen flex items-center justify-center">
      <div class="text-center">
        <div class="animate-spin rounded-full h-10 w-10 border-b-2 border-[#276100] mx-auto mb-4"></div>
        <p class="text-gray-500 text-sm">{{ message }}</p>
      </div>
    </div>
  `
})
export class CallbackComponent implements OnInit {
  message = 'Đang xử lý đăng nhập...';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    const params = this.route.snapshot.queryParams;
    const code   = params['code'];
    const state  = params['state'];
    const error  = params['error'];

    if (error) {
      this.message = `Lỗi: ${params['error_description'] || error}`;
      setTimeout(() => this.router.navigate(['/login']), 2000);
      return;
    }

    const savedState = sessionStorage.getItem('kc_state');
    if (!code || state !== savedState) {
      this.message = 'Xác thực thất bại. Đang chuyển hướng...';
      setTimeout(() => this.router.navigate(['/login']), 2000);
      return;
    }

    sessionStorage.removeItem('kc_state');
    sessionStorage.removeItem('kc_nonce');

    // Bước 1: Đổi code lấy token
    this.authService.exchangeCode(code).subscribe({
      next: (resp) => {
        if (!resp.access_token) {
          this.message = 'Không nhận được token.';
          setTimeout(() => this.router.navigate(['/login']), 2000);
          return;
        }

        this.authService.setToken(resp.access_token);
        if (resp.refresh_token) this.authService.setRefreshToken(resp.refresh_token);
        if (resp.id_token)      this.authService.setIdToken(resp.id_token);
        localStorage.setItem('auth_type', 'keycloak');

        // Bước 2: Lấy user thực từ DB qua /api/auth/me
        this.message = 'Đang tải thông tin người dùng...';
        this.http.get<any>('/api/auth/me').subscribe({
          next: (user) => {
            this.authService.setUser(user);
            this.router.navigate(['/home']);
          },
          error: () => {
            // Fallback: decode token nếu không tìm thấy trong DB
            this.authService.setKeycloakUser(resp.access_token);
            this.router.navigate(['/home']);
          }
        });
      },
      error: () => {
        this.message = 'Đăng nhập thất bại. Đang chuyển hướng...';
        setTimeout(() => this.router.navigate(['/login']), 2000);
      }
    });
  }
}

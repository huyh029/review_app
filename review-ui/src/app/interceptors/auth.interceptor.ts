import { HttpInterceptorFn, HttpRequest, HttpHandlerFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { ToastService } from '../services/toast.service';
import { AuthService } from '../page/auth/services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('token');
  if (token) {
    req = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  }

  const router = inject(Router);
  const toast = inject(ToastService);
  const authService = inject(AuthService);

  return next(req).pipe(
    catchError(err => {
      const msg = err.error?.message || err.message || 'Đã xảy ra lỗi';

      if (err.status === 401) {
        // Thử refresh token Keycloak nếu đang dùng Keycloak
        const refreshToken = authService.getRefreshToken();
        if (refreshToken && authService.isKeycloakAuth()) {
          return authService.keycloakRefresh(refreshToken).pipe(
            switchMap(resp => {
              authService.setToken(resp.access_token);
              if (resp.refresh_token) authService.setRefreshToken(resp.refresh_token);
              // Retry request với token mới
              const retryReq = req.clone({
                setHeaders: { Authorization: `Bearer ${resp.access_token}` }
              });
              return next(retryReq);
            }),
            catchError(() => {
              authService.clearAuth();
              router.navigate(['/login']);
              toast.error('Phiên đăng nhập hết hạn');
              return throwError(() => err);
            })
          );
        }
        authService.clearAuth();
        router.navigate(['/login']);
        toast.error('Phiên đăng nhập hết hạn');
      } else if (err.status === 403) {
        toast.error('Bạn không có quyền thực hiện thao tác này');
      } else if (err.status === 404) {
        toast.error('Không tìm thấy dữ liệu');
      } else if (err.status === 500) {
        toast.error('Lỗi máy chủ: ' + msg);
      } else if (err.status >= 400) {
        toast.error(msg);
      }

      return throwError(() => err);
    })
  );
};

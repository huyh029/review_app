import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { SidebarService } from '../../../services/sidebar.service';
import { PageTitleService } from '../../../services/page-title.service';
import { AuthService, UserDto } from '../../auth/services/auth.service';

@Component({
  selector: 'app-header',
  imports: [CommonModule],
  templateUrl: './header.component.html'
})
export class HeaderComponent implements OnInit, OnDestroy {
  pageTitle = 'Trang chủ';
  userName  = '';
  userRole  = '';
  user: UserDto | null = null;

  private subs = new Subscription();

  constructor(
    private router: Router,
    private sidebarService: SidebarService,
    private pageTitleService: PageTitleService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.subs.add(
      this.pageTitleService.title$.subscribe(title => this.pageTitle = title)
    );

    // Reactive — cập nhật ngay khi user thay đổi
    this.subs.add(
      this.authService.user$.subscribe(user => {
        this.user     = user;
        this.userName = user?.fullName ?? '';
        this.userRole = user ? `${user.departmentName} - ${user.roleName}` : '';
      })
    );
  }

  ngOnDestroy() {
    this.subs.unsubscribe();
  }

  logout() {
    const isKeycloak = this.authService.isKeycloakAuth();
    const idToken    = this.authService.getIdToken();
    this.authService.clearAuth();

    if (isKeycloak) {
      const KC_URL   = 'http://192.168.1.6:8080';
      const KC_REALM = 'vbdh-realm';
      const params = new URLSearchParams({
        client_id:                'vbdh-client',
        post_logout_redirect_uri: `${window.location.origin}/login`,
        ...(idToken ? { id_token_hint: idToken } : {})
      });
      window.location.href = `${KC_URL}/realms/${KC_REALM}/protocol/openid-connect/logout?${params}`;
    } else {
      this.router.navigate(['/login']);
    }
  }

  toggleSidebar() {
    this.sidebarService.toggle();
  }
}

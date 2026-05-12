import { Component, OnInit } from '@angular/core';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  template: `
    <div class="w-full h-screen flex items-center justify-center">
      <div class="animate-spin rounded-full h-10 w-10 border-b-2 border-[#276100]"></div>
    </div>
  `
})
export class LoginComponent implements OnInit {
  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    this.authService.redirectToKeycloak();
  }
}

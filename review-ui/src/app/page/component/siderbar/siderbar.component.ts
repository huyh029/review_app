import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { LucideAngularModule } from 'lucide-angular';
import { SidebarService } from '../../../services/sidebar.service';
import { PageTitleService } from '../../../services/page-title.service';

@Component({
  selector: 'app-siderbar',
  imports: [CommonModule, RouterModule, LucideAngularModule],
  templateUrl: './siderbar.component.html'
})
export class SiderbarComponent implements OnInit {
  configExpanded = false;
  sidebarOpen = true;

  menuItems = [
    { label: 'Trang chủ', route: '/home' },
    { label: 'Bảng đánh giá', route: '/evaluation-board' },
    { label: 'Báo cáo', route: '/reports' }
  ];

  submenuItems = [
    { label: 'Đối tượng đánh giá', route: '/configuration/evaluation-object' },
    { label: 'Luồng đánh giá', route: '/configuration/evaluation-flow' },
    { label: 'Tiêu chí đánh giá', route: '/configuration/evaluation-criteria' },
    { label: 'Loại báo cáo', route: '/configuration/report-type' },
    { label: 'Quản lý Quyền Truy cập', route: '/configuration/api-access' }
  ];

  constructor(
    private sidebarService: SidebarService,
    private pageTitleService: PageTitleService,
    private router: Router
  ) {}

  ngOnInit() {
    this.sidebarService.sidebarOpen$.subscribe((isOpen: boolean) => {
      this.sidebarOpen = isOpen;
    });
  }

  onMenuItemClick(label: string) {
    this.pageTitleService.setTitle(label);
  }

  toggleConfig() {
    this.configExpanded = !this.configExpanded;
  }

  toggleSidebar() {
    this.sidebarService.toggle();
  }
}

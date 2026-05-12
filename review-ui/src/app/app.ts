import { Component, signal } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { SiderbarComponent } from './page/component/siderbar/siderbar.component';
import { HeaderComponent } from './page/component/header/header.component';
import { PaginationFooterComponent } from './page/component/pagination-footer/pagination-footer.component';
import { CommonModule } from '@angular/common';
import { SidebarService } from './services/sidebar.service';
import { PaginationService } from './services/pagination.service';
import { ToastComponent } from './components/toast/toast.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, SiderbarComponent, HeaderComponent, PaginationFooterComponent, CommonModule, ToastComponent],
  providers: [SidebarService, PaginationService],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('review-ui');

  constructor(private router: Router, private paginationService: PaginationService) {}

  get isLoginPage(): boolean {
    return this.router.url === '/login' || this.router.url === '';
  }

  get showPaginationFooter(): boolean {
    const url = this.router.url;
    // Hide pagination for role config and evaluation criteria detail pages
    const isDetailPage = url.includes('/configuration/evaluation-criteria/new') || 
                        (url.includes('/configuration/evaluation-criteria/') && url.includes('/edit')) ||
                        url.includes('/configuration/evaluation-flow/new') ||
                        (url.includes('/configuration/evaluation-flow/') && url.includes('/edit')) ||
                        url.includes('/configuration/report-type/new') ||
                        (url.includes('/configuration/report-type/') && url.includes('/edit'));
    const isEvaluationBoardDetailPage = url.includes('/evaluation-board/') && url.includes('/detail/');
    const isLoginPage = url.includes('/login') || url === '';
    
    // Show pagination if URL matches list pages AND pagination data exists
    const shouldShow = !url.includes('/configuration/evaluation-object/role') && !isDetailPage && !isEvaluationBoardDetailPage && !isLoginPage;
    const hasPaginationData = this.paginationService.getPagination() !== null;
    
    return shouldShow && hasPaginationData;
  }

  onPageChange(page: number) {
    console.log('App: Chuyển đến trang:', page);
    const currentPagination = this.paginationService.getPagination();
    if (currentPagination) {
      this.paginationService.setPagination({
        ...currentPagination,
        currentPage: page
      });
    }
  }

  onItemsPerPageChange(itemsPerPage: number) {
    console.log('App: Thay đổi số dòng:', itemsPerPage);
    this.paginationService.setItemsPerPage(itemsPerPage);
    const currentPagination = this.paginationService.getPagination();
    if (currentPagination) {
      this.paginationService.setPagination({
        ...currentPagination,
        currentPage: 1,
        itemsPerPage: itemsPerPage
      });
    }
  }
}

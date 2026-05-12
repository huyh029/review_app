import { Component, ChangeDetectionStrategy, ChangeDetectorRef, ViewChild, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FilterComponent, FilterField, FilterCriteria } from '../../../component/filter/filter.component';
import { TableComponent, TableColumn, TableResponse, SelectionState } from '../../../component/table/table.component';
import { DownloadButtonComponent } from '../../../component/download-button/download-button.component';
import { ConfirmationDialogComponent } from '../../../component/confirmation-dialog/confirmation-dialog.component';
import { PaginationService } from '../../../../services/pagination.service';
import { EvaluationCriteriaService, CriteriaSetDto, DeleteManyCriteriaSetRequest } from './evaluation-criteria.service';

@Component({
  selector: 'app-evaluation-criteria',
  standalone: true,
  imports: [CommonModule, FilterComponent, TableComponent, DownloadButtonComponent, ConfirmationDialogComponent],
  templateUrl: './evaluation-criteria.component.html',
  styleUrls: ['./evaluation-criteria.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EvaluationCriteriaComponent implements OnInit {
  @ViewChild('table') table!: TableComponent;
  
  filterFields: FilterField[] = [
    {
      key: 'objectType',
      label: 'Đối tương đánh giá',
      type: 'select',
      options: [
        { label: 'Tất cả đối tương', value: '' },
        { label: 'Cán Bộ', value: 'Cán Bộ' },
        { label: 'Lãnh đạo', value: 'Lãnh đạo' }
      ]
    },
    {
      key: 'evaluationType',
      label: 'Kỳ đánh giá',
      type: 'select',
      options: [
        { label: 'Tất cả kỳ', value: '' },
        { label: 'Tháng', value: 'Tháng' },
        { label: 'Quý', value: 'Quý' },
        { label: 'Năm', value: 'Năm' }
      ]
    },
    {
      key: 'evaluationYear',
      label: 'Năm đánh giá',
      type: 'select',
      options: [
        { label: 'Tất cả năm', value: '' },
        { label: '2024', value: '2024' },
        { label: '2025', value: '2025' },
        { label: '2026', value: '2026' }
      ]
    }
  ];

  columns: TableColumn[] = [
    { key: 'stt', label: 'STT', textAlign: 'center' },
    { key: 'name', label: 'TÊN BỘ TIÊU CHÍ', bold: true },
    { key: 'applicableYears', label: 'NĂM ÁP DỤNG', textAlign: 'left' },
    { key: 'applicableMonths', label: 'THÁNG ÁP DỤNG', textAlign: 'left' },
    { 
      key: 'actions', 
      label: 'HOẠT ĐỘNG', 
      textAlign: 'center',
      isIcon: true,
      iconMap: {
        'actions': '<svg class="w-5 h-5 text-blue-600 cursor-pointer hover:text-blue-800" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"></path></svg>|<svg class="w-5 h-5 text-red-600 cursor-pointer hover:text-red-800" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"></path></svg>'
      }
    }
  ];

  tableResponse: TableResponse;
  selectedRows: any[] = [];
  isAllSelected = false;
  includeIds: any[] = [];
  excludeIds: any[] = [];
  showConfirmDialog = false;
  rowToDelete: any = null;
  loading = false;
  currentPage = 1;
  currentPageSize = 10;
  currentSearch = '';
  private isFirstLoad = true;

  constructor(
    private paginationService: PaginationService,
    private cdr: ChangeDetectorRef,
    private router: Router,
    private evaluationCriteriaService: EvaluationCriteriaService
  ) {
    this.tableResponse = { data: [], pagination: { currentPage: 1, totalPages: 1, totalItems: 0, itemsPerPage: 10 } };
  }

  ngOnInit() {
    this.loadData();
    
    // Subscribe to pagination changes from PaginationService (from pagination footer)
    this.paginationService.pagination$.subscribe((pagination: any) => {
      if (!this.isFirstLoad && pagination && pagination.currentPage && 
          (pagination.currentPage !== this.currentPage || pagination.itemsPerPage !== this.currentPageSize)) {
        this.currentPage = pagination.currentPage;
        this.currentPageSize = pagination.itemsPerPage || 10;
        this.loadData();
      }
      this.isFirstLoad = false;
    });
  }

  loadData() {
    this.loading = true;
    this.evaluationCriteriaService.getAll(this.currentSearch, this.currentPage, this.currentPageSize).subscribe({
      next: (response) => {
        const tableData = response.data.map((item, index) => ({
          id: item.id,
          stt: (this.currentPage - 1) * this.currentPageSize + index + 1,
          name: item.name,
          applicableYears: item.applicableYears,
          applicableMonths: item.applicableMonths,
          status: item.isActive === 1 ? 'Kích hoạt' : 'Khóa',
          actions: 'actions'
        }));

        this.tableResponse = {
          data: tableData,
          pagination: {
            currentPage: response.pagination.currentPage,
            totalPages: response.pagination.totalPages,
            totalItems: response.pagination.totalItems,
            itemsPerPage: response.pagination.itemsPerPage
          }
        };

        // Update PaginationService so pagination footer shows up
        if (this.tableResponse.pagination) {
          this.paginationService.setPagination(this.tableResponse.pagination);
          this.paginationService.setItemsPerPage(this.tableResponse.pagination.itemsPerPage);
        }
        
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err: any) => {
        console.error('Lỗi khi tải dữ liệu:', err);
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  onSelectionChange(selected: any[]) {
    this.selectedRows = selected;
    this.cdr.markForCheck();
  }

  onSelectionStateChange(state: SelectionState) {
    this.isAllSelected = state.isAll;
    this.includeIds = state.includeIds;
    this.excludeIds = state.excludeIds;
    console.log('[deleteMul]', {
      isAll: this.isAllSelected,
      includeIds: this.includeIds,
      excludeIds: this.excludeIds,
      deleteCount: this.deleteCount
    });
    this.cdr.markForCheck();
  }

  get deleteCount(): number {
    if (this.isAllSelected) {
      return (this.tableResponse.pagination?.totalItems ?? 0) - this.excludeIds.length;
    }
    return this.includeIds.length;
  }

  onDelete() {
    this.showConfirmDialog = true;
    this.cdr.markForCheck();
  }

  onConfirmDelete() {
    if (this.rowToDelete) {
      this.evaluationCriteriaService.delete(this.rowToDelete.id).subscribe({
        next: () => {
          console.log('Xóa thành công:', this.rowToDelete);
          this.loadData();
          this.showConfirmDialog = false;
          this.rowToDelete = null;
          this.cdr.markForCheck();
        },
        error: (err: any) => {
          console.error('Lỗi khi xóa:', err);
          this.cdr.markForCheck();
        }
      });
    } else if (this.isAllSelected || this.includeIds.length > 0) {
      const request: DeleteManyCriteriaSetRequest = {
        isAll: this.isAllSelected,
        includeIds: this.isAllSelected ? undefined : this.includeIds,
        excludeIds: this.isAllSelected ? this.excludeIds : undefined,
        filter: this.isAllSelected ? { search: this.currentSearch || undefined } : undefined
      };
      this.evaluationCriteriaService.deleteMany(request).subscribe({
        next: () => {
          this.loadData();
          this.showConfirmDialog = false;
          this.selectedRows = [];
          this.isAllSelected = false;
          this.includeIds = [];
          this.excludeIds = [];
          this.table.clearSelection();
          this.cdr.markForCheck();
        },
        error: (err: any) => {
          console.error('Lỗi khi xóa:', err);
          this.cdr.markForCheck();
        }
      });
    }
  }

  onCancelDelete() {
    this.showConfirmDialog = false;
    this.rowToDelete = null;
    this.cdr.markForCheck();
  }

  onRowActionClick(event: { row: any; actionIndex: number }) {
    if (event.actionIndex === 1) {
      this.rowToDelete = event.row;
      this.showConfirmDialog = false;
      this.cdr.markForCheck();
      setTimeout(() => {
        this.showConfirmDialog = true;
        this.cdr.markForCheck();
      }, 0);
    } else if (event.actionIndex === 0) {
      this.router.navigate(['/configuration/evaluation-criteria', event.row.id, 'edit']);
    }
  }

  onRowClick(row: any) {
    this.router.navigate(['/configuration/evaluation-criteria', row.id, 'edit']);
  }

  onFilterChange(criteria: FilterCriteria) {
    console.log('Filter criteria:', criteria);
    this.currentPage = 1;
    this.currentSearch = criteria['search'] || '';
    this.paginationService.setPagination({
      currentPage: 1,
      totalPages: 1,
      totalItems: 0,
      itemsPerPage: this.currentPageSize
    });
    this.loadData();
  }

  getDeleteMessage(): string {
    if (this.rowToDelete) {
      const months = this.rowToDelete.applicableMonths ? ` - Tháng: ${this.rowToDelete.applicableMonths}` : '';
      const years = this.rowToDelete.applicableYears ? ` - Năm: ${this.rowToDelete.applicableYears}` : '';
      return `Bạn có chắc chắn muốn xóa bộ tiêu chí "${this.rowToDelete.name}"${months}${years} không?`;
    }
    return `Bạn có chắc chắn muốn xóa ${this.deleteCount} bộ tiêu chí đã chọn không?`;
  }

  onCreateNew() {
    this.router.navigate(['/configuration/evaluation-criteria/new']);
  }
}

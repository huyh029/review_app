import { Component, ChangeDetectionStrategy, ChangeDetectorRef, ViewChild, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FilterComponent, FilterField, FilterCriteria } from '../../../component/filter/filter.component';
import { TableComponent, TableColumn, TableResponse } from '../../../component/table/table.component';
import { DownloadButtonComponent } from '../../../component/download-button/download-button.component';
import { ConfirmationDialogComponent } from '../../../component/confirmation-dialog/confirmation-dialog.component';
import { PaginationService } from '../../../../services/pagination.service';
import { EvaluationBoardService, EvaluationFilter, DeleteManyRequest } from '../evaluation-board.service';
import { Subscription } from 'rxjs';

interface SelectOption {
  label: string;
  value: string;
}

@Component({
  selector: 'app-self-evaluation',
  imports: [CommonModule, FilterComponent, TableComponent, DownloadButtonComponent, ConfirmationDialogComponent],
  templateUrl: './self-evaluation.component.html',
  styleUrls: ['./self-evaluation.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SelfEvaluationComponent implements OnInit, OnDestroy {
  @ViewChild('table') table!: TableComponent;

  filterFields: FilterField[] = [
    {
      key: 'status',
      label: 'Trạng thái',
      type: 'select',
      options: [
        { label: 'Tất cả trạng thái', value: '' },
        { label: 'Dự thảo', value: 'draft' },
        { label: 'Chờ đánh giá', value: 'pending' },
        { label: 'Chờ đánh giá của thủ trưởng', value: 'pending_director' },
        { label: 'Hoàn thành', value: 'completed' }
      ]
    },
    {
      key: 'month',
      label: 'Kỳ đánh giá',
      type: 'select',
      options: [
        { label: 'Tất cả kỳ', value: '' },
        ...Array.from({ length: 12 }, (_, i) => ({ label: `Tháng ${i + 1}`, value: `${i + 1}` }))
      ]
    },
    {
      key: 'year',
      label: 'Năm đánh giá',
      type: 'select',
      options: this.generateYearOptions()
    }
  ];

  columns: TableColumn[] = [
    { key: 'fullName', label: 'HỌ TÊN', bold: true },
    { key: 'evaluationPeriod', label: 'KỲ ĐÁNH GIÁ' },
    { key: 'department', label: 'ĐƠN VỊ' },
    { key: 'selfScore', label: 'ĐIỂM CÁ NHÂN', textAlign: 'center' },
    { key: 'managerScore', label: 'ĐIỂM ĐƠN VỊ', textAlign: 'center' },
    { key: 'status', label: 'TRẠNG THÁI', wrapper: 'div', textAlign: 'center' },
    {
      key: 'actions',
      label: 'HOẠT ĐỘNG',
      textAlign: 'center',
      isIcon: true,
      iconMap: {
        'edit': '<svg class="w-5 h-5 text-blue-600 hover:text-blue-800" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"></path></svg>|<svg class="w-5 h-5 text-red-600 hover:text-red-800" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"></path></svg>',
        'view': '<svg class="w-5 h-5 text-blue-600 hover:text-blue-800" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"></path><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"></path></svg>'
      }
    }
  ];

  tableResponse: TableResponse = { data: [], pagination: undefined };
  enableMultiSelect = true;
  selectedRows: any[] = [];
  isAllSelected = false;
  includeIds: any[] = [];
  excludeIds: any[] = [];
  showConfirmDialog = false;
  rowToDelete: any = null;
  currentFilter: EvaluationFilter = { page: 1, pageSize: 10 };
  private paginationSub!: Subscription;

  constructor(
    private paginationService: PaginationService,
    private cdr: ChangeDetectorRef,
    private router: Router,
    private evaluationService: EvaluationBoardService
  ) {}

  ngOnInit() {
    this.loadData();
    this.paginationSub = this.paginationService.pagination$.subscribe(pagination => {
      if (!pagination) return;
      const newPage = pagination.currentPage;
      const newPageSize = pagination.itemsPerPage;
      if (newPage !== this.currentFilter.page || newPageSize !== this.currentFilter.pageSize) {
        this.currentFilter = { ...this.currentFilter, page: newPage, pageSize: newPageSize };
        this.loadData();
      }
    });
  }

  ngOnDestroy() {
    this.paginationSub?.unsubscribe();
  }

  private statusLabel(status: string): string {
    const map: { [k: string]: string } = {
      draft: 'Dự thảo',
      pending: 'Chờ đánh giá',
      pending_director: 'Chờ đánh giá của thủ trưởng',
      completed: 'Hoàn thành'
    };
    return map[status] ?? status;
  }

  loadData() {
    this.evaluationService.getSelfList(this.currentFilter).subscribe({
      next: (res) => {
        this.tableResponse = {
          data: res.data.map(e => ({
            ...e,
            selfScore: e.selfScore != null ? e.selfScore : '',
            managerScore: e.managerScore != null ? e.managerScore : '',
            rawStatus: e.status,
            status: this.statusLabel(e.status),
            actions: e.status === 'draft' ? 'edit' : 'view'
          })),
          pagination: res.pagination
        };
        this.paginationService.setPagination(res.pagination);
        this.paginationService.setItemsPerPage(res.pagination.itemsPerPage);
        this.cdr.markForCheck();
      },
      error: (err) => console.error('Lỗi tải danh sách:', err)
    });
  }

  onFilterChange(criteria: FilterCriteria) {
    this.currentFilter = {
      status: criteria['status'] || undefined,
      month: criteria['month'] ? parseInt(criteria['month']) : undefined,
      year: criteria['year'] ? parseInt(criteria['year']) : undefined,
      page: 1,
      pageSize: this.currentFilter.pageSize
    };
    this.loadData();
  }

  onIsAllChange(isAll: boolean) {
    this.isAllSelected = isAll;
    this.includeIds = [];
    this.excludeIds = [];
    this.logDeleteMul();
    this.cdr.markForCheck();
  }

  onIncludeIdsChange(ids: any[]) {
    this.includeIds = ids;
    this.logDeleteMul();
    this.cdr.markForCheck();
  }

  onExcludeIdsChange(ids: any[]) {
    this.excludeIds = ids;
    this.logDeleteMul();
    this.cdr.markForCheck();
  }

  private logDeleteMul() {
    console.log('[deleteMul]', {
      isAll: this.isAllSelected,
      includeIds: this.includeIds,
      excludeIds: this.excludeIds,
      deleteCount: this.deleteCount
    });
  }

  get deleteCount(): number {
    if (this.isAllSelected) {
      return (this.tableResponse.pagination?.selectableCount ?? 0) - this.excludeIds.length;
    }
    return this.includeIds.length;
  }
  onDelete() {
    if (this.isAllSelected || this.includeIds.length > 0) {
      this.rowToDelete = null; // đảm bảo không dùng single delete message
      this.showConfirmDialog = true;
      this.cdr.markForCheck();
    }
  }

  onConfirmDelete() {
    if (this.rowToDelete) {
      this.evaluationService.deleteSelf(this.rowToDelete.id).subscribe({
        next: () => {
          this.showConfirmDialog = false;
          this.rowToDelete = null;
          this.loadData();
          this.cdr.markForCheck();
        },
        error: (err) => console.error('Lỗi xóa:', err)
      });
    } else if (this.isAllSelected || this.includeIds.length > 0) {
      const request: DeleteManyRequest = {
        isAll: this.isAllSelected,
        ids: this.isAllSelected ? undefined : this.includeIds,
        excludeIds: this.isAllSelected ? this.excludeIds : undefined,
        filter: this.isAllSelected ? { ...this.currentFilter, status: 'draft' } : undefined
      };
      this.evaluationService.deleteManySelf(request).subscribe({
        next: () => {
          this.showConfirmDialog = false;
          this.selectedRows = [];
          this.includeIds = [];
          this.excludeIds = [];
          this.isAllSelected = false;
          this.table.clearSelection();
          this.loadData();
          this.cdr.markForCheck();
        },
        error: (err) => console.error('Lỗi xóa:', err)
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
      this.router.navigate(['/evaluation-board/self', 'detail', event.row.id]);
    }
  }

  selectionFilter = (row: any) => row.rawStatus === 'draft';

  private generateYearOptions(): SelectOption[] {
    const currentYear = new Date().getFullYear();
    const options: SelectOption[] = [{ label: 'Tất cả năm', value: '' }];
    for (let year = currentYear; year >= 1970; year--) {
      options.push({ label: year.toString(), value: year.toString() });
    }
    return options;
  }

  getDeleteMessage(): string {
    if (this.rowToDelete) {
      return `Bạn có chắc chắn muốn xóa phiếu đánh giá của "${this.rowToDelete.fullName}" - ${this.rowToDelete.evaluationPeriod} (${this.rowToDelete.department}) không?`;
    }
    const count = this.deleteCount;
    return `Bạn có chắc chắn muốn xóa ${count} phiếu đánh giá dự thảo đã chọn không? Hành động này không thể hoàn tác.`;
  }

  onAddNew() {
    this.router.navigate(['/evaluation-board/self', 'detail', 'new']);
  }
}

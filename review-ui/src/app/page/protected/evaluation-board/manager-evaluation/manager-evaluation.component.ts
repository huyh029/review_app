import { Component, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FilterComponent, FilterField, FilterCriteria } from '../../../component/filter/filter.component';
import { SelectComponent, SelectOption } from '../../../component/select/select.component';
import { TableComponent, TableColumn, TableResponse } from '../../../component/table/table.component';
import { DownloadButtonComponent } from '../../../component/download-button/download-button.component';
import { PaginationService } from '../../../../services/pagination.service';
import { EvaluationBoardService, EvaluationFilter } from '../evaluation-board.service';

@Component({
  selector: 'app-manager-evaluation',
  imports: [CommonModule, FilterComponent, TableComponent, DownloadButtonComponent],
  templateUrl: './manager-evaluation.component.html',
  styleUrls: ['./manager-evaluation.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ManagerEvaluationComponent {
  filterFields: FilterField[] = [
    {
      key: 'status',
      label: 'Trạng thái',
      type: 'select',
      options: [
        { label: 'Tất cả trạng thái', value: '' },
        { label: 'Chờ đánh giá', value: 'pending' },
        { label: 'Chờ đánh giá của thủ trưởng', value: 'pending_director' }
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
        'edit': '<svg class="w-5 h-5 text-blue-600 hover:text-blue-800" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"></path></svg>'
      }
    }
  ];

  tableResponse: TableResponse = { data: [], pagination: undefined };
  enableMultiSelect = false;
  selectedRows: any[] = [];
  currentFilter: EvaluationFilter = { page: 1, pageSize: 10 };

  constructor(
    private paginationService: PaginationService,
    private cdr: ChangeDetectorRef,
    private router: Router,
    private evaluationService: EvaluationBoardService
  ) {
    this.loadData();
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
    this.evaluationService.getManagerList(this.currentFilter).subscribe({
      next: (res) => {
        this.tableResponse = {
          data: res.data.map(e => ({ ...e, status: this.statusLabel(e.status), actions: 'edit' })),
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

  onSelectionChange(selectedRows: any[]) {
    this.selectedRows = selectedRows;
  }

  selectionFilter = (row: any) => row.status === 'pending';

  private generateYearOptions(): SelectOption[] {
    const currentYear = new Date().getFullYear();
    const options: SelectOption[] = [{ label: 'Tất cả năm', value: '' }];
    for (let year = currentYear; year >= 1970; year--) {
      options.push({ label: year.toString(), value: year.toString() });
    }
    return options;
  }

  onRowActionClick(event: { row: any; actionIndex: number }) {
    this.router.navigate(['/evaluation-board/manager', 'detail', event.row.id]);
  }
}

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
  selector: 'app-result-evaluation',
  imports: [CommonModule, FilterComponent, TableComponent, DownloadButtonComponent],
  templateUrl: './result-evaluation.component.html',
  styleUrls: ['./result-evaluation.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ResultEvaluationComponent {
  filterFields: FilterField[] = [
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
        'view': '<svg class="w-5 h-5 text-blue-600 hover:text-blue-800" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"></path><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"></path></svg>'
      }
    }
  ];

  tableResponse: TableResponse = { data: [], pagination: undefined };
  enableMultiSelect = false;
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
    this.evaluationService.getResultList(this.currentFilter).subscribe({
      next: (res) => {
        this.tableResponse = {
          data: res.data.map(e => ({ ...e, status: this.statusLabel(e.status), actions: 'view' })),
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
      month: criteria['month'] ? parseInt(criteria['month']) : undefined,
      year: criteria['year'] ? parseInt(criteria['year']) : undefined,
      page: 1,
      pageSize: this.currentFilter.pageSize
    };
    this.loadData();
  }

  private generateYearOptions(): SelectOption[] {
    const currentYear = new Date().getFullYear();
    const options: SelectOption[] = [{ label: 'Tất cả năm', value: '' }];
    for (let year = currentYear; year >= 1970; year--) {
      options.push({ label: year.toString(), value: year.toString() });
    }
    return options;
  }

  onRowActionClick(event: { row: any; actionIndex: number }) {
    this.router.navigate(['/evaluation-board/result', 'detail', event.row.id]);
  }
}

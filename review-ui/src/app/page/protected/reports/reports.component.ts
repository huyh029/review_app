import { Component, ChangeDetectionStrategy, ChangeDetectorRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FilterComponent, FilterField, FilterCriteria } from '../../component/filter/filter.component';
import { SelectOption } from '../../component/select/select.component';
import { TableComponent, TableColumn, TableResponse } from '../../component/table/table.component';
import { DownloadButtonComponent } from '../../component/download-button/download-button.component';
import { ReportsService, ReportTypeOptionDto } from './reports.service';
import { PaginationService } from '../../../services/pagination.service';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, FilterComponent, TableComponent, DownloadButtonComponent],
  templateUrl: './reports.component.html',
  styleUrls: ['./reports.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ReportsComponent implements OnInit {
  currentPage = 1;
  currentPageSize = 10;
  currentReportType = '';
  currentMonth: number | undefined;
  currentYear: number | undefined;
  private isFirstLoad = true;

  filterFields: FilterField[] = [];

  tableColumns: TableColumn[] = [
    { key: 'stt', label: 'STT', textAlign: 'center' },
    { key: 'canBo', label: 'CÁN BỘ', textAlign: 'left' },
    { key: 'donVi', label: 'ĐƠN VỊ', textAlign: 'left' },
    {
      key: 'danhGia',
      label: 'ĐÁNH GIÁ, CHẤM ĐIỂM THEO TIÊU CHÍ',
      children: [
        { key: 'selfScore', label: 'Cá nhân tự chấm', textAlign: 'center' },
        { key: 'managerScore', label: 'Cơ quan tổ chức đơn vị đánh giá', textAlign: 'center' }
      ]
    },
    {
      key: 'ketQua',
      label: 'KẾT QUẢ PHÂN LOẠI CÁN BỘ',
      children: [
        { key: 'selfClassification', label: 'Cá nhân tự phân loại', textAlign: 'center' },
        { key: 'managerClassification', label: 'Kết quả phân loại tại đơn vị', textAlign: 'center' }
      ]
    }
  ];

  tableResponse: TableResponse = {
    data: [],
    pagination: { currentPage: 1, totalPages: 1, totalItems: 0, itemsPerPage: 10 }
  };

  constructor(
    private reportsService: ReportsService,
    private paginationService: PaginationService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.reportsService.getReportTypeOptions().subscribe({
      next: (options) => {
        this.buildFilterFields(options);
        this.cdr.markForCheck();
      },
      error: () => this.buildFilterFields([])
    });

    this.loadData();

    this.paginationService.pagination$.subscribe((pagination: any) => {
      if (!this.isFirstLoad && pagination &&
          (pagination.currentPage !== this.currentPage || pagination.itemsPerPage !== this.currentPageSize)) {
        this.currentPage = pagination.currentPage;
        this.currentPageSize = pagination.itemsPerPage || 10;
        this.loadData();
      }
      this.isFirstLoad = false;
    });
  }

  private buildFilterFields(reportTypes: ReportTypeOptionDto[]): void {
    const reportTypeOptions: SelectOption[] = [
      { label: 'Tất cả loại', value: '' },
      ...reportTypes.map(r => ({ label: r.name, value: r.code }))
    ];

    this.filterFields = [
      { key: 'reportType', label: 'Loại báo cáo', type: 'select', options: reportTypeOptions },
      {
        key: 'evaluationPeriod',
        label: 'Kỳ đánh giá (tháng)',
        type: 'select',
        options: [
          { label: 'Tất cả tháng', value: '' },
          ...Array.from({ length: 12 }, (_, i) => ({ label: `Tháng ${i + 1}`, value: String(i + 1) }))
        ]
      },
      { key: 'evaluationYear', label: 'Năm', type: 'select', options: this.generateYearOptions() }
    ];
  }

  loadData(): void {
    this.reportsService.getReport(
      this.currentReportType || undefined,
      this.currentMonth,
      this.currentYear,
      this.currentPage,
      this.currentPageSize
    ).subscribe({
      next: (response) => {
        this.tableResponse = {
          data: response.data,
          pagination: {
            currentPage: response.pagination.currentPage,
            totalPages: response.pagination.totalPages,
            totalItems: response.pagination.totalItems,
            itemsPerPage: response.pagination.itemsPerPage
          }
        };
        this.paginationService.setPagination(this.tableResponse.pagination!);
        this.paginationService.setItemsPerPage(this.tableResponse.pagination!.itemsPerPage);
        this.cdr.markForCheck();
      },
      error: (err) => console.error('Error loading report:', err)
    });
  }

  onFilterChange(criteria: FilterCriteria): void {
    this.currentPage = 1;
    this.currentReportType = criteria['reportType'] || '';
    this.currentMonth = criteria['evaluationPeriod'] ? parseInt(criteria['evaluationPeriod'], 10) : undefined;
    this.currentYear = criteria['evaluationYear'] ? parseInt(criteria['evaluationYear'], 10) : undefined;
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
}

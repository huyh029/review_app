import { Component, ChangeDetectionStrategy, ChangeDetectorRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { DropdownCheckboxMultiComponent } from '../../../../component/dropdown-checkbox-multi/dropdown-checkbox-multi.component';
import { ReportTypeService, CreateReportTypeRequest, UpdateReportTypeRequest } from '../report-type.service';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-report-type-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, DropdownCheckboxMultiComponent],
  templateUrl: './report-type-detail.component.html',
  styleUrl: './report-type-detail.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ReportTypeDetailComponent implements OnInit {
  isEditMode = false;
  pageTitle = 'Thêm mới loại báo cáo';
  currentCode: string | null = null;

  code = '';
  name = '';
  status = 'active';
  selectedYears: (string | number)[] = [];
  selectedMonths: (string | number)[] = [];
  selectedCriteria: (string | number)[] = [];

  criteriaOptions: { id: number; name: string }[] = [];

  yearOptions = this.generateYearOptions();

  monthOptions = [
    { id: 'all', name: 'Chọn tất cả' },
    ...Array.from({ length: 12 }, (_, i) => ({ id: String(i + 1), name: `Tháng ${i + 1}` }))
  ];

  constructor(
    private cdr: ChangeDetectorRef,
    private route: ActivatedRoute,
    private router: Router,
    private reportTypeService: ReportTypeService,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    this.loadCriteriaOptions();

    this.route.params.subscribe(params => {
      if (params['id']) {
        this.isEditMode = true;
        this.currentCode = params['id'];
        this.pageTitle = 'Chỉnh sửa loại báo cáo';
        this.loadData(params['id']);
      } else {
        this.isEditMode = false;
        this.pageTitle = 'Thêm mới loại báo cáo';
      }
      this.cdr.markForCheck();
    });
  }

  private loadCriteriaOptions(): void {
    this.http.get<any>('/api/page/configuration/evaluation-criteria?page=1&pageSize=1000').subscribe({
      next: (res) => {
        this.criteriaOptions = (res.data || []).map((c: any) => ({ id: c.id, name: c.name }));
        this.cdr.markForCheck();
      },
      error: (err) => console.error('Error loading criteria:', err)
    });
  }

  private loadData(code: string): void {
    this.reportTypeService.getByCode(code).subscribe({
      next: (data) => {
        this.code = data.code;
        this.name = data.name;
        this.status = data.isActive === 1 ? 'active' : 'inactive';
        this.selectedYears = data.applicableYears ? JSON.parse(data.applicableYears).map(String) : [];
        this.selectedMonths = data.applicableMonths ? JSON.parse(data.applicableMonths).map(String) : [];
        this.selectedCriteria = data.criteria ? JSON.parse(data.criteria).map(Number) : [];
        this.cdr.markForCheck();
      },
      error: (err) => console.error('Error loading report type:', err)
    });
  }

  onYearChange(selected: (string | number)[]): void {
    this.selectedYears = selected;
  }

  onMonthChange(selected: (string | number)[]): void {
    this.selectedMonths = selected.filter(v => v !== 'all');
  }

  onCriteriaChange(selected: (string | number)[]): void {
    this.selectedCriteria = selected;
  }

  onSave(): void {
    const applicableYears = JSON.stringify(this.selectedYears);
    const applicableMonths = JSON.stringify(this.selectedMonths);
    const criteria = JSON.stringify(this.selectedCriteria);

    if (this.isEditMode && this.currentCode) {
      const request: UpdateReportTypeRequest = {
        name: this.name,
        applicableYears,
        applicableMonths,
        criteria,
        isActive: this.status === 'active' ? 1 : 0
      };
      this.reportTypeService.update(this.currentCode, request).subscribe({
        next: () => this.router.navigate(['/configuration/report-type']),
        error: (err) => console.error('Error updating:', err)
      });
    } else {
      const request: CreateReportTypeRequest = {
        code: this.code,
        name: this.name,
        applicableYears,
        applicableMonths,
        criteria
      };
      this.reportTypeService.create(request).subscribe({
        next: () => this.router.navigate(['/configuration/report-type']),
        error: (err) => console.error('Error creating:', err)
      });
    }
  }

  onDelete(): void {
    if (this.currentCode) {
      this.reportTypeService.delete(this.currentCode).subscribe({
        next: () => this.router.navigate(['/configuration/report-type']),
        error: (err) => console.error('Error deleting:', err)
      });
    }
  }

  onCancel(): void {
    this.router.navigate(['/configuration/report-type']);
  }

  private generateYearOptions() {
    const currentYear = new Date().getFullYear();
    const years = [];
    for (let year = currentYear; year >= 1970; year--) {
      years.push({ id: year.toString(), name: year.toString() });
    }
    return years;
  }
}

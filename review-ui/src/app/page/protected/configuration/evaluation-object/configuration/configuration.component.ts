import { Component, ChangeDetectionStrategy, ChangeDetectorRef, ViewChild, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { FilterComponent, FilterField, FilterCriteria } from '../../../../component/filter/filter.component';
import { TableComponent, TableColumn, TableResponse, SelectionState } from '../../../../component/table/table.component';
import { ConfirmationDialogComponent } from '../../../../component/confirmation-dialog/confirmation-dialog.component';
import { PaginationService } from '../../../../../services/pagination.service';
import { EvaluationObjectService, DeleteManyEvaluationObjectRequest } from './evaluation-object.service';

@Component({
  selector: 'app-configuration',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, FilterComponent, TableComponent, ConfirmationDialogComponent],
  templateUrl: './configuration.component.html',
  styleUrls: ['./configuration.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ConfigurationComponent implements OnInit {
  @ViewChild('table') table!: TableComponent;
  
  filterFields: FilterField[] = [];
  selectedRows: any[] = [];
  isAllSelected = false;
  includeIds: any[] = [];
  excludeIds: any[] = [];
  showConfirmDialog = false;
  rowToDelete: any = null;
  showFormDialog = false;
  formGroup!: FormGroup;
  isEditMode = false;
  editingRow: any = null;
  loading = false;
  currentPage = 1;
  currentPageSize = 10;
  currentSearch = '';

  columns: TableColumn[] = [
    { key: 'stt', label: 'STT', textAlign: 'center' },
    { key: 'code', label: 'MÃ ĐỐI TƯỢNG', textAlign: 'left', color: '#2563eb' },
    { key: 'name', label: 'TÊN', bold: true },
    { key: 'status', label: 'TRẠNG THÁI', wrapper: 'div', textAlign: 'center' },
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
  private isFirstLoad = true;

  constructor(
    private paginationService: PaginationService, 
    private cdr: ChangeDetectorRef, 
    private fb: FormBuilder,
    private evaluationObjectService: EvaluationObjectService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.tableResponse = { data: [], pagination: { currentPage: 1, totalPages: 1, totalItems: 0, itemsPerPage: 10 } };
    this.initForm();
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

  initForm() {
    this.formGroup = this.fb.group({
      code: ['', [Validators.required]],
      name: ['', [Validators.required]],
      status: ['Kích hoạt', [Validators.required]]
    });
  }

  loadData() {
    this.loading = true;
    this.evaluationObjectService.getAll(this.currentSearch, this.currentPage, this.currentPageSize).subscribe({
      next: (response) => {
        const tableData = response.data.map((item, index) => ({
          stt: (this.currentPage - 1) * this.currentPageSize + index + 1,
          code: item.code,
          name: item.name,
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

  onIsAllChange(isAll: boolean) {
    this.isAllSelected = isAll;
    this.includeIds = [];
    this.excludeIds = [];
    this.cdr.markForCheck();
  }

  onIncludeIdsChange(ids: any[]) {
    this.includeIds = ids;
    this.cdr.markForCheck();
  }

  onExcludeIdsChange(ids: any[]) {
    this.excludeIds = ids;
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
      this.evaluationObjectService.delete(this.rowToDelete.code).subscribe({
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
      const request: DeleteManyEvaluationObjectRequest = {
        isAll: this.isAllSelected,
        includeIds: this.isAllSelected ? undefined : this.includeIds,
        excludeIds: this.isAllSelected ? this.excludeIds : undefined,
        filter: this.isAllSelected ? { search: this.currentSearch || undefined } : undefined
      };
      this.evaluationObjectService.deleteMany(request).subscribe({
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
    // actionIndex 0 = edit, 1 = delete
    if (event.actionIndex === 1) {
      // Delete action - show dialog without updating selectedRows
      this.rowToDelete = event.row;
      this.showConfirmDialog = false;
      this.cdr.markForCheck();
      setTimeout(() => {
        this.showConfirmDialog = true;
        this.cdr.markForCheck();
      }, 0);
    } else if (event.actionIndex === 0) {
      // Edit action
      this.onEditRow(event.row);
    }
  }

  onTableRowClick(row: any) {
    // When user clicks on a row, navigate to role tab with code
    console.log('Row clicked:', row.code);
    this.router.navigate(['../role', row.code], { relativeTo: this.route });
  }


  onFilterChange(criteria: FilterCriteria) {
    console.log('Filter criteria:', criteria);
    // Reset to first page when searching
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

  onPaginationChange(event: any) {
    // Called when pagination changes
    this.currentPage = event.page || 1;
    this.currentPageSize = event.pageSize || 10;
    this.loadData();
  }

  getDeleteMessage(): string {
    if (this.rowToDelete) {
      return `Bạn có chắc chắn muốn xóa đối tượng "${this.rowToDelete.name}" (${this.rowToDelete.code}) không?`;
    }
    return `Bạn có chắc chắn muốn xóa ${this.deleteCount} đối tượng đánh giá đã chọn không?`;
  }

  onAddNew() {
    this.isEditMode = false;
    this.editingRow = null;
    this.formGroup.reset({ status: 'Kích hoạt' });
    this.showFormDialog = true;
    this.cdr.markForCheck();
  }

  onEditRow(row: any) {
    this.isEditMode = true;
    this.editingRow = row;
    this.formGroup.patchValue({
      code: row.code,
      name: row.name,
      status: row.status
    });
    this.showFormDialog = true;
    this.cdr.markForCheck();
  }

  onCloseForm() {
    this.showFormDialog = false;
    this.isEditMode = false;
    this.editingRow = null;
    this.cdr.markForCheck();
  }

  onSubmitForm() {
    if (this.formGroup.valid) {
      const formValue = this.formGroup.value;
      const isActive = formValue.status === 'Kích hoạt' ? 1 : 0;

      if (this.isEditMode) {
        this.evaluationObjectService.update(this.editingRow.code, {
          name: formValue.name,
          isActive: isActive
        }).subscribe({
          next: () => {
            console.log('Cập nhật đối tượng thành công:', formValue);
            this.loadData();
            this.showFormDialog = false;
            this.isEditMode = false;
            this.editingRow = null;
            this.formGroup.reset({ status: 'Kích hoạt' });
            this.cdr.markForCheck();
          },
          error: (err: any) => {
            console.error('Lỗi khi cập nhật:', err);
            this.cdr.markForCheck();
          }
        });
      } else {
        this.evaluationObjectService.create({
          code: formValue.code,
          name: formValue.name
        }).subscribe({
          next: () => {
            console.log('Thêm mới đối tượng thành công:', formValue);
            this.loadData();
            this.showFormDialog = false;
            this.isEditMode = false;
            this.editingRow = null;
            this.formGroup.reset({ status: 'Kích hoạt' });
            this.cdr.markForCheck();
          },
          error: (err: any) => {
            console.error('Lỗi khi thêm mới:', err);
            this.cdr.markForCheck();
          }
        });
      }
    }
  }

  onDeleteEdit() {
    if (this.editingRow) {
      this.rowToDelete = this.editingRow;
      this.showFormDialog = false;
      this.cdr.markForCheck();
      setTimeout(() => {
        this.showConfirmDialog = true;
        this.cdr.markForCheck();
      }, 0);
    }
  }
}

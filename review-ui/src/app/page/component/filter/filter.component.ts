import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SelectComponent, SelectOption } from '../select/select.component';

export interface FilterCriteria {
  [key: string]: any;
}

export interface FilterField {
  key: string;
  label: string;
  type: 'text' | 'select';
  placeholder?: string;
  options?: SelectOption[];
}

@Component({
  selector: 'app-filter',
  imports: [CommonModule, FormsModule, SelectComponent],
  templateUrl: './filter.component.html'
})
export class FilterComponent {
  @Input() fields: FilterField[] = [];
  @Input() searchPlaceholder: string = 'Nhập từ khóa tìm kiếm...';
  @Input() gap: string = 'gap-4';
  @Input() showLabel: boolean = true;
  @Input() searchMode: 'realtime' | 'click' = 'realtime'; // 'realtime' hoặc 'click'
  @Output() filterChange = new EventEmitter<FilterCriteria>();

  filterCriteria: FilterCriteria = {};

  // Default options nếu không được truyền từ parent
  departmentOptions: SelectOption[] = [
    { label: 'Tất cả đơn vị', value: '' },
    { label: 'Phòng 1', value: 'Phòng 1' },
    { label: 'Phòng 2', value: 'Phòng 2' },
    { label: 'Phòng 3', value: 'Phòng 3' },
    { label: 'Phòng 4', value: 'Phòng 4' },
    { label: 'Phòng 5', value: 'Phòng 5' }
  ];

  statusOptions: SelectOption[] = [
    { label: 'Tất cả trạng thái', value: '' },
    { label: 'Dự thảo', value: 'Dự thảo' },
    { label: 'Chờ đánh giá', value: 'Chờ đánh giá' },
    { label: 'Chờ đánh giá của thủ trưởng', value: 'Chờ đánh giá của thủ trưởng' },
    { label: 'Hoàn thành', value: 'Hoàn thành' }
  ];

  evaluationPeriodOptions: SelectOption[] = [
    { label: 'Tất cả kỳ', value: '' },
    { label: 'Tháng 1', value: 'Tháng 1' },
    { label: 'Tháng 2', value: 'Tháng 2' },
    { label: 'Tháng 3', value: 'Tháng 3' },
    { label: 'Tháng 4', value: 'Tháng 4' },
    { label: 'Tháng 5', value: 'Tháng 5' },
    { label: 'Tháng 6', value: 'Tháng 6' },
    { label: 'Tháng 7', value: 'Tháng 7' },
    { label: 'Tháng 8', value: 'Tháng 8' },
    { label: 'Tháng 9', value: 'Tháng 9' },
    { label: 'Tháng 10', value: 'Tháng 10' },
    { label: 'Tháng 11', value: 'Tháng 11' },
    { label: 'Tháng 12', value: 'Tháng 12' }
  ];

  evaluationYearOptions: SelectOption[] = this.generateYearOptions();

  onFilterChange() {
    this.filterChange.emit(this.filterCriteria);
  }

  resetFilter() {
    this.filterCriteria = {};
    this.filterChange.emit(this.filterCriteria);
  }

  getOptions(field: FilterField): SelectOption[] {
    if (field.options) {
      return field.options;
    }
    // Fallback to default options based on key
    switch (field.key) {
      case 'department':
        return this.departmentOptions;
      case 'status':
        return this.statusOptions;
      case 'evaluationPeriod':
        return this.evaluationPeriodOptions;
      case 'evaluationYear':
        return this.evaluationYearOptions;
      default:
        return [];
    }
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

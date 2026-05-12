import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { SelectComponent, SelectOption } from '../select/select.component';

export interface TableColumn {
  key: string;
  label: string;
  children?: TableColumn[];
  // Cell styling properties
  bold?: boolean;
  color?: string;
  textAlign?: 'left' | 'center' | 'right';
  wrapper?: 'div' | 'span';
  // Icon properties
  isIcon?: boolean;
  iconMap?: { [key: string]: string }; // Map value to icon SVG
}

export interface SelectionState {
  isAll: boolean;
  includeIds: any[];
  excludeIds: any[];
}

export type SelectionFilter = (row: any) => boolean;

export interface TableResponse {
  data: any[];
  pagination?: {
    currentPage: number;
    totalPages: number;
    totalItems: number;
    itemsPerPage: number;
    selectableCount?: number;
  };
}

@Component({
  selector: 'app-table',
  imports: [CommonModule, FormsModule, SelectComponent],
  templateUrl: './table.component.html',
  styleUrls: ['./table.component.css']
})
export class TableComponent {
  @Input() set response(value: TableResponse | any[]) {
    if (Array.isArray(value)) {
      this._data = value;
    } else if (value && value.data) {
      this._data = value.data;
    }
  }

  // Internal state — updated from inputs AND from user interactions
  private _isAll: boolean = false;
  private _includeIds: any[] = [];
  private _excludeIds: any[] = [];

  @Input() set isAll(v: boolean) { this._isAll = v; }
  @Input() set includeIds(v: any[]) { this._includeIds = v; }
  @Input() set excludeIds(v: any[]) { this._excludeIds = v; }

  @Input() totalSelectedCount: number = 0;
  @Input() columns: TableColumn[] = [];
  @Input() enableMultiSelect: boolean = false;
  @Input() rowIdKey: string = 'id'; // configurable row identifier
  @Input() selectionFilter: SelectionFilter = () => true;
  @Output() includeIdsChange = new EventEmitter<any[]>();
  @Output() excludeIdsChange = new EventEmitter<any[]>();
  @Output() isAllChange = new EventEmitter<boolean>();
  @Output() selectionStateChange = new EventEmitter<SelectionState>();
  @Output() selectionChange = new EventEmitter<any[]>(); // backward compat
  @Output() rowActionClick = new EventEmitter<{ row: any; actionIndex: number }>();
  @Output() rowClick = new EventEmitter<any>();

  private _data: any[] = [];
  Math = Math;

  // Kept for compatibility
  selectedRows = new Set<number>();

  constructor(private sanitizer: DomSanitizer) {}

  get data(): any[] {
    return this._data;
  }

  private getRowId(row: any): any {
    return row[this.rowIdKey];
  }

  isRowChecked(row: any): boolean {
    if (!this.isRowSelectable(row)) return false;
    const id = this.getRowId(row);
    if (this._isAll) {
      return !this._excludeIds.includes(id);
    } else {
      return this._includeIds.includes(id);
    }
  }

  isAllChecked(): boolean {
    const selectable = this._data.filter(r => this.isRowSelectable(r));
    if (selectable.length === 0) return false;
    return selectable.every(r => this.isRowChecked(r));
  }

  isSomeChecked(): boolean {
    const selectable = this._data.filter(r => this.isRowSelectable(r));
    return selectable.some(r => this.isRowChecked(r)) && !this.isAllChecked();
  }

  private emitSelectionState(isAll: boolean, includeIds: any[], excludeIds: any[], rows: any[] = []) {
    this.isAllChange.emit(isAll);
    this.includeIdsChange.emit(includeIds);
    this.excludeIdsChange.emit(excludeIds);
    this.selectionStateChange.emit({ isAll, includeIds, excludeIds });
    this.selectionChange.emit(rows);
  }

  toggleSelectAll() {
    if (this.isAllChecked()) {
      this._isAll = false;
      this._includeIds = [];
      this._excludeIds = [];
      this.emitSelectionState(false, [], [], []);
    } else {
      this._isAll = true;
      this._includeIds = [];
      this._excludeIds = [];
      this.emitSelectionState(true, [], [], this._data.filter(r => this.isRowSelectable(r)));
    }
  }

  toggleRowSelection(index: number) {
    const row = this._data[index];
    if (!this.isRowSelectable(row)) return;
    const id = this.getRowId(row);
    const checked = this.isRowChecked(row);

    if (this._isAll) {
      const newExclude = checked
        ? [...this._excludeIds, id]
        : this._excludeIds.filter(e => e !== id);
      this._excludeIds = newExclude;
      this.emitSelectionState(true, [], newExclude, []);
    } else {
      const newInclude = checked
        ? this._includeIds.filter(e => e !== id)
        : [...this._includeIds, id];
      this._includeIds = newInclude;
      this.emitSelectionState(false, newInclude, [], this._data.filter(r => newInclude.includes(this.getRowId(r))));
    }
  }

  isRowSelected(index: number): boolean {
    return this.isRowChecked(this._data[index]);
  }

  isAllSelected(): boolean {
    return this.isAllChecked();
  }

  isSomeSelected(): boolean {
    return this.isSomeChecked();
  }

  clearSelection() {
    this._isAll = false;
    this._includeIds = [];
    this._excludeIds = [];
    this.emitSelectionState(false, [], [], []);
    this.selectedRows.clear();
  }

  // Lấy tất cả leaf columns (cột không có children)
  get leafColumns(): TableColumn[] {
    const leaves: TableColumn[] = [];
    
    const traverse = (cols: TableColumn[]) => {
      cols.forEach(col => {
        if (col.children && col.children.length > 0) {
          traverse(col.children);
        } else {
          leaves.push(col);
        }
      });
    };
    
    traverse(this.columns);
    return leaves;
  }

  // Kiểm tra xem column có children không
  hasChildren(column: TableColumn): boolean {
    return !!(column.children && column.children.length > 0);
  }

  // Lấy colspan cho column
  getColspan(column: TableColumn): number {
    if (!this.hasChildren(column)) {
      return 1;
    }
    return (column.children || []).reduce((sum, child) => sum + this.getColspan(child), 0);
  }

  // Lấy giá trị từ row
  getValue(row: any, columnKey: string): any {
    const value = row[columnKey];
    if (typeof value === 'object') {
      return JSON.stringify(value);
    }
    return value === null || value === undefined ? '' : value;
  }

  // Lấy style cho cell dựa trên column config
  getCellStyle(column: TableColumn): any {
    const style: any = {};
    
    if (column.bold) {
      style['font-weight'] = 'bold';
    }
    
    if (column.color) {
      style['color'] = column.color;
    }
    
    if (column.textAlign) {
      style['text-align'] = column.textAlign;
    }
    
    return style;
  }

  // Lấy class cho cell dựa trên column config
  getCellClass(column: TableColumn): string {
    const classes: string[] = [];
    
    if (column.bold) {
      classes.push('font-bold');
    }
    
    if (column.color) {
      classes.push(`text-[${column.color}]`);
    }
    
    if (column.textAlign === 'center') {
      classes.push('text-center');
    } else if (column.textAlign === 'right') {
      classes.push('text-right');
    } else {
      classes.push('text-left');
    }
    
    return classes.join(' ');
  }

  // Lấy wrapper element type
  getWrapperElement(column: TableColumn): string {
    return column.wrapper || 'span';
  }

  // Kiểm tra xem column có phải icon không
  isIconColumn(column: TableColumn): boolean {
    return !!column.isIcon;
  }

  // Lấy icon SVG cho giá trị
  getIconSvg(column: TableColumn, value: any): string {
    if (column.iconMap && column.iconMap[value]) {
      return column.iconMap[value];
    }
    return '';
  }

  // Sanitize HTML cho icon
  getSafeHtml(html: string): SafeHtml {
    return this.sanitizer.bypassSecurityTrustHtml(html);
  }

  // Lấy badge style dựa trên giá trị (cho wrapper: 'div')
  getBadgeStyle(value: any): any {
    const statusColors: { [key: string]: { bg: string; text: string } } = {
      'Kích hoạt': { bg: '#d1fae5', text: '#065f46' },
      'Khóa': { bg: '#fee2e2', text: '#991b1b' },
      'Hoàn thành': { bg: '#d1fae5', text: '#065f46' },
      'Dự thảo': { bg: '#fef3c7', text: '#92400e' },
      'Chờ đánh giá': { bg: '#dbeafe', text: '#0c2d6b' },
      'Chờ đánh giá của thủ trưởng': { bg: '#fce7f3', text: '#831843' }
    };

    const colors = statusColors[value] || { bg: '#f3f4f6', text: '#374151' };
    return {
      'background-color': colors.bg,
      'color': colors.text,
      'padding': '0.375rem 0.75rem',
      'border-radius': '9999px',
      'display': 'inline-block',
      'font-size': '0.875rem',
      'font-weight': '500'
    };
  }

  // Kiểm tra xem row có thể chọn được không
  isRowSelectable(row: any): boolean {
    return this.selectionFilter(row);
  }

  // Lấy danh sách các hàng được chọn
  getSelectedRows(): any[] {
    return this._data.filter(r => this.isRowChecked(r));
  }

  // Emit event khi click action icon
  onActionClick(row: any, actionIndex: number) {
    this.rowActionClick.emit({ row, actionIndex });
  }

  // Emit event khi click vào hàng
  onRowClick(row: any) {
    this.rowClick.emit(row);
  }
}

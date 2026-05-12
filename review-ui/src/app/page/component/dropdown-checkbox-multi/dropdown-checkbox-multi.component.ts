import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy, HostListener, ViewChild, ElementRef, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface DropdownOption {
  id: number | string;
  name: string;
}

@Component({
  selector: 'app-dropdown-checkbox-multi',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dropdown-checkbox-multi.component.html',
  styleUrls: ['./dropdown-checkbox-multi.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DropdownCheckboxMultiComponent {
  @Input() options: DropdownOption[] = [];
  @Input() placeholder: string = 'Chọn...';
  @Input() selectedValues: (string | number)[] = [];
  @Input() showSelectAll: boolean = false;
  @Input() columns: number = 1;
  @Output() selectionChange = new EventEmitter<(string | number)[]>();
  @ViewChild('dropdownContainer') dropdownContainer!: ElementRef;

  isDropdownOpen = false;

  constructor(private cdr: ChangeDetectorRef) {}

  toggleDropdown(): void {
    this.isDropdownOpen = !this.isDropdownOpen;
    this.cdr.markForCheck();
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (this.dropdownContainer && !this.dropdownContainer.nativeElement.contains(event.target)) {
      this.isDropdownOpen = false;
      this.cdr.markForCheck();
    }
  }

  toggleOption(option: DropdownOption): void {
    // Nếu là "Chọn tất cả"
    if (option.id === 'all') {
      if (this.isAllSelected()) {
        // Bỏ chọn tất cả
        this.selectedValues = [];
      } else {
        // Chọn tất cả (trừ "Chọn tất cả")
        this.selectedValues = this.getOptionsToSelect()
          .map(o => o.id);
      }
    } else {
      // Xử lý các option thường
      const index = this.selectedValues.indexOf(option.id);
      if (index > -1) {
        this.selectedValues.splice(index, 1);
      } else {
        this.selectedValues.push(option.id);
      }
    }
    this.selectionChange.emit([...this.selectedValues]);
    this.cdr.markForCheck();
  }

  isSelected(id: string | number): boolean {
    return this.selectedValues.includes(id);
  }

  isAllSelected(): boolean {
    const optionsToSelect = this.getOptionsToSelect();
    return optionsToSelect.length > 0 && 
           optionsToSelect.every(o => this.selectedValues.includes(o.id));
  }

  getOptionsToSelect(): DropdownOption[] {
    return this.options.filter(o => o.id !== 'all');
  }

  getGridStyle(): string {
    return `grid-template-columns: repeat(${this.columns}, 1fr)`;
  }

  getSelectedNames(): string {
    if (this.selectedValues.length === 0) {
      return '';
    }
    
    // Nếu chọn tất cả các option (không bao gồm "Chọn tất cả")
    const optionsToSelect = this.getOptionsToSelect();
    let displayText = '';
    
    if (this.selectedValues.length === optionsToSelect.length) {
      // Hiển thị "Tất cả các tháng" hoặc tương tự
      displayText = 'Tất cả (' + optionsToSelect.length + ')';
    } else {
      // Nếu chọn một số
      displayText = this.selectedValues
        .map(id => this.options.find(o => o.id === id)?.name)
        .filter(Boolean)
        .join(', ');
    }
    
    // Truncate nếu dài quá 30 ký tự
    if (displayText.length > 30) {
      return displayText.substring(0, 27) + '...';
    }
    
    return displayText;
  }
}

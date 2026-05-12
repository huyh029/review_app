import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy, HostListener, ViewChild, ElementRef, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface DropdownOption {
  id: number | string;
  name: string;
}

@Component({
  selector: 'app-dropdown-checkbox',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dropdown-checkbox.component.html',
  styleUrls: ['./dropdown-checkbox.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DropdownCheckboxComponent {
  @Input() options: DropdownOption[] = [];
  @Input() placeholder: string = 'Chọn...';
  @Input() selectedValue: string | number = '';
  @Output() selectionChange = new EventEmitter<string | number>();
  @ViewChild('dropdownContainer') dropdownContainer!: ElementRef;

  isDropdownOpen = false;

  constructor(private cdr: ChangeDetectorRef) {}

  toggleDropdown(): void {
    this.isDropdownOpen = !this.isDropdownOpen;
    this.cdr.markForCheck();
  }

  selectOption(option: DropdownOption): void {
    if (this.selectedValue === option.id) {
      // Deselect if already selected
      this.selectedValue = '';
      this.selectionChange.emit('');
    } else {
      // Select option
      this.selectedValue = option.id;
      this.selectionChange.emit(option.id);
    }
    this.isDropdownOpen = false;
    this.cdr.markForCheck();
  }

  getSelectedName(): string {
    const selected = this.options.find(o => o.id === this.selectedValue);
    return selected ? selected.name : this.placeholder;
  }

  isSelected(option: DropdownOption): boolean {
    return this.selectedValue === option.id;
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (this.dropdownContainer && !this.dropdownContainer.nativeElement.contains(event.target)) {
      this.isDropdownOpen = false;
      this.cdr.markForCheck();
    }
  }
}

import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy, HostListener, ViewChild, ElementRef, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface DropdownOption {
  id: number | string;
  name: string;
}

@Component({
  selector: 'app-dropdown-select',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dropdown-select.component.html',
  styleUrls: ['./dropdown-select.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DropdownSelectComponent {
  @Input() options: DropdownOption[] = [];
  @Input() placeholder: string = 'Chọn...';
  @Input() selectedValue: string | number = '';
  @Input() disabled: boolean = false;
  @Output() selectionChange = new EventEmitter<string | number>();
  @ViewChild('dropdownContainer') dropdownContainer!: ElementRef;

  isDropdownOpen = false;

  constructor(private cdr: ChangeDetectorRef) {}

  toggleDropdown(): void {
    if (this.disabled) return;
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

  selectOption(option: DropdownOption): void {
    this.selectedValue = option.id;
    this.selectionChange.emit(option.id);
    this.isDropdownOpen = false;
    this.cdr.markForCheck();
  }

  getSelectedName(): string {
    const selected = this.options.find(o => o.id === this.selectedValue);
    return selected ? selected.name : this.placeholder;
  }
}

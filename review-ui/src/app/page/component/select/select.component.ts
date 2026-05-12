import { Component, Input, Output, EventEmitter, HostListener, OnChanges, SimpleChanges, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

export interface SelectOption {
  label: string;
  value: any;
}

export type SelectDirection = 'up' | 'down';

@Component({
  selector: 'app-select',
  imports: [CommonModule, FormsModule],
  templateUrl: './select.component.html',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SelectComponent),
      multi: true
    }
  ]
})
export class SelectComponent implements OnChanges, ControlValueAccessor {
  @Input() options: SelectOption[] = [];
  @Input() placeholder: string = 'Chọn...';
  @Input() disabled: boolean = false;
  @Input() direction: SelectDirection = 'down';
  @Output() valueChange = new EventEmitter<any>();

  isOpen = false;
  internalValue: any = null;

  // ControlValueAccessor methods
  onChange: any = () => {};
  onTouched: any = () => {};

  ngOnChanges(changes: SimpleChanges) {
    if (changes['value']) {
      this.internalValue = changes['value'].currentValue;
    }
  }

  get selectedLabel(): string {
    const selected = this.options.find(opt => opt.value === this.internalValue);
    return selected ? selected.label : this.placeholder;
  }

  toggleDropdown() {
    if (!this.disabled) {
      this.isOpen = !this.isOpen;
    }
  }

  selectOption(option: SelectOption) {
    this.internalValue = option.value;
    this.onChange(option.value);
    this.valueChange.emit(option.value);
    this.isOpen = false;
  }

  // ControlValueAccessor implementation
  writeValue(value: any): void {
    this.internalValue = value;
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    const target = event.target as HTMLElement;
    if (!target.closest('.app-select')) {
      this.isOpen = false;
    }
  }
}

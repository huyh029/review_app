import { Component, OnInit, OnChanges, ChangeDetectorRef, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { DropdownCheckboxMultiComponent } from '../../../../../component/dropdown-checkbox-multi/dropdown-checkbox-multi.component';
import { CriteriaTableComponent, type CriteriaItem } from '../../../../../component/criteria-table/criteria-table.component';
import { EvaluationCriteriaDetailService, CreateEvaluationCriteriaDetailRequest } from '../../evaluation-criteria-detail.service';

export interface DropdownOption {
  id: number | string;
  name: string;
}

@Component({
  selector: 'app-general-info',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DropdownCheckboxMultiComponent, CriteriaTableComponent],
  templateUrl: './general-info.component.html',
  styleUrls: ['./general-info.component.css']
})
export class GeneralInfoComponent implements OnInit, OnChanges {
  @Input() criteriaSetName: string = '';
  @Input() selectedObjects: string[] = [];
  @Input() selectedYears: (string | number)[] = [];
  @Input() selectedPeriods: (string | number)[] = [];
  @Input() criteriaItems: CriteriaItem[] = [];
  
  @Output() criteriaSetNameChange = new EventEmitter<string>();
  @Output() selectedObjectsChange = new EventEmitter<string[]>();
  @Output() selectedYearsChange = new EventEmitter<(string | number)[]>();
  @Output() selectedPeriodsChange = new EventEmitter<(string | number)[]>();
  @Output() criteriaItemsChange = new EventEmitter<CriteriaItem[]>();

  form: FormGroup;
  applicableObjectOptions: DropdownOption[] = [];
  loading = false;

  yearOptions: DropdownOption[] = this.generateYearOptions();

  periodOptions: DropdownOption[] = [
    { id: 'all', name: 'Chọn tất cả' },
    { id: '1', name: 'Tháng 1' },
    { id: '2', name: 'Tháng 2' },
    { id: '3', name: 'Tháng 3' },
    { id: '4', name: 'Tháng 4' },
    { id: '5', name: 'Tháng 5' },
    { id: '6', name: 'Tháng 6' },
    { id: '7', name: 'Tháng 7' },
    { id: '8', name: 'Tháng 8' },
    { id: '9', name: 'Tháng 9' },
    { id: '10', name: 'Tháng 10' },
    { id: '11', name: 'Tháng 11' },
    { id: '12', name: 'Tháng 12' }
  ];

  constructor(
    private fb: FormBuilder,
    private service: EvaluationCriteriaDetailService,
    private cdr: ChangeDetectorRef
  ) {
    this.form = this.fb.group({
      name: ['', [Validators.required]],
      applicableObject: ['', [Validators.required]],
      year: ['', [Validators.required]],
      period: ['', [Validators.required]]
    });
  }

  ngOnInit() {
    this.loadEvaluationObjects();
    this.initializeForm();
  }

  ngOnChanges() {
    this.initializeForm();
  }

  private initializeForm(): void {
    this.form.patchValue({
      name: this.criteriaSetName,
      applicableObject: this.selectedObjects,
      year: this.selectedYears,
      period: this.selectedPeriods
    });  }

  loadEvaluationObjects() {
    this.loading = true;
    this.service.getActiveEvaluationObjects().subscribe({
      next: (data) => {
        this.applicableObjectOptions = data.map(obj => ({
          id: obj.code,
          name: obj.name
        }));
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error('Error loading evaluation objects:', err);
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  generateYearOptions(): DropdownOption[] {
    const currentYear = new Date().getFullYear();
    const years: DropdownOption[] = [];
    
    for (let year = currentYear; year >= 1970; year--) {
      years.push({ id: year.toString(), name: year.toString() });
    }
    
    return years;
  }

  onNameChange(value: string): void {
    this.criteriaSetNameChange.emit(value);
  }

  onApplicableObjectChange(value: (string | number)[]): void {
    this.selectedObjectsChange.emit(value.map(v => v.toString()));
  }

  onYearChange(years: (string | number)[]): void {
    this.selectedYearsChange.emit(years);
  }

  onPeriodChange(periods: (string | number)[]): void {
    this.selectedPeriodsChange.emit(periods);
  }

  onAddCriteria(): void {
    const newItem: CriteriaItem = {
      id: Date.now().toString(),
      code: '',
      content: '',
      maxScore: 0,
      scoreType: 'Điểm cộng',
      expanded: false,
      children: []
    };
    
    const updatedItems = [...this.criteriaItems, newItem];
    this.criteriaItemsChange.emit(updatedItems);
  }

  onEditCriteria(item: CriteriaItem): void {
    console.log('Edit criteria', item);
  }

  onDeleteCriteria(id: string | number): void {
    const updatedItems = this.criteriaItems.filter(item => item.id !== id);
    this.criteriaItemsChange.emit(updatedItems);
  }

  onUpdateCriteria(item: CriteriaItem): void {
    const index = this.criteriaItems.findIndex(c => c.id === item.id);
    if (index !== -1) {
      const updatedItems = [...this.criteriaItems];
      updatedItems[index] = item;
      this.criteriaItemsChange.emit(updatedItems);
    }
  }
}

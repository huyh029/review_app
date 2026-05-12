import { Component, ChangeDetectionStrategy, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { GeneralInfoComponent } from './general-info/general-info.component';
import { CriteriaClassificationComponent, ClassificationItem } from './criteria-classification/criteria-classification.component';
import { TemplateComponent } from './template/template.component';
import { EvaluationCriteriaService } from '../evaluation-criteria.service';
import { EvaluationCriteriaDetailService } from '../evaluation-criteria-detail.service';
import { CriteriaTableComponent, type CriteriaItem } from '../../../../component/criteria-table/criteria-table.component';

@Component({
  selector: 'app-evaluation-criteria-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, GeneralInfoComponent, CriteriaClassificationComponent, TemplateComponent],
  templateUrl: './evaluation-criteria-detail.component.html',
  styleUrls: ['./evaluation-criteria-detail.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EvaluationCriteriaDetailComponent implements OnInit {
  tabs = [
    { label: 'Thông tin chung', type: 'general' },
    { label: 'Phân loại đánh giá', type: 'criteria' },
    { label: 'Biểu mẫu', type: 'template' }
  ];
  
  activeTab: string = 'general';
  isEditMode: boolean = false;
  criteriaId: string | null = null;
  title: string = 'Tạo bộ tiêu chí mới';
  saving: boolean = false;

  // Data stored in parent component
  criteriaSetName: string = '';
  selectedObjects: string[] = [];
  selectedYears: (string | number)[] = [];
  selectedPeriods: (string | number)[] = [];
  criteriaItems: CriteriaItem[] = [] as CriteriaItem[];
  classifications: ClassificationItem[] = [
    {
      id: 'class-1',
      code: 'KHTNV',
      name: 'Không hoàn thành nhiệm vụ',
      shortName: 'KHTNV',
      scoreFrom: 0,
      scoreTo: 49
    },
    {
      id: 'class-2',
      code: 'HTNV',
      name: 'Hoàn thành nhiệm vụ',
      shortName: 'HTNV',
      scoreFrom: 50,
      scoreTo: 69
    },
    {
      id: 'class-3',
      code: 'HTTTNV',
      name: 'Hoàn thành tốt nhiệm vụ',
      shortName: 'HTTTNV',
      scoreFrom: 70,
      scoreTo: 84
    },
    {
      id: 'class-4',
      code: 'HTXSNV',
      name: 'Hoàn thành xuất sắc nhiệm vụ',
      shortName: 'HTXSNV',
      scoreFrom: 85,
      scoreTo: 100
    }
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private criteriaService: EvaluationCriteriaService,
    private criteriaDetailService: EvaluationCriteriaDetailService
  ) {}

  ngOnInit() {
    this.route.params.subscribe(params => {
      this.criteriaId = params['id'] || null;
      this.isEditMode = !!this.criteriaId;
      this.title = this.isEditMode ? 'Chỉnh sửa bộ tiêu chí' : 'Tạo bộ tiêu chí mới';
      
      if (this.isEditMode && this.criteriaId) {
        this.loadCriteriaDetail(parseInt(this.criteriaId));
      }
      
      this.cdr.markForCheck();
    });
  }

  loadCriteriaDetail(id: number) {
    this.criteriaDetailService.getDetail(id).subscribe({
      next: (data) => {
        this.criteriaSetName = data.name;
        this.selectedObjects = data.objectCodes || [];
        this.selectedYears = data.applicableYears ? data.applicableYears.split(',').map(y => y.trim()) : [];
        this.selectedPeriods = data.applicableMonths ? data.applicableMonths.split(',').map(m => m.trim()) : [];
        
        // Build tree structure from flat criteria list
        this.criteriaItems = this.buildCriteriaTree(data.criteria);
        
        // Map classifications
        this.classifications = data.classifications.map(cl => ({
          id: cl.virtualId,
          code: cl.code,
          name: cl.name,
          shortName: cl.abbreviation,
          scoreFrom: cl.minScore,
          scoreTo: cl.maxScore
        }));
        
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error('Error loading criteria detail:', err);
        this.cdr.markForCheck();
      }
    });
  }

  private buildCriteriaTree(flatCriteria: any[]): CriteriaItem[] {
    const map = new Map<string, CriteriaItem>();
    const roots: CriteriaItem[] = [];

    // Create map of all items
    flatCriteria.forEach(c => {
      const item: CriteriaItem = {
        id: c.virtualCode,
        code: c.displayCode,
        content: c.content,
        maxScore: c.maxScore,
        scoreType: c.scoreType,
        children: [],
        expanded: true
      };
      map.set(c.virtualCode, item);
    });

    // Build tree by linking children to parents
    flatCriteria.forEach(c => {
      const item = map.get(c.virtualCode)!;
      if (c.virtualParentCode && c.virtualParentCode.trim() !== '') {
        const parent = map.get(c.virtualParentCode);
        if (parent) {
          parent.children!.push(item);
        }
      } else {
        roots.push(item);
      }
    });

    return roots;
  }

  selectTab(type: string) {
    this.activeTab = type;
  }

  onBack() {
    this.router.navigate(['/configuration/evaluation-criteria']);
  }

  onCancel() {
    this.router.navigate(['/configuration/evaluation-criteria']);
  }

  onDelete() {
    if (confirm('Bạn có chắc chắn muốn xóa bộ tiêu chí này không?')) {
      console.log('Deleting criteria:', this.criteriaId);
      // TODO: Call service to delete
      this.router.navigate(['/configuration/evaluation-criteria']);
    }
  }

  // Handle data changes from child components
  onCriteriaSetNameChange(name: string): void {
    this.criteriaSetName = name;
    this.cdr.markForCheck();
  }

  onApplicableObjectChange(value: string[]): void {
    this.selectedObjects = value;
    this.cdr.markForCheck();
  }

  onSelectedYearsChange(years: (string | number)[]): void {
    this.selectedYears = years;
    this.cdr.markForCheck();
  }

  onSelectedPeriodsChange(periods: (string | number)[]): void {
    this.selectedPeriods = periods;
    this.cdr.markForCheck();
  }

  onCriteriaItemsChange(items: CriteriaItem[]): void {
    this.criteriaItems = items;
    this.cdr.markForCheck();
  }

  onClassificationChange(classifications: ClassificationItem[]): void {
    this.classifications = classifications;
    this.cdr.markForCheck();
  }

  onSave() {
    this.saving = true;
    console.log('Saving criteria');
    console.log('Criteria set name:', this.criteriaSetName);
    console.log('Criteria items:', this.criteriaItems);
    console.log('Classifications:', this.classifications);
    
    // Validate
    if (!this.criteriaSetName) {
      console.error('Criteria set name is required');
      this.saving = false;
      this.cdr.markForCheck();
      return;
    }
    
    // Convert tree structure to request format
    const criteriaRequests = this.criteriaItems
      .filter(item => item.code)
      .map(item => this.convertCriteriaItemToRequest(item));

    // Prepare classifications
    const classificationRequests = this.classifications.map(classification => ({
      criteriaSetId: 0, // Will be set by backend
      code: classification.code,
      virtualId: classification.id?.toString() || '',
      name: classification.name,
      abbreviation: classification.shortName,
      minScore: classification.scoreFrom,
      maxScore: classification.scoreTo
    }));

    // Create batch request
    const batchRequest = {
      name: this.criteriaSetName,
      objectCodes: this.selectedObjects,
      applicableYears: Array.isArray(this.selectedYears) ? this.selectedYears.join(',') : '',
      applicableMonths: Array.isArray(this.selectedPeriods) ? this.selectedPeriods.join(',') : '',
      criteria: criteriaRequests,
      classifications: classificationRequests
    };

    if (this.isEditMode && this.criteriaId) {
      // Update existing criteria set
      this.criteriaDetailService.updateDetail(parseInt(this.criteriaId), batchRequest).subscribe({
        next: (result) => {
          console.log('Criteria set detail updated successfully:', result);
          this.saving = false;
          this.cdr.markForCheck();
          this.router.navigate(['/configuration/evaluation-criteria']);
        },
        error: (err) => {
          console.error('Error updating criteria set detail:', err);
          this.saving = false;
          this.cdr.markForCheck();
        }
      });
    } else {
      // Create new criteria set
      this.criteriaService.createCriteriaSetDetail(batchRequest).subscribe({
        next: (result) => {
          console.log('Criteria set detail created successfully:', result);
          this.saving = false;
          this.cdr.markForCheck();
          this.router.navigate(['/configuration/evaluation-criteria']);
        },
        error: (err) => {
          console.error('Error creating criteria set detail:', err);
          this.saving = false;
          this.cdr.markForCheck();
        }
      });
    }
  }

  private convertCriteriaItemToRequest(item: CriteriaItem): any {
    return {
      displayCode: item.code || '',
      content: item.content || '',
      maxScore: item.maxScore ?? null,
      scoreType: item.scoreType || 'Điểm cộng',
      children: item.children && item.children.length > 0
        ? item.children
            .filter(child => child.content) // Filter by content, not code
            .map(child => this.convertCriteriaItemToRequest(child))
        : []
    };
  }
}

import { Component, ChangeDetectionStrategy, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EvaluationCriteriaDetailService, EvaluationObjectDto } from '../../evaluation-criteria-detail.service';

@Component({
  selector: 'app-template',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './template.component.html',
  styleUrls: ['./template.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TemplateComponent implements OnInit {
  evaluationObjects: EvaluationObjectDto[] = [];
  loading = false;

  constructor(
    private service: EvaluationCriteriaDetailService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.loadEvaluationObjects();
  }

  loadEvaluationObjects() {
    this.loading = true;
    this.service.getActiveEvaluationObjects().subscribe({
      next: (data) => {
        this.evaluationObjects = data;
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
}

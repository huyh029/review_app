import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface CriteriaNode {
  id: string;
  name: string;
  children?: CriteriaNode[];
  expanded?: boolean;
}

@Component({
  selector: 'app-evaluation-criteria-tree',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './evaluation-criteria-tree.component.html',
  styleUrls: ['./evaluation-criteria-tree.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EvaluationCriteriaTreeComponent {
  @Input() title: string = 'Cây vai trò đánh giá';
  @Input() subtitle: string = '(Con đánh giá Cha)';
  @Input() data: CriteriaNode[] = [];
  
  @Output() addCriteria = new EventEmitter<void>();
  @Output() editCriteria = new EventEmitter<CriteriaNode>();
  @Output() deleteCriteria = new EventEmitter<CriteriaNode>();

  toggleNode(node: CriteriaNode) {
    node.expanded = !node.expanded;
  }

  onAddCriteria() {
    this.addCriteria.emit();
  }

  onEditCriteria(node: CriteriaNode) {
    this.editCriteria.emit(node);
  }

  onDeleteCriteria(node: CriteriaNode) {
    this.deleteCriteria.emit(node);
  }
}

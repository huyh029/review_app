import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TreeNode, Individual } from './tree-view.component';

@Component({
  selector: 'app-tree-node',
  standalone: true,
  imports: [CommonModule, TreeNodeComponent],
  templateUrl: './tree-node.component.html',
  styleUrls: ['./tree-node.component.css']
})
export class TreeNodeComponent {
  @Input() node!: TreeNode;
  @Input() level = 0;
  @Input() emptyColumnCount: number = 2;
  @Input() headers: string[] = [];
  @Input() evaluationObjectHeaders: any[] = []; // Array of {code, name}
  @Output() checkboxChange = new EventEmitter<{userId: string, evaluationObjectCode: string, isChecked: boolean}>();
  
  expanded = true;
  individualsExpanded = false; // mặc định đóng danh sách users

  hasChildren(): boolean {
    return !!(this.node.children && this.node.children.length > 0);
  }

  hasIndividuals(): boolean {
    return !!(this.node.individuals && this.node.individuals.length > 0);
  }

  onToggle() {
    this.expanded = !this.expanded;
  }

  toggleIndividuals() {
    this.individualsExpanded = !this.individualsExpanded;
  }

  getIndividualLabel(individual: Individual): string {
    return `${individual.name} (${individual.code})`;
  }

  getEmptyColumns(): number[] {
    return Array(this.emptyColumnCount).fill(0);
  }

  isChecked(individual: Individual, columnIndex: number): boolean {
    if (!individual.selectedObjectIds || columnIndex >= this.evaluationObjectHeaders.length) {
      return false;
    }
    const evaluationObjectCode = this.evaluationObjectHeaders[columnIndex].code;
    return individual.selectedObjectIds.includes(evaluationObjectCode);
  }

  onCheckboxChange(individual: Individual, columnIndex: number, event: any) {
    const evaluationObjectCode = this.evaluationObjectHeaders[columnIndex].code;
    this.checkboxChange.emit({
      userId: individual.id,
      evaluationObjectCode: evaluationObjectCode,
      isChecked: event.target.checked
    });
  }
}

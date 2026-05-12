import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DepartmentTreeNodeComponent } from './department-tree-node.component';

export interface DepartmentNode {
  id: string;
  code: string;
  name: string;
  checked?: boolean;
  expanded?: boolean;
  children?: DepartmentNode[] | null;
}

@Component({
  selector: 'app-department-selection',
  standalone: true,
  imports: [CommonModule, FormsModule, DepartmentTreeNodeComponent],
  templateUrl: './department-selection.component.html',
  styleUrls: ['./department-selection.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DepartmentSelectionComponent {
  @Input() isOpen = false;
  @Input() departments: DepartmentNode[] = [];
  @Output() close = new EventEmitter<void>();
  @Output() confirm = new EventEmitter<DepartmentNode[]>();

  constructor(private cdr: ChangeDetectorRef) {}

  onClose(): void {
    this.close.emit();
  }

  onConfirm(): void {
    const selected = this.getSelectedDepartments();
    this.confirm.emit(selected);
  }

  toggleExpand(node: DepartmentNode): void {
    node.expanded = !node.expanded;
    this.cdr.markForCheck();
  }

  private getSelectedDepartments(): DepartmentNode[] {
    const selected: DepartmentNode[] = [];
    
    const collect = (nodes: DepartmentNode[]) => {
      nodes.forEach(node => {
        if (node.checked) {
          selected.push(node);
        }
        if (node.children && node.children.length > 0) {
          collect(node.children);
        }
      });
    };
    
    collect(this.departments);
    return selected;
  }

  hasChildren(node: DepartmentNode): boolean {
    return node.children !== null && node.children !== undefined && node.children.length > 0;
  }
}

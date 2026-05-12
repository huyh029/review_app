import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

export interface DepartmentNode {
  id: string;
  code: string;
  name: string;
  checked?: boolean;
  expanded?: boolean;
  children?: DepartmentNode[] | null;
}

@Component({
  selector: 'app-department-tree-node',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="flex items-center space-x-2" [style.padding-left]="(level * 1.5) + 'rem'">
      <button 
        *ngIf="hasChildren()"
        type="button"
        (click)="onToggleExpand()"
        class="flex items-center justify-center w-6 h-6 rounded-md hover:bg-gray-200">
        <svg 
          fill="none" 
          stroke="currentColor" 
          viewBox="0 0 24 24" 
          xmlns="http://www.w3.org/2000/svg" 
          class="w-4 h-4 text-gray-500 transition-transform"
          [class.rotate-90]="node.expanded">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"></path>
        </svg>
      </button>
      <button 
        *ngIf="!hasChildren()"
        type="button"
        class="flex items-center justify-center w-6 h-6 rounded-md hover:bg-gray-200 invisible">
        <svg 
          fill="none" 
          stroke="currentColor" 
          viewBox="0 0 24 24" 
          xmlns="http://www.w3.org/2000/svg" 
          class="w-4 h-4 text-gray-500 transition-transform rotate-90">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"></path>
        </svg>
      </button>
      <input 
        type="checkbox" 
        [(ngModel)]="node.checked"
        (change)="onCheckChange()"
        class="h-4 w-4 rounded border-gray-300 text-[#276100] focus:ring-[#276100] accent-[#276100]"
        [id]="'temp-dept-' + node.id">
      <label 
        [for]="'temp-dept-' + node.id"
        [class.font-semibold]="level === 0"
        [class.text-gray-800]="level === 0"
        [class.text-gray-700]="level > 0"
        class="cursor-pointer select-none">
        {{ node.name }}
      </label>
    </div>
    <ul *ngIf="node.expanded && hasChildren()" class="mt-2 space-y-2">
      <li *ngFor="let child of node.children">
        <app-department-tree-node 
          [node]="child"
          [level]="level + 1"
          (nodeToggleExpand)="onChildToggleExpand($event)">
        </app-department-tree-node>
      </li>
    </ul>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DepartmentTreeNodeComponent {
  @Input() node!: DepartmentNode;
  @Input() level: number = 0;
  @Output() nodeToggleExpand = new EventEmitter<DepartmentNode>();

  constructor(private cdr: ChangeDetectorRef) {}

  hasChildren(): boolean {
    return this.node.children !== null && this.node.children !== undefined && this.node.children.length > 0;
  }

  onToggleExpand(): void {
    this.node.expanded = !this.node.expanded;
    this.cdr.markForCheck();
    this.nodeToggleExpand.emit(this.node);
  }

  onCheckChange(): void {
    this.cdr.markForCheck();
  }

  onChildToggleExpand(node: DepartmentNode): void {
    this.nodeToggleExpand.emit(node);
  }
}

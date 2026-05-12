import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TreeNodeComponent } from './tree-node.component';

export interface TreeNode {
  id: string;
  code: string;
  name: string;
  children?: TreeNode[];
  individuals?: Individual[];
}

export interface Individual {
  id: string;
  name: string;
  code: string;
  roleName?: string;
  selectedObjectIds?: string[];
}

@Component({
  selector: 'app-tree-view',
  standalone: true,
  imports: [CommonModule, TreeNodeComponent],
  templateUrl: './tree-view.component.html',
  styleUrls: ['./tree-view.component.css']
})
export class TreeViewComponent {
  @Input() data: TreeNode[] = [];
  @Input() headers: string[] = [];
  @Input() evaluationObjectHeaders: any[] = []; // Array of {code, name}
  @Input() showZoom: boolean = true;
  @Output() changeDetected = new EventEmitter<{userId: string, evaluationObjectCode: string, isAdded: boolean}>();
  
  expandedNodes = new Set<string>();
  zoomLevel = 100;
  selectedData: Map<string, string[]> = new Map(); // userId -> [evaluationObjectCodes]
  originalData: Map<string, string[]> = new Map(); // Track original state

  get emptyColumnCount(): number {
    return Math.max(0, this.headers.length - 1);
  }

  constructor() {
    this.expandAllNodes();
  }

  ngOnInit() {
    // Store original data
    this.storeOriginalData();
  }

  storeOriginalData() {
    const traverse = (nodes: TreeNode[]) => {
      nodes.forEach(node => {
        if (node.individuals) {
          node.individuals.forEach(individual => {
            if (individual.selectedObjectIds && individual.selectedObjectIds.length > 0) {
              this.originalData.set(individual.id, [...individual.selectedObjectIds]);
            }
          });
        }
        if (node.children) {
          traverse(node.children);
        }
      });
    };
    traverse(this.data);
  }

  expandAllNodes() {
    const traverse = (nodes: TreeNode[]) => {
      nodes.forEach(node => {
        this.expandedNodes.add(node.id);
        if (node.children && node.children.length > 0) {
          traverse(node.children);
        }
      });
    };
    traverse(this.data);
  }

  toggleNode(nodeId: string) {
    if (this.expandedNodes.has(nodeId)) {
      this.expandedNodes.delete(nodeId);
    } else {
      this.expandedNodes.add(nodeId);
    }
  }

  isExpanded(nodeId: string): boolean {
    return this.expandedNodes.has(nodeId);
  }

  hasChildren(node: TreeNode): boolean {
    return !!(node.children && node.children.length > 0);
  }

  hasIndividuals(node: TreeNode): boolean {
    return !!(node.individuals && node.individuals.length > 0);
  }

  zoomIn() {
    if (this.zoomLevel < 200) {
      this.zoomLevel += 10;
    }
  }

  zoomOut() {
    if (this.zoomLevel > 50) {
      this.zoomLevel -= 10;
    }
  }

  resetZoom() {
    this.zoomLevel = 100;
  }

  getZoomStyle() {
    return {
      'transform': `scale(${this.zoomLevel / 100})`,
      'transform-origin': 'top left',
      'transition': 'transform 0.2s ease'
    };
  }

  getTreeMinWidth(): string {
    // If emptyColumnCount <= 3, no min-width needed
    // If emptyColumnCount > 3, add 100px for each column beyond 3
    if (this.emptyColumnCount <= 3) {
      return 'auto';
    }
    const extraColumns = this.emptyColumnCount - 3;
    return `calc(100% + ${extraColumns * 100}px)`;
  }

  getSelectedData(): Map<string, string[]> {
    return this.selectedData;
  }

  setCheckboxValue(userId: string, evaluationObjectCode: string, isChecked: boolean) {
    if (!this.selectedData.has(userId)) {
      this.selectedData.set(userId, []);
    }
    const codes = this.selectedData.get(userId)!;
    
    // Track if this is a change from original
    const originalCodes = this.originalData.get(userId) || [];
    const wasOriginallyChecked = originalCodes.includes(evaluationObjectCode);
    
    if (isChecked) {
      if (!codes.includes(evaluationObjectCode)) {
        codes.push(evaluationObjectCode);
        // Emit change if it's different from original
        if (!wasOriginallyChecked) {
          this.changeDetected.emit({
            userId: userId,
            evaluationObjectCode: evaluationObjectCode,
            isAdded: true
          });
        }
      }
    } else {
      const index = codes.indexOf(evaluationObjectCode);
      if (index > -1) {
        codes.splice(index, 1);
        // Emit change if it's different from original
        if (wasOriginallyChecked) {
          this.changeDetected.emit({
            userId: userId,
            evaluationObjectCode: evaluationObjectCode,
            isAdded: false
          });
        }
      }
    }
  }

  getEvaluationObjectCode(columnIndex: number): string {
    if (columnIndex < 0 || columnIndex >= this.evaluationObjectHeaders.length) {
      return '';
    }
    return this.evaluationObjectHeaders[columnIndex].code;
  }
}

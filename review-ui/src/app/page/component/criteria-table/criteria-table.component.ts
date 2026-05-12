import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

export interface CriteriaItem {
  id?: string | number;
  code?: string;
  content: string;
  maxScore?: number;
  scoreType: string;
  children?: CriteriaItem[];
  expanded?: boolean;
  level?: number;
  isDragging?: boolean;
}

@Component({
  selector: 'app-criteria-table',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './criteria-table.component.html',
  styleUrls: ['./criteria-table.component.css'],
  changeDetection: ChangeDetectionStrategy.Default
})
export class CriteriaTableComponent {
  @Input() items: CriteriaItem[] = [];
  @Output() addItem = new EventEmitter<void>();
  @Output() editItem = new EventEmitter<CriteriaItem>();
  @Output() deleteItem = new EventEmitter<string | number>();
  @Output() updateItem = new EventEmitter<CriteriaItem>();

  draggedItem: CriteriaItem | null = null;
  draggedFromParent: CriteriaItem | null = null;
  dropPosition: 'before' | 'after' | 'inside' = 'inside';
  dropTargetId: string | number | null = null;
  dropZonePosition: 'top' | 'bottom' | null = null;
  dragStartY: number = 0;
  showExcelDropdown: boolean = false;
  hoveredItemId: string | number | null = null;

  constructor(private cdr: ChangeDetectorRef) {}

  onAdd(): void {
    this.addItem.emit();
  }

  onAddChild(item: CriteriaItem): void {
    if (!item.children) {
      item.children = [];
    }
    
    const newChild: CriteriaItem = {
      id: `${item.id}-${item.children.length + 1}`,
      code: '',
      content: '',
      maxScore: undefined,
      scoreType: 'Điểm cộng'
    };
    
    item.children.push(newChild);
    item.expanded = true;
    this.cdr.markForCheck();
  }

  onEdit(item: CriteriaItem): void {
    this.editItem.emit(item);
  }

  onDelete(id: string | number | undefined): void {
    if (id !== undefined) {
      this.deleteItem.emit(id);
    }
  }

  finishEdit(item: CriteriaItem): void {
    this.updateItem.emit(item);
  }

  validateMaxScore(item: CriteriaItem): void {
    // Cho phép undefined/null (không nhập)
    if (item.maxScore === undefined || item.maxScore === null) {
      item.maxScore = undefined;
      this.updateItem.emit(item);
      return;
    }
    
    // Giới hạn từ 0 đến 100
    if (item.maxScore < 0) {
      item.maxScore = 0;
    } else if (item.maxScore > 100) {
      item.maxScore = 100;
    }
    this.updateItem.emit(item);
  }

  validateMaxScoreInput(item: CriteriaItem): void {
    // Cho phép undefined/null (không nhập)
    if (isNaN(item.maxScore as any) || item.maxScore === null || item.maxScore === undefined) {
      item.maxScore = undefined;
      return;
    }
    
    // Giới hạn từ 0 đến 100
    if (item.maxScore < 0) {
      item.maxScore = 0;
    } else if (item.maxScore > 100) {
      item.maxScore = 100;
    }
  }

  preventInvalidInput(event: KeyboardEvent, item: CriteriaItem): void {
    const input = event.target as HTMLInputElement;
    const currentValue = input.value;
    
    // Chỉ cho phép digits, backspace, delete, arrow keys, tab
    if (!/[0-9]/.test(event.key) && 
        event.key !== 'Backspace' && 
        event.key !== 'Delete' && 
        event.key !== 'ArrowLeft' && 
        event.key !== 'ArrowRight' &&
        event.key !== 'Tab') {
      event.preventDefault();
      return;
    }
    
    // Nếu giá trị hiện tại là 100 và cố gắng thêm digit, chặn
    if (event.key >= '0' && event.key <= '9') {
      const numValue = parseInt(currentValue);
      if (numValue === 100) {
        event.preventDefault();
      }
    }
  }

  toggleScoreType(item: CriteriaItem): void {
    if (item.scoreType === 'Điểm cộng') {
      item.scoreType = 'Điểm trừ';
    } else {
      item.scoreType = 'Điểm cộng';
    }
    this.updateItem.emit(item);
  }

  autoResizeTextarea(event: Event): void {
    const textarea = event.target as HTMLTextAreaElement;
    textarea.style.height = 'auto';
    textarea.style.height = textarea.scrollHeight + 'px';
  }

  toggleExpand(item: CriteriaItem): void {
    item.expanded = !item.expanded;
  }

  getVisibleItems(): CriteriaItem[] {
    const result: CriteriaItem[] = [];
    
    const addItems = (items: CriteriaItem[], level: number = 0) => {
      items.forEach(item => {
        item.level = level;
        result.push(item);
        if (item.expanded && item.children && item.children.length > 0) {
          addItems(item.children, level + 1);
        }
      });
    };
    
    addItems(this.items);
    return result;
  }

  getItemLevel(item: CriteriaItem): number {
    let level = 0;
    
    const findLevel = (items: CriteriaItem[], target: CriteriaItem, currentLevel: number): boolean => {
      for (const i of items) {
        if (i.id === target.id) {
          level = currentLevel;
          return true;
        }
        if (i.children && findLevel(i.children, target, currentLevel + 1)) {
          return true;
        }
      }
      return false;
    };
    
    findLevel(this.items, item, 0);
    return level;
  }

  hasChildren(item: CriteriaItem): boolean {
    return item.children !== undefined && item.children.length > 0;
  }

  isParentItem(item: CriteriaItem): boolean {
    return this.hasChildren(item);
  }

  getTotalScoreByType(scoreType: string): number {
    // Chỉ tính tổng của các root items có maxScore
    return this.items.reduce((sum, item) => {
      if (item.scoreType === scoreType && item.maxScore !== undefined && item.maxScore !== null) {
        return sum + item.maxScore;
      }
      return sum;
    }, 0);
  }

  onAddRootCriteria(): void {
    const newRootItem: CriteriaItem = {
      id: `root-${Date.now()}`,
      code: '',
      content: '',
      maxScore: undefined,
      scoreType: 'Điểm cộng'
    };
    
    this.items.push(newRootItem);
    this.cdr.markForCheck();
  }

  onRegenerateCode(): void {
    const generateCodes = (items: CriteriaItem[], parentCode: string = ''): void => {
      items.forEach((item, index) => {
        const newCode = parentCode ? `${parentCode}.${index + 1}` : `${index + 1}`;
        item.code = newCode;
        
        if (item.children && item.children.length > 0) {
          generateCodes(item.children, newCode);
        }
      });
    };

    generateCodes(this.items);
    this.cdr.markForCheck();
  }

  toggleExcelDropdown(): void {
    this.showExcelDropdown = !this.showExcelDropdown;
  }

  onExcelAction(action: 'import' | 'download' | 'upload'): void {
    this.showExcelDropdown = false;
    
    switch (action) {
      case 'import':
        console.log('Import from Excel');
        break;
      case 'download':
        console.log('Download template');
        break;
      case 'upload':
        console.log('Upload file');
        break;
    }
  }

  onInsertRowBefore(item: CriteriaItem): void {
    const newItem: CriteriaItem = {
      id: `item-${Date.now()}`,
      code: '',
      content: '',
      maxScore: undefined,
      scoreType: 'Điểm cộng'
    };

    this.insertNewItemBefore(item, newItem);
    this.cdr.markForCheck();
  }

  onInsertRowAfter(item: CriteriaItem): void {
    const newItem: CriteriaItem = {
      id: `item-${Date.now()}`,
      code: '',
      content: '',
      maxScore: undefined,
      scoreType: 'Điểm cộng'
    };

    // Nếu có con thì add vào đầu con, nếu không có con thì add cùng cấp
    if (item.children && item.children.length > 0) {
      item.children.unshift(newItem);
      item.expanded = true;
    } else {
      this.insertNewItemAfter(item, newItem);
    }
    this.cdr.markForCheck();
  }

  private insertNewItemBefore(targetItem: CriteriaItem, newItem: CriteriaItem): void {
    const insertIntoArray = (items: CriteriaItem[]): boolean => {
      const index = items.indexOf(targetItem);
      if (index > -1) {
        items.splice(index, 0, newItem);
        return true;
      }
      for (const item of items) {
        if (item.children && insertIntoArray(item.children)) {
          return true;
        }
      }
      return false;
    };

    insertIntoArray(this.items);
  }

  private insertNewItemAfter(targetItem: CriteriaItem, newItem: CriteriaItem): void {
    const insertIntoArray = (items: CriteriaItem[]): boolean => {
      const index = items.indexOf(targetItem);
      if (index > -1) {
        items.splice(index + 1, 0, newItem);
        return true;
      }
      for (const item of items) {
        if (item.children && insertIntoArray(item.children)) {
          return true;
        }
      }
      return false;
    };

    insertIntoArray(this.items);
  }

  onDragStart(event: DragEvent, item: CriteriaItem): void {
    this.draggedItem = item;
    item.isDragging = true;
    this.dragStartY = event.clientY;
    if (event.dataTransfer) {
      event.dataTransfer.effectAllowed = 'move';
      event.dataTransfer.setData('text/plain', item.id?.toString() || '');
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    if (event.dataTransfer) {
      event.dataTransfer.dropEffect = 'move';
    }
  }

  onDragOverRow(event: DragEvent, item: CriteriaItem): void {
    event.preventDefault();
    if (event.dataTransfer) {
      event.dataTransfer.dropEffect = 'move';
    }

    const tr = (event.target as HTMLElement).closest('tr');
    if (tr) {
      const rect = tr.getBoundingClientRect();
      const relativeY = event.clientY - rect.top;
      const relativeX = event.clientX - rect.left;
      
      this.dropTargetId = item.id || null;

      // Drag cell width is approximately 40px
      const dragCellWidth = 40;
      const isDraggingOverDragCell = relativeX < dragCellWidth;

      if (isDraggingOverDragCell) {
        // Over drag cell: split into top/bottom for before/after
        const isTopHalf = relativeY < rect.height / 2;
        this.dropZonePosition = isTopHalf ? 'top' : 'bottom';
      } else {
        // Over content area: always add as child
        this.dropZonePosition = 'bottom';
      }
    }
  }

  onDragLeave(event: DragEvent): void {
    const tr = (event.target as HTMLElement).closest('tr');
    if (tr && !tr.contains(event.relatedTarget as Node)) {
      this.dropTargetId = null;
      this.dropZonePosition = null;
    }
  }

  onDragEnd(event: DragEvent): void {
    if (this.draggedItem) {
      this.draggedItem.isDragging = false;
    }
    this.draggedItem = null;
    this.draggedFromParent = null;
  }

  onDrop(event: DragEvent, targetItem: CriteriaItem): void {
    event.preventDefault();
    
    if (!this.draggedItem) {
      return;
    }

    // Prevent dropping onto itself
    if (this.draggedItem.id === targetItem.id) {
      this.draggedItem.isDragging = false;
      this.draggedItem = null;
      this.dropTargetId = null;
      this.dropZonePosition = null;
      return;
    }

    // Prevent dropping onto its own children (would create circular reference)
    if (this.isDescendantOf(this.draggedItem, targetItem)) {
      this.draggedItem.isDragging = false;
      this.draggedItem = null;
      this.dropTargetId = null;
      this.dropZonePosition = null;
      return;
    }

    this.removeItemFromTree(this.draggedItem);

    if (this.dropZonePosition === 'top') {
      // Insert as sibling before target
      this.insertItemBeforeTarget(targetItem);
    } else if (this.dropZonePosition === 'bottom') {
      // Insert as sibling after target
      this.insertItemAfterTarget(targetItem);
    }

    this.draggedItem.isDragging = false;
    this.draggedItem = null;
    this.dropTargetId = null;
    this.dropZonePosition = null;
    this.cdr.markForCheck();
  }

  private insertItemAfterTarget(targetItem: CriteriaItem): void {
    if (!this.draggedItem) return;

    const insertIntoArray = (items: CriteriaItem[]): boolean => {
      const index = items.indexOf(targetItem);
      if (index > -1) {
        items.splice(index + 1, 0, this.draggedItem!);
        return true;
      }
      for (const item of items) {
        if (item.children && insertIntoArray(item.children)) {
          return true;
        }
      }
      return false;
    };

    insertIntoArray(this.items);
  }

  private insertItemBeforeTarget(targetItem: CriteriaItem): void {
    if (!this.draggedItem) return;

    const insertIntoArray = (items: CriteriaItem[]): boolean => {
      const index = items.indexOf(targetItem);
      if (index > -1) {
        items.splice(index, 0, this.draggedItem!);
        return true;
      }
      for (const item of items) {
        if (item.children && insertIntoArray(item.children)) {
          return true;
        }
      }
      return false;
    };

    insertIntoArray(this.items);
  }

  private isDescendantOf(parent: CriteriaItem, potentialChild: CriteriaItem): boolean {
    if (!parent.children) {
      return false;
    }

    for (const child of parent.children) {
      if (child.id === potentialChild.id) {
        return true;
      }
      if (this.isDescendantOf(child, potentialChild)) {
        return true;
      }
    }

    return false;
  }

  private removeItemFromTree(item: CriteriaItem): void {
    const remove = (items: CriteriaItem[]): boolean => {
      const index = items.indexOf(item);
      if (index > -1) {
        items.splice(index, 1);
        return true;
      }
      for (const i of items) {
        if (i.children && remove(i.children)) {
          return true;
        }
      }
      return false;
    };

    remove(this.items);
  }
}


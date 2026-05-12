import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

export interface TreeNode {
  id: string;
  name: string;
  code: string;
  roleCode?: string;
  objectCode?: string;
  children?: TreeNode[];
  expanded?: boolean;
  isDragging?: boolean;
  level?: number;
}

export interface AvailableItem {
  code: string;
  name: string;
}

@Component({
  selector: 'app-evaluation-tree',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './evaluation-tree.component.html',
  styleUrls: ['./evaluation-tree.component.css'],
  changeDetection: ChangeDetectionStrategy.Default
})
export class EvaluationTreeComponent {
  @Input() items: TreeNode[] = [];
  @Input() itemType: 'role' | 'object' = 'role'; // 'role' hoặc 'object'
  @Input() availableItems: AvailableItem[] = [];
  @Output() addItem = new EventEmitter<TreeNode>();
  @Output() deleteItem = new EventEmitter<string>();
  @Output() toggleNode = new EventEmitter<string>();
  @Output() handleDrop = new EventEmitter<any>();

  draggedItem: TreeNode | null = null;
  draggedFromParent: TreeNode | null = null;
  dropPosition: 'before' | 'after' | 'inside' = 'inside';
  dropTargetId: string | null = null;
  dropZonePosition: 'top' | 'bottom' | 'inside' | null = null;
  dragStartY: number = 0;
  hoveredItemId: string | null = null;

  selectedItemIndex: number = -1;
  isSelectingItem: boolean = false;
  focusedItemId: string | null = null;

  constructor(private cdr: ChangeDetectorRef) {}

  onAddItem(item: TreeNode): void {
    this.addItem.emit(item);
  }

  onDeleteItem(id: string): void {
    this.deleteItem.emit(id);
  }

  onToggleNode(nodeId: string): void {
    this.toggleNode.emit(nodeId);
  }

  toggleExpand(item: TreeNode): void {
    item.expanded = !item.expanded;
  }

  getFilteredItems(currentName: string): AvailableItem[] {
    if (!currentName) {
      return [...this.availableItems];
    }
    const query = currentName.toLowerCase();
    return this.availableItems
      .filter(item => item.name.toLowerCase().includes(query))
      .sort((a, b) => {
        const aName = a.name.toLowerCase();
        const bName = b.name.toLowerCase();
        const aIdx = aName.indexOf(query);
        const bIdx = bName.indexOf(query);
        // Ưu tiên match ở vị trí nhỏ hơn (đầu chuỗi trước)
        if (aIdx !== bIdx) return aIdx - bIdx;
        // Nếu cùng vị trí, ưu tiên tên ngắn hơn (giống nhất)
        return aName.length - bName.length;
      });
  }

  onNameInputChange(item: TreeNode): void {
    this.selectedItemIndex = -1;
    this.cdr.markForCheck();
  }

  onNameInputFocus(item: TreeNode): void {
    this.focusedItemId = item.id || null;
    this.cdr.markForCheck();
  }

  onNameInputBlurEvent(item: TreeNode): void {
    this.focusedItemId = null;
    this.onNameBlur(item);
  }

  onNameKeydown(event: KeyboardEvent, item: TreeNode): void {
    const filteredItems = this.getFilteredItems(item.name);
    
    if (event.key === 'ArrowDown') {
      event.preventDefault();
      this.selectedItemIndex = Math.min(this.selectedItemIndex + 1, filteredItems.length - 1);
      this.scrollSelectedIntoView();
      this.cdr.markForCheck();
    } else if (event.key === 'ArrowUp') {
      event.preventDefault();
      this.selectedItemIndex = Math.max(this.selectedItemIndex - 1, -1);
      this.scrollSelectedIntoView();
      this.cdr.markForCheck();
    } else if (event.key === 'Enter') {
      event.preventDefault();
      if (this.selectedItemIndex >= 0 && this.selectedItemIndex < filteredItems.length) {
        item.name = filteredItems[this.selectedItemIndex].name;
        if (this.itemType === 'role') {
          item.roleCode = filteredItems[this.selectedItemIndex].code;
        } else {
          item.objectCode = filteredItems[this.selectedItemIndex].code;
        }
      }
      this.onNameBlur(item);
      const input = event.target as HTMLInputElement;
      input.blur();
    } else if (event.key === 'Tab') {
      event.preventDefault();
      this.onNameBlur(item);
      const input = event.target as HTMLInputElement;
      input.blur();
    }
  }

  private scrollSelectedIntoView(): void {
    setTimeout(() => {
      const dropdowns = document.querySelectorAll('.item-dropdown');
      dropdowns.forEach(dropdown => {
        const selectedElement = dropdown.querySelector('.item-option.selected') as HTMLElement;
        if (selectedElement) {
          const selectedRect = selectedElement.getBoundingClientRect();
          const dropdownRect = (dropdown as HTMLElement).getBoundingClientRect();
          
          if (selectedRect.top < dropdownRect.top) {
            (dropdown as HTMLElement).scrollTop -= (dropdownRect.top - selectedRect.top);
          } else if (selectedRect.bottom > dropdownRect.bottom) {
            (dropdown as HTMLElement).scrollTop += (selectedRect.bottom - dropdownRect.bottom);
          }
          
          selectedElement.scrollIntoView({ block: 'nearest', inline: 'nearest' });
        }
      });
    }, 0);
  }

  onNameBlur(item: TreeNode): void {
    if (this.isSelectingItem) {
      this.isSelectingItem = false;
      return;
    }
    
    if (!item.name || item.name.trim() === '') {
      this.selectedItemIndex = -1;
      this.cdr.markForCheck();
      return;
    }
    
    const filteredItems = this.getFilteredItems(item.name);
    
    if (this.selectedItemIndex >= 0 && this.selectedItemIndex < filteredItems.length) {
      item.name = filteredItems[this.selectedItemIndex].name;
      if (this.itemType === 'role') {
        item.roleCode = filteredItems[this.selectedItemIndex].code;
      } else {
        item.objectCode = filteredItems[this.selectedItemIndex].code;
      }
    } else if (filteredItems.length > 0 && item.name !== filteredItems[0].name) {
      item.name = filteredItems[0].name;
      if (this.itemType === 'role') {
        item.roleCode = filteredItems[0].code;
      } else {
        item.objectCode = filteredItems[0].code;
      }
    }
    
    this.selectedItemIndex = -1;
    this.cdr.markForCheck();
  }

  selectItem(item: TreeNode, selectedItem: AvailableItem): void {
    item.name = selectedItem.name;
    item.code = item.code; // giữ nguyên virtual code
    if (this.itemType === 'role') {
      item.roleCode = selectedItem.code;
    } else {
      item.objectCode = selectedItem.code;
    }
    this.selectedItemIndex = -1;
    this.isSelectingItem = false;
    this.cdr.detectChanges();
  }

  getVisibleItems(): TreeNode[] {
    const result: TreeNode[] = [];
    
    const addItems = (items: TreeNode[], level: number = 0) => {
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

  hasChildren(item: TreeNode): boolean {
    return item.children !== undefined && item.children.length > 0;
  }

  onDragStart(event: DragEvent, item: TreeNode): void {
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

  onDragOverRow(event: DragEvent, item: TreeNode): void {
    event.preventDefault();
    if (event.dataTransfer) {
      event.dataTransfer.dropEffect = 'move';
    }

    const row = (event.target as HTMLElement).closest('.tree-row');
    if (row) {
      const rect = row.getBoundingClientRect();
      const relativeY = event.clientY - rect.top;
      const relativeX = event.clientX - rect.left;
      
      this.dropTargetId = item.id || null;

      const dragCellWidth = 50;
      const isDraggingOverDragCell = relativeX < dragCellWidth;

      if (isDraggingOverDragCell) {
        const isTopHalf = relativeY < rect.height / 2;
        this.dropZonePosition = isTopHalf ? 'top' : 'bottom';
      } else {
        this.dropZonePosition = 'inside';
      }
    }
  }

  onDragLeave(event: DragEvent): void {
    const row = (event.target as HTMLElement).closest('.tree-row');
    if (row && !row.contains(event.relatedTarget as Node)) {
      this.dropTargetId = null;
      this.dropZonePosition = null;
    }
  }

  onDragEnd(): void {
    if (this.draggedItem) {
      this.draggedItem.isDragging = false;
    }
    this.draggedItem = null;
    this.draggedFromParent = null;
  }

  onDrop(event: DragEvent, targetItem: TreeNode): void {
    event.preventDefault();
    
    if (!this.draggedItem) {
      return;
    }

    if (this.draggedItem.id === targetItem.id) {
      this.draggedItem.isDragging = false;
      this.draggedItem = null;
      this.dropTargetId = null;
      this.dropZonePosition = null;
      return;
    }

    if (this.isDescendantOf(this.draggedItem, targetItem)) {
      this.draggedItem.isDragging = false;
      this.draggedItem = null;
      this.dropTargetId = null;
      this.dropZonePosition = null;
      return;
    }

    this.removeItemFromTree(this.draggedItem);

    if (this.dropZonePosition === 'top') {
      this.insertItemBeforeTarget(targetItem);
    } else if (this.dropZonePosition === 'bottom') {
      this.insertItemAfterTarget(targetItem);
    } else if (this.dropZonePosition === 'inside') {
      this.insertItemAsChild(targetItem);
    }

    this.draggedItem.isDragging = false;
    this.draggedItem = null;
    this.dropTargetId = null;
    this.dropZonePosition = null;
    this.cdr.markForCheck();
  }

  private insertItemAsChild(targetItem: TreeNode): void {
    if (!this.draggedItem) return;

    if (!targetItem.children) {
      targetItem.children = [];
    }
    targetItem.children.unshift(this.draggedItem);
    targetItem.expanded = true;
  }

  private insertItemAfterTarget(targetItem: TreeNode): void {
    if (!this.draggedItem) return;

    const insertIntoArray = (items: TreeNode[]): boolean => {
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

  private insertItemBeforeTarget(targetItem: TreeNode): void {
    if (!this.draggedItem) return;

    const insertIntoArray = (items: TreeNode[]): boolean => {
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

  private isDescendantOf(parent: TreeNode, potentialChild: TreeNode): boolean {
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

  private removeItemFromTree(item: TreeNode): void {
    const remove = (items: TreeNode[]): boolean => {
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

  getItemTypeLabel(): string {
    return this.itemType === 'role' ? 'vai trò' : 'đối tượng';
  }

  getItemTypeLabelCapitalized(): string {
    return this.itemType === 'role' ? 'Vai trò' : 'Đối tượng';
  }
}

import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

export interface ClassificationItem {
  id?: string | number;
  code: string;
  name: string;
  shortName: string;
  scoreFrom: number;
  scoreTo: number;
  isDragging?: boolean;
}

@Component({
  selector: 'app-criteria-classification',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './criteria-classification.component.html',
  styleUrl: './criteria-classification.component.css',
  changeDetection: ChangeDetectionStrategy.Default
})
export class CriteriaClassificationComponent {
  @Input() classifications: ClassificationItem[] = [];
  @Output() classificationsChange = new EventEmitter<ClassificationItem[]>();
  @Output() addItem = new EventEmitter<void>();
  @Output() editItem = new EventEmitter<ClassificationItem>();
  @Output() deleteItem = new EventEmitter<string | number>();
  @Output() updateItem = new EventEmitter<ClassificationItem>();

  hoveredItemId: string | number | null = null;
  draggedItem: ClassificationItem | null = null;
  dropTargetId: string | number | null = null;
  dropZonePosition: 'top' | 'bottom' | null = null;
  maxAllowedValues: Map<string | number | undefined, number> = new Map();

  constructor(private cdr: ChangeDetectorRef) {}

  onAddClassification(): void {
    if (this.classifications.length === 0) {
      // Nếu không có item nào, tạo item đầu tiên từ 0-100
      const newItem: ClassificationItem = {
        id: `classification-${Date.now()}`,
        code: '',
        name: '',
        shortName: '',
        scoreFrom: 0,
        scoreTo: 100
      };
      this.classifications.push(newItem);
    } else {
      // Thực hiện như onInsertRowAfter cho phân loại cuối cùng
      const lastItem = this.classifications[this.classifications.length - 1];
      const midpoint = Math.floor((lastItem.scoreFrom + lastItem.scoreTo) / 2);
      
      const newItem: ClassificationItem = {
        id: `classification-${Date.now()}`,
        code: '',
        name: '',
        shortName: '',
        scoreFrom: midpoint + 1,
        scoreTo: lastItem.scoreTo
      };

      // Update last item's range
      lastItem.scoreTo = midpoint;

      this.classifications.push(newItem);
    }
    
    this.classificationsChange.emit(this.classifications);
    this.cdr.markForCheck();
  }

  finishEdit(item: ClassificationItem): void {
    // Validate and apply push logic
    this.validateScore(item);
    this.updateItem.emit(item);
    this.classificationsChange.emit(this.classifications);
    this.cdr.markForCheck();
  }

  validateScore(item: ClassificationItem): void {
    const currentIndex = this.classifications.indexOf(item);
    
    // Clamp scoreFrom and scoreTo to 0-100
    if (item.scoreFrom < 0) {
      item.scoreFrom = 0;
    } else if (item.scoreFrom > 100) {
      item.scoreFrom = 100;
    }

    if (item.scoreTo < 0) {
      item.scoreTo = 0;
    } else if (item.scoreTo > 100) {
      item.scoreTo = 100;
    }

    // Ensure scoreFrom <= scoreTo
    if (item.scoreFrom > item.scoreTo) {
      item.scoreTo = item.scoreFrom;
    }

    // Ensure scoreTo >= scoreFrom + 1 (at least 1 point difference)
    if (item.scoreTo < item.scoreFrom + 1) {
      item.scoreTo = item.scoreFrom + 1;
    }

    // Calculate max allowed value for this item
    const maxAllowed = this.calculateMaxAllowed(currentIndex);
    if (item.scoreTo > maxAllowed) {
      item.scoreTo = maxAllowed;
    }

    // Push scores to next items
    if (currentIndex > -1 && currentIndex < this.classifications.length - 1) {
      this.pushScoresToNextItems(currentIndex);
    }
  }

  private calculateMaxAllowed(itemIndex: number): number {
    // Calculate how many items are after this one
    const itemsAfter = this.classifications.length - itemIndex - 1;
    
    // Each item after needs at least 2 points (1 for scoreFrom, 1 for scoreTo difference)
    // So if there are N items after, we need at least N*2 points for them
    // Max for current item = 100 - (itemsAfter * 2)
    return 100 - (itemsAfter * 2);
  }

  getMaxAllowed(item: ClassificationItem): number {
    const currentIndex = this.classifications.indexOf(item);
    return this.calculateMaxAllowed(currentIndex);
  }

  onScoreToKeydown(event: KeyboardEvent, item: ClassificationItem): void {
    const maxAllowed = this.getMaxAllowed(item);
    
    // Allow: Backspace, Delete, Tab, Escape, Enter
    if ([8, 9, 27, 13, 46].indexOf(event.keyCode) !== -1 ||
        // Allow: Ctrl+C, Ctrl+V, Ctrl+X, Ctrl+A
        (event.keyCode === 65 && event.ctrlKey === true) ||
        (event.keyCode === 67 && event.ctrlKey === true) ||
        (event.keyCode === 86 && event.ctrlKey === true) ||
        (event.keyCode === 88 && event.ctrlKey === true) ||
        // Allow: home, end, left, right
        (event.keyCode >= 35 && event.keyCode <= 39)) {
      return;
    }
    
    // Prevent minus sign (keyCode 189 or 109)
    if (event.keyCode === 189 || event.keyCode === 109) {
      event.preventDefault();
      return;
    }
    
    // If already at max, prevent any number input
    if (item.scoreTo === maxAllowed) {
      // Allow only delete/backspace operations
      if (event.keyCode !== 8 && event.keyCode !== 46) {
        event.preventDefault();
      }
    }
  }

  onScoreToBlur(item: ClassificationItem): void {
    this.finishEdit(item);
    
    // Check if this is the last item and scoreTo is not 100
    const isLastItem = this.classifications[this.classifications.length - 1] === item;
    if (isLastItem && item.scoreTo !== 100) {
      // Auto add new item from (scoreTo + 1) to 100
      const newItem: ClassificationItem = {
        id: `classification-${Date.now()}`,
        code: '',
        name: '',
        shortName: '',
        scoreFrom: item.scoreTo + 1,
        scoreTo: 100
      };
      
      this.classifications.push(newItem);
      this.cdr.markForCheck();
    }
  }

  private pushScoresToNextItems(startIndex: number): void {
    for (let i = startIndex + 1; i < this.classifications.length; i++) {
      const prevItem = this.classifications[i - 1];
      const currentItem = this.classifications[i];
      
      // Set current item's scoreFrom to previous item's scoreTo + 1
      currentItem.scoreFrom = prevItem.scoreTo + 1;
      
      // Ensure current item's scoreTo is at least scoreFrom + 1
      if (currentItem.scoreTo < currentItem.scoreFrom + 1) {
        currentItem.scoreTo = currentItem.scoreFrom + 1;
      }
    }
  }



  onDelete(id: string | number | undefined): void {
    if (id !== undefined) {
      const index = this.classifications.findIndex(item => item.id === id);
      if (index > -1) {
        const deletedItem = this.classifications[index];
        const hasBefore = index > 0;
        const hasAfter = index < this.classifications.length - 1;
        
        if (hasBefore && hasAfter) {
          // Có cả phần trên và phần dưới: chia đôi
          const midpoint = Math.floor((deletedItem.scoreFrom + deletedItem.scoreTo) / 2);
          this.classifications[index - 1].scoreTo = midpoint;
          this.classifications[index + 1].scoreFrom = midpoint + 1;
        } else if (hasBefore) {
          // Chỉ có phần trên: gộp toàn bộ cho phần trên
          this.classifications[index - 1].scoreTo = deletedItem.scoreTo;
        } else if (hasAfter) {
          // Chỉ có phần dưới: gộp toàn bộ cho phần dưới
          this.classifications[index + 1].scoreFrom = deletedItem.scoreFrom;
        }
        
        // Remove the item
        this.classifications.splice(index, 1);
        this.classificationsChange.emit(this.classifications);
        this.cdr.markForCheck();
      }
      this.deleteItem.emit(id);
    }
  }

  onInsertRowBefore(item: ClassificationItem): void {
    // Calculate midpoint of current item's range
    const midpoint = Math.floor((item.scoreFrom + item.scoreTo) / 2);
    
    const newItem: ClassificationItem = {
      id: `classification-${Date.now()}`,
      code: '',
      name: '',
      shortName: '',
      scoreFrom: item.scoreFrom,
      scoreTo: midpoint
    };

    // Update current item's range
    item.scoreFrom = midpoint + 1;

    const index = this.classifications.indexOf(item);
    if (index > -1) {
      this.classifications.splice(index, 0, newItem);
    }
    this.classificationsChange.emit(this.classifications);
    this.cdr.markForCheck();
  }

  onInsertRowAfter(item: ClassificationItem): void {
    // Calculate midpoint of current item's range
    const midpoint = Math.floor((item.scoreFrom + item.scoreTo) / 2);
    
    const newItem: ClassificationItem = {
      id: `classification-${Date.now()}`,
      code: '',
      name: '',
      shortName: '',
      scoreFrom: midpoint + 1,
      scoreTo: item.scoreTo
    };

    // Update current item's range
    item.scoreTo = midpoint;

    const index = this.classifications.indexOf(item);
    if (index > -1) {
      this.classifications.splice(index + 1, 0, newItem);
    }
    this.classificationsChange.emit(this.classifications);
    this.cdr.markForCheck();
  }

  onDragStart(event: DragEvent, item: ClassificationItem): void {
    this.draggedItem = item;
    item.isDragging = true;
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

  onDragOverRow(event: DragEvent, item: ClassificationItem): void {
    event.preventDefault();
    if (event.dataTransfer) {
      event.dataTransfer.dropEffect = 'move';
    }

    const tr = (event.target as HTMLElement).closest('tr');
    if (tr) {
      const rect = tr.getBoundingClientRect();
      const relativeY = event.clientY - rect.top;
      
      this.dropTargetId = item.id || null;
      const isTopHalf = relativeY < rect.height / 2;
      this.dropZonePosition = isTopHalf ? 'top' : 'bottom';
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
    this.dropTargetId = null;
    this.dropZonePosition = null;
  }

  onDrop(event: DragEvent, targetItem: ClassificationItem): void {
    event.preventDefault();
    
    if (!this.draggedItem || this.draggedItem.id === targetItem.id) {
      this.draggedItem = null;
      this.dropTargetId = null;
      this.dropZonePosition = null;
      return;
    }

    const draggedIndex = this.classifications.indexOf(this.draggedItem);
    const targetIndex = this.classifications.indexOf(targetItem);

    if (draggedIndex > -1 && targetIndex > -1) {
      this.classifications.splice(draggedIndex, 1);
      
      if (this.dropZonePosition === 'top') {
        this.classifications.splice(targetIndex, 0, this.draggedItem);
      } else {
        this.classifications.splice(targetIndex + 1, 0, this.draggedItem);
      }
    }

    if (this.draggedItem) {
      this.draggedItem.isDragging = false;
    }
    this.draggedItem = null;
    this.dropTargetId = null;
    this.dropZonePosition = null;
    this.classificationsChange.emit(this.classifications);
    this.cdr.markForCheck();
  }
}

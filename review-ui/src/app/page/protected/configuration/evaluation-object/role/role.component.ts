import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { FilterComponent, FilterField, FilterCriteria } from '../../../../component/filter/filter.component';
import { TreeViewComponent, TreeNode } from '../../../../component/tree-view/tree-view.component';
import { EvaluationObjectRoleService, EvaluationObjectRoleDto } from '../configuration/evaluation-object-role.service';

@Component({
  selector: 'app-role',
  standalone: true,
  imports: [CommonModule, FilterComponent, TreeViewComponent],
  templateUrl: './role.component.html',
  styleUrls: ['./role.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RoleComponent implements OnInit {
  @ViewChild(TreeViewComponent) treeViewComponent!: TreeViewComponent;
  @ViewChild('filterComponent') filterComponent: any;

  filterFields: FilterField[] = [];
  treeData: TreeNode[] = [];
  evaluationObjectCode: string = '';
  loading = false;
  saving = false;
  roleData: EvaluationObjectRoleDto[] = [];
  headers: string[] = ['TÊN ĐƠN VỊ/ CÁ NHÂN'];
  evaluationObjectHeaders: any[] = [];
  
  // Track changes: {userId}_{evaluationObjectCode} -> true (added) or false (removed)
  changes: Map<string, boolean> = new Map();

  constructor(
    private roleService: EvaluationObjectRoleService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    // Load all role data as tree
    this.loadRoleData();
  }

  loadRoleData() {
    this.loading = true;
    // Call endpoint to get tree structure
    this.roleService.getAllAsTree().subscribe({
      next: (response: any) => {
        this.treeData = [response.data];
        this.evaluationObjectHeaders = response.headers || [];
        // Add evaluation object names as headers, but store codes for saving
        if (response.headers && response.headers.length > 0) {
          this.headers = ['TÊN ĐƠN VỊ/ CÁ NHÂN', ...response.headers.map((h: any) => h.name)];
        }
        // Apply changes to tree data
        this.applyChangesToTree();
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err: any) => {
        console.error('Lỗi khi tải dữ liệu vai trò:', err);
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  applyChangesToTree() {
    // Apply tracked changes to the tree
    const traverse = (nodes: TreeNode[]) => {
      nodes.forEach(node => {
        if (node.individuals) {
          node.individuals.forEach(individual => {
            if (!individual.selectedObjectIds) {
              individual.selectedObjectIds = [];
            }
            this.changes.forEach((isAdded, changeKey) => {
              const [userId, evalObjCode] = changeKey.split('_');
              if (individual.id.toString() === userId) {
                if (isAdded && !individual.selectedObjectIds!.includes(evalObjCode)) {
                  individual.selectedObjectIds!.push(evalObjCode);
                } else if (!isAdded && individual.selectedObjectIds!.includes(evalObjCode)) {
                  const index = individual.selectedObjectIds!.indexOf(evalObjCode);
                  individual.selectedObjectIds!.splice(index, 1);
                }
              }
            });
          });
        }
        if (node.children) {
          traverse(node.children);
        }
      });
    };
    traverse(this.treeData);
  }

  onFilterChange(criteria: FilterCriteria) {
    console.log('Filter criteria:', criteria);
    const search = criteria['search'] || '';
    this.performSearch(search);
  }

  performSearch(searchTerm?: string) {
    // Get search value from filter component if not provided
    if (!searchTerm && this.filterComponent) {
      searchTerm = this.filterComponent.getSearchValue?.() || '';
    }
    
    if (!searchTerm) {
      this.loadRoleData();
      return;
    }

    // Use a separate loading state for search
    this.loading = true;
    // Search with tree structure
    this.roleService.search(searchTerm).subscribe({
      next: (response: any) => {
        this.treeData = [response.data];
        this.evaluationObjectHeaders = response.headers || [];
        if (response.headers && response.headers.length > 0) {
          this.headers = ['TÊN ĐƠN VỊ/ CÁ NHÂN', ...response.headers.map((h: any) => h.name)];
        }
        // Apply changes to search results
        this.applyChangesToTree();
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err: any) => {
        console.error('Lỗi khi tìm kiếm:', err);
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  onChangeDetected(event: any) {
    const changeKey = `${event.userId}_${event.evaluationObjectCode}`;
    this.changes.set(changeKey, event.isAdded);
    console.log('Change tracked:', changeKey, event.isAdded);
  }

  onSave() {
    if (!this.treeViewComponent) {
      console.error('Tree view component not found');
      return;
    }

    this.saving = true;
    const selectedData = this.treeViewComponent.getSelectedData();
    
    // Convert to array of requests
    const requests: any[] = [];
    selectedData.forEach((evaluationObjectCodes, userId) => {
      evaluationObjectCodes.forEach(code => {
        requests.push({
          userId: userId,
          evaluationObjectCode: code
        });
      });
    });

    console.log('Saving role assignments:', requests);

    // Call backend to save
    if (requests.length === 0) {
      console.warn('No roles selected');
      this.saving = false;
      this.cdr.markForCheck();
      return;
    }

    // Call batch save endpoint
    this.roleService.batchSave(requests).subscribe({
      next: () => {
        console.log('Roles saved successfully');
        // Clear changes after successful save
        this.changes.clear();
        this.saving = false;
        this.cdr.markForCheck();
      },
      error: (err: any) => {
        console.error('Lỗi khi lưu vai trò:', err);
        this.saving = false;
        this.cdr.markForCheck();
      },
      complete: () => {
        this.saving = false;
        this.cdr.markForCheck();
      }
    });
  }
}

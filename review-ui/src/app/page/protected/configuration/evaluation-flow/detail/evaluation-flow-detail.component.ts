import { Component, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TableComponent, TableColumn, TableResponse } from '../../../../component/table/table.component';
import { EvaluationTreeComponent, TreeNode } from '../../../../component/evaluation-tree/evaluation-tree.component';
import { DropdownCheckboxMultiComponent } from '../../../../component/dropdown-checkbox-multi/dropdown-checkbox-multi.component';
import { DepartmentSelectionComponent, DepartmentNode } from '../../../../component/department-selection/department-selection.component';
import { EvaluationFlowService, EvaluationFlowDetailDto, CreateEvaluationFlowDetailRequest, UpdateEvaluationFlowDetailRequest, TreeNodeRequest, DepartmentRequest, AvailableItem } from '../evaluation-flow.service';

@Component({
  selector: 'app-evaluation-flow-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, TableComponent, EvaluationTreeComponent, DropdownCheckboxMultiComponent, DepartmentSelectionComponent],
  templateUrl: './evaluation-flow-detail.component.html',
  styleUrls: ['./evaluation-flow-detail.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EvaluationFlowDetailComponent {
  formGroup!: FormGroup;
  isEditMode = false;
  flowCode: string | null = null;
  showDepartmentModal = false;
  loading = false;
  
  departmentColumns: TableColumn[] = [
    { key: 'code', label: 'Mã đơn vị', textAlign: 'left', color: '#2563eb' },
    { key: 'name', label: 'Tên đơn vị', textAlign: 'left' }
  ];
  
  departmentTableResponse: TableResponse;
  roleTreeData: TreeNode[] = [];
  objectTreeData: TreeNode[] = [];
  departmentTreeData: DepartmentNode[] = [];

  roleAvailableItems: AvailableItem[] = [];
  objectAvailableItems: AvailableItem[] = [];

  criteriaOptions: any[] = [];
  selectedCriteria: (string | number)[] = [];

  constructor(
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef,
    private route: ActivatedRoute,
    private router: Router,
    private evaluationFlowService: EvaluationFlowService
  ) {
    this.departmentTableResponse = { data: [], pagination: { currentPage: 1, totalPages: 1, totalItems: 0, itemsPerPage: 10 } };
    this.initForm();
    this.loadCriteriaList();
    this.loadDepartmentTree();
    this.loadRoleList();
    this.loadObjectList();
    this.checkEditMode();
  }

  initForm() {
    this.formGroup = this.fb.group({
      code: ['', [Validators.required]],
      name: ['', [Validators.required]],
      status: ['Kích hoạt', [Validators.required]]
    });
  }

  loadCriteriaList() {
    this.evaluationFlowService.getCriteriaList().subscribe({
      next: (data) => {
        this.criteriaOptions = data.map(c => ({
          id: c.id,
          name: c.name
        }));
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error('Error loading criteria list:', err);
      }
    });
  }

  loadDepartmentTree() {
    this.evaluationFlowService.getDepartmentTree().subscribe({
      next: (data) => {
        console.log('Department tree data loaded:', data);
        // Map từ DepartmentTreeDto sang DepartmentNode
        this.departmentTreeData = this.mapDepartmentTreeData(data);
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error('Error loading department tree:', err);
      }
    });
  }

  private mapDepartmentTreeData(dtos: any[]): DepartmentNode[] {
    return dtos.map(dto => ({
      id: dto.id,
      code: dto.code,
      name: dto.name,
      checked: dto.checked || false,
      expanded: dto.expanded || false,
      children: dto.children && dto.children.length > 0 ? this.mapDepartmentTreeData(dto.children) : null
    }));
  }

  loadRoleList() {
    this.evaluationFlowService.getRoleList().subscribe({
      next: (data) => {
        console.log('Role list data loaded:', data);
        this.roleAvailableItems = data.map(r => ({ code: r.code, name: r.name }));
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error('Error loading role list:', err);
      }
    });
  }

  loadObjectList() {
    this.evaluationFlowService.getObjectList().subscribe({
      next: (data) => {
        console.log('Object list data loaded:', data);
        this.objectAvailableItems = data.map(o => ({ code: o.code, name: o.name }));
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error('Error loading object list:', err);
      }
    });
  }

  generateDepartmentTreeData(): DepartmentNode[] {
    return [
      {
        id: "G00",
        code: "G00",
        name: "Tất cả đơn vị",
        checked: false,
        expanded: true,
        children: [
          {
            id: "G01.000.000",
            code: "G01.000.000",
            name: "Bộ Công an",
            checked: false,
            expanded: true,
            children: [
              {
                id: "G01.501.000",
                code: "G01.501.000",
                name: "Văn Phòng Bộ Công an",
                checked: false,
                expanded: true,
                children: [
                  {
                    id: "G01.501.001.000",
                    code: "G01.501.001.000",
                    name: "Phòng 1",
                    checked: false,
                    expanded: false,
                    children: null
                  },
                  {
                    id: "G01.501.002.000",
                    code: "G01.501.002.000",
                    name: "Phòng 2",
                    checked: false,
                    expanded: false,
                    children: null
                  },
                  {
                    id: "G01.501.003.000",
                    code: "G01.501.003.000",
                    name: "Phòng 3",
                    checked: false,
                    expanded: false,
                    children: null
                  },
                  {
                    id: "G01.501.004.000",
                    code: "G01.501.004.000",
                    name: "Phòng 4",
                    checked: false,
                    expanded: false,
                    children: null
                  },
                  {
                    id: "G01.501.005.000",
                    code: "G01.501.005.000",
                    name: "Phòng 5",
                    checked: false,
                    expanded: false,
                    children: null
                  },
                  {
                    id: "G01.501.006.000",
                    code: "G01.501.006.000",
                    name: "Phòng 6",
                    checked: false,
                    expanded: false,
                    children: null
                  },
                  {
                    id: "G01.501.007.000",
                    code: "G01.501.007.000",
                    name: "Phòng 7",
                    checked: false,
                    expanded: false,
                    children: null
                  },
                  {
                    id: "G01.501.008.000",
                    code: "G01.501.008.000",
                    name: "Phòng 8",
                    checked: false,
                    expanded: true,
                    children: [
                      {
                        id: "G01.501.008.0001",
                        code: "G01.501.008.0001",
                        name: "Đội Kế hoạch",
                        checked: false,
                        expanded: false,
                        children: null
                      },
                      {
                        id: "G01.501.008.0002",
                        code: "G01.501.008.0002",
                        name: "Đội Kế toán, tài vụ",
                        checked: false,
                        expanded: false,
                        children: null
                      },
                      {
                        id: "G01.501.008.0003",
                        code: "G01.501.008.0003",
                        name: "Đội xe",
                        checked: false,
                        expanded: false,
                        children: null
                      },
                      {
                        id: "G01.501.008.0005",
                        code: "G01.501.008.0005",
                        name: "Đội Hậu cần phía nam",
                        checked: false,
                        expanded: false,
                        children: null
                      }
                    ]
                  },
                  {
                    id: "G01.501.012.000",
                    code: "G01.501.012.000",
                    name: "Trung tâm Thông tin chỉ huy",
                    checked: false,
                    expanded: true,
                    children: [
                      {
                        id: "G01.501.012.001",
                        code: "G01.501.012.001",
                        name: "Ban 1 - TTTTCH",
                        checked: false,
                        expanded: false,
                        children: null
                      },
                      {
                        id: "G01.501.012.002",
                        code: "G01.501.012.002",
                        name: "Ban 2 - TTTTCH",
                        checked: false,
                        expanded: false,
                        children: null
                      },
                      {
                        id: "G01.501.012.003",
                        code: "G01.501.012.003",
                        name: "Ban 3 - TTTTCH",
                        checked: false,
                        expanded: false,
                        children: null
                      },
                      {
                        id: "G01.501.012.004",
                        code: "G01.501.012.004",
                        name: "Ban 4 - TTTTCH",
                        checked: false,
                        expanded: false,
                        children: null
                      }
                    ]
                  }
                ]
              }
            ]
          }
        ]
      }
    ];
  }

  checkEditMode() {
    this.route.params.subscribe((params: any) => {
      if (params['code']) {
        this.isEditMode = true;
        this.flowCode = params['code'];
        this.loadFlowData(params['code']);
      }
      this.cdr.markForCheck();
    });
  }

  loadFlowData(code: string) {
    this.loading = true;
    this.evaluationFlowService.getDetail(code).subscribe({
      next: (data) => {
        this.formGroup.patchValue({
          code: data.code,
          name: data.name,
          status: data.isActive === 1 ? 'Kích hoạt' : 'Khóa'
        });

        this.departmentTableResponse = {
          data: data.departments,
          pagination: {
            currentPage: 1,
            totalPages: 1,
            totalItems: data.departments.length,
            itemsPerPage: 10
          }
        };

        this.roleTreeData = this.convertToTreeNodes(data.roles);
        this.objectTreeData = this.convertToTreeNodes(data.objects);
        this.selectedCriteria = data.criteria.map(c => parseInt(c, 10));

        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error('Error loading flow data:', err);
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  private convertToTreeNodes(treeRequests: TreeNodeRequest[]): TreeNode[] {
    return treeRequests.map((req, index) => this.convertTreeNode(req, index + 1));
  }

  private convertTreeNode(req: TreeNodeRequest, index: number): TreeNode {
    return {
      id: `node-${Date.now()}-${Math.random()}`,
      code: req.code,
      name: req.name,
      roleCode: req.roleCode,
      objectCode: req.objectCode,
      expanded: true,
      children: req.children ? req.children.map((child, idx) => this.convertTreeNode(child, idx + 1)) : []
    };
  }

  onSubmit() {
    if (this.formGroup.valid) {
      const request = this.buildRequest();
      
      if (this.isEditMode && this.flowCode) {
        this.evaluationFlowService.updateDetail(this.flowCode, request as UpdateEvaluationFlowDetailRequest).subscribe({
          next: () => {
            console.log('Flow updated successfully');
            this.router.navigate(['/configuration/evaluation-flow']);
          },
          error: (err) => {
            console.error('Error updating flow:', err);
          }
        });
      } else {
        this.evaluationFlowService.createDetail(request as CreateEvaluationFlowDetailRequest).subscribe({
          next: () => {
            console.log('Flow created successfully');
            this.router.navigate(['/configuration/evaluation-flow']);
          },
          error: (err) => {
            console.error('Error creating flow:', err);
          }
        });
      }
    }
  }

  private buildRequest(): CreateEvaluationFlowDetailRequest | UpdateEvaluationFlowDetailRequest {
    const departments: string[] = this.departmentTableResponse.data.map(d => d.code);

    const roles = this.convertTreeNodesToRoleNodeDtos(this.roleTreeData);
    const objects = this.convertTreeNodesToObjectNodeDtos(this.objectTreeData);

    const baseRequest = {
      name: this.formGroup.get('name')?.value,
      departments,
      roles,
      objects,
      criteria: this.selectedCriteria.map(c => c.toString())
    };

    if (this.isEditMode) {
      return {
        ...baseRequest,
        isActive: this.formGroup.get('status')?.value === 'Kích hoạt' ? 1 : 0
      } as UpdateEvaluationFlowDetailRequest;
    } else {
      return {
        code: this.formGroup.get('code')?.value,
        ...baseRequest
      } as CreateEvaluationFlowDetailRequest;
    }
  }

  private convertTreeNodesToRoleNodeDtos(nodes: TreeNode[]): any[] {
    return nodes.map(node => ({
      id: node.id,
      code: node.code,
      roleCode: node.roleCode,
      name: node.name,
      children: node.children && node.children.length > 0 ? this.convertTreeNodesToRoleNodeDtos(node.children) : []
    }));
  }

  private convertTreeNodesToObjectNodeDtos(nodes: TreeNode[]): any[] {
    return nodes.map(node => ({
      id: node.id,
      code: node.code,
      objectCode: node.objectCode,
      name: node.name,
      children: node.children && node.children.length > 0 ? this.convertTreeNodesToObjectNodeDtos(node.children) : []
    }));
  }

  private convertTreeNodesToRequests(nodes: TreeNode[]): TreeNodeRequest[] {
    return nodes.map(node => ({
      code: node.code,
      name: node.name,
      children: node.children && node.children.length > 0 ? this.convertTreeNodesToRequests(node.children) : undefined
    }));
  }

  onCancel() {
    this.router.navigate(['/configuration/evaluation-flow']);
  }

  onDelete() {
    if (confirm('Bạn có chắc chắn muốn xóa luồng này không?')) {
      if (this.flowCode) {
        this.evaluationFlowService.delete(this.flowCode).subscribe({
          next: () => {
            console.log('Flow deleted successfully');
            this.router.navigate(['/configuration/evaluation-flow']);
          },
          error: (err) => {
            console.error('Error deleting flow:', err);
          }
        });
      }
    }
  }

  onAddRole(node: TreeNode) {
    const childIndex = (node.children?.length || 0) + 1;
    const newCode = node.code ? `${node.code}.${childIndex}` : `${childIndex}`;
    
    const newRole: TreeNode = {
      id: `role-${Date.now()}`,
      code: newCode,
      name: '',
      expanded: false,
      children: []
    };
    
    if (!node.children) {
      node.children = [];
    }
    node.children.push(newRole);
    node.expanded = true;
    
    this.cdr.detectChanges();
  }

  onAddRootRole() {
    const newRootIndex = this.roleTreeData.length + 1;
    const newRole: TreeNode = {
      id: `role-${Date.now()}`,
      code: `${newRootIndex}`,
      name: '',
      expanded: false,
      children: []
    };
    this.roleTreeData = [...this.roleTreeData, newRole];
    this.cdr.detectChanges();
  }

  onDeleteRole(nodeId: string) {
    this.deleteNodeFromTree(this.roleTreeData, nodeId);
    this.cdr.detectChanges();
  }

  onAddObject(node: TreeNode) {
    const childIndex = (node.children?.length || 0) + 1;
    const newCode = node.code ? `${node.code}.${childIndex}` : `${childIndex}`;
    
    const newObject: TreeNode = {
      id: `obj-${Date.now()}`,
      code: newCode,
      name: '',
      expanded: false,
      children: []
    };
    
    if (!node.children) {
      node.children = [];
    }
    node.children.push(newObject);
    node.expanded = true;
    
    this.cdr.detectChanges();
  }

  onAddRootObject() {
    const newRootIndex = this.objectTreeData.length + 1;
    const newObject: TreeNode = {
      id: `obj-${Date.now()}`,
      code: `${newRootIndex}`,
      name: '',
      expanded: false,
      children: []
    };
    this.objectTreeData = [...this.objectTreeData, newObject];
    this.cdr.detectChanges();
  }

  onDeleteObject(nodeId: string) {
    this.deleteObjectFromTree(this.objectTreeData, nodeId);
    this.cdr.detectChanges();
  }

  onToggleObjectNode(nodeId: string) {
    this.toggleObjectNodeInTree(this.objectTreeData, nodeId);
    this.cdr.detectChanges();
  }

  onHandleObjectDrop(event: any) {
    if (!event || !event.draggedNode || !event.targetNode) return;

    const draggedNode = event.draggedNode;
    const targetNode = event.targetNode;

    this.removeObjectFromTree(this.objectTreeData, draggedNode.id);

    if (!targetNode.children) {
      targetNode.children = [];
    }
    targetNode.children.push(draggedNode);
    targetNode.expanded = true;

    this.updateAllObjectCodes(this.objectTreeData);

    this.objectTreeData = [...this.objectTreeData];
    this.cdr.detectChanges();
  }

  private removeObjectFromTree(nodes: TreeNode[], nodeId: string): boolean {
    for (let i = 0; i < nodes.length; i++) {
      if (nodes[i].id === nodeId) {
        nodes.splice(i, 1);
        return true;
      }
      if (nodes[i].children && nodes[i].children!.length > 0 && this.removeObjectFromTree(nodes[i].children!, nodeId)) {
        return true;
      }
    }
    return false;
  }

  private updateAllObjectCodes(nodes: TreeNode[], parentCode: string = '') {
    nodes.forEach((node, index) => {
      const newCode = parentCode ? `${parentCode}.${index + 1}` : `${index + 1}`;
      node.code = newCode;
      if (node.children && node.children.length > 0) {
        this.updateAllObjectCodes(node.children, newCode);
      }
    });
  }

  private toggleObjectNodeInTree(nodes: TreeNode[], nodeId: string): boolean {
    for (let i = 0; i < nodes.length; i++) {
      if (nodes[i].id === nodeId) {
        nodes[i].expanded = !nodes[i].expanded;
        return true;
      }
      if (nodes[i].children && nodes[i].children!.length > 0) {
        if (this.toggleObjectNodeInTree(nodes[i].children!, nodeId)) {
          return true;
        }
      }
    }
    return false;
  }

  private deleteObjectFromTree(nodes: TreeNode[], nodeId: string): boolean {
    for (let i = 0; i < nodes.length; i++) {
      if (nodes[i].id === nodeId) {
        nodes.splice(i, 1);
        return true;
      }
      if (nodes[i].children && nodes[i].children!.length > 0) {
        if (this.deleteObjectFromTree(nodes[i].children!, nodeId)) {
          return true;
        }
      }
    }
    return false;
  }

  onOpenDepartmentModal() {
    this.showDepartmentModal = true;
    this.cdr.detectChanges();
  }

  onCloseDepartmentModal() {
    this.showDepartmentModal = false;
    this.cdr.detectChanges();
  }

  onConfirmDepartmentSelection(selected: DepartmentNode[]): void {
    const tableData = selected.map(dept => ({
      code: dept.code,
      name: dept.name
    }));
    
    this.departmentTableResponse = {
      ...this.departmentTableResponse,
      data: tableData,
      pagination: {
        ...this.departmentTableResponse.pagination!,
        totalItems: tableData.length
      }
    };
    
    this.showDepartmentModal = false;
    this.cdr.detectChanges();
  }

  onCriteriaChange(criteria: (string | number)[]): void {
    this.selectedCriteria = criteria;
  }

  onToggleNode(nodeId: string) {
    this.toggleNodeInTree(this.roleTreeData, nodeId);
    this.cdr.detectChanges();
  }

  onHandleDrop(event: any) {
    if (!event || !event.draggedNode || !event.targetNode) return;

    const draggedNode = event.draggedNode;
    const targetNode = event.targetNode;

    this.removeNodeFromTree(this.roleTreeData, draggedNode.id);

    if (!targetNode.children) {
      targetNode.children = [];
    }
    targetNode.children.push(draggedNode);
    targetNode.expanded = true;

    this.updateAllCodes(this.roleTreeData);

    this.roleTreeData = [...this.roleTreeData];
    this.cdr.detectChanges();
  }

  private removeNodeFromTree(nodes: TreeNode[], nodeId: string): boolean {
    for (let i = 0; i < nodes.length; i++) {
      if (nodes[i].id === nodeId) {
        nodes.splice(i, 1);
        return true;
      }
      if (nodes[i].children && nodes[i].children!.length > 0 && this.removeNodeFromTree(nodes[i].children!, nodeId)) {
        return true;
      }
    }
    return false;
  }

  private updateAllCodes(nodes: TreeNode[], parentCode: string = '') {
    nodes.forEach((node, index) => {
      const newCode = parentCode ? `${parentCode}.${index + 1}` : `${index + 1}`;
      node.code = newCode;
      if (node.children && node.children.length > 0) {
        this.updateAllCodes(node.children, newCode);
      }
    });
  }

  onHandleDragEnd() {
    // Handle drag end
  }

  private toggleNodeInTree(nodes: TreeNode[], nodeId: string): boolean {
    for (let i = 0; i < nodes.length; i++) {
      if (nodes[i].id === nodeId) {
        nodes[i].expanded = !nodes[i].expanded;
        return true;
      }
      if (nodes[i].children && nodes[i].children!.length > 0) {
        if (this.toggleNodeInTree(nodes[i].children!, nodeId)) {
          return true;
        }
      }
    }
    return false;
  }

  private deleteNodeFromTree(nodes: TreeNode[], nodeId: string): boolean {
    for (let i = 0; i < nodes.length; i++) {
      if (nodes[i].id === nodeId) {
        nodes.splice(i, 1);
        return true;
      }
      if (nodes[i].children && nodes[i].children!.length > 0) {
        if (this.deleteNodeFromTree(nodes[i].children!, nodeId)) {
          return true;
        }
      }
    }
    return false;
  }
}

import { Component, ChangeDetectionStrategy, ChangeDetectorRef, ViewChild, ElementRef, HostListener, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { DropdownSelectComponent, DropdownOption } from '../../../component/dropdown-select/dropdown-select.component';
import { EmojiPickerComponent } from '../../../component/emoji-picker/emoji-picker.component';
import { CommentSectionComponent } from '../../../component/comment-section/comment-section.component';
import { EvaluationBoardService, EvaluationDetail, CriteriaNode as CriteriaNodeModel, ScoreInput, NewEvaluationTemplate, CommentItem, ManagerUser } from '../evaluation-board.service';
import { ToastService } from '../../../../services/toast.service';
import { AuthService } from '../../../auth/services/auth.service';

@Component({
  selector: 'app-evaluation-board-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, DropdownSelectComponent, EmojiPickerComponent, CommentSectionComponent],
  templateUrl: './evaluation-board-detail.component.html',
  styleUrls: ['./evaluation-board-detail.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EvaluationBoardDetailComponent implements AfterViewChecked {
  @ViewChild('imageCanvas') imageCanvasRef!: ElementRef<HTMLCanvasElement>;

  isEditMode = false;
  evaluationType: 'self' | 'manager' | 'result' = 'self';
  evaluationId: string | null = null;

  // Make Math available in template
  Math = Math;

  // Filter properties
  filterName: string = '';
  filterPeriod: string | number = '';
  filterYear: string | number = '';
  filterDepartment: string = '';

  // Evaluation data
  evaluationData: EvaluationDetail | null = null;
  flattenedCriteria: any[] = [];
  comments: CommentItem[] = [];
  selfScores: { [key: string]: number } = {};
  managerScores: { [key: string]: number | null } = {};
  criteriaSetName: string = '';

  // New evaluation - template loaded from API
  newTemplate: NewEvaluationTemplate | null = null;
  
  // Send to manager modal
  showManagerModal: boolean = false;
  managerList: DropdownOption[] = [];
  selectedManagerId: string | null = null;
  managerSearchQuery: string = '';
  isLoadingManagers: boolean = false;

  // Image editor properties
  showImageEditor: boolean = false;  editingImageIndex: number = -1;
  imageBrightness: number = 100;
  imageContrast: number = 100;
  imageSaturation: number = 100;
  imageRotation: number = 0;
  originalImageUrl: string = '';
  imageCanvas: HTMLCanvasElement | null = null;
  editorMode: 'adjust' | 'crop' | 'draw' = 'adjust';
  
  // Crop properties
  cropStartX: number = 0;
  cropStartY: number = 0;
  cropEndX: number = 0;
  cropEndY: number = 0;
  isCropping: boolean = false;
  
  // Draw properties
  drawColor: string = '#000000';
  drawSize: number = 3;
  isDrawing: boolean = false;
  drawStartX: number = 0;
  drawStartY: number = 0;
  drawingCanvas: HTMLCanvasElement | null = null;
  drawTool: 'pen' | 'line' | 'rectangle' | 'circle' | 'arrow' = 'pen';
  previewCanvas: HTMLCanvasElement | null = null;
  savedDrawingImageData: ImageData | null = null;
  drawingLayerCanvas: HTMLCanvasElement | null = null;

  periodOptions = [
    { id: '1', name: 'Tháng 1' },
    { id: '2', name: 'Tháng 2' },
    { id: '3', name: 'Tháng 3' },
    { id: '4', name: 'Tháng 4' },
    { id: '5', name: 'Tháng 5' },
    { id: '6', name: 'Tháng 6' },
    { id: '7', name: 'Tháng 7' },
    { id: '8', name: 'Tháng 8' },
    { id: '9', name: 'Tháng 9' },
    { id: '10', name: 'Tháng 10' },
    { id: '11', name: 'Tháng 11' },
    { id: '12', name: 'Tháng 12' }
  ];

  yearOptions = this.generateYearOptions();

  statusOptions = [
    { label: 'Tất cả trạng thái', value: '' },
    { label: 'Dự thảo', value: 'draft' },
    { label: 'Chờ đánh giá', value: 'pending' },
    { label: 'Hoàn thành', value: 'completed' }
  ];

  get currentYear(): number {
    return new Date().getFullYear();
  }

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private elementRef: ElementRef,
    private evaluationService: EvaluationBoardService,
    private toast: ToastService,
    private authService: AuthService
  ) {
    this.checkMode();
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    // Removed - moved to comment-section component
  }

  ngAfterViewChecked() {
    // Removed - moved to comment-section component
  }

  initializeSampleData() {
    // kept for compatibility
  }

  loadEvaluationData(id: string) {
    const loader = this.evaluationType === 'manager'
      ? this.evaluationService.getManagerDetail(id)
      : this.evaluationType === 'result'
        ? this.evaluationService.getResultDetail(id)
        : this.evaluationService.getSelfDetail(id);

    loader.subscribe({
      next: (data) => {
        this.evaluationData = data;
        this.selfScores = {};
        this.managerScores = {};
        data.scores.forEach(s => {
          this.selfScores[s.virtualCode] = s.selfScore;
          this.managerScores[s.virtualCode] = s.managerScore ?? null;
        });
        this.calcParentScores(data.criteriaTree);
        this.flattenedCriteria = this.flattenCriteriaTree(data.criteriaTree);
        this.filterName = data.fullName;
        this.filterDepartment = data.department;
        this.filterPeriod = data.month.toString();
        this.filterYear = data.year.toString();
        this.criteriaSetName = '';
        this.loadComments(id);
        this.cdr.markForCheck();
      },
      error: (err) => console.error('Lỗi tải phiếu đánh giá:', err)
    });
  }

  loadComments(evaluationId: string) {
    this.evaluationService.getComments(evaluationId).subscribe({
      next: (comments) => {
        this.comments = comments;
        this.cdr.markForCheck();
      },
      error: (err) => console.error('Lỗi tải bình luận:', err)
    });
  }

  onSendComment(data: { content: string; replyToCommentId?: string; files?: File[] }) {
    if (!this.evaluationId) return;
    this.evaluationService.addComment({ evaluationId: this.evaluationId!, content: data.content, replyToCommentId: data.replyToCommentId }).subscribe({
      next: async (comment) => {
        if (data.files?.length) {
          for (const f of data.files) {
            try {
              await this.evaluationService.addCommentFile(comment.id, f).toPromise();
            } catch (e) {
              console.error('Lỗi upload file:', e);
            }
          }
        }
        this.loadComments(this.evaluationId!);
      },
      error: (err) => console.error('Lỗi gửi bình luận:', err)
    });
  }

  onDeleteApiComment(commentId: string) {
    if (!this.evaluationId) return;
    this.evaluationService.deleteComment(commentId).subscribe({
      next: () => this.loadComments(this.evaluationId!),
      error: (err) => console.error('Lỗi xóa bình luận:', err)
    });
  }

  onReactionAdded(event: { commentIndex: number; commentId?: string; emoji: string }) {
    if (!this.evaluationId || !event.commentId) return;
    const userId = this.authService.getUser()?.id;
    if (!userId) return;
    this.evaluationService.addReaction(event.commentId, event.emoji, userId).subscribe({
      next: () => this.loadComments(this.evaluationId!),
      error: (err) => console.error('Lỗi thêm reaction:', err)
    });
  }

  onReactionRemoved(event: { commentIndex: number; commentId?: string; userName: string }) {
    if (!this.evaluationId || !event.commentId) return;
    const userId = this.authService.getUser()?.id;
    if (!userId) return;
    this.evaluationService.deleteReaction(event.commentId, userId).subscribe({
      next: () => this.loadComments(this.evaluationId!),
      error: (err) => console.error('Lỗi xóa reaction:', err)
    });
  }

  private calcParentScores(tree: CriteriaNodeModel[]): void {
    tree.forEach(node => {
      if (node.children && node.children.length > 0) {
        this.calcParentScores(node.children);

        const calcScore = (getScore: (n: CriteriaNodeModel) => number | null): number | null => {
          let hasAny = false;
          let total = 0;
          node.children.forEach(child => {
            const s = getScore(child);
            if (s !== null && s !== undefined) {
              hasAny = true;
              const isDeduction = child.scoreType?.toLowerCase().includes('trừ') || child.scoreType?.toLowerCase().includes('deduct');
              total += isDeduction ? -s : s;
            }
          });
          return hasAny ? total : null;
        };

        node.selfScore = calcScore(c => c.selfScore ?? this.selfScores[c.virtualCode] ?? null);
        node.managerScore = calcScore(c => c.managerScore ?? this.managerScores[c.virtualCode] ?? null);
      }
    });
  }

  flattenCriteriaTree(tree: CriteriaNodeModel[], parent: any = null): any[] {
    let result: any[] = [];
    tree.forEach(node => {
      const item: any = {
        virtualCode: node.virtualCode,
        code: node.displayCode ?? '',
        name: node.content,
        maxScore: node.maxScore,
        selfScore: node.selfScore ?? this.selfScores[node.virtualCode] ?? null,
        managerScore: node.managerScore ?? this.managerScores[node.virtualCode] ?? null,
        isParent: node.children && node.children.length > 0,
        hasChildren: node.children && node.children.length > 0,
        level: parent ? (parent.level || 0) + 1 : 0,
        isDeduction: node.scoreType?.toLowerCase().includes('trừ') || node.scoreType?.toLowerCase().includes('deduct'),
        directChildren: [] as any[]
      };
      if (parent) parent.directChildren.push(item);
      result.push(item);
      if (node.children && node.children.length > 0) {
        result = result.concat(this.flattenCriteriaTree(node.children, item));
      }
    });
    return result;
  }

  get totalSelfScore(): number | null {
    const roots = this.flattenedCriteria.filter(c => c.level === 0);
    let hasAny = false, total = 0;
    roots.forEach(r => {
      const s = this.calcScore(r, 'selfScore');
      if (s !== null && s !== undefined) { hasAny = true; total += r.isDeduction ? -s : s; }
    });
    return hasAny ? total : null;
  }

  get totalManagerScore(): number | null {
    const roots = this.flattenedCriteria.filter(c => c.level === 0);
    let hasAny = false, total = 0;
    roots.forEach(r => {
      const s = this.calcScore(r, 'managerScore');
      if (s !== null && s !== undefined) { hasAny = true; total += r.isDeduction ? -s : s; }
    });
    return hasAny ? total : null;
  }

  isInRange(score: number | null, min: number | null, max: number | null): boolean {
    if (score === null || min === null || max === null) return false;
    return score >= min && score <= max;
  }

  onScoreKeydown(e: KeyboardEvent, value: number | null, max: number | null): void {
    const allowed = ['Backspace', 'Delete', 'Tab', 'ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown'];
    if (allowed.includes(e.key)) return;
    if (!/^\d$/.test(e.key)) { e.preventDefault(); return; }
    if ((value ?? 0) >= (max ?? 100)) e.preventDefault();
  }

  calcScore(item: any, field: 'selfScore' | 'managerScore'): number | null {
    if (!item.hasChildren) return item[field];
    let hasAny = false;
    let total = 0;
    for (const child of item.directChildren) {
      const s = this.calcScore(child, field);
      if (s !== null && s !== undefined) {
        hasAny = true;
        total += child.isDeduction ? -s : s;
      }
    }
    if (!hasAny) return null;
    const max = item.maxScore ?? Infinity;
    return Math.min(Math.max(total, 0), max);
  }

  checkMode() {
    this.route.params.subscribe(params => {
      this.evaluationType = params['type'] || 'self';
      this.evaluationId = params['id'];
      this.isEditMode = params['id'] && params['id'] !== 'new';
      if (this.isEditMode && this.evaluationId) {
        this.loadEvaluationData(this.evaluationId!);
      } else if (this.evaluationId === 'new') {
        this.loadNewTemplate();
      }
      this.cdr.markForCheck();
    });
  }

  loadNewTemplate(month?: number, year?: number, prevPeriod?: string | number, prevYear?: string | number) {
    const criteriaId = this.newTemplate?.criteriaSetId ?? undefined;
    this.evaluationService.getNewTemplate(month, year, criteriaId).subscribe({
      next: (template) => {
        if (!template.isChanged) {
          if (!template.isHaveCriteria) {
            // Revert dropdown về giá trị trước
            if (prevPeriod !== undefined) this.filterPeriod = prevPeriod;
            if (prevYear !== undefined) this.filterYear = prevYear;
            this.toast.warning(`Không có tiêu chí cho tháng ${month}/${year}`);
            this.cdr.markForCheck();
          }
          return;
        }
        this.newTemplate = template;
        this.filterName = template.fullName;
        this.filterDepartment = template.department;
        this.filterPeriod = template.currentMonth.toString();
        this.filterYear = template.currentYear.toString();
        this.criteriaSetName = template.criteriaSetName;
        this.selfScores = {};
        this.managerScores = {};
        this.calcParentScores(template.criteriaTree);
        this.flattenedCriteria = this.flattenCriteriaTree(template.criteriaTree);
        this.cdr.markForCheck();
      },
      error: (err) => console.error('Lỗi tải mẫu đánh giá:', err)
    });
  }

  getTitle(): string {
    if (this.evaluationId === 'new') {
      return 'Thêm mới đánh giá';
    } else if (this.isEditMode) {
      return 'Chỉnh sửa đánh giá';
    } else {
      return 'Chi tiết phiếu đánh giá';
    }
  }

  onBack() {
    this.router.navigate(['/evaluation-board', this.evaluationType]);
  }

  onCancel() {
    this.router.navigate(['/evaluation-board', this.evaluationType]);
  }

  onDelete() {
    if (!this.evaluationId) return;
    if (confirm('Bạn có chắc chắn muốn xóa phiếu đánh giá này không?')) {
      this.evaluationService.deleteSelf(this.evaluationId!).subscribe({
        next: () => this.router.navigate(['/evaluation-board', this.evaluationType]),
        error: (err) => console.error('Lỗi xóa:', err)
      });
    }
  }

  onSave() {
    const scores: ScoreInput[] = this.flattenedCriteria
      .filter(c => !c.hasChildren)
      .map(c => ({ virtualCode: c.virtualCode, selfScore: c.selfScore ?? 0 }));

    if (this.evaluationId === 'new') {
      if (!this.newTemplate || !this.filterPeriod || !this.filterYear) {
        alert('Vui lòng chọn bộ tiêu chí, kỳ và năm đánh giá');
        return;
      }
      this.evaluationService.createSelf({
        month: parseInt(this.filterPeriod.toString()),
        year: parseInt(this.filterYear.toString()),
        criteriaSetId: this.newTemplate.criteriaSetId,
        scores
      }).subscribe({
        next: (data) => {
          this.router.navigate(['/evaluation-board/self', 'detail', data.id]);
        },
        error: (err) => console.error('Lỗi tạo phiếu:', err)
      });
    } else if (this.evaluationId) {
      this.evaluationService.updateSelf(this.evaluationId!, scores).subscribe({
        next: () => alert('Đã lưu phiếu đánh giá'),
        error: (err) => console.error('Lỗi lưu:', err)
      });
    }
  }

  onReview() {
    if (!this.evaluationId) return;
    this.evaluationService.reviewEvaluation(this.evaluationId!).subscribe({
      next: () => this.router.navigate(['/evaluation-board', this.evaluationType]),
      error: (err) => console.error('Lỗi trả về:', err)
    });
  }

  onApprove() {
    if (!this.evaluationId) return;
    const scores = this.flattenedCriteria
      .filter(c => !c.hasChildren)
      .map(c => ({ virtualCode: c.virtualCode, managerScore: c.managerScore }));

    const isPendingDirector = this.evaluationData?.status === 'pending_director';
    const call = isPendingDirector
      ? this.evaluationService.updateEvaluationScores(this.evaluationId!, scores)
      : this.evaluationService.approveEvaluation(this.evaluationId!, scores);

    call.subscribe({
      next: () => { this.toast.success('Đã lưu phiếu đánh giá'); },
      error: (err) => console.error('Lỗi lưu:', err)
    });
  }

  onSendToManager() {
    if (!this.evaluationId) return;
    this.selectedManagerId = null;
    this.managerSearchQuery = '';
    const criteriaSetId = this.evaluationData?.criteriaSetId ?? this.newTemplate?.criteriaSetId;
    if (!criteriaSetId) return;
    this.isLoadingManagers = true;
    this.evaluationService.getManagers(criteriaSetId).subscribe({
      next: (managers) => {
        this.isLoadingManagers = false;
        if (managers.length === 0) {
          this.toast.warning('Không có lãnh đạo phù hợp để gửi phiếu đánh giá.');
          this.cdr.markForCheck();
          return;
        }
        this.managerList = managers.map(m => ({ ...m, name: m.fullName }));
        this.showManagerModal = true;
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error('Lỗi tải danh sách lãnh đạo:', err);
        this.isLoadingManagers = false;
        this.toast.error('Lỗi tải danh sách lãnh đạo.');
        this.cdr.markForCheck();
      }
    });
  }

  onConfirmSendToManager() {
    if (!this.selectedManagerId) return;

    if (this.evaluationId === 'new') {
      if (!this.newTemplate || !this.filterPeriod || !this.filterYear) return;
      const scores: ScoreInput[] = this.flattenedCriteria
        .filter(c => !c.hasChildren)
        .map(c => ({ virtualCode: c.virtualCode, selfScore: c.selfScore ?? 0 }));
      this.evaluationService.saveAndSubmit({
        month: parseInt(this.filterPeriod.toString()),
        year: parseInt(this.filterYear.toString()),
        criteriaSetId: this.newTemplate.criteriaSetId,
        scores,
        managerId: this.selectedManagerId
      }).subscribe({
        next: () => {
          this.showManagerModal = false;
          this.router.navigate(['/evaluation-board', this.evaluationType]);
        },
        error: (err) => console.error('Lỗi nộp phiếu:', err)
      });
    } else {
      if (!this.evaluationId) return;
      this.evaluationService.submitSelf(this.evaluationId!, this.selectedManagerId).subscribe({
        next: () => {
          this.showManagerModal = false;
          this.router.navigate(['/evaluation-board', this.evaluationType]);
        },
        error: (err) => console.error('Lỗi nộp phiếu:', err)
      });
    }
  }

  onCloseManagerModal() {
    this.showManagerModal = false;
  }

  onComplete() {
    if (!this.evaluationId) return;
    this.evaluationService.completeEvaluation(this.evaluationId!).subscribe({
      next: () => this.router.navigate(['/evaluation-board', this.evaluationType]),
      error: (err) => console.error('Lỗi hoàn thành:', err)
    });
  }

  onRecall() {
    if (!this.evaluationId) return;
    this.evaluationService.recallSelf(this.evaluationId!).subscribe({
      next: () => {
        this.toast.success('Đã thu hồi phiếu đánh giá');
        this.loadEvaluationData(this.evaluationId!);
      },
      error: (err) => console.error('Lỗi thu hồi:', err)
    });
  }

  private generateYearOptions() {
    const currentYear = new Date().getFullYear();
    const options = [];
    
    for (let year = currentYear; year >= 1970; year--) {
      options.push({ id: year.toString(), name: year.toString() });
    }
    
    return options;
  }

  onFilterPeriodChange(value: string | number) {
    const prev = this.filterPeriod;
    this.filterPeriod = value;
    if (this.evaluationId === 'new') {
      const month = value ? parseInt(value.toString()) : undefined;
      const year = this.filterYear ? parseInt(this.filterYear.toString()) : undefined;
      this.loadNewTemplate(month, year, prev, this.filterYear);
    }
  }

  onFilterYearChange(value: string | number) {
    const prev = this.filterYear;
    this.filterYear = value;
    if (this.evaluationId === 'new') {
      const month = this.filterPeriod ? parseInt(this.filterPeriod.toString()) : undefined;
      const year = value ? parseInt(value.toString()) : undefined;
      this.loadNewTemplate(month, year, this.filterPeriod, prev);
    }
  }

  editImage(index: number) {
    // Removed - moved to comment-section component
  }

  loadImageToCanvas() {
    const canvas = this.imageCanvasRef?.nativeElement;
    if (!canvas) return;

    const ctx = canvas.getContext('2d', { willReadFrequently: true });
    if (!ctx) return;

    const img = new Image();
    img.onload = () => {
      canvas.width = img.width;
      canvas.height = img.height;

      // Create drawing layer canvas with willReadFrequently
      if (!this.drawingLayerCanvas) {
        this.drawingLayerCanvas = document.createElement('canvas');
        this.drawingLayerCanvas.width = canvas.width;
        this.drawingLayerCanvas.height = canvas.height;
        // Get context with willReadFrequently to avoid warnings
        const drawCtx = this.drawingLayerCanvas.getContext('2d', { willReadFrequently: true });
        if (drawCtx) {
          // Initialize with transparent background
          drawCtx.clearRect(0, 0, this.drawingLayerCanvas.width, this.drawingLayerCanvas.height);
        }
      }

      this.updateImagePreview();
    };
    img.src = this.originalImageUrl;
  }

  updateImagePreview() {
    const canvas = this.imageCanvasRef?.nativeElement;
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    const img = new Image();
    img.onload = () => {
      canvas.width = img.width;
      canvas.height = img.height;

      ctx.save();
      ctx.translate(canvas.width / 2, canvas.height / 2);
      ctx.rotate((this.imageRotation * Math.PI) / 180);
      ctx.translate(-canvas.width / 2, -canvas.height / 2);

      ctx.filter = `brightness(${this.imageBrightness}%) contrast(${this.imageContrast}%) saturate(${this.imageSaturation}%)`;
      ctx.drawImage(img, 0, 0);

      ctx.restore();

      // Draw crop box if in crop mode
      if (this.editorMode === 'crop' && this.cropEndX > 0 && this.cropEndY > 0) {
        this.drawCropBox(ctx);
      }
    };
    img.src = this.originalImageUrl;
  }

  setEditorMode(mode: 'adjust' | 'crop' | 'draw') {
    // Save current drawing before switching mode
    if (this.editorMode === 'draw') {
      const canvas = this.imageCanvasRef?.nativeElement;
      if (canvas) {
        const ctx = canvas.getContext('2d');
        if (ctx) {
          this.savedDrawingImageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
        }
      }
    }

    this.editorMode = mode;
    
    if (mode === 'draw') {
      setTimeout(() => {
        this.setupDrawingMode();
      }, 100);
    } else {
      this.updateImagePreview();
    }
  }

  setupDrawingMode() {
    const canvas = this.imageCanvasRef?.nativeElement;
    if (!canvas) return;

    // Draw the image to main canvas
    this.updateImagePreview();

    // If we have saved drawing data, restore it
    if (this.savedDrawingImageData) {
      const ctx = canvas.getContext('2d', { willReadFrequently: true });
      if (ctx) {
        ctx.putImageData(this.savedDrawingImageData, 0, 0);
      }
    } else {
      // Initialize savedDrawingImageData with current canvas state
      const ctx = canvas.getContext('2d', { willReadFrequently: true });
      if (ctx) {
        this.savedDrawingImageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
      }
    }
  }

  drawCropBox(ctx: CanvasRenderingContext2D) {
    const minX = Math.min(this.cropStartX, this.cropEndX);
    const minY = Math.min(this.cropStartY, this.cropEndY);
    const maxX = Math.max(this.cropStartX, this.cropEndX);
    const maxY = Math.max(this.cropStartY, this.cropEndY);

    ctx.strokeStyle = '#00ff00';
    ctx.lineWidth = 2;
    ctx.strokeRect(minX, minY, maxX - minX, maxY - minY);
  }

  applyCrop() {
    // Removed - moved to comment-section component
  }

  setupDrawing() {
    const canvas = this.imageCanvasRef?.nativeElement;
    if (!canvas) return;

    canvas.addEventListener('mousedown', (e) => this.onDrawStart(e));
    canvas.addEventListener('mousemove', (e) => this.onDrawMove(e));
    canvas.addEventListener('mouseup', (e) => this.onDrawEnd(e));
    canvas.addEventListener('mouseleave', (e) => this.onDrawEnd(e));

    this.updateImagePreview();
  }

  onDrawStart(e: MouseEvent) {
    if (this.editorMode !== 'draw') return;

    const canvas = this.imageCanvasRef?.nativeElement;
    if (!canvas) return;

    const rect = canvas.getBoundingClientRect();
    this.drawStartX = e.clientX - rect.left;
    this.drawStartY = e.clientY - rect.top;
    this.isDrawing = true;
  }

  onDrawMove(e: MouseEvent) {
    if (!this.isDrawing || this.editorMode !== 'draw') return;

    const canvas = this.imageCanvasRef?.nativeElement;
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    const rect = canvas.getBoundingClientRect();
    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;

    ctx.strokeStyle = this.drawColor;
    ctx.lineWidth = this.drawSize;
    ctx.lineCap = 'round';
    ctx.lineJoin = 'round';

    ctx.beginPath();
    ctx.moveTo(this.drawStartX, this.drawStartY);
    ctx.lineTo(x, y);
    ctx.stroke();

    this.drawStartX = x;
    this.drawStartY = y;
  }

  onDrawEnd(e: MouseEvent) {
    this.isDrawing = false;
  }

  clearDrawing() {
    this.updateImagePreview();
    const canvas = this.imageCanvasRef?.nativeElement;
    if (canvas) {
      const ctx = canvas.getContext('2d', { willReadFrequently: true });
      if (ctx) {
        this.savedDrawingImageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
      }
    }
  }

  onCanvasMouseDown(e: MouseEvent) {
    const canvas = this.imageCanvasRef?.nativeElement;
    if (!canvas) return;

    const rect = canvas.getBoundingClientRect();
    const scaleX = canvas.width / rect.width;
    const scaleY = canvas.height / rect.height;
    
    const x = (e.clientX - rect.left) * scaleX;
    const y = (e.clientY - rect.top) * scaleY;

    if (this.editorMode === 'crop') {
      this.cropStartX = x;
      this.cropStartY = y;
      this.isCropping = true;
    } else if (this.editorMode === 'draw') {
      this.drawStartX = x;
      this.drawStartY = y;
      this.isDrawing = true;
    }
  }

  onCanvasMouseMove(e: MouseEvent) {
    const canvas = this.imageCanvasRef?.nativeElement;
    if (!canvas) return;

    const rect = canvas.getBoundingClientRect();
    const scaleX = canvas.width / rect.width;
    const scaleY = canvas.height / rect.height;
    
    const x = (e.clientX - rect.left) * scaleX;
    const y = (e.clientY - rect.top) * scaleY;

    if (this.editorMode === 'crop' && this.isCropping) {
      this.cropEndX = x;
      this.cropEndY = y;
      this.updateImagePreview();
    } else if (this.editorMode === 'draw' && this.isDrawing) {
      const ctx = canvas.getContext('2d', { willReadFrequently: true });
      if (!ctx) return;

      if (this.drawTool === 'pen') {
        ctx.strokeStyle = this.drawColor;
        ctx.lineWidth = this.drawSize;
        ctx.lineCap = 'round';
        ctx.lineJoin = 'round';

        ctx.beginPath();
        ctx.moveTo(this.drawStartX, this.drawStartY);
        ctx.lineTo(x, y);
        ctx.stroke();

        this.drawStartX = x;
        this.drawStartY = y;
      } else {
        // For shapes, show preview
        this.drawShapePreview(canvas, x, y);
      }
    }
  }

  drawShapePreview(canvas: HTMLCanvasElement, endX: number, endY: number) {
    // Restore the saved drawing state (image + previous drawings)
    if (this.savedDrawingImageData) {
      const ctx = canvas.getContext('2d', { willReadFrequently: true });
      if (ctx) {
        ctx.putImageData(this.savedDrawingImageData, 0, 0);
      }
    } else {
      // If no saved state, redraw the image
      this.updateImagePreview();
    }

    // Draw shape preview on top
    const ctx = canvas.getContext('2d', { willReadFrequently: true });
    if (!ctx) return;

    ctx.strokeStyle = this.drawColor;
    ctx.fillStyle = this.drawColor;
    ctx.lineWidth = this.drawSize;

    const width = endX - this.drawStartX;
    const height = endY - this.drawStartY;

    switch (this.drawTool) {
      case 'line':
        ctx.beginPath();
        ctx.moveTo(this.drawStartX, this.drawStartY);
        ctx.lineTo(endX, endY);
        ctx.stroke();
        break;

      case 'rectangle':
        ctx.strokeRect(this.drawStartX, this.drawStartY, width, height);
        break;

      case 'circle':
        const radius = Math.sqrt(width * width + height * height) / 2;
        const centerX = this.drawStartX + width / 2;
        const centerY = this.drawStartY + height / 2;
        ctx.beginPath();
        ctx.arc(centerX, centerY, radius, 0, 2 * Math.PI);
        ctx.stroke();
        break;

      case 'arrow':
        this.drawArrow(ctx, this.drawStartX, this.drawStartY, endX, endY);
        break;
    }
  }

  finalizeShape(canvas: HTMLCanvasElement, endX: number, endY: number) {
    const ctx = canvas.getContext('2d', { willReadFrequently: true });
    if (!ctx) return;

    ctx.strokeStyle = this.drawColor;
    ctx.fillStyle = this.drawColor;
    ctx.lineWidth = this.drawSize;

    const width = endX - this.drawStartX;
    const height = endY - this.drawStartY;

    switch (this.drawTool) {
      case 'line':
        ctx.beginPath();
        ctx.moveTo(this.drawStartX, this.drawStartY);
        ctx.lineTo(endX, endY);
        ctx.stroke();
        break;

      case 'rectangle':
        ctx.strokeRect(this.drawStartX, this.drawStartY, width, height);
        break;

      case 'circle':
        const radius = Math.sqrt(width * width + height * height) / 2;
        const centerX = this.drawStartX + width / 2;
        const centerY = this.drawStartY + height / 2;
        ctx.beginPath();
        ctx.arc(centerX, centerY, radius, 0, 2 * Math.PI);
        ctx.stroke();
        break;

      case 'arrow':
        this.drawArrow(ctx, this.drawStartX, this.drawStartY, endX, endY);
        break;
    }
  }

  drawArrow(ctx: CanvasRenderingContext2D, fromX: number, fromY: number, toX: number, toY: number) {
    const headlen = 15;
    const angle = Math.atan2(toY - fromY, toX - fromX);

    // Draw line
    ctx.beginPath();
    ctx.moveTo(fromX, fromY);
    ctx.lineTo(toX, toY);
    ctx.stroke();

    // Draw arrowhead
    ctx.beginPath();
    ctx.moveTo(toX, toY);
    ctx.lineTo(toX - headlen * Math.cos(angle - Math.PI / 6), toY - headlen * Math.sin(angle - Math.PI / 6));
    ctx.lineTo(toX - headlen * Math.cos(angle + Math.PI / 6), toY - headlen * Math.sin(angle + Math.PI / 6));
    ctx.closePath();
    ctx.fill();
  }

  onCanvasMouseUp(e: MouseEvent) {
    if (this.editorMode === 'draw' && this.isDrawing) {
      const canvas = this.imageCanvasRef?.nativeElement;
      if (canvas) {
        const rect = canvas.getBoundingClientRect();
        const scaleX = canvas.width / rect.width;
        const scaleY = canvas.height / rect.height;
        
        const x = (e.clientX - rect.left) * scaleX;
        const y = (e.clientY - rect.top) * scaleY;

        // For shapes, finalize the drawing
        if (this.drawTool !== 'pen') {
          this.finalizeShape(canvas, x, y);
        }

        const ctx = canvas.getContext('2d', { willReadFrequently: true });
        if (ctx) {
          // Save canvas state after finalizing
          this.savedDrawingImageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
        }
      }
    }
    this.isCropping = false;
    this.isDrawing = false;
  }

  onCanvasMouseLeave(e: MouseEvent) {
    this.isCropping = false;
    this.isDrawing = false;
  }

  rotateImage(degrees: number) {
    this.imageRotation = (this.imageRotation + degrees) % 360;
    this.updateImagePreview();
  }

  resetImage() {
    this.imageBrightness = 100;
    this.imageContrast = 100;
    this.imageRotation = 0;
    this.updateImagePreview();
  }

  saveImageEdits() {
    // Removed - moved to comment-section component
  }

  closeImageEditor() {
    this.showImageEditor = false;
    this.editingImageIndex = -1;
    this.originalImageUrl = '';
    this.cdr.markForCheck();
  }

  onAddImage() {
    // Removed - moved to comment-section component
  }
}



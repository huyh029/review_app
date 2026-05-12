import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService, Toast } from '../../services/toast.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  styles: [`
    .toast-enter {
      animation: slideIn 0.3s ease forwards;
    }
    @keyframes slideIn {
      from { transform: translateX(110%); opacity: 0; }
      to   { transform: translateX(0);   opacity: 1; }
    }
  `],
  template: `
    <div class="fixed top-5 right-5 z-[9999] flex flex-col gap-3 w-[340px]">
      @for (toast of toastService.toasts(); track toast.id) {
        <div class="toast-enter flex items-start gap-3 rounded-xl shadow-xl px-4 py-3 text-sm"
          style="background-color: #ffffff;"
          [ngClass]="{
          'bg-white border-l-4 border-[#276100]': toast.type === 'success',
            'bg-white border-l-4 border-red-500':   toast.type === 'error',
            'bg-white border-l-4 border-yellow-400': toast.type === 'warning',
            'bg-white border-l-4 border-blue-400':  toast.type === 'info'
          }">

          <!-- Icon -->
          <div class="mt-0.5 flex-shrink-0 w-6 h-6 rounded-full flex items-center justify-center text-white text-xs font-bold"
            [ngClass]="{
              'bg-[#276100]': toast.type === 'success',
              'bg-red-500':   toast.type === 'error',
              'bg-yellow-400': toast.type === 'warning',
              'bg-blue-400':  toast.type === 'info'
            }">
            {{ toast.type === 'success' ? '✓' : toast.type === 'error' ? '✕' : toast.type === 'warning' ? '!' : 'i' }}
          </div>

          <!-- Content -->
          <div class="flex-1 min-w-0">
            <p class="font-semibold text-gray-800 text-[13px]">
              {{ toast.type === 'success' ? 'Thành công' : toast.type === 'error' ? 'Lỗi' : toast.type === 'warning' ? 'Cảnh báo' : 'Thông báo' }}
            </p>
            <p class="text-gray-500 text-[12px] mt-0.5 break-words">{{ toast.message }}</p>
          </div>

          <!-- Close -->
          <button class="flex-shrink-0 text-gray-300 hover:text-gray-500 transition-colors text-lg leading-none mt-0.5"
            (click)="toastService.remove(toast.id)">×</button>
        </div>
      }
    </div>
  `
})
export class ToastComponent {
  toastService = inject(ToastService);
}

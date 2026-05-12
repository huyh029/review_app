import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SelectComponent, SelectOption } from '../select/select.component';
import { PaginationService } from '../../../services/pagination.service';

@Component({
  selector: 'app-pagination-footer',
  imports: [CommonModule, FormsModule, SelectComponent],
  templateUrl: './pagination-footer.component.html',
  styleUrls: ['./pagination-footer.component.css']
})
export class PaginationFooterComponent implements OnInit {
  @Output() pageChange = new EventEmitter<number>();
  @Output() itemsPerPageChange = new EventEmitter<number>();

  pagination: any = null;
  itemsPerPage: number = 10;
  Math = Math;

  itemsPerPageOptions: SelectOption[] = [
    { label: '5 dòng', value: 5 },
    { label: '10 dòng', value: 10 },
    { label: '20 dòng', value: 20 },
    { label: '50 dòng', value: 50 }
  ];

  constructor(private paginationService: PaginationService) {}

  ngOnInit() {
    this.paginationService.pagination$.subscribe(pagination => {
      this.pagination = pagination;
    });

    this.paginationService.itemsPerPage$.subscribe(itemsPerPage => {
      this.itemsPerPage = itemsPerPage;
    });
  }

  getPageNumbers(): (number | string)[] {
    if (!this.pagination) return [];
    
    const pages: (number | string)[] = [];
    const totalPages = this.pagination.totalPages;
    const currentPage = this.pagination.currentPage;
    const maxPagesToShow = 7;
    
    if (totalPages <= maxPagesToShow) {
      for (let i = 1; i <= totalPages; i++) {
        pages.push(i);
      }
    } else {
      pages.push(1);
      
      let startPage = Math.max(2, currentPage - 2);
      let endPage = Math.min(totalPages - 1, currentPage + 2);
      
      if (startPage > 2) {
        pages.push('...');
      }
      
      for (let i = startPage; i <= endPage; i++) {
        pages.push(i);
      }
      
      if (endPage < totalPages - 1) {
        pages.push('...');
      }
      
      pages.push(totalPages);
    }
    
    return pages;
  }

  previousPage() {
    if (this.pagination && this.pagination.currentPage > 1) {
      this.goToPage(this.pagination.currentPage - 1);
    }
  }

  nextPage() {
    if (this.pagination && this.pagination.currentPage < this.pagination.totalPages) {
      this.goToPage(this.pagination.currentPage + 1);
    }
  }

  goToPage(page: number) {
    if (this.pagination && page >= 1 && page <= this.pagination.totalPages) {
      this.pageChange.emit(page);
    }
  }

  onItemsPerPageChange() {
    this.itemsPerPageChange.emit(this.itemsPerPage);
  }
}

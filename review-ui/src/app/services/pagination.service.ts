import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PaginationService {
  private paginationSubject = new BehaviorSubject<any>(null);
  private itemsPerPageSubject = new BehaviorSubject<number>(10);

  pagination$: Observable<any> = this.paginationSubject.asObservable();
  itemsPerPage$: Observable<number> = this.itemsPerPageSubject.asObservable();

  setPagination(pagination: any) {
    this.paginationSubject.next(pagination);
  }

  setItemsPerPage(itemsPerPage: number) {
    this.itemsPerPageSubject.next(itemsPerPage);
  }

  getPagination(): any {
    return this.paginationSubject.value;
  }

  getItemsPerPage(): number {
    return this.itemsPerPageSubject.value;
  }
}

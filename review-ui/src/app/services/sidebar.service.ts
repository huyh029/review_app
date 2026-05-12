import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SidebarService {
  private sidebarOpen = new BehaviorSubject<boolean>(true);
  sidebarOpen$ = this.sidebarOpen.asObservable();

  toggle() {
    this.sidebarOpen.next(!this.sidebarOpen.value);
  }

  open() {
    this.sidebarOpen.next(true);
  }

  close() {
    this.sidebarOpen.next(false);
  }

  isOpen() {
    return this.sidebarOpen.value;
  }
}

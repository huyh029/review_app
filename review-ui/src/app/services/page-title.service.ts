import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PageTitleService {
  private titleSubject = new BehaviorSubject<string>('Trang chủ');
  public title$: Observable<string> = this.titleSubject.asObservable();

  setTitle(title: string) {
    this.titleSubject.next(title);
  }

  getTitle(): string {
    return this.titleSubject.value;
  }
}

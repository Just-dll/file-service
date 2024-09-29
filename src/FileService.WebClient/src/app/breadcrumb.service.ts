import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class BreadcrumbService {
  // BehaviorSubject to emit breadcrumb load status and folderId
  private breadcrumbsLoadedSubject = new BehaviorSubject<number | null>(null);
  breadcrumbsLoaded$ = this.breadcrumbsLoadedSubject.asObservable();

  setBreadcrumbsLoaded(folderId: number) {
    this.breadcrumbsLoadedSubject.next(folderId);
  }
}

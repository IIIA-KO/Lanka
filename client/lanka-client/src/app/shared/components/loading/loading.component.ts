import { Component, inject } from '@angular/core';
import { CommonModule, AsyncPipe } from '@angular/common';
import { LoadingService } from '../../../core/services/loading/loading.service';

@Component({
  selector: 'app-loading',
  standalone: true,
  imports: [CommonModule, AsyncPipe],
  templateUrl: './loading.component.html',
  styleUrls: ['./loading.component.css']
})
export class LoadingComponent {
  public isLoading$ = this.loadingService.isLoading$;
  private readonly loadingService = inject(LoadingService);
}

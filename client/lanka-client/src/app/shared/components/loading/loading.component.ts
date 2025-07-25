import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoadingService } from '../../../core/services/loading/loading.service';
import { AsyncPipe } from '@angular/common';

@Component({
  selector: 'lnk-loading',
  standalone: true,
  imports: [CommonModule, AsyncPipe],
  templateUrl: './loading.component.html',
  styleUrls: ['./loading.component.css']
})
export class LoadingComponent {
  isLoading$: typeof this.loadingService.isLoading$;

  constructor(private loadingService: LoadingService) {
    this.isLoading$ = this.loadingService.isLoading$;
  }
}

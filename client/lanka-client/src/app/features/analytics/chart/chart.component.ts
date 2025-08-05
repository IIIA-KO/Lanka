import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { ChartModule } from 'primeng/chart';

@Component({
  selector: 'analytics-chart',
  standalone: true,
  imports: [ChartModule],
  template: `
    <p-chart
      [type]="type"
      [data]="chartData"
      [options]="chartOptions"
    ></p-chart>
  `,
})
export class AnalyticsChartComponent implements OnChanges {
  @Input() type: 'pie' | 'bar' | 'line' = 'pie';
  @Input() labels: string[] = [];
  @Input() values: number[] = [];
  @Input() title: string = '';
  @Input() customColors?: string[];

  chartData: any;
  chartOptions: any;

  ngOnChanges(changes: SimpleChanges): void {
    const colors = this.customColors ?? this.generateColors(this.values.length);

    this.chartData = {
      labels: this.labels,
      datasets: [
        {
          label: this.title,
          data: this.values,
          backgroundColor: colors,
        },
      ],
    };

    this.chartOptions = {
      responsive: true,
      plugins: {
        legend: { position: 'top' },
        tooltip: { enabled: true },
      },
    };
  }

  private generateColors(count: number): string[] {
    const palette = [
      '#42A5F5',
      '#66BB6A',
      '#FFA726',
      '#AB47BC',
      '#FF7043',
      '#26A69A',
      '#8D6E63',
    ];

    const baseColors = [...palette];

    while (baseColors.length < count - 1) {
      baseColors.push(...palette);
    }

    baseColors.length = count - 1;

    baseColors.push('#C0C0C0');

    return baseColors;
  }
}

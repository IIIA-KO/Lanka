import { Component, Input, OnChanges, inject, effect } from '@angular/core';
import { ChartModule } from 'primeng/chart';
import ChartDataLabels from 'chartjs-plugin-datalabels';
import { ThemeService } from '../../../core/services/theme/theme.service';

@Component({
  selector: 'app-analytics-chart',
  standalone: true,
  imports: [ChartModule],
  template: `
    <p-chart
      [type]="type"
      [data]="chartData"
      [options]="chartOptions"
      [plugins]="plugins"
    ></p-chart>
  `,
})
export class AnalyticsChartComponent implements OnChanges {
  @Input() public type: 'pie' | 'bar' | 'line' = 'pie';
  @Input() public labels: string[] = [];
  @Input() public values: number[] = [];
  @Input() public title = '';
  @Input() public customColors?: string[];
  @Input() public maxPieSegments = 5; // Limit pie chart segments
  @Input() public colorMapping?: Record<string, string>; // Map labels to specific colors
  @Input() public colorMode: 'palette' | 'saturation' = 'palette';
  @Input() public baseColor = '#42A5F5'; // Base color for saturation mode

  public chartData: {
    labels: string[];
    datasets: {
      label: string;
      data: number[];
      backgroundColor: string[];
      borderColor?: string;
      borderWidth?: number;
    }[];
  } = {
    labels: [],
    datasets: [],
  };

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  public chartOptions: any = {};
  public plugins = [ChartDataLabels];

  private readonly themeService = inject(ThemeService);

  constructor() {
    effect(() => {
      // Register dependency on the theme signal
      this.themeService.isDark();
      
      // Re-render chart when the theme is toggled if we already have data
      if (this.labels && this.labels.length > 0) {
        // We use setTimeout to allow Angular's change detection to settle before recreating objects
        setTimeout(() => this.updateChart(), 0);
      }
    });
  }

  public ngOnChanges(): void {
    this.updateChart();
  }

  private updateChart(): void {
    let processedLabels = [...this.labels];
    let processedValues = [...this.values];

    // Limit pie chart segments to maxPieSegments
    if (this.type === 'pie' && this.labels.length > this.maxPieSegments) {
      // Sort by value descending to keep top segments
      const combined = this.labels.map((label, index) => ({
        label,
        value: this.values[index] || 0,
      }));
      combined.sort((a, b) => b.value - a.value);

      // Take top (maxPieSegments - 1) and sum the rest as "Others"
      const topSegments = combined.slice(0, this.maxPieSegments - 1);
      const othersSegments = combined.slice(this.maxPieSegments - 1);
      const othersSum = othersSegments.reduce((sum, item) => sum + item.value, 0);

      processedLabels = [...topSegments.map(s => s.label), 'Others'];
      processedValues = [...topSegments.map(s => s.value), othersSum];
    }

    const colors = this.generateColors(processedLabels, processedValues);
    const isDark = this.themeService.isDark();
    const borderColor = isDark ? '#1e293b' : '#ffffff';

    this.chartData = {
      labels: processedLabels,
      datasets: [
        {
          label: this.title,
          data: processedValues,
          backgroundColor: colors,
          borderColor: borderColor,
          borderWidth: 2,
        },
      ],
    };

    this.chartOptions = this.buildOptions();
  }

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  private buildOptions(): any {
    const isDark = this.themeService.isDark();
    const textColor = isDark ? '#cbd5e1' : '#475569';
    const gridColor = isDark ? 'rgba(255, 255, 255, 0.1)' : 'rgba(0, 0, 0, 0.1)';

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const options: any = {
      responsive: true,
      plugins: {
        legend: { 
          position: 'top',
          display: this.type !== 'bar',
          labels: { color: textColor }
        },
        tooltip: {
          enabled: true,
          callbacks: {
            label: (context: { label: string; raw: number }) => {
              const label = context.label || '';
              const value = context.raw;
              return `${label}: ${typeof value === 'number' ? value.toFixed(2) : value}%`;
            }
          }
        },
        datalabels: {
          display: this.type === 'pie',
          color: '#ffffff',
          font: { weight: 'bold' },
          formatter: (value: number) => {
            if (value < 5) return '';
            return value.toFixed(1) + '%';
          }
        }
      },
    };

    if (this.type === 'bar') {
      options.scales = {
        x: {
          ticks: { color: textColor },
          grid: { color: gridColor }
        },
        y: {
          beginAtZero: true,
          ticks: {
            color: textColor,
            callback: (value: number) => `${value}%`
          },
          grid: { color: gridColor }
        }
      };
    }

    return options;
  }

  private generateColors(labels: string[], values: number[]): string[] {
    // 1. Priority: Custom Colors direct input
    if (this.customColors && this.customColors.length > 0) {
      return this.customColors;
    }

    // 2. Priority: Color Mapping (Case-insensitive check)
    if (this.colorMapping) {
      return labels.map(label => {
        // Direct match
        if (this.colorMapping![label]) return this.colorMapping![label];
        // Case-insensitive match
        const lowerLabel = label.toLowerCase();
        const key = Object.keys(this.colorMapping!).find(k => k.toLowerCase() === lowerLabel);
        return key ? this.colorMapping![key] : '#C0C0C0';
      });
    }

    // 3. Priority: Saturation Mode (Using Rank-based Fading)
    if (this.colorMode === 'saturation') {
        // Combine to sort, finding rank
        const indexed = values.map((v, i) => ({ val: v, idx: i }));
        // Sort descending
        indexed.sort((a, b) => b.val - a.val);

        // Map back to original order
        const colors = new Array(values.length);
        const total = values.length;

        indexed.forEach((item, rank) => {
             // Rank 0 = Top = Most Vibrant
             // Rank N = Bottom = Most Pale
             colors[item.idx] = this.generateRankedColor(this.baseColor, rank, total);
        });

        return colors;
    }

    // 4. Fallback: Palette Mode
    return this.generatePaletteColors(labels.length);
  }

  private generatePaletteColors(count: number): string[] {
    if (count <= 0) return [];

    const palette = [
      '#42A5F5', '#66BB6A', '#FFA726', '#AB47BC',
      '#FF7043', '#26A69A', '#8D6E63'
    ];

    if (count <= palette.length) {
      return palette.slice(0, count);
    }

    const colors: string[] = [];
    for (let i = 0; i < count; i++) {
      colors.push(palette[i % palette.length]);
    }

    // Others is usually last if grouped
    if (this.type === 'pie' && count > 1) {
       colors[count - 1] = '#C0C0C0';
    }

    return colors;
  }

  private generateRankedColor(hex: string, rank: number, total: number): string {
      const maxFactor = 1.0;
      const minFactor = 0.35;

      let ratio = 0;
      if (total > 1) {
          ratio = rank / (total - 1);
      }

      const factor = maxFactor - (ratio * (maxFactor - minFactor));

      return this.adjustVibrancy(hex, factor);
  }

  private adjustVibrancy(hex: string, factor: number): string {
      let r = 0, g = 0, b = 0;
      if (hex.length === 4) {
          r = parseInt('0x' + hex[1] + hex[1]);
          g = parseInt('0x' + hex[2] + hex[2]);
          b = parseInt('0x' + hex[3] + hex[3]);
      } else if (hex.length === 7) {
          r = parseInt('0x' + hex[1] + hex[2]);
          g = parseInt('0x' + hex[3] + hex[4]);
          b = parseInt('0x' + hex[5] + hex[6]);
      }

      // RGB to HSL
      r /= 255; g /= 255; b /= 255;
      const cmin = Math.min(r,g,b), cmax = Math.max(r,g,b), delta = cmax - cmin;
      let h = 0, s = 0, l = 0;

      if (delta === 0) h = 0;
      else if (cmax === r) h = ((g - b) / delta) % 6;
      else if (cmax === g) h = (b - r) / delta + 2;
      else h = (r - g) / delta + 4;

      h = Math.round(h * 60);
      if (h < 0) h += 360;

      l = (cmax + cmin) / 2;
      s = delta === 0 ? 0 : delta / (1 - Math.abs(2 * l - 1));

      const newS = s * factor;
      const targetL = 0.93;
      const newL = l + (targetL - l) * (1 - factor);

      return `hsl(${h}, ${(newS * 100).toFixed(1)}%, ${(newL * 100).toFixed(1)}%)`;
  }
}

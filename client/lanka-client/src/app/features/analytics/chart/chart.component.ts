import { Component, Input, OnChanges } from '@angular/core';
import { ChartModule } from 'primeng/chart';

@Component({
  selector: 'app-analytics-chart',
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
    }[];
  } = {
    labels: [],
    datasets: [],
  };

  public chartOptions: {
    responsive: boolean;
    plugins: {
      legend: { position: 'top' };
      tooltip: { enabled: boolean };
    };
  } = {
    responsive: true,
    plugins: {
      legend: { position: 'top' },
      tooltip: { enabled: true },
    },
  };

  public ngOnChanges(): void {
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

    this.chartData = {
      labels: processedLabels,
      datasets: [
        {
          label: this.title,
          data: processedValues,
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
      // Step-based fading logic.
      // Even if values are identical, their rank will differ, ensuring distinctive colors.
      
      // Calculate a "Factor" from 1.0 (Top) down to 0.3 (Bottom)
      // If total is 1, factor is 1.0
      // If total is 5, factors: 1.0, 0.825, 0.65, 0.475, 0.3
      
      const maxFactor = 1.0;
      const minFactor = 0.35; // Don't go too pale
      
      let ratio = 0;
      if (total > 1) {
          ratio = rank / (total - 1);
      }
      
      const factor = maxFactor - (ratio * (maxFactor - minFactor));

      return this.adjustVibrancy(hex, factor);
  }

  private adjustVibrancy(hex: string, factor: number): string {
      // Factor 1.0 = Original Color
      // Factor 0.3 = Very Pale / Light / Desaturated version
      
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

      // Modify Saturation and Lightness
      // We want to reduce saturation and increase lightness as factor decreases.
      
      // New Saturation: Scale down
      const newS = s * factor; 
      
      // New Lightness: Scale UP towards white/100%. 
      // If factor is 1, L is L. If factor is 0, L is 1.0 (white).
      // L + (1 - L) * (1 - factor)
      
      // We can also mix just Opacity if needed, but HSL is cleaner for pie charts
      
      // Let's keep it simpler:
      // Just adjust Lightness to be lighter for lower ranks.
      // Current L is base. Target L is 0.95 (near white).
      const targetL = 0.93;
      const newL = l + (targetL - l) * (1 - factor);

      return `hsl(${h}, ${(newS * 100).toFixed(1)}%, ${(newL * 100).toFixed(1)}%)`;
  }
}

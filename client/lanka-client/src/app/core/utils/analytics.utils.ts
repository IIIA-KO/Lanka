const GENDER_LABEL_MAP: Record<string, string> = {
  'F': 'Female',
  'M': 'Male',
  'U': 'Unknown'
};

const REACH_LABEL_MAP: Record<string, string> = {
  'follower': 'Followers',
  'followers': 'Followers',
  'non_follower': 'Non-Followers',
  'non_followers': 'Non-Followers'
};

export function expandGenderLabel(code: string): string {
  return GENDER_LABEL_MAP[code] ?? code;
}

export function expandReachLabel(code: string): string {
  const normalized = code.toLowerCase().replace(/-/g, '_');
  return REACH_LABEL_MAP[normalized] ?? formatMetricName(code);
}

export function formatMetricName(name: string): string {
  return name
    .replace(/_/g, ' ')
    .replace(/\b\w/g, c => c.toUpperCase());
}

export const GENDER_COLOR_MAPPING: Record<string, string> = {
  'Female': '#ea5284',
  'Male': '#2196F3',
  'Unknown': '#9E9E9E'
};

export const REACH_COLOR_MAPPING: Record<string, string> = {
  'Followers': '#26A69A',
  'Non-Followers': '#B0BEC5'
};

export function formatMetric(value?: number | null): string {
  if (value == null) return '—';
  if (value >= 1_000_000) return `${(value / 1_000_000).toFixed(1)}M`;
  if (value >= 1_000) return `${(value / 1_000).toFixed(1)}K`;
  return value.toLocaleString();
}

export type MetricStatus = 'poor' | 'average' | 'good' | 'excellent' | 'none';

export function evaluateEngagementRate(er: number | null | undefined): MetricStatus {
  if (er == null) return 'none';
  if (er < 1.0) return 'poor';
  if (er < 3.5) return 'average';
  if (er < 6.0) return 'good';
  return 'excellent';
}

export function evaluateReachRate(rr: number | null | undefined): MetricStatus {
  if (rr == null) return 'none';
  if (rr < 10.0) return 'poor';
  if (rr < 25.0) return 'average';
  if (rr < 50.0) return 'good';
  return 'excellent';
}

export function evaluateErReach(erReach: number | null | undefined): MetricStatus {
  if (erReach == null) return 'none';
  if (erReach < 3.0) return 'poor';
  if (erReach < 8.0) return 'average';
  if (erReach < 15.0) return 'good';
  return 'excellent';
}

export function evaluateCpe(cpe: number | null | undefined): MetricStatus {
  if (cpe == null) return 'none';
  if (cpe > 0.5) return 'poor';
  if (cpe > 0.1) return 'average';
  if (cpe >= 0.05) return 'good';
  return 'excellent';
}

export function getMetricStatusColor(status: MetricStatus): "success" | "info" | "warning" | "danger" | "secondary" {
  switch(status) {
    case 'poor': return 'danger';
    case 'average': return 'warning';
    case 'good': return 'info';
    case 'excellent': return 'success';
    default: return 'secondary';
  }
}

export function getMetricStatusIcon(status: MetricStatus): string {
  switch(status) {
    case 'poor': return 'pi pi-arrow-down';
    case 'average': return 'pi pi-minus';
    case 'good': return 'pi pi-arrow-up';
    case 'excellent': return 'pi pi-star-fill';
    default: return '';
  }
}

export function getMetricStatusLabel(status: MetricStatus): string {
  if (status === 'none') return '';
  return 'COMMON.STATUS.' + status.toUpperCase();
}

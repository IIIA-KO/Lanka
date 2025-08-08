// Enhanced Statistics Models
export interface IEngagementStatistics {
  reachRate: number;
  engagementRate: number;
  erRech: number;
}

export interface IEngagementStatisticsResponse {
  statisticsPeriod: string;
  reachRate: number;
  engagementRate: number;
  erReach: number;
}

export interface IInteractionStatistics {
  engagementRate: number;
  averageLikes: number;
  averageComments: number;
  cpe: number;
}

export interface IInteractionStatisticsResponse {
  statisticsPeriod: string;
  metrics: ITimeSeriesMetricData[];
}

export interface IOverviewStatistics {
  metrics: {
    name: string;
    value: number;
  };
}

export interface IOverviewStatisticsResponse {
  statisticsPeriod: string;
  metrics: ITotalValueMetricData[];
}

export interface ITotalValueMetricData {
  name: string;
  period: string;
  totalValue: number;
  title?: string;
  description?: string;
}

export interface IMetricsStatisticsResponse {
  statisticsPeriod: string;
  metrics: ITimeSeriesMetricData[];
}

export interface ITimeSeriesMetricData {
  name: string;
  period: string;
  values: IMetricValue[];
  title?: string;
  description?: string;
}

export interface IMetricValue {
  value: number;
  endTime: string;
}

export interface ITableStatistics {
  metrics: {
    name: string;
    values: Record<string, number>;
  }[];
}

export interface IEngagementStatistics {
  reachRate: number;
  engagementRate: number;
  erRech: number;
}

export interface IInteractionStatistics {
  engagementRate: number;
  averageLikes: number;
  averageComments: number;
  cpe: number;
}

export interface IOverviewStatistics {
  metrics: {
    name: string;
    value: number;
  };
}

export interface ITimeSeriesMetricData {
  name: string;
  values: Record<string, number>;
}

export interface ITableStatistics {
  metrics: {
    name: string;
    values: Record<string, number>;
  }[];
}

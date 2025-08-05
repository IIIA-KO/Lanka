export interface IAgeDistribution {
  agePercentages: {
    ageGroup: string;
    percentage: number;
  }[];
}

export interface IGenderDistribution {
  genderPercentages: {
    gender: string;
    percentage: number;
  }[];
}

export enum LocationType {
  city = 1,
  country = 2
}

export interface ILocationDistribution {
  locationPercentages: {
    location: string;
    percentage: number;
  }[];
}

export enum StatisticsPeriod {
  day = 1,
  week = 7,
  day21 = 21
}

export interface IReachDistribution {
  reachPercentages: {
    followType: string;
    percentage: number;
  }[];
}

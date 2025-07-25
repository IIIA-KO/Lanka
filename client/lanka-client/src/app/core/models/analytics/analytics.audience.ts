export interface IAgeRatio {
  agePercentages: {
    ageGroup: string;
    percentage: number;
  }[];
}

export interface IGenderRatio {
  genderPercentages: {
    gender: string;
    percentage: number;
  }[];
}

export enum LocationType {
  city = 1,
  country = 2
}

export interface ILocationRatio {
  locationPercentages: {
    location: string;
    percentage: number;
  }[];
}

export enum StatisticPeriod {
  day = 1,
  week = 7,
  day21 = 21
}

export interface IReachRatio {
  reachPercentages: {
    followType: string;
    percentage: number;
  }[];
}

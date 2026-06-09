import {
  IAgeDistribution,
  IGenderDistribution,
  ILocationDistribution,
  IReachDistribution,
} from './analytics.audience';
import { IPostsResponse } from './analytics.posts';
import {
  IEngagementStatisticsResponse,
  IInteractionStatisticsResponse,
  IOverviewStatisticsResponse,
} from './analytics.statistics';

export interface IProfileSummaryResponse {
  overview: IOverviewStatisticsResponse | null;
  engagement: IEngagementStatisticsResponse | null;
  interaction: IInteractionStatisticsResponse | null;
  ageDistribution: IAgeDistribution | null;
  genderDistribution: IGenderDistribution | null;
  locationCountry: ILocationDistribution | null;
  locationCity: ILocationDistribution | null;
  reachDistribution: IReachDistribution | null;
  recentPosts: IPostsResponse | null;
}

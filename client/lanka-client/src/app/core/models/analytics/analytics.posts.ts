// Instagram Posts Models
export interface IInstagramPost {
  id: string;
  mediaType: string;
  mediaUrl: string;
  permalink: string;
  thumbnailUrl: string;
  timestamp: string;
  insights: IInstagramInsight[];
}

export interface IInstagramInsight {
  name: string;
  period: string;
  values: IInsightValue[];
  title?: string;
  description?: string;
}

export interface IInsightValue {
  value: number;
  endTime?: string;
}

export interface IPostsResponse {
  posts: IInstagramPost[];
  paging: IInstagramPagingResponse;
}

export interface IInstagramPagingResponse {
  cursors: IPagingCursors;
  next?: string;
  previous?: string;
}

export interface IPagingCursors {
  before?: string;
  after?: string;
}

// Post filter and query parameters
export interface IPostsQueryParams {
  limit?: number;
  after?: string;
  before?: string;
  since?: string;
  until?: string;
}
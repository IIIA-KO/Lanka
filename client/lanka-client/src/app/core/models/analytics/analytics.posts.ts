export interface IInstagramPagingResponse {
  before?: string | null;
  after?: string | null;
  nextCursor?: string | null;
  previousCursor?: string | null;
}

export interface IInstagramInsight {
  name: string;
  value?: number | null;
}

export interface IInstagramPost {
  id: string;
  mediaType: string;
  mediaUrl: string;
  permalink: string;
  thumbnailUrl: string;
  timestamp: Date;
  insights: IInstagramInsight[];
}

export interface IInstagramPostsResponse {
  posts: IInstagramPost[];
  paging: IInstagramPagingResponse;
}

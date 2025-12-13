// Deliverable Format Enum
export enum DeliverableFormat {
  InstagramPost = 'Instagram Post',
  InstagramReel = 'Instagram Reel',
  InstagramStory = 'Instagram Story',
  InstagramCarousel = 'Instagram Carousel',
  TikTokVideo = 'TikTok Video',
  YouTubeVideo = 'YouTube Video',
  YouTubeShort = 'YouTube Short',
  BlogPost = 'Blog Post',
  Custom = 'Custom'
}

// Deliverable Template Interface
export interface IDeliverableTemplate {
  format: DeliverableFormat;
  description: string;
  typicalRequirements: string;
  estimatedDuration?: string;
}

// Campaign Models
export interface ICampaign {
  id: string;
  status: CampaignStatus;
  name: string;
  description: string;
  offerId: string;
  clientId: string;
  creatorId: string;
  price: IPrice;
  expectedCompletionDate: string;
  deliverables: IDeliverable[];
  scheduledOnUtc: string;
  createdAt: string;
  updatedAt: string;
}

// Deliverable Model
export interface IDeliverable {
  description: string;
  format: string;
  requirements?: string;
  deadline?: string;
}

// Price Model
export interface IPrice {
  amount: number;
  currency: string;
}

// Campaign Creation Request
export interface ICreateCampaignRequest {
  offerId: string;
  name: string;
  description: string;
  price?: IPrice;
  expectedCompletionDate: string;
  deliverables: IDeliverable[];
}

export interface IPendCampaignRequest {
  name: string;
  description: string;
  scheduledOnUtc: string;
  offerId: string;
}

// Offer Models
export interface IOffer {
  id: string;
  name: string;
  priceAmount: number;
  priceCurrency: string;
  description: string;
  format?: DeliverableFormat;
  deliverableTemplates?: IDeliverableTemplate[];
  createdAt?: string;
}

export interface ICreateOfferRequest {
  name: string;
  priceAmount: number;
  priceCurrency: string;
  description: string;
  format?: DeliverableFormat;
  deliverableTemplates?: IDeliverableTemplate[];
}

export interface IEditOfferRequest {
  offerId: string;
  name: string;
  price: IPrice;
  description: string;
  format?: DeliverableFormat;
  deliverableTemplates?: IDeliverableTemplate[];
}

// Pact Models
export interface IPact {
  id: string;
  bloggerId: string;
  content: string;
  offers: IOffer[];
}

export interface ICreatePactRequest {
  content: string;
}

export interface IEditPactRequest {
  pactId: string;
  content: string;
}

// Review Models
export interface IReview {
  id: string;
  clientId: string;
  creatorId: string;
  offerId: string;
  campaignId: string;
  rating: number;
  comment: string;
  createdOnUtc: string;
}

export interface ICreateReviewRequest {
  campaignId: string;
  rating: number;
  comment: string;
}

export interface IEditReviewRequest {
  reviewId: string;
  rating: number;
  comment: string;
}

// Blogger Models
export interface IBlogger {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  birthDate: string;
  bio: string;
  pactId?: string;
  profilePhotoUri?: string;
  instagramUsername?: string;
  instagramFollowersCount?: number;
  instagramMediaCount?: number;
}

// Campaign Status enum
export enum CampaignStatus {
  Pending = 'Pending',
  Confirmed = 'Confirmed',
  Rejected = 'Rejected',
  Done = 'Done',
  Completed = 'Completed',
  Cancelled = 'Cancelled'
}

// Currency codes
export enum Currency {
  USD = 'USD',
  EUR = 'EUR',
  UAH = 'UAH',
  GBP = 'GBP'
}

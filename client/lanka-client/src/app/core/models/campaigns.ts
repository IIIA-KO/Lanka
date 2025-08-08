// Campaign Models
export interface ICampaign {
  id: string;
  status: string;
  name: string;
  description: string;
  offerId: string;
  clientId: string;
  creatorId: string;
  scheduledOnUtc: string;
}

// Offer Models
export interface IOffer {
  id: string;
  name: string;
  priceAmount: number;
  priceCurrency: string;
  description: string;
}

export interface ICreateOfferRequest {
  name: string;
  priceAmount: number;
  priceCurrency: string;
  description: string;
}

export interface IEditOfferRequest {
  offerId: string;
  name: string;
  priceAmount: number;
  priceCurrency: string;
  description: string;
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

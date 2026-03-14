export interface IBloggerProfile {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  birthDate: string;
  bio?: string;
  pactId?: string | null;
  profilePhotoUri?: string;
  instagramUsername?: string;
  instagramFollowersCount?: number;
  instagramMediaCount?: number;
  engagementRate?: number;
  audienceTopAgeGroup?: string;
  audienceTopGender?: string;
  audienceTopGenderPercentage?: number;
  audienceTopCountry?: string;
  audienceTopCountryPercentage?: number;
  category?: string;
}

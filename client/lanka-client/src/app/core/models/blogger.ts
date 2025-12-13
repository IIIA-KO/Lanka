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
  rating?: number; // Average rating from reviews
  country?: string;
  category?: string;
}

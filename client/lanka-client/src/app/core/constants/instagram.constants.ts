import { environment } from "../../../environments/environment.development";

export const instagramClientId = environment.instagramClientId;
export const instagramConfigId = environment.instagramConfigId;
export const instagramRedirectUri = 'https://localhost:4200/link-instagram/callback';
export const instagramScope = 'instagram_basic,instagram_manage_insights,instagram_manage_comments,pages_show_list,pages_read_engagement';
export const instagramResponseType = 'code';

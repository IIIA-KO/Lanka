const DEFAULT_API_URL = 'https://localhost:4308';

export const environment = {
  production: false,
  get apiUrl(): string {
    return globalThis.window?.__lankaConfig?.apiUrl ?? DEFAULT_API_URL;
  },
  instagramClientId: '483628147722528',
  instagramConfigId: '797712865571270',
};

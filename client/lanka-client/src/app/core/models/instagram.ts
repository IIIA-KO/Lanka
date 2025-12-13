export type InstagramOperationType = 'linking' | 'renewal';

export type InstagramOperationState =
  | 'pending'
  | 'processing'
  | 'completed'
  | 'failed'
  | 'not_found';

export interface IInstagramStatusResponse {
  status: InstagramOperationState | string;
  message?: string;
  timestamp?: string;
}

export interface IInstagramStatus {
  operation: InstagramOperationType;
  status: InstagramOperationState;
  message: string;
  timestamp: string;
  isFinal: boolean;
}

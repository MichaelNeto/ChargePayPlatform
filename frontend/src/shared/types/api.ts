export interface ApiError {
  field: string;
  code: string;
  type: string;
  message: string;
}

export interface ApiResponse<T> {
  success: boolean;
  code: string;
  message: string;
  data: T | null;
  errors: ApiError[];
  metadata: {
    timestamp: string;
    traceId: string;
    correlationId: string;
    apiVersion: string;
  };
}
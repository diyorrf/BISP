export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
}

export interface VerifyCodeRequest {
  email: string;
  code: string;
}

export interface AuthResponse {
  success: boolean;
  token?: string;
  message?: string;
}

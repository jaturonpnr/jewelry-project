/** Mirrors backend AuthResponseDto */
export interface AuthResponse {
  userId: string;
  email: string;
  fullName: string;
  role: string;
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
  refreshTokenExpiresAt: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface CurrentUser {
  userId: string;
  email: string;
  fullName: string;
  role: string;
}

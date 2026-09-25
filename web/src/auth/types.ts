import type { Role } from '@/shared/api/types';

/** From backend Identity/Dtos. Role is serialised as text. */
export interface UserDto {
  id: string;
  email: string;
  fullName: string;
  role: Role;
  isActive: boolean;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  accessToken: string;
  expiresAt: string;
  user: UserDto;
}

export interface CreateUserRequest {
  email: string;
  password: string;
  fullName: string;
  role: Role;
}

import { http } from '@/shared/api/http';
import type { AuthResponse, LoginRequest } from './types';

export async function login(request: LoginRequest): Promise<AuthResponse> {
  const { data } = await http.post<AuthResponse>('/api/auth/login', request);
  return data;
}

export interface User {
  id: number;
  name: string;
  email: string;
  lastLoginTime: string | null;
  status: 'unverified' | 'active' | 'blocked';
  registrationTime: string;
}

export interface AuthResponse {
  token: string;
  email: string;
  name: string;
  status: string;
}

export interface LoginCredentials {
  email: string;
  password: string;
}

export interface RegisterCredentials {
  name: string;
  email: string;
  password: string;
}

export interface UserActionDto {
  userIds: number[];
}
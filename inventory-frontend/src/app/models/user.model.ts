export interface User {
  id: number;
  username: string;
  email: string;
  role: UserRole;
}

export enum UserRole {
  User = 'User',
  Admin = 'Admin'
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  role?: UserRole;
}

export interface AuthResponse {
  token: string;
  user: User;
}

import axios from 'axios';
import { AuthResponse, LoginCredentials, RegisterCredentials, User } from '../types/User';

const API_URL = 'http://localhost:5080/api';

const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401 && !error.config.url.includes('/auth/login')) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export const authService = {
  login: (credentials: LoginCredentials) => 
    api.post<AuthResponse>('/auth/login', credentials),
  
  register: (credentials: RegisterCredentials) => 
    api.post<AuthResponse>('/auth/register', credentials),
};

export const userService = {
  getUsers: () => 
    api.get<User[]>('/users'),
  
  blockUsers: (userIds: number[]) => 
    api.post('/users/block', { userIds }),
  
  unblockUsers: (userIds: number[]) => 
    api.post('/users/unblock', { userIds }),
  
  deleteUsers: (userIds: number[]) => 
    api.post('/users/delete', { userIds }),
  
  deleteUnverified: () => 
    api.post('/users/delete-unverified'),
};

export default api;
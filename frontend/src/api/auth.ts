import { post } from './client'

interface AuthResponse {
  token: string
}

export const register = (email: string, name: string, password: string) =>
  post<AuthResponse>('/auth/register', { email, name, password })

export const login = (email: string, password: string) =>
  post<AuthResponse>('/auth/login', { email, password })

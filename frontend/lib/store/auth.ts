import { create } from 'zustand'
import type { User, LoginRequest, RegisterRequest } from '@/types'
import api, { setAccessToken, clearAccessToken } from '@/lib/api'

interface AuthState {
  user:            User | null
  isLoading:       boolean
  isAuthenticated: boolean

  login:          (data: LoginRequest) => Promise<void>
  register:       (data: RegisterRequest) => Promise<void>
  logout:         () => Promise<void>
  refreshSession: () => Promise<boolean>
}

export const useAuthStore = create<AuthState>((set) => ({
  user:            null,
  isLoading:       false,
  isAuthenticated: false,

  login: async (data) => {
    set({ isLoading: true })
    try {
      const res = await api.post('/api/auth/login', data)
      setAccessToken(res.data.accessToken)
      set({ user: res.data.user, isAuthenticated: true })
    } finally {
      set({ isLoading: false })
    }
  },

  register: async (data) => {
    set({ isLoading: true })
    try {
      const res = await api.post('/api/auth/register', data)
      setAccessToken(res.data.accessToken)
      set({ user: res.data.user, isAuthenticated: true })
    } finally {
      set({ isLoading: false })
    }
  },

  logout: async () => {
    await api.post('/api/auth/logout').catch(() => {})
    clearAccessToken()
    set({ user: null, isAuthenticated: false })
  },

  refreshSession: async () => {
    try {
      const res = await api.post('/api/auth/refresh')
      setAccessToken(res.data.accessToken)
      set({ user: res.data.user, isAuthenticated: true })
      return true
    } catch {
      clearAccessToken()
      set({ user: null, isAuthenticated: false })
      return false
    }
  },
}))
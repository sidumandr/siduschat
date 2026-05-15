import { create } from 'zustand'
import type { Message, Room } from '@/types'
import api from '@/lib/api'

interface ChatState {
  rooms:        Room[]
  activeRoomId: string | null
  messages:     Record<string, Message[]>    // roomId -> message lists
  typingUsers:  Record<string, string[]>     // roomId -> typing userId lists

  setActiveRoom:  (roomId: string) => void
  loadRooms:      () => Promise<void>
  loadMessages:   (roomId: string, page?: number) => Promise<void>
  addMessage:     (msg: Message) => void
  removeMessage:  (roomId: string, messageId: string) => void
  setTyping:      (roomId: string, userId: string, isTyping: boolean) => void
  addRoom:        (room: Room) => void
}

export const useChatStore = create<ChatState>((set) => ({
  rooms:        [],
  activeRoomId: null,
  messages:     {},
  typingUsers:  {},

  setActiveRoom: (roomId) => set({ activeRoomId: roomId }),

  loadRooms: async () => {
    const res = await api.get<Room[]>('/api/rooms')
    set({ rooms: res.data })
  },

  loadMessages: async (roomId, page = 1) => {
    const res = await api.get<Message[]>(`/api/rooms/${roomId}/messages`, {
      params: { page, pageSize: 50 },
    })
    const incoming = [...res.data].reverse()
    set((state) => ({
      messages: {
        ...state.messages,
        [roomId]: page === 1
          ? incoming
          : [...incoming, ...(state.messages[roomId] ?? [])],
      },
    }))
  },

  addMessage: (msg) =>
    set((state) => ({
      messages: {
        ...state.messages,
        [msg.roomId]: [...(state.messages[msg.roomId] ?? []), msg],
      },
    })),

  removeMessage: (roomId, messageId) =>
    set((state) => ({
      messages: {
        ...state.messages,
        [roomId]: (state.messages[roomId] ?? []).map((m) =>
          m.id === messageId
            ? { ...m, isDeleted: true, content: '[Bu mesaj silindi]' }
            : m
        ),
      },
    })),

  setTyping: (roomId, userId, isTyping) =>
    set((state) => {
      const current = state.typingUsers[roomId] ?? []
      return {
        typingUsers: {
          ...state.typingUsers,
          [roomId]: isTyping
            ? current.includes(userId) ? current : [...current, userId]
            : current.filter((id) => id !== userId),
        },
      }
    }),

  addRoom: (room) =>
    set((state) => ({ rooms: [room, ...state.rooms] })),
}))
export interface User {
    id: string,
    username: string,
    displayName: string | null
    avatarUrl: string | null
    isOnline: boolean,
    lastSeenAt: string | null
}

export interface Room {
    id: string,
    name: string,
    description: string | null,
    type: 'Public' | 'Private' | 'DirectMessage',
    memberCount: number,
    createdAt: string
}

export interface Message {
    id: string,
    roomId: string,
    sender: User,
    content: string,
    isDeleted: boolean,
    replyToMessageId: string | null,
    createdAt: string,
    updatedAt: string | null
}

export interface AuthTokensDto {
    accessToken: string,
    refreshToken: string,
    expiresAt: string,
    user: User
}

export interface RegisterRequest {
    username: string,
    email: string,
    password: string,
    displayName: string | null
}

export interface LoginRequest {
    email: string,
    password: string
}

export interface SendMessagesRequest {
    roomId: string,
    content: string,
    replyToMessageId?: string
}

export interface CreateRoomRequest {
    name: string,
    description: string | null,
    type: 'Public' | 'Private'
}

export interface PresenceEvent {
    userId: string
}

export interface TypingEvent {
    roomId: string,
    userId: string,
    isTyping: boolean
}
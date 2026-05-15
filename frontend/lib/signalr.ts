import * as signalR from '@microsoft/signalr'
import { getAccessToken } from './api'
import type { Message, PresenceEvent, TypingEvent } from '@/types'

type EventMap = {
  'message:new':     (msg: Message) => void
  'message:deleted': (payload: { messageId: string; roomId: string }) => void
  'presence:online': (payload: PresenceEvent) => void
  'presence:offline':(payload: PresenceEvent) => void
  'user:typing':     (payload: TypingEvent) => void
}

class ChatConnection {
  private conn: signalR.HubConnection | null = null

  build() {
    this.conn = new signalR.HubConnectionBuilder()
      .withUrl(
        process.env.NEXT_PUBLIC_SIGNALR_URL ?? 'http://localhost:5000/hubs/chat',
        {
          accessTokenFactory: () => getAccessToken() ?? '',
          transport: signalR.HttpTransportType.WebSockets,
        }
      )
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Warning)
      .build()

    return this
  }

  async start() {
    if (!this.conn) this.build()
    if (this.conn!.state === signalR.HubConnectionState.Disconnected) {
      await this.conn!.start()
      console.log('SignalR bağlandı ✓')
    }
  }

  async stop() {
    await this.conn?.stop()
  }

  on<E extends keyof EventMap>(event: E, handler: EventMap[E]) {
    this.conn?.on(event, handler as (...args: unknown[]) => void)
  }

  off<E extends keyof EventMap>(event: E, handler: EventMap[E]) {
    this.conn?.off(event, handler as (...args: unknown[]) => void)
  }

  async invoke(method: string, ...args: unknown[]) {
    return this.conn?.invoke(method, ...args)
  }

  get state() {
    return this.conn?.state ?? signalR.HubConnectionState.Disconnected
  }

  onReconnecting(cb: () => void) { this.conn?.onreconnecting(() => cb()) }
  onReconnected(cb: () => void)  { this.conn?.onreconnected(() => cb()) }
}

export const chatConnection = new ChatConnection()
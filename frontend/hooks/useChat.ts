'use client'

import { useEffect, useRef } from 'react'
import { chatConnection } from '@/lib/signalr'
import { useChatStore } from '@/lib/store/chat'
import type { Message, PresenceEvent, TypingEvent } from '@/types'

export function useChat() {
  const { addMessage, removeMessage, setTyping } = useChatStore()
  const typingTimers = useRef<Record<string, NodeJS.Timeout>>({})

  useEffect(() => {
    const onNewMessage = (msg: Message) => {
      addMessage(msg)
    }

    const onDeleted = ({ messageId, roomId }: { messageId: string; roomId: string }) => {
      removeMessage(roomId, messageId)
    }

    const onTyping = ({ roomId, userId, isTyping }: TypingEvent) => {
      setTyping(roomId, userId, isTyping)

      const key = `${roomId}:${userId}`
      if (isTyping) {
        clearTimeout(typingTimers.current[key])
        typingTimers.current[key] = setTimeout(
          () => setTyping(roomId, userId, false),
          3000
        )
      }
    }

    chatConnection.on('message:new',     onNewMessage)
    chatConnection.on('message:deleted', onDeleted)
    chatConnection.on('user:typing',     onTyping)

    return () => {
      chatConnection.off('message:new',     onNewMessage)
      chatConnection.off('message:deleted', onDeleted)
      chatConnection.off('user:typing',     onTyping)
    }
  }, [addMessage, removeMessage, setTyping])

  const sendMessage = (roomId: string, content: string) =>
    chatConnection.invoke('SendMessage', { roomId, content })

  const sendTyping = (roomId: string, isTyping: boolean) =>
    chatConnection.invoke('Typing', roomId, isTyping)

  return { sendMessage, sendTyping }
}
'use client'

import { useEffect, useRef } from 'react'
import { MessageBubble } from './MessageBubble'
import type { Message } from '@/types'

interface Props {
  messages:        Message[]
  currentUserId:   string
  typingUsernames: string[]
}

export function MessageList({ messages, currentUserId, typingUsernames }: Props) {
  const bottomRef  = useRef<HTMLDivElement>(null)
  const prevLength = useRef(0)

  useEffect(() => {
    if (messages.length > prevLength.current) {
      bottomRef.current?.scrollIntoView({ behavior: 'smooth' })
    }
    prevLength.current = messages.length
  }, [messages.length])

  const grouped = messages.map((msg, i) => {
    const prev = messages[i - 1]
    const showAvatar =
      !prev ||
      prev.sender.id !== msg.sender.id ||
      new Date(msg.createdAt).getTime() - new Date(prev.createdAt).getTime() > 5 * 60 * 1000
    return { ...msg, showAvatar }
  })

  return (
    <div className="flex-1 overflow-y-auto chat-scroll px-4 py-3 space-y-0.5">
      {grouped.map((msg) => (
        <MessageBubble
          key={msg.id}
          message={msg}
          isOwn={msg.sender.id === currentUserId}
          showAvatar={msg.showAvatar}
        />
      ))}

      {/* typing */}
      {typingUsernames.length > 0 && (
        <div className="flex items-center gap-2 text-xs text-muted-foreground py-1 px-2">
          <span className="flex gap-0.5 items-center">
            {[0, 1, 2].map((i) => (
              <span
                key={i}
                className="w-1.5 h-1.5 bg-muted-foreground rounded-full typing-dot"
                style={{ animationDelay: `${i * 0.2}s` }}
              />
            ))}
          </span>
          {typingUsernames.join(', ')} yazıyor…
        </div>
      )}

      <div ref={bottomRef} />
    </div>
  )
}
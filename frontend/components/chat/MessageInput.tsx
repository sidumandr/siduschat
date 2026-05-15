'use client'

import { useState, useRef, useCallback, useEffect } from 'react'
import { Send } from 'lucide-react'
import { cn } from '@/lib/utils'

interface Props {
  onSend:    (content: string) => Promise<void>
  onTyping?: (isTyping: boolean) => void
  disabled?: boolean
}

export function MessageInput({ onSend, onTyping, disabled }: Props) {
  const [value, setValue]       = useState('')
  const [sending, setSending]   = useState(false)
  
  const typingTimer             = useRef<ReturnType<typeof setTimeout> | null>(null)
  const isTypingRef             = useRef(false)
  const textareaRef             = useRef<HTMLTextAreaElement>(null)

  const notifyTyping = useCallback((typing: boolean) => {
    if (isTypingRef.current === typing) return
    isTypingRef.current = typing
    onTyping?.(typing)
  }, [onTyping])

  useEffect(() => {
    return () => {
      if (typingTimer.current) clearTimeout(typingTimer.current)
    }
  }, [])

  const handleChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
    const newValue = e.target.value
    setValue(newValue)

    if (newValue.trim().length > 0) {
      notifyTyping(true)
      if (typingTimer.current) clearTimeout(typingTimer.current)
      typingTimer.current = setTimeout(() => notifyTyping(false), 2000)
    } else {
      notifyTyping(false)
    }

    e.target.style.height = 'auto'
    e.target.style.height = `${Math.min(e.target.scrollHeight, 120)}px`
  }

  const submit = async () => {
    const text = value.trim()
    if (!text || sending) return

    if (typingTimer.current) {
      clearTimeout(typingTimer.current)
      typingTimer.current = null
    }
    
    notifyTyping(false)
    setSending(true)
    try {
      await onSend(text)
      setValue('')
      if (textareaRef.current) textareaRef.current.style.height = '40px'
    } finally {
      setSending(false)
    }
  }

  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault()
      submit()
    }
  }

  return (
    <div className="flex items-end gap-2 p-3 border-t bg-background">
      <textarea
        ref={textareaRef}
        value={value}
        onChange={handleChange}
        onKeyDown={handleKeyDown}
        placeholder="Mesaj yaz… (Shift+Enter yeni satır)"
        rows={1}
        disabled={disabled || sending}
        className={cn(
          'flex-1 resize-none rounded-xl border bg-muted px-3 py-2 text-sm',
          'focus:outline-none focus:ring-2 focus:ring-ring',
          'min-h-10 max-h-30 disabled:opacity-50'
        )}
      />
      <button
        onClick={submit}
        type="button"
        disabled={!value.trim() || sending || disabled}
        className={cn(
          'h-10 w-10 rounded-xl flex items-center justify-center shrink-0 transition-all',
          'bg-primary text-primary-foreground hover:bg-primary/90',
          'disabled:opacity-40 disabled:cursor-not-allowed active:scale-95'
        )}
      >
        <Send className="h-4 w-4" />
      </button>
    </div>
  )
}
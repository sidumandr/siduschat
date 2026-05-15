import { cn, formatMessageTime, getInitials } from '@/lib/utils'
import type { Message } from '@/types'

interface Props {
  message:    Message
  isOwn:      boolean
  showAvatar: boolean
}

export function MessageBubble({ message, isOwn, showAvatar }: Props) {
  if (message.isDeleted && !message.content.startsWith('[')) return null

  return (
    <div className={cn('flex gap-2 items-end mb-1', isOwn && 'flex-row-reverse')}>

      {/* avatar */}
      {showAvatar ? (
        <div className="w-7 h-7 rounded-full bg-primary/10 flex items-center justify-center text-[11px] font-semibold shrink-0 select-none">
          {message.sender.avatarUrl ? (
            <img
              src={message.sender.avatarUrl}
              alt={message.sender.displayName ?? message.sender.username}
              className="w-full h-full rounded-full object-cover"
            />
          ) : (
            getInitials(message.sender.displayName ?? message.sender.username)
          )}
        </div>
      ) : (
        <div className="w-7 shrink-0" />
      )}

      <div className={cn('flex flex-col max-w-[70%]', isOwn && 'items-end')}>
        {showAvatar && !isOwn && (
          <span className="text-xs text-muted-foreground mb-0.5 ml-1">
            {message.sender.displayName ?? message.sender.username}
          </span>
        )}

        {/* mesasage bubble */}
        <div className={cn(
          'px-3 py-2 rounded-2xl text-sm leading-relaxed wrap-break-words',
          isOwn
            ? 'bg-primary text-primary-foreground rounded-br-sm'
            : 'bg-muted rounded-bl-sm',
          message.isDeleted && 'italic opacity-50'
        )}>
          {/* Reply  */}
          {message.replyToMessageId && (
            <div className={cn(
              'text-xs border-l-2 pl-2 mb-1.5 opacity-70',
              isOwn ? 'border-primary-foreground/40' : 'border-muted-foreground/40'
            )}>
              Yanıtlanan mesaj
            </div>
          )}
          {message.content}
        </div>

        {/* time */}
        <span className="text-[10px] text-muted-foreground mt-0.5 mx-1">
          {formatMessageTime(message.createdAt)}
          {message.updatedAt && ' · düzenlendi'}
        </span>
      </div>
    </div>
  )
}
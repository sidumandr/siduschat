'use client'

import { useEffect, useState } from 'react'
import { useRouter }           from 'next/navigation'
import { RoomSidebar }         from '@/components/chat/RoomSidebar'
import { MessageList }         from '@/components/chat/MessageList'
import { MessageInput }        from '@/components/chat/MessageInput'
import { useAuthStore }        from '@/lib/store/auth'
import { useChatStore }        from '@/lib/store/chat'
import { useChat }             from '@/hooks/useChat'
import { chatConnection }      from '@/lib/signalr'
import { CreateRoomModal } from '@/components/chat/CreateRoomModal'

export default function ChatPage() {
  const router = useRouter()

  const { user, isAuthenticated, refreshSession } = useAuthStore()
  const {
    rooms, activeRoomId, messages, typingUsers,
    setActiveRoom, loadRooms, loadMessages,
  } = useChatStore()
  const { sendMessage, sendTyping } = useChat()

  const [isConnecting, setIsConnecting] = useState(true)
  const [showCreateRoom, setShowCreateRoom] = useState(false)

  useEffect(() => {
    refreshSession().then((ok) => {
      if (!ok) router.replace('/auth/login')
      else setIsConnecting(false)
    })
  }, [])  // eslint-disable-line

  useEffect(() => {
    if (!isAuthenticated || isConnecting) return

    chatConnection.start().catch(console.error)
    loadRooms()

    return () => { chatConnection.stop() }
  }, [isAuthenticated, isConnecting])  // eslint-disable-line
  
  useEffect(() => {
    if (activeRoomId) loadMessages(activeRoomId)
  }, [activeRoomId])  // eslint-disable-line

  if (isConnecting || !user) {
    return (
      <div className="h-screen flex items-center justify-center text-muted-foreground text-sm">
        Yükleniyor…
      </div>
    )
  }

  const activeMessages   = activeRoomId ? (messages[activeRoomId] ?? [])  : []
  const typingIds        = activeRoomId ? (typingUsers[activeRoomId] ?? []) : []
  const activeRoom       = rooms.find((r) => r.id === activeRoomId)

  const typingNames = typingIds.map((id) => id.slice(0, 6))

  return (
    <div className="flex h-screen overflow-hidden bg-background">
      <RoomSidebar
        rooms={rooms}
        activeRoomId={activeRoomId}
        currentUser={user}
        onSelectRoom={setActiveRoom}
        onCreateRoom={() => setShowCreateRoom(true)} 
      />

      <main className="flex-1 flex flex-col min-w-0">
        {activeRoomId ? (
          <>
            <div className="h-12 border-b flex items-center px-4 gap-2 shrink-0">
              <span className="font-medium text-sm"># {activeRoom?.name}</span>
              {activeRoom?.description && (
                <>
                  <span className="text-border">|</span>
                  <span className="text-xs text-muted-foreground truncate">
                    {activeRoom.description}
                  </span>
                </>
              )}
            </div>

            <MessageList
              messages={activeMessages}
              currentUserId={user.id}
              typingUsernames={typingNames}
            />

            <MessageInput
              onSend={(content) => sendMessage(activeRoomId, content)}
              onTyping={(isTyping) => sendTyping(activeRoomId, isTyping)}
            />
          </>
        ) : (
          <div className="flex-1 flex flex-col items-center justify-center gap-2 text-muted-foreground">
            <span className="text-4xl">💬</span>
            <p className="text-sm">Sohbet başlatmak için bir oda seç</p>
          </div>
        )}
      </main>

      {showCreateRoom && (
        <CreateRoomModal onClose={() => setShowCreateRoom(false)} />
      )}
    </div>
  )
}
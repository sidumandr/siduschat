'use client'

import { Hash, Lock, MessageCircle, LogOut, Plus } from 'lucide-react'
import { cn } from '@/lib/utils'
import { useAuthStore } from '@/lib/store/auth'
import { useRouter } from 'next/navigation'
import type { Room, User } from '@/types'
import { getInitials } from '@/lib/utils'

interface Props {
  rooms:         Room[]
  activeRoomId:  string | null
  currentUser:   User
  onSelectRoom:  (roomId: string) => void
  onCreateRoom:  () => void  
}

export function RoomSidebar({ rooms, activeRoomId, currentUser, onSelectRoom, onCreateRoom }: Props) {
  const { logout } = useAuthStore()
  const router     = useRouter()

  const channels = rooms.filter((r) => r.type !== 'DirectMessage')
  const dms      = rooms.filter((r) => r.type === 'DirectMessage')

  const handleLogout = async () => {
    await logout()
    router.replace('/auth/login')
  }

  return (
    <aside className="w-60 border-r bg-muted/20 flex flex-col h-full shrink-0">

    <div className="h-12 border-b flex items-center justify-between px-4">
      <span className="font-semibold text-sm">💬 ChatApp</span>
      <button
        onClick={onCreateRoom}
        className="p-1.5 rounded-md hover:bg-muted transition-colors text-muted-foreground hover:text-foreground"
        title="Yeni oda oluştur"
      >
        <Plus className="h-4 w-4" />
      </button>
</div>

      {/* room list */}
      <nav className="flex-1 overflow-y-auto py-2 space-y-4">
        {channels.length > 0 && (
          <RoomGroup
            label="Kanallar"
            rooms={channels}
            activeRoomId={activeRoomId}
            onSelect={onSelectRoom}
          />
        )}
        {dms.length > 0 && (
          <RoomGroup
            label="Doğrudan mesajlar"
            rooms={dms}
            activeRoomId={activeRoomId}
            onSelect={onSelectRoom}
          />
        )}
        {rooms.length === 0 && (
          <p className="px-4 text-xs text-muted-foreground">
            Henüz bir odaya katılmadın.
          </p>
        )}
      </nav>

      {/* user info + logout */}
      <div className="border-t p-3 flex items-center gap-2">
        <div className="w-8 h-8 rounded-full bg-primary/10 flex items-center justify-center text-xs font-semibold shrink-0">
          {getInitials(currentUser.displayName ?? currentUser.username)}
        </div>
        <div className="flex-1 min-w-0">
          <p className="text-sm font-medium truncate">
            {currentUser.displayName ?? currentUser.username}
          </p>
          <p className="text-xs text-muted-foreground truncate">
            @{currentUser.username}
          </p>
        </div>
        <button
          onClick={handleLogout}
          className="p-1.5 rounded-md hover:bg-muted transition-colors text-muted-foreground hover:text-foreground"
          title="Çıkış yap"
        >
          <LogOut className="h-4 w-4" />
        </button>
      </div>

    </aside>
  )
}

function RoomGroup({ label, rooms, activeRoomId, onSelect }: {
  label:        string
  rooms:        Room[]
  activeRoomId: string | null
  onSelect:     (id: string) => void
}) {
  return (
    <div>
      <p className="px-3 py-1 text-xs font-medium text-muted-foreground uppercase tracking-wider">
        {label}
      </p>
      {rooms.map((room) => {
        const Icon     = room.type === 'DirectMessage' ? MessageCircle : room.type === 'Private' ? Lock : Hash
        const isActive = room.id === activeRoomId

        return (
          <button
            key={room.id}
            onClick={() => onSelect(room.id)}
            className={cn(
              'w-full flex items-center gap-2 px-3 py-1.5 mx-1 rounded-md text-sm text-left transition-colors',
              'hover:bg-accent',
              isActive ? 'bg-accent font-medium' : 'text-muted-foreground hover:text-foreground'
            )}
          >
            <Icon className="h-3.5 w-3.5 shrink-0" />
            <span className="truncate">{room.name}</span>
          </button>
        )
      })}
    </div>
  )
}
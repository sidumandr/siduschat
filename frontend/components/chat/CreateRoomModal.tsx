'use client'

import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { X } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { cn } from '@/lib/utils'
import api from '@/lib/api'
import { useChatStore } from '@/lib/store/chat'
import type { Room } from '@/types'

const schema = z.object({
  name:        z.string().min(2, 'En az 2 karakter').max(50),
  description: z.string().max(200).optional(),
  type:        z.enum(['Public', 'Private']),
})
type FormData = z.infer<typeof schema>

interface Props {
  onClose: () => void
}

export function CreateRoomModal({ onClose }: Props) {
  const { addRoom, setActiveRoom } = useChatStore()
  const [isLoading, setIsLoading] = useState(false)

  const {
    register,
    handleSubmit,
    watch,
    setValue,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { type: 'Public' },
  })

  const selectedType = watch('type')

  const onSubmit = async (data: FormData) => {
    setIsLoading(true)
    try {
      const res = await api.post<Room>('/api/rooms', data)
      addRoom(res.data)
      setActiveRoom(res.data.id)
      onClose()
    } catch (err: unknown) {
      console.error(err)
    } finally {
      setIsLoading(false)
    }
  }

  return (
    // Backdrop
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/40"
      onClick={onClose}
    >
      {/* modal box */}
      <div
        className="bg-background border rounded-2xl shadow-lg w-full max-w-md mx-4 p-6 space-y-5"
        onClick={(e) => e.stopPropagation()}
      >
        {/* header */}
        <div className="flex items-center justify-between">
          <h2 className="text-lg font-semibold">Yeni oda oluştur</h2>
          <button
            onClick={onClose}
            className="p-1.5 rounded-md hover:bg-muted transition-colors text-muted-foreground"
          >
            <X className="h-4 w-4" />
          </button>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">

          {/* select room type */}
          <div className="space-y-1.5">
            <Label>Oda tipi</Label>
            <div className="grid grid-cols-2 gap-2">
              {(['Public', 'Private'] as const).map((t) => (
                <button
                  key={t}
                  type="button"
                  onClick={() => setValue('type', t)}
                  className={cn(
                    'px-3 py-2.5 rounded-lg border text-sm font-medium transition-colors text-left',
                    selectedType === t
                      ? 'border-primary bg-primary/5 text-primary'
                      : 'border-border hover:bg-muted text-muted-foreground'
                  )}
                >
                  <span className="block">{t === 'Public' ? '🌐 Herkese açık' : '🔒 Özel'}</span>
                  <span className="text-xs font-normal opacity-70">
                    {t === 'Public' ? 'Herkes katılabilir' : 'Davetiye ile'}
                  </span>
                </button>
              ))}
            </div>
          </div>

          {/* room name */}
          <div className="space-y-1.5">
            <Label htmlFor="name">Oda adı</Label>
            <Input
              id="name"
              placeholder="örn: genel, teknoloji, duyurular"
              {...register('name')}
            />
            {errors.name && (
              <p className="text-xs text-destructive">{errors.name.message}</p>
            )}
          </div>

          {/* desc */}
          <div className="space-y-1.5">
            <Label htmlFor="description">
              Açıklama
              <span className="text-muted-foreground font-normal ml-1">(isteğe bağlı)</span>
            </Label>
            <Input
              id="description"
              placeholder="Bu oda ne için?"
              {...register('description')}
            />
          </div>

          {/* buttons */}
          <div className="flex gap-2 pt-1">
            <Button
              type="button"
              variant="outline"
              className="flex-1"
              onClick={onClose}
            >
              İptal
            </Button>
            <Button
              type="submit"
              className="flex-1"
              disabled={isLoading}
            >
              {isLoading ? 'Oluşturuluyor…' : 'Oluştur'}
            </Button>
          </div>

        </form>
      </div>
    </div>
  )
}
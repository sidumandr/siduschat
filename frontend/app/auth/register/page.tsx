'use client'

import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useRouter } from 'next/navigation'
import Link from 'next/link'
import { useAuthStore } from '@/lib/store/auth'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'

const schema = z.object({
  username: z
    .string()
    .min(3, 'En az 3 karakter')
    .max(30)
    .regex(/^[a-z0-9_]+$/, 'Sadece küçük harf, rakam ve alt çizgi'),
  displayName: z.string().min(1, 'Zorunlu').max(50),
  email:       z.string().email('Geçerli bir email gir'),
  password:    z.string().min(8, 'En az 8 karakter'),
  confirm:     z.string(),
}).refine((d) => d.password === d.confirm, {
  message: 'Şifreler eşleşmiyor',
  path:    ['confirm'],
})
type FormData = z.infer<typeof schema>

export default function RegisterPage() {
  const router = useRouter()
  const { register: registerUser, isLoading } = useAuthStore()

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors },
  } = useForm<FormData>({ resolver: zodResolver(schema) })

  const onSubmit = async (data: FormData) => {
    try {
      await registerUser({
        username:    data.username,
        email:       data.email,
        password:    data.password,
        displayName: data.displayName,
      })
      router.replace('/chat')
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: { error?: string } } })
          ?.response?.data?.error ?? 'Kayıt başarısız'
      setError('root', { message: msg })
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-muted/30 px-4">
      <div className="w-full max-w-sm space-y-6 bg-background border rounded-2xl p-8 shadow-sm">

        <div>
          <h1 className="text-2xl font-semibold">Hesap oluştur</h1>
          <p className="text-sm text-muted-foreground mt-1">Ücretsiz başla</p>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">

          <div className="space-y-1.5">
            <Label htmlFor="username">Kullanıcı adı</Label>
            <Input
              id="username"
              placeholder="ahmet_yilmaz"
              autoCapitalize="none"
              {...register('username')}
            />
            {errors.username && (
              <p className="text-xs text-destructive">{errors.username.message}</p>
            )}
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="displayName">Görünen ad</Label>
            <Input
              id="displayName"
              placeholder="Ahmet Yılmaz"
              {...register('displayName')}
            />
            {errors.displayName && (
              <p className="text-xs text-destructive">{errors.displayName.message}</p>
            )}
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="email">Email</Label>
            <Input
              id="email"
              type="email"
              placeholder="sen@ornek.com"
              {...register('email')}
            />
            {errors.email && (
              <p className="text-xs text-destructive">{errors.email.message}</p>
            )}
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="password">Şifre</Label>
            <Input
              id="password"
              type="password"
              placeholder="••••••••"
              {...register('password')}
            />
            {errors.password && (
              <p className="text-xs text-destructive">{errors.password.message}</p>
            )}
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="confirm">Şifre tekrar</Label>
            <Input
              id="confirm"
              type="password"
              placeholder="••••••••"
              {...register('confirm')}
            />
            {errors.confirm && (
              <p className="text-xs text-destructive">{errors.confirm.message}</p>
            )}
          </div>

          {errors.root && (
            <p className="text-sm text-destructive">{errors.root.message}</p>
          )}

          <Button type="submit" className="w-full" disabled={isLoading}>
            {isLoading ? 'Hesap oluşturuluyor…' : 'Hesap oluştur'}
          </Button>
        </form>

        <p className="text-sm text-center text-muted-foreground">
          Zaten hesabın var mı?{' '}
          <Link href="/auth/login" className="text-primary hover:underline">
            Giriş yap
          </Link>
        </p>

      </div>
    </div>
  )
}
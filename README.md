# SidusChat

Next.js 15, .NET 9, SignalR, PostgreSQL ve Redis ile geliştirilmiş gerçek zamanlı sohbet uygulaması.

## Stack

| Katman | Teknoloji |
|--------|-----------|
| Frontend | Next.js 15, TypeScript, Tailwind CSS, Shadcn UI |
| Realtime | SignalR |
| Backend | ASP.NET Core 9, Clean Architecture |
| Auth | JWT + Refresh Token (HttpOnly Cookie) |
| Veritabanı | PostgreSQL 16 + Entity Framework Core 9 |
| Cache | Redis 7 |

## Başlangıç

### Gereksinimler
- .NET 9 SDK
- Node.js 20+
- Docker Desktop

### Kurulum

```bash
# 1. PostgreSQL ve Redis başlat
cd backend
docker compose up -d

# 2. Backend
dotnet run --project src/ChatApp.API

# 3. Frontend (yeni terminal)
cd ../frontend
cp .env.example .env.local
npm install
npm run dev
```

Uygulama: `http://localhost:3000`  
API Docs: `http://localhost:5000/swagger`

## Proje Yapısı

```
siduschat/
├── backend/
│   └── src/
│       ├── ChatApp.Domain/        # Entity'ler, interface'ler
│       ├── ChatApp.Application/   # İş mantığı, servisler
│       ├── ChatApp.Infrastructure/# EF Core, Redis, JWT
│       └── ChatApp.API/           # Controller'lar, Hub, Middleware
└── frontend/
    ├── app/                       # Next.js sayfaları
    ├── components/                # UI bileşenleri
    ├── hooks/                     # Custom hook'lar
    ├── lib/                       # API client, store, utils
    └── types/                     # TypeScript tipleri
```
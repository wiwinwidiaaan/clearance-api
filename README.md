# Clearance API

Backend REST API untuk platform e-commerce clearance sale — marketplace untuk barang
surplus, retur, dan overstock. Menangani katalog produk, manajemen stok & diskon,
proses order, dan autentikasi user. Dipakai bersama oleh web storefront (React) dan
mobile app (React Native).

## Tech Stack

- **ASP.NET Core 8** — Web API
- **SQL Server** — database
- **Entity Framework Core** — ORM & migration
- **ASP.NET Core Identity + JWT** — autentikasi & otorisasi
- **Docker & Docker Compose** — containerization
- **GitHub Actions** — CI/CD (build, test, build image)

## Fitur

- 🔐 Register & login dengan JWT Bearer token
- 📦 Katalog produk dengan kategori, kondisi barang (baru/overstock/retur/refurbished), dan pencarian
- 🏷️ Manajemen diskon, termasuk flash-sale dengan periode aktif
- 🛒 Checkout dengan validasi stok real-time (pakai database transaction, aman dari race condition)
- 📋 Riwayat pesanan per user

## Menjalankan Secara Lokal

### Opsi 1: Docker (direkomendasikan)

```bash
docker compose up --build
```

API akan jalan di `http://localhost:8080`, Swagger UI di `http://localhost:8080/swagger`.

### Opsi 2: Manual (perlu .NET 8 SDK & SQL Server terinstall)

```bash
dotnet restore
dotnet ef database update
dotnet run
```

## Struktur Project

```
Controllers/     -> AuthController, ProductsController, OrdersController
Models/          -> Product, Inventory, Discount, Order, OrderItem, ApplicationUser
DTOs/            -> Data transfer object (request/response API)
Data/            -> AppDbContext (EF Core)
Services/        -> TokenService (generate JWT)
Migrations/      -> EF Core migration history
Program.cs       -> Entry point & konfigurasi (Identity, JWT, CORS, Swagger)
```

## Endpoint Utama

| Method | Endpoint               | Keterangan                                | Butuh Auth |
| ------ | ---------------------- | ----------------------------------------- | ---------- |
| POST   | `/api/auth/register`   | Daftar akun baru                          | Tidak      |
| POST   | `/api/auth/login`      | Login, dapat JWT token                    | Tidak      |
| GET    | `/api/products`        | List produk (filter `category`, `search`) | Tidak      |
| GET    | `/api/products/{id}`   | Detail produk                             | Tidak      |
| POST   | `/api/products`        | Tambah produk baru                        | Ya         |
| POST   | `/api/orders/checkout` | Buat pesanan dari keranjang               | Ya         |
| GET    | `/api/orders`          | Riwayat pesanan user yang login           | Ya         |

## Konfigurasi

Salin nilai di `appsettings.json` sesuai environment Anda, terutama:

- `ConnectionStrings:DefaultConnection` — koneksi ke SQL Server
- `Jwt:Key` — secret untuk sign token (**ganti** sebelum deploy produksi)

Untuk produksi, kredensial ini sebaiknya disimpan sebagai environment variable /
secret, bukan langsung di `appsettings.json`. Detail lengkap ada di `LEARNING-NOTES.md`.

## Project Terkait

- **[Clearance Web](https://github.com/wiwinwidiaaan/clearance-web)** — web storefront (React), konsumsi API ini untuk browsing & checkout
- **[Clearance Mobile](https://github.com/wiwinwidiaaan/clearance-mobile)** — mobile app (React Native/Android), shopping experience + notifikasi flash-sale

## Lisensi

MIT — bebas dipakai sebagai referensi untuk project Anda sendiri.

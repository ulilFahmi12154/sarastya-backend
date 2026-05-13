# TaskFlow API

TaskFlow API adalah backend RESTful untuk mengelola proyek dan tugas. API ini memiliki autentikasi JWT, CRUD project, CRUD task, validasi input, logging, dokumentasi Swagger/OpenAPI, dan database PostgreSQL.

## Fitur Utama

- Register dan login user.
- JWT Bearer Token untuk proteksi endpoint project dan task.
- CRUD project.
- CRUD task yang berelasi ke project.
- Relasi one-to-many: satu project memiliki banyak task.
- Password user disimpan sebagai hash BCrypt, bukan plain text.
- Response API menggunakan format `ApiResponse` yang konsisten.
- Swagger UI dengan tombol `Authorize` untuk JWT.
- Logging Serilog ke console dan file lokal.

## Teknologi

- ASP.NET Core 8
- PostgreSQL
- Entity Framework Core
- Dapper
- JWT Bearer Authentication
- BCrypt.Net
- FluentValidation
- Serilog
- Swagger/OpenAPI
- Docker dan Docker Compose

## Struktur Folder

```text
.
├── database/
│   └── schema.sql
├── src/
│   ├── TaskFlow.API/
│   │   ├── Controllers/
│   │   ├── Middleware/
│   │   └── Program.cs
│   ├── TaskFlow.Application/
│   │   ├── DTOs/
│   │   └── Validation/
│   ├── TaskFlow.Core/
│   │   ├── Domain/
│   │   └── Interfaces/
│   └── TaskFlow.Infrastructure/
│       ├── Persistence/
│       ├── Repositories/
│       └── Security/
├── docker-compose.yml
├── Dockerfile
└── README.md
```

## Arsitektur Singkat

Project ini memakai pendekatan Clean Architecture/Onion sederhana:

- `TaskFlow.Core` berisi entity domain dan kontrak repository.
- `TaskFlow.Application` berisi DTO dan FluentValidation validator.
- `TaskFlow.Infrastructure` berisi akses database, repository, dan password hasher.
- `TaskFlow.API` berisi controller, middleware, Swagger, JWT, dan dependency injection.

Pola akses data:

- Read (`GET`) menggunakan Dapper/raw SQL.
- Create, update, delete (`POST`, `PUT`, `DELETE`) menggunakan Entity Framework Core.

## Konfigurasi Environment

File `.env` sengaja diabaikan oleh Git. Untuk local override, copy file contoh:

```bash
cp .env.example .env
```

Nilai default di `docker-compose.yml` aman untuk development lokal. Untuk production, gunakan environment variable asli dari platform deployment dan jangan commit secret ke repository.

Contoh variable:

```env
POSTGRES_DB=taskflow
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
JWT_ISSUER=TaskFlow
JWT_AUDIENCE=TaskFlowClients
JWT_KEY=CHANGE_THIS_TO_A_LONG_RANDOM_SECRET_FOR_LOCAL_DEV_ONLY
ASPNETCORE_ENVIRONMENT=Development
```

## Menjalankan Dengan Docker Compose

Pastikan Docker Desktop atau Docker Engine sudah berjalan, lalu jalankan dari root repository:

```bash
docker compose up --build
```

Service yang berjalan:

- API: `http://localhost:8080`
- PostgreSQL: `localhost:5432`
- Swagger UI: `http://localhost:8080/swagger`

Untuk menjalankan di background:

```bash
docker compose up --build -d
```

Untuk menghentikan container:

```bash
docker compose down
```

Jika ingin reset database lokal beserta volume:

```bash
docker compose down -v
docker compose up --build
```

## Database PostgreSQL

Database berjalan di container `postgres:16-alpine`. Schema awal dibuat dari:

```text
database/schema.sql
```

Tabel utama:

- `users`
- `projects`
- `tasks`

Relasi:

- `projects.id` ke `tasks.project_id`
- Saat project dihapus, task terkait ikut terhapus dengan cascade delete.

## Akses Swagger UI

Setelah container berjalan, buka:

```text
http://localhost:8080/swagger
```

Swagger sudah dikonfigurasi dengan JWT Bearer. Setelah login, klik tombol `Authorize`, lalu masukkan token dengan format:

```text
Bearer TOKEN_DARI_LOGIN
```

## Alur Test API

### 1. Register

Endpoint:

```http
POST /api/auth/register
```

Body:

```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

### 2. Login

Endpoint:

```http
POST /api/auth/login
```

Body:

```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

Response login akan berisi JWT token.

### 3. Authorize Di Swagger

Copy token dari response login, klik `Authorize`, lalu isi:

```text
Bearer TOKEN_DARI_LOGIN
```

### 4. CRUD Project

Gunakan endpoint:

- `GET /api/projects`
- `GET /api/projects/{id}`
- `POST /api/projects`
- `PUT /api/projects/{id}`
- `DELETE /api/projects/{id}`

Contoh body create project:

```json
{
  "name": "Website Redesign",
  "description": "Redesign landing page dan dashboard",
  "startDate": "2026-05-13T00:00:00",
  "endDate": "2026-06-13T00:00:00"
}
```

### 5. CRUD Task

Gunakan endpoint:

- `GET /api/tasks`
- `GET /api/tasks?projectId={projectId}`
- `GET /api/tasks/{id}`
- `POST /api/tasks`
- `PUT /api/tasks/{id}`
- `DELETE /api/tasks/{id}`

Contoh body create task:

```json
{
  "projectId": "GUID_PROJECT",
  "title": "Buat endpoint task",
  "content": "Implementasi CRUD task",
  "status": "Todo",
  "priority": 1,
  "dueDate": "2026-05-20T00:00:00"
}
```

Nilai status yang tersedia:

- `Todo`
- `Doing`
- `Done`

## Logging

Serilog mencatat log ke:

- Console container.
- File harian di folder `logs/`.

Folder `logs/` diabaikan oleh Git.

## Catatan Deployment

- Jangan gunakan JWT key development untuk production.
- Set `JWT_KEY`, `POSTGRES_PASSWORD`, dan connection string melalui environment variable di platform deployment.
- Jangan commit `.env`, `appsettings.Production.json`, backup database, atau file credential lokal.
- Swagger saat ini aktif pada environment `Development`.
- Jika Swagger ingin dibuka di production, aktifkan secara sadar dan lindungi aksesnya sesuai kebutuhan.

## Status Repository

Repository ini disiapkan untuk GitHub dengan:

- `.gitignore` untuk file build, IDE, log, dan secret lokal.
- `.env.example` sebagai template konfigurasi.
- Docker Compose untuk API dan PostgreSQL.
- README backend untuk setup dan presentasi.

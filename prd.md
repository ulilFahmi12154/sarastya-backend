1. Product Requirements Document (PRD) - Backend
Nama Proyek: TaskFlow API (Sistem Manajemen Proyek & Tugas)
Deskripsi: RESTful API untuk mengelola proyek dan tugas-tugas di dalamnya, dilengkapi dengan sistem autentikasi.

Fitur Utama (MVP)
Autentikasi & Otorisasi: Registrasi, Login, dan proteksi endpoint menggunakan JWT.

Manajemen Proyek (Entitas Utama 1): CRUD Proyek (Nama, Deskripsi, Tanggal Mulai).

Manajemen Tugas (Entitas Utama 2): CRUD Tugas yang berelasi dengan Proyek (Judul, Status: Todo/Doing/Done, Deadline).

Relasi: One-to-Many (Satu Proyek memiliki banyak Tugas).

Kriteria Teknis
Framework: ASP.NET Core 8.0 LTS.

Database: PostgreSQL.

Arsitektur: Clean Architecture atau Onion Architecture (Modular).

Data Access:

Read: Raw SQL (Dapper) atau Stored Procedures (untuk efisiensi).

CUD: Entity Framework Core (untuk kemudahan manipulasi data).

Keamanan: JWT Bearer Token, Password Hashing (BCrypt/Argon2).

Lainnya: Serilog (Logging), FluentValidation, Global Exception Handling, Swagger UI.
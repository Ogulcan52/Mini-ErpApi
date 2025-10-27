# ERP (Clean-ish) — SQL Server + JWT + Katmanlı Mimari (.NET 8)

Bu paket; SQL Server'a geçmiş, JWT kimlik doğrulama içeren ve katmanlı bir mimariyle düzenlenmiş **çalışan** bir ERP Web API örneğidir.

## Katmanlar
- **ERP.Domain**: Entity ve temel modeller
- **ERP.Application**: DTO'lar, arayüzler, mapping, validation
- **ERP.Infrastructure**: EF Core SQL Server, Identity, JWT üretimi, servis implementasyonları
- **ERP.Api**: Controller'lar, DI ve pipeline

## Hızlı Başlangıç
1) `appsettings.Development.json` içinde `DefaultConnection` değerini kendi SQL Server bağlantı dizesiyle güncelleyin.
2) Terminal:
```bash
cd ERP.Api
dotnet restore
dotnet ef migrations add InitialCreate -p ../ERP.Infrastructure -s .
dotnet ef database update -p ../ERP.Infrastructure -s .
dotnet run
```
3) Swagger: `https://localhost:7184/swagger` (veya konsolda yazan port)
4) Önce kullanıcı oluşturun:
   - `POST /api/auth/register`  (email, password)
   - `POST /api/auth/login` => **JWT** döner
5) JWT'yi Swagger'da **Authorize** butonuna `Bearer {token}` şeklinde yapıştırın.

> Not: İlk çalıştırmada örnek ürün/müşteri seed'i otomatik eklenir.

## Ekstra
- Policy: `RequireAdmin` (Role = "Admin") — admin kullanıcısı seed edilir (email: `admin@erp.local`, şifre: `Admin*12345`).
- Katmanlar arası bağımlılıklar: Api -> Application, Infrastructure; Infrastructure -> Domain; Application -> Domain.

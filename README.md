# Cargo Carrier Selection API

Sipariş oluşturulurken girilen desi bilgisine göre en uygun kargo firmasını otomatik seçen .NET 6 Web API projesidir.

## Kullanılan Teknolojiler

- .NET 6 Web API
- Entity Framework Core 6 (Code First)
- Microsoft SQL Server (LocalDB / Express / Full)
- Swagger (Swashbuckle)
- Hangfire (Background Jobs)
- Repository Pattern
- N-Tier Architecture

## Mimari

```
CargoAPI.sln
├── CargoAPI.API              → Controller'lar, Program.cs, Swagger
├── CargoAPI.Business         → Servis arayüzleri ve iş mantığı
├── CargoAPI.DataAccess       → DbContext, Repository'ler, Migration'lar
└── CargoAPI.Entities         → Entity sınıfları, DTO'lar
```

**Referans zinciri:** API → Business → DataAccess → Entities

## Gereksinimler

- [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) (x64 önerilir)
- SQL Server (LocalDB, Express veya Full)
- dotnet-ef CLI aracı

dotnet-ef kurulumu:

```bash
dotnet tool install --global dotnet-ef
```

## Veritabanı Tabloları

| Tablo | Açıklama |
|---|---|
| Carriers | Kargo firmaları |
| CarrierConfigurations | Kargo firma konfigürasyonları (desi aralıkları ve fiyatlar) |
| Orders | Siparişler |
| CarrierReports | Kargo firması günlük maliyet raporları |
| Hangfire tables | Hangfire job storage (State, Job, Server, etc.) |

## Kurulum

### 1. Bağlantı Dizesini Ayarlayın

`CargoAPI.API/appsettings.json` dosyasındaki bağlantı dizesini kendi ortamınıza göre güncelleyin:

**LocalDB (Visual Studio ile birlikte gelir):**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=CargoDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

**SQL Server Express:**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=CargoDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

**SQL Server (varsayılan instance):**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=CargoDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### 2. Migration Oluşturun ve Veritabanını Güncelleyin

**Seçenek A: EF Core Migrations kullanın**

```bash
dotnet ef migrations add InitialCreate --project CargoAPI.DataAccess --startup-project CargoAPI.API
dotnet ef database update --project CargoAPI.DataAccess --startup-project CargoAPI.API
```

**Seçenek B: SQL Script kullanın**

`database/CargoDb_Create.sql` dosyası tüm migration'ları içeren idempotent bir scripttir. SQL Server Management Studio (SSMS) veya sqlcmd ile çalıştırabilirsiniz:

```bash
sqlcmd -S "(localdb)\MSSQLLocalDB" -i database/CargoDb_Create.sql
```

> **Not:** Hangfire kendi iç tablolarını (Hangfire.State, Hangfire.Job, vb.) uygulama ilk çalıştığında otomatik olarak oluşturur. Bu tablolar SQL script'te yer almaz.

### 3. Projeyi Çalıştırın

```bash
dotnet run --project CargoAPI.API
```

Swagger arayüzü: **http://localhost:5246/swagger**

> **Sorun giderme:** .NET 6 SDK yerine daha yeni bir SDK kullanıyorsanız ve runtime hatası alırsanız, komutların başına `DOTNET_ROLL_FORWARD=LatestMajor` ekleyebilirsiniz. Ancak önerilen yöntem .NET 6 SDK'yı yüklemektir.

## Hangfire Kurulumu ve Çalıştırma

Bu projede Hangfire, bonus geliştirme kapsamında arka plan işlerini yönetmek için kullanılmıştır.

### Kurulum

Hangfire için manuel bir kurulum adımı gerekmez. Gerekli NuGet paketleri proje içerisinde tanımlıdır:

- `Hangfire.AspNetCore`
- `Hangfire.SqlServer`

Uygulama çalıştırıldığında, Hangfire otomatik olarak aynı MSSQL / LocalDB veritabanına bağlanır ve kendi iç tablolarını (Hangfire.State, Hangfire.Job, vb.) ilk çalıştırmada otomatik olarak oluşturur.

### Dashboard

Hangfire Dashboard, arka plan işlerini izlemek ve yönetmek için kullanılır.

**URL:** http://localhost:5246/hangfire

Dashboard üzerinden:
- Recurring jobs (saatlik rapor job'u)
- Job history ve status
- Failed jobs ve retry işlemleri
- Server status

gibi bilgileri görebilirsiniz.

### Recurring Job: carrier-reports

**Job adı:** `carrier-reports`

**Çalışma sıklığı:** Her saat başı

**İşlem adımları:**
1. Tüm siparişleri okur
2. Siparişleri `CarrierId` ve `OrderDate.Date` alanlarına göre gruplar
3. Her grup için `OrderCarrierCost` değerlerini toplar
4. `CarrierReports` tablosunda aynı `CarrierId` ve tarih için rapor varsa günceller, yoksa yeni kayıt ekler (upsert)

**Manuel tetikleme endpoint'i:**
```
POST /api/CarrierReports/generate
```

**Raporları listeleme endpoint'i:**
```
GET /api/CarrierReports
```

**Örnek hesaplama:**
- Sipariş 1: `orderDesi: 5` → 32.00₺
- Sipariş 2: `orderDesi: 13` → 44.00₺
- Toplam rapor maliyeti: **76.00₺**

## API Endpoints

### Kargo Firmaları

| Metot | Route | Açıklama |
|---|---|---|
| GET | /api/Carriers | Tüm kargo firmalarını listele |
| POST | /api/Carriers | Kargo firması ekle |
| PUT | /api/Carriers | Kargo firması güncelle |
| DELETE | /api/Carriers/{id} | Kargo firması sil |

### Kargo Firma Konfigürasyonları

| Metot | Route | Açıklama |
|---|---|---|
| GET | /api/CarrierConfigurations | Tüm konfigürasyonları listele |
| POST | /api/CarrierConfigurations | Konfigürasyon ekle |
| PUT | /api/CarrierConfigurations | Konfigürasyon güncelle |
| DELETE | /api/CarrierConfigurations/{id} | Konfigürasyon sil |

### Siparişler

| Metot | Route | Açıklama |
|---|---|---|
| GET | /api/Orders | Tüm siparişleri listele |
| POST | /api/Orders | Sipariş oluştur (desi gönderilir, kargo otomatik seçilir) |
| DELETE | /api/Orders/{id} | Sipariş sil |

### Kargo Raporları (Hangfire)

| Metot | Route | Açıklama |
|---|---|---|
| GET | /api/CarrierReports | Tüm kargo raporlarını listele |
| POST | /api/CarrierReports/generate | Raporları manuel olarak oluştur/trigger et |

## Swagger Test Sırası ve Örnek JSON'lar

Aşağıdaki sırayla test edin:

### 1. Kargo Firması Ekle (POST /api/Carriers)

```json
{
  "carrierName": "Yurtiçi Kargo",
  "carrierIsActive": true,
  "carrierPlusDesiCost": 4,
  "carrierConfigurationId": 0
}
```

### 2. Konfigürasyon Ekle (POST /api/CarrierConfigurations)

```json
{
  "carrierId": 1,
  "carrierMaxDesi": 10,
  "carrierMinDesi": 1,
  "carrierCost": 32.00
}
```

### 3. Sipariş Ekle — Desi Aralık İçinde (POST /api/Orders)

```json
{
  "orderDesi": 5
}
```

**Beklenen sonuç:** `"Sipariş eklendi. Kargo ücreti: 32.00₺"` (5 desisi 1–10 aralığında)

### 4. Sipariş Ekle — Desi Aralık Dışında (POST /api/Orders)

```json
{
  "orderDesi": 13
}
```

**Beklenen sonuç:** `"Sipariş eklendi. Kargo ücreti: 44₺"` (32 + 4 × 3 = 44)

### 5. Siparişleri Listele (GET /api/Orders)

Her iki siparişi de `carrierId` ve `orderCarrierCost` alanlarıyla birlikte gösterir.

### 6. Raporları Oluştur (POST /api/CarrierReports/generate)

Siparişler üzerinden kargo başına günlük toplam maliyet raporlarını oluşturur.

**Çalışma mantığı:**
- Tüm siparişleri okur
- Siparişleri CarrierId ve OrderDate.Date'e göre gruplar
- Her grup için OrderCarrierCost toplamını hesaplar
- Aynı CarrierId ve tarih için rapor varsa günceller, yoksa yeni oluşturur (upsert)

### 7. Raporları Listele (GET /api/CarrierReports)

Oluşturulan raporları listeler. Örnek çıktı:

```json
[
  {
    "carrierReportId": 1,
    "carrierId": 1,
    "carrierCost": 76.00,
    "carrierReportDate": "2026-04-29T00:00:00"
  }
]
```

Anlamı: "29 Nisan 2026 tarihinde, 1 numaralı kargo firmasına toplam 76.00₺ ödeme yapılmıştır."

## Otomatik Rapor Job'u

Hangfire recurring job her saat başı otomatik olarak raporları günceller. Job:
- Siparişleri okur
- Günlük kargo başına toplam maliyetleri hesaplar
- CarrierReports tablosuna kaydeder/günceller

## Kargo Ücreti Hesaplama Mantığı

### Durum 1: Desi bir aralığın içinde

Sipariş desisi herhangi bir konfigürasyonun MinDesi–MaxDesi aralığına giriyorsa, bu aralıktaki **en düşük fiyatlı** kargo firması seçilir.

### Durum 2: Desi hiçbir aralığın içinde değil

1. Tüm aktif konfigürasyonlar arasından sipariş desisine **en yakın MaxDesi** değerine sahip olan bulunur.
2. Desi farkı hesaplanır: `fark = |siparişDesi - maxDesi|`
3. Ek maliyet hesaplanır: `sonFiyat = kargoFiyatı + (plusDesiÜcreti × fark)`

**Örnek:**

```
Sipariş Desisi: 13
Konfigürasyon MaxDesi: 10
Kargo Fiyatı: 32₺
+1 Desi Fiyatı: 4₺

Hesaplama: 32 + (4 × (13 - 10)) = 32 + 12 = 44₺
```

## Validasyon Kuralları

| Alan | Kural |
|---|---|
| CarrierName | Boş olamaz |
| CarrierPlusDesiCost | 0 veya daha büyük olmalı |
| CarrierMinDesi | 0'dan büyük olmalı |
| CarrierMaxDesi | CarrierMinDesi'den küçük olamaz |
| CarrierCost | 0'dan büyük olmalı |
| OrderDesi | 0'dan büyük olmalı |

Geçersiz istek gönderildiğinde API **400 Bad Request** ile hata mesajı döner.

## Hata Yönetimi ve Logging

### Global Exception Middleware

Uygulamada beklenmeyen hataları yakalamak için global exception middleware kullanılmıştır. Bu yapı sayesinde kontrol edilmeyen hatalarda API tutarlı bir JSON yanıtı döndürür ve hassas stack trace bilgileri dışarıya açılmaz.


Örnek hata yanıtı:

```json
{
  "statusCode": 500,
  "message": "Beklenmeyen bir hata oluştu."
}
```

sqllocaldb start MSSQLLocalDB
sqllocaldb info MSSQLLocalDB
dotnet run --project CargoAPI.API

dotnet ef database update --project CargoAPI.DataAccess --startup-project CargoAPI.API

---


**Bu proje Mahmoud Karzoun tarafından tasarlanmış ve geliştirilmiştir.**

# Course.Assessment

**Course.Assessment**, .NET (C#) ile geliştirilmiş bir kurs ödevi için yapılmış bir uygulamadır. Bu proje, kurs değerlendirme gereksinimlerini karşılamak amacıyla hazırlanmış olup Docker ve .NET ekosistemi ile entegre çalışır.

---

## 📌 İçindekiler

- [Genel Bakış](#-genel-bakış)  
- [Özellikler](#-özellikler)  
- [Teknolojiler](#-teknolojiler)  
- [Kurulum & Çalıştırma](#-kurulum--çalıştırma)  
  - [Gerekli Yazılımlar](#-gerekli-yazılımlar)  
  - [Proje Yapılandırması](#-proje-yapılandırması)  
  - [Config Yapılandırması](#-config-yapilandirması)  

---

## 🧠 Genel Bakış

Bu proje, kuyruk sistemlerini öğrenmek ve mikroservis mimarisine adapte olmak amacıyla hazırlanmıştır.  
Kod tabanı .NET ile yazılmış olup backend servisler, API ve iş mantığını içerir. Proje, **DDD**, **Clean Architecture** ve **Event Driven Design** prensiplerine uygun olarak geliştirilmiştir.

---

## ✨ Özellikler

- .NET (C#) tabanlı backend uygulaması  
- RESTful API endpoints  
- Minimal Apis
- Docker destekli çalışma ortamı  
- Aspire ile entegre çalışabilir  
- DDD ve Clean Arhitecture'a uygun tasarım  
- Kolayca genişletilebilir ve geliştirilebilir açık mimari  

---

## 🛠️ Teknolojiler

| Teknoloji | Açıklama |
|-----------|-----------|
| .NET / C# | Uygulama dili ve framework |
| RedisStreams | Kuyruk sistemi |
| Kafka | Kuyruk sistemi |
| RabbitMQ | Kuyruk sistemi |
| Docker | Containerization |
| Aspire | Containerization ve çalıştırma |
| Entity Framework Core | ORM (Object-Relational Mapping) |
| PostgreSQL | Veri tabanı |
| Quartz | Delayed queue ve zamanlanmış görevler |

---

## 🚀 Kurulum & Çalıştırma

### 📎 Gerekli Yazılımlar

- [.NET SDK (.NET 10)](https://dotnet.microsoft.com/)  
- [.NET Aspire SDK](https://aspire.dev/get-started/aspire-sdk/)  
- [Docker](https://www.docker.com/)  

### 📂 Proje Yapılandırması

1. Docker:
   ```bash
   git clone https://github.com/muratyasar7/Course.Assessment.git
   cd Course.Assessment
   docker-compose up -d
2. Aspire
   ```bash
    git clone https://github.com/muratyasar7/Course.Assessment.git
    cd  Course.Assessment/src/Aspire/Course.Assessment.AppHost
    dotnet run

### ⚙️ Config Yapılandırması
Proje 3 farklı kuyruk sisteminde de çalışacak şekilde ayarlanmıştır. Bir tanesi seçilmelidir. Dockerda environment variable olarak, Aspire'da da appsettings içine verilmesi gerekmektedir. **QueueSystem** key ile setlenmesi gerekmektedir. Sadece Docker ile çalıştığında Kafka consumerda bir sorun var. Bu düzeltilecektir. 
1. Docker:
   ```bash
    QueueSystem: RabbitMq  # RabbitMq, Kafka, RedisStreams
    Environment Variable olarak verilmesi gerekmektedir
2. Aspire
   ```bash
   "QueueSystem": "Kafka" // RabbitMq, Kafka, RedisStreams
   Appsettings Json içinde verilmesi gerekmektedir 

Başka bir ayar yapılmasına gerek yoktur. Db otomatik oluşacaktır. 

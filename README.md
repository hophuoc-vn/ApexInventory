# Apex: Enterprise-Grade .NET 10 Web API

**Apex** is a high-performance, scalable backend system engineered with **Clean Architecture** and **Domain-Driven Design (DDD)** principles. It is designed to handle high-precision financial transactions, automated background analytics, and robust data integrity for modern fintech applications.

---

## Key Architectural Pillars

### 1. Clean Architecture & Decoupling
The system is divided into four distinct layers to ensure a "Plug-and-Play" infrastructure:
* **Domain:** The core "Brain" containing business entities and repository contracts. Zero dependencies on external frameworks.
* **Application:** Coordinates data flow using **DTOs**, **AutoMapper**, and **FluentValidation**.
* **Infrastructure:** The "Muscle" implementing **Entity Framework Core**, **Unit of Work**, and **Background Workers**.
* **API:** The "Gateway" utilizing **JWT Authentication**, **API Versioning (v1.0)**, and **Global Exception Middleware**.

### 2. High-Performance Background Processing
Unlike standard APIs that calculate reports on-the-fly, Apex utilizes a dedicated `SalesStatisticsWorker` (`BackgroundService`):
* **Incremental Aggregation:** Instead of scanning millions of rows, the worker processes only the "delta" (new orders) since the last execution.
* **Thread Safety:** Implements the **Manual Scope Pattern** using `IServiceScopeFactory` to safely manage database contexts within a Singleton service.

### 3. Financial Data Integrity
* **Precision:** All monetary values are strictly enforced with `Decimal(18, 4)` precision.
* **Concurrency:** Utilizes **Optimistic Concurrency (RowVersion)** to prevent race conditions during high-traffic inventory updates.
* **Auditability:** Implements **Soft Delete** logic via Global Query Filters, ensuring data is never truly lost but remains invisible to standard queries.

---

## Testing Strategy (The Testing Pyramid)

Apex is built for 100% verifiability. The testing suite includes:
* **Unit Tests:** Isolated testing of Domain logic and Business Rules using **Moq**.
* **Integration Tests:** Database-level verification of complex queries, Soft Delete filters, and Unique Index constraints.
* **System Tests:** Lifecycle testing of the `SalesStatisticsWorker` to ensure memory efficiency and scope disposal.
* **Middleware Tests:** Validating that the Global Exception Handler returns professional `ErrorResponse` DTOs.

---

## Tech Stack & Patterns

* **Framework:** .NET 10 (C# 14)
* **Database:** SQL Server / Entity Framework Core
* **Patterns:** Repository & Unit of Work, Decorator Pattern (via Scrutor), Singleton-to-Scoped Worker.
* **Security:** JWT Bearer Authentication, Password Hashing.
* **Observability:** Serilog (File & Console logging).

---

## Developed by Jho
*Software Developer based in Vietnam | Specialized in .NET Backend & Fintech Systems.*
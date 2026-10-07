 AccessoriesStore

A backend-focused e-commerce API built with **ASP.NET Core 8**, following **Clean Architecture** principles and designed for a scalable accessories store.

The project provides the core backend infrastructure for managing products, categories, customers, shopping carts, orders, inventory, payments, product reviews, wishlists, authentication, and image storage.

> **Project Status:** Backend development in progress
> **Frontend:** Angular

---

 ✨ Features

 🔐 Authentication & Authorization

* User registration and login
* JWT-based authentication
* Refresh token support
* ASP.NET Core Identity
* Role-based authorization
* Password reset flow
* Secure token configuration using User Secrets

 🛍️ Product Management

* Product CRUD operations
* Product categories
* Product filtering
* Product pagination
* Product images
* Product ratings and reviews
* Slug generation

 🛒 Shopping Cart

* Add products to cart
* Update cart item quantities
* Remove cart items
* Retrieve current user's cart

 📦 Orders & Inventory

* Create orders
* Order item management
* Order status management
* Order confirmation
* Stock management
* Inventory updates

 💳 Payments

* Payment workflow
* Paymob integration
* Payment transaction handling
* Paymob webhook support

 ❤️ Wishlist

* Add products to wishlist
* Remove products from wishlist
* Retrieve user's wishlist

 ⭐ Product Reviews

* Create product reviews
* Retrieve product reviews
* Product rating calculations

 🖼️ Image Management

* Product image upload
* Cloudinary integration
* Image storage abstraction

 📧 Email

* Email service abstraction
* SMTP-based email configuration
* Designed to support authentication and application notifications

 📚 API Documentation

* Swagger / OpenAPI
* JWT Bearer authentication support
* RESTful API endpoints

---

 🏗️ Architecture

The backend follows **Clean Architecture** to keep business logic independent from infrastructure and framework-specific concerns.

```text
                    ┌──────────────────────┐
                    │      API Layer       │
                    │ Controllers / HTTP   │
                    └──────────┬───────────┘
                               │
                               ▼
                    ┌──────────────────────┐
                    │ Application Layer    │
                    │ DTOs / Abstractions  │
                    │ Services Contracts    │
                    └──────────┬───────────┘
                               │
                               ▼
                    ┌──────────────────────┐
                    │    Domain Layer      │
                    │ Entities / Enums     │
                    │ Business Rules       │
                    └──────────────────────┘
                               ▲
                               │
                    ┌──────────┴───────────┐
                    │ Infrastructure Layer │
                    │ EF Core / Identity   │
                    │ Cloudinary / Paymob  │
                    │ Email / Services     │
                    └──────────────────────┘
```

 Solution Structure

```text
AccessoriesStore/
│
├── AccessoriesStore.Api/
│   ├── Controllers/
│   ├── DTOs/
│   ├── Filters/
│   ├── Middleware/
│   └── Program.cs
│
├── AccessoriesStore.Application/
│   ├── Abstractions/
│   ├── Common/
│   ├── DTOs/
│   ├── Features/
│   └── Mappings/
│
├── AccessoriesStore.Domain/
│   ├── Entities/
│   ├── Enums/
│   └── Exceptions/
│
├── AccessoriesStore.Infrastructure/
│   ├── Identity/
│   ├── Persistence/
│   │   ├── Configurations/
│   │   └── Migrations/
│   ├── Services/
│   └── Settings/
│
└── AccessoriesStore.Tests/
```

---

 🛠️ Technologies

| Category          | Technology                  |
| ----------------- | --------------------------- |
| Language          | C#                          |
| Framework         | ASP.NET Core 8              |
| API               | ASP.NET Core Web API        |
| ORM               | Entity Framework Core       |
| Database          | SQL Server                  |
| Authentication    | ASP.NET Core Identity + JWT |
| Authorization     | Role-Based Authorization    |
| Image Storage     | Cloudinary                  |
| Payment Gateway   | Paymob                      |
| API Documentation | Swagger / OpenAPI           |
| Testing           | .NET Test Project           |
| Version Control   | Git / GitHub                |
| Frontend          | Angular                     |

---

 🔑 Authentication Flow

The application uses JWT access tokens together with refresh tokens.

```text
Client
   │
   ├── Register / Login
   │
   ▼
ASP.NET Core Identity
   │
   ▼
JWT Access Token + Refresh Token
   │
   ▼
Authenticated API Requests
   │
   └── Access Token expires
              │
              ▼
        Refresh Token
              │
              ▼
       New Access Token
```

Protected endpoints use the JWT Bearer authentication scheme and role-based authorization where required.

---

 🗄️ Database

The project uses:

* **SQL Server**
* **Entity Framework Core**
* Code First approach
* Fluent API configurations
* EF Core migrations
* ASP.NET Core Identity database integration

Database configuration is kept outside the repository's sensitive configuration.

---

 ☁️ External Services

 Cloudinary

Used for storing product images.

The application communicates with Cloudinary through an image-storage abstraction rather than coupling the application layer directly to the provider.

 Paymob

Used as the payment gateway integration.

The backend includes payment endpoints and a dedicated webhook endpoint for receiving payment callbacks.

 Email

The application contains an email service abstraction with SMTP configuration.

---

 🚀 Getting Started

 Prerequisites

Make sure you have installed:

* .NET 8 SDK
* SQL Server
* Visual Studio 2022 or another compatible IDE
* Git

 1. Clone the repository

```bash
git clone https://github.com/LOZYG22/AccessoriesStore.git
cd AccessoriesStore
```

 2. Configure User Secrets

The project uses **ASP.NET Core User Secrets** for sensitive configuration values.

Sensitive values include:

* JWT secret
* Cloudinary credentials
* Paymob credentials
* Email credentials

Do **not** add these values directly to `appsettings.json` before committing the project.

For local development, configure the required secrets through Visual Studio or the .NET CLI.

 3. Configure SQL Server

The development database uses SQL Server.

Update the local connection string if your SQL Server instance is different from the default development configuration.

 4. Apply EF Core migrations

From the API project directory:

```bash
dotnet ef database update
```

 5. Run the application

```bash
dotnet run
```

Swagger will be available through the application's configured development URL.

---

 📖 API Documentation

Once the application is running in the development environment, open Swagger UI to explore and test the API.

The API currently contains endpoints for:

* Authentication
* Addresses
* Categories
* Products
* Product Images
* Cart
* Orders
* Inventory
* Payments
* Paymob Webhooks
* Product Reviews
* Wishlist

---

 🧪 Testing

The solution includes a dedicated test project:

```text
AccessoriesStore.Tests/
```

Tests are kept separate from the production application projects to maintain a clean solution structure.

---

 🔒 Security

Sensitive configuration is intentionally excluded from source control.

The repository does **not** contain:

* JWT secrets
* Cloudinary API secrets
* Paymob private credentials
* Email passwords
* Local development secrets

These values are supplied through **ASP.NET Core User Secrets** during local development.

---

 📌 Project Goals

AccessoriesStore is being developed as a practical full-stack e-commerce project with a focus on:

* Clean backend architecture
* Secure authentication and authorization
* Real-world e-commerce workflows
* External service integrations
* Maintainable code organization
* RESTful API design
* Production-oriented development practices

The project is also intended as a portfolio project demonstrating practical experience with the **.NET ecosystem**.

---

 🔮 Future Development

Planned development includes:

* Complete Angular frontend
* Responsive e-commerce UI
* Customer shopping experience
* Admin management interface
* Frontend integration with the REST API
* Deployment and production configuration
* Additional automated test coverage
* Further performance and security improvements

---

 👨‍💻 Author

**Mahmoud Shaaban Bondouk**

Junior Full-Stack .NET Developer

* GitHub: [LOZYG22](https://github.com/LOZYG22)
* Focus: C# · ASP.NET Core · EF Core · SQL Server · Angular

---

 📄 License

This project is currently intended as a personal portfolio and learning project.

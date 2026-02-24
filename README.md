# Support Ticket API

This is a Support Ticket Management API built using ASP.NET Core Web API with:

- Entity Framework Core
- SQL Server
- JWT Authentication
- Role-Based Access (Manager, Support, User)

---

## Setup Instructions

### 1. Clone the project

```bash
git clone <your-repo-url>
cd SupportTicketAPI
```

---

### 2. Update Database Connection

Open `appsettings.json` and update:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=SupportTicketDB;Trusted_Connection=True;TrustServerCertificate=True;"
},
"Jwt": {
  "Secret": "YOUR_SECRET_KEY_MIN_32_CHARS",
  "Issuer": "SupportTicketAPI",
  "Audience": "SupportTicketAPIUsers"
}
```

---

### 3. Apply Migrations

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

### 4. Install Required Packages (if needed)

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package BCrypt.Net-Next
dotnet add package FluentValidation.AspNetCore
```

---

## Run the Project

```bash
dotnet run
```

API will run at:

```
https://localhost:3000
```

Swagger:

```
https://localhost:3000/swagger
```

---

## How to Use

1. Login using `/api/auth/login`
2. Copy JWT token
3. Click **Authorize** in Swagger
4. Paste:  
   `Bearer YOUR_TOKEN`
5. Test protected endpoints

---

## Roles

- **Manager** → Manage users and all tickets
- **Support** → Manage assigned tickets
- **User** → Create and view own tickets

---


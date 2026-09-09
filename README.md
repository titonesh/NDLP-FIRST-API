# NDLP FIRST API

A enterprise-grade ASP.NET Web API backend built on **.NET Framework 4.8** that powers **Customer Profile Management**, **KYC Document Verification**, **Registered & Trusted Device Security**, and **Audit Trail Logging**.

---

## 🚀 Key Features & Modules

- **Customer Profile Management**
  - Search, retrieve, create, and update customer profiles with pagination and field-level updates.
- **KYC Verification & Compliance**
  - Submit KYC verification documents (National ID, Passport, Utility Bill, Tax Certificate, Proof of Address).
  - Verify KYC status, query tier limits, and evaluate compliance requirements.
- **Registered & Trusted Device Management**
  - Register customer mobile/web devices with hardware fingerprinting.
  - Manage trusted device statuses, biometric binding signatures, and device lifecycle (activation, deactivation, remote lock).
- **Security & Authorization**
  - OAuth 2.0 Bearer Token authentication via OWIN Katana middleware.
  - Role-based and claim-aware authorization headers.
- **Enterprise Standards**
  - Centralized **Global Exception Handling Filter** (`GlobalExceptionFilter`) returning standardized envelope responses (`ApiResponse<T>`).
  - Automated **Audit Trail Logging** (`AuditLog`) capturing requests, changes, IP addresses, and user agents.

---

## 🛠️ Technology Stack

| Component | Technology |
|---|---|
| **Framework** | .NET Framework 4.8 / ASP.NET Web API 2 |
| **Database ORM** | Entity Framework 6.4 (Code First / Migrations) |
| **Authentication** | ASP.NET Identity 2.2 + OWIN OAuth 2.0 |
| **JSON Serialization** | Newtonsoft.Json |
| **API Documentation & Testing** | Help Page / Postman Collection |

---

## 📁 Project Structure

```
NDLP-FIRST-API/
├── App_Data/
│   └── CustomerProfile_KYC_Device_PostmanCollection.json  # Postman test suite
├── App_Start/
│   ├── IdentityConfig.cs
│   ├── Startup.Auth.cs
│   └── WebApiConfig.cs
├── Controllers/
│   ├── AccountController.cs         # Auth & user registration
│   ├── CustomerProfileController.cs # Customer profile API
│   ├── DeviceController.cs          # Registered device management
│   └── KycController.cs             # KYC verification & compliance
├── Exceptions/
│   └── BusinessExceptions.cs        # Custom business exception types
├── Filters/
│   └── GlobalExceptionFilter.cs     # Global ASP.NET Web API error filter
├── Models/
│   ├── DTOs/                        # Request & Response Data Transfer Objects
│   ├── CustomerProfile.cs           # Entity Model
│   ├── KycRecord.cs                 # Entity Model
│   ├── RegisteredDevice.cs          # Entity Model
│   └── AuditLog.cs                  # Audit Entity Model
├── Services/
│   ├── CustomerService.cs
│   ├── KycService.cs
│   ├── DeviceService.cs
│   └── AuditService.cs
├── Web.config                       # Application configuration & connection strings
└── NDLP-FIRST-API.csproj
```

---

## 🔌 API Endpoints Summary

### 🔑 Authentication (`/token` & `/api/Account`)
| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/token` | Issue OAuth 2.0 Bearer token (`grant_type=password`) |
| `POST` | `/api/Account/Register` | Register a new user account |
| `GET` | `/api/Account/UserInfo` | Get authenticated user info |
| `POST` | `/api/Account/ChangePassword` | Change user password |

### 👤 Customer Profile (`/api/v1/customers`)
| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/v1/customers` | Search & list customer profiles (paginated) |
| `GET` | `/api/v1/customers/{customerId}` | Retrieve specific customer profile |
| `POST` | `/api/v1/customers` | Create a new customer profile |
| `PUT` | `/api/v1/customers/{customerId}/update` | Update customer profile details |

### 📄 KYC Verification (`/api/v1/customers/{customerId}/kyc`)
| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/v1/customers/{customerId}/kyc` | Fetch current KYC verification record |
| `POST` | `/api/v1/customers/{customerId}/kyc` | Submit new KYC documents |
| `POST` | `/api/v1/customers/{customerId}/kyc/validate` | Validate KYC compliance & tier limits |

### 📱 Device Management (`/api/v1`)
| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/v1/customers/{customerId}/devices` | List registered devices for customer |
| `POST` | `/api/v1/customers/{customerId}/devices` | Register a new device |
| `PUT` | `/api/v1/devices/{deviceId}/update` | Update device trust / biometric status |
| `POST` | `/api/v1/devices/{deviceId}/deactivate` | Deactivate/revoke a registered device |

---

## ⚙️ Getting Started & Local Setup

### Prerequisites
- **Visual Studio 2019 / 2022** (with *.NET desktop development* and *ASP.NET and web development* workloads)
- **SQL Server / LocalDB** (Express or Full edition)
- **Postman** (for API testing)

### Installation & Run

1. **Clone the Repository**
   ```bash
   git clone https://github.com/titonesh/NDLP-FIRST-API.git
   cd NDLP-FIRST-API
   ```

2. **Configure Database Connection**
   Open `NDLP-FIRST-API/Web.config` and verify the `DefaultConnection` string under `<connectionStrings>` points to your SQL Server instance:
   ```xml
   <add name="DefaultConnection" 
        connectionString="Data Source=(LocalDb)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\aspnet-NDLP-FIRST-API-20260807010109.mdf;Initial Catalog=aspnet-NDLP-FIRST-API-20260807010109;Integrated Security=True" 
        providerName="System.Data.SqlClient" />
   ```

3. **Apply Database Migrations**
   In Visual Studio, open **Package Manager Console** (`Tools > NuGet Package Manager > Package Manager Console`) and run:
   ```powershell
   Update-Database
   ```

4. **Run the API**
   Press `F5` or `Ctrl + F5` in Visual Studio to run the API via IIS Express. The server usually runs at:
   `https://localhost:44347/`

---

## 🧪 Testing with Postman

A pre-configured Postman Collection is included directly in the repository:
📁 `App_Data/CustomerProfile_KYC_Device_PostmanCollection.json`

### Importing into Postman:
1. Open **Postman**.
2. Click **Import** (top left).
3. Choose the file `App_Data/CustomerProfile_KYC_Device_PostmanCollection.json` from the repository directory.
4. Execute the **Token Request** endpoint first to retrieve an `access_token`, then use it to test Customer, KYC, and Device endpoints.

---

## 🛡️ Exception & Envelope Response Format

All API endpoints return standard HTTP status codes wrapped in a uniform response envelope:

```json
{
  "Success": true,
  "Message": "Customer profile retrieved successfully",
  "Data": {
    "CustomerId": "CUST-1001",
    "FullName": "John Doe",
    "Email": "john.doe@example.com",
    "KycTier": "Tier2"
  },
  "Errors": null,
  "Timestamp": "2026-09-09T17:42:00Z"
}
```

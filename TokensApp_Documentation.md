# TokensApp - Complete Flow and Debugging Step-by-Step Guide

## 1. Project Introduction
The **TokensApp** is a .NET Core 8 Web API built to manage vendor services (like Clinics, Saloons, etc.) and generate sequential appointment tokens for customers.

### Project Architecture Flow:
1. **Program.cs**: The entry point. Handles `AppDbContext` configuration, Dependency Injection, and Swagger middleware setup.
2. **appsettings.json**: Houses the `TokensDb` connection string targeting a local PostgreSQL instance `real_tokens_app`.
3. **AppDbContext (Data Layer)**: Entity Framework Core context linking Models to DB tables map.
4. **Models**: Defines four main entities: `VendorCategory`, `Vendor`, `VendorService`, and `Appointment`.
5. **TokensController**: Exposes 6 main REST APIs.

---

## 2. API Endpoints Flow

### A. Core APIs Available:
1. **Categories**: `GET /api/Tokens/categories` - Returns active categories.
2. **Vendors**: `GET /api/Tokens/vendors` - Returns vendors (with category mappings).
3. **Services**: `GET /api/Tokens/vendors/{id}/services` - Returns services for a specific vendor.
4. **Appointments**: `POST /api/Tokens/appointments` - Creates an appointment token.
5. **Get Appointment**: `GET /api/Tokens/appointments/{id}` - Gets details of the token.
6. **Cancel Appointment**: `PATCH /api/Tokens/appointments/{id}/cancel` - Cancels the token.

---

## 3. Step-by-Step Debugging and Testing Guide

### Step 1: Initialize Database
The application relies on PostgreSQL. Ensure the service is running on `Port 5432` with user `postgres` and password `Root@123`.

### Step 2: Start the Application
Run the project via the .NET CLI:
```bash
dotnet run
```
*Note: If you face a File Lock Error (MSB3026), run `taskkill /F /PID <process_id>` to kill the existing hung instance.*

### Step 3: Test API Flow (Postman or PowerShell)
We executed step-by-step tests:

#### 1. Test Categories (Success)
**Request:** `GET http://localhost:5241/api/Tokens/categories`
**Result:** Executed successfully, returning `vendorCategoryId`, `categoryName`, etc.

#### 2. Test Vendors (Success)
**Request:** `GET http://localhost:5241/api/Tokens/vendors`
**Result:** Successfully fetched vendor details like `vendorName` and `category`.

#### 3. Test Vendor Services (Failed - Debugged)
**Request:** `GET http://localhost:5241/api/Tokens/vendors/1/services`
**Result:** `HTTP 500 Internal Server Error`.
**Root Cause identified from logs:**
```
relation "vendor_services" does not exist
```
**Fix:** The backend model for `VendorService` evaluates to a DB table named `vendor_services`. However, EF Core Entity Migrations were not applied to the PostgreSQL database, meaning the actual table is missing in the DB.
**How to Fix:** Run `dotnet ef migrations add InitialCreate` and `dotnet ef database update` OR manually execute a SQL create script in pgAdmin for `vendor_services`.

#### 4. Test Appointments (Failed - Cascade Debug)
**Request:** `POST http://localhost:5241/api/Tokens/appointments`
**Result:** Similar `500 Internal Server error`.
**Root Cause:** Since the underlying schema creation (Entity Migrations) was skipped in the project, tables like `vendor_services` are missing.

---

## 4. Conclusion and Next Steps
1. The codebase is functionally correct in its API definition.
2. To complete the app flow seamlessly, the **Database Schema must be synchronized** with the defined C# Models via EF Core Migrations.
3. Once `vendor_services` and related constraints are present in the DB, the entire workflow of token generation will execute perfectly.

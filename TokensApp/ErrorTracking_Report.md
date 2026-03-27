# TokensApp Error Tracking Report

This document outlines all the areas in the TokensApp where error tracking and logging have been implemented to capture logical gaps, bad requests, and database failures.

## 1. Global Level Tracking
**Location:** `Program.cs` (Global Middleware)
- **Tracked Event:** Unhandled system exceptions that bypass controller logic.
- **Log Level:** `Error`
- **Response:** Standardized HTTP 500 JSON response preventing raw exception leakage.

## 2. API Level Tracking (TokensController.cs)
The `TokensController` is fully instrumented with `ILogger<TokensController>` to track execution flow and errors.

### A. GET `/api/tokens/categories`
- **Info Tracking:** Logs attempts to fetch active categories and successful retrievals.
- **Error Tracking:** `try-catch` captures issues like DB connection loss. Logs exact exception message and stack trace.

### B. GET `/api/tokens/vendors`
- **Info Tracking:** Logs inputs `CategoryId` and `City`, preventing silent failures of filters.
- **Error Tracking:** Captures LINQ/Entity framework parsing errors or DB mapping exceptions safely.

### C. GET `/api/tokens/vendors/{vendorId}/services`
- **Warning Tracking:** Logs when a `vendorId` does not exist or is marked inactive.
- **Error Tracking:** Captures failure in fetching related vendor services.

### D. POST `/api/tokens/appointments`
This endpoint involves complex logic (Token Generation) and has strict tracking.
- **Warning Tracking:** 
  - Tracks if the requested `VendorId` doesn't exist or is inactive.
  - Tracks if the requested `ServiceId` doesn't belong to the specified vendor or is inactive.
- **Error Tracking:**
  - `DbUpdateException`: specifically captured to track concurrency failures or primary/foreign key constraint violations during Token creation.
  - General `Exception`: Catches anything else and logs context around `VendorId`.

### E. GET `/api/tokens/appointments/{id}`
- **Warning Tracking:** Logs attempts to fetch non-existent `AppointmentId`s.
- **Error Tracking:** DB failure tracking when querying for appointment details.

### F. PATCH `/api/tokens/appointments/{id}/cancel`
- **Warning Tracking:**
  - Logs if the appointment does not exist.
  - Logs rule violations (e.g., trying to cancel an already `COMPLETED` or `CANCELLED` appointment).
- **Error Tracking:** Captures failures during the State modification (`Status = CANCELLED`) and saving to the database.

## Summary
With these improvements, any logic gap or system failure will automatically generate an entry in the backend console/log sink with precise details on which API, which parameters, and the nature of the failure, creating a traceable path for developers.

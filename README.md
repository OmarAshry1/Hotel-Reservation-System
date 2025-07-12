# Hotel Reservation System

A Windows Forms application for managing hotel operations including reservations, rooms, guests, staff, and payments.

## Features

- Room Management
  - Add/Edit/Delete rooms
  - Track room status and availability
  - Manage room types and capacities

- Guest Management 
  - Register new guests
  - Maintain guest information
  - View guest history

- Reservation Management
  - Create new reservations
  - Modify existing bookings
  - Check-in/Check-out processing

- Staff Management
  - Manage hotel staff records
  - Track staff assignments
  - Handle staff scheduling

- Payment Processing
  - Process guest payments
  - Generate invoices
  - Track payment history

## Technology Stack

- C# (.NET 9.0)
- Windows Forms
- SQL Server Database
- System.Data.SqlClient for database connectivity

## Requirements

- Windows 7 or later
- .NET 9.0 Runtime
- SQL Server Database
- Visual Studio 2022 or later (for development)

## Setup

1. Clone the repository
2. Open the solution in Visual Studio
3. Restore NuGet packages
4. Update the connection string in `DatabaseManager.cs` to point to your SQL Server
5. Run the SQL scripts in `SQLQuerynew.sql` to create the database schema
6. Build and run the application

## Project Structure

- `Form1.cs` - Main application form
- `RoomForm.cs` - Room management interface
- `GuestForm.cs` - Guest management interface
- `ReservationForm.cs` - Reservation handling
- `StaffForm.cs` - Staff management
- `PaymentForm.cs` - Payment processing
- `DatabaseManager.cs` - Database connectivity and operations


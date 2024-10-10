# Product Inventory Management System
A .NET Core-based application that manages product and inventory records with SQL Server. Features include user authentication, role-based access control, and a secure RESTful API.

## Table of Contents
* Overview
* Features
* Technologies
* Getting Started
   - Prerequisites
   - Database Setup
* Running the Project
  - Using Swagger
  - Using HTML Pages
* API Endpoints
* Project Structure
* Security

## Overview
The Product Inventory Management System is designed to help businesses manage their product inventory efficiently. This system allows users to sign up, log in, manage product details such as name, description, price, SKU, and quantity, and filter products by categories.

## Features
* User Authentication: Secure login and signup functionality with encrypted password storage.
* Product Management: Add, view, update, and delete products.
* Category Filter: Filter products based on categories.
* Real-Time Updates: Changes in the product details are updated in real time.
* Role-based access control: This ensures a set of permissions is assigned to a user.

## Technologies
* Backend: .NET Core 8, SQL Server, JWT Authentication
* Frontend: HTML, JavaScript
* Database: SQL Server
* API Testing: Swagger

# Getting Started
## Prerequisites
* .NET 8 SDK
* SQL Server

## Database Setup
 * A **.bak file** for database setup is included under the **DbScript** folder. Use this file to create the SQL Server database by restoring the database from the .bak file.
 * Once the database is restored, update the connection string in the appsettings.json file to match your SQL Server credentials. Use SQL Server authentication for security:
   
### Set up the database connection in appsettings.json:
```csharp
 "ConnectionStrings": {
       "DefaultConnection": "Server=your_server;Database=your_db;User Id=your_username;Password=your_password;Trusted_Connection=True;TrustServerCertificate=True;"
    }
```
## Running the Project
You can interact with the system in two ways:
### 1. Using Swagger-
   * Before accessing any API endpoints, you need to sign up or log in because authentication is required for authorization.
   *  When you log in or sign up, a JWT token is generated. Copy this token from the response.
   *  Use the Authorize button (visible on the Swagger UI) to enter the token. Once authorized, you will be able to access the API endpoints.
   *  When using POST methods, if any field has id, ignore it because it auto-increments, so you don't need to add id.
### 2. Using HTML Pages-
   * Comment out the following lines in **Program.cs**:
```csharp
app.UseSwagger();
app.UseSwaggerUI();
```
   * Modify **launchSettings.json**: In the Properties folder, open launchSettings.json, and replace the **"launchUrl"** <br> (launchUrl": "swagger") of the project with:
```csharp
"launchUrl": "html/Login.html"
```
   * From here, you can: 
       - **Sign Up**: Create a new user account.
       - **Log In**: Access the system using your credentials. On successful login, you'll be redirected to the product management page.
       - **Product Management**: After logging in,  Product Management page appears where you can view, add, update, and delete products. Real-time updates are also enabled here.
      - **JavaScript Operations**: The JavaScript code in **script.js** handles API calls for signup, login, and product management. This script communicates with the backend using the secure APIs and handles user interactions.

## API Endpoints
| Endpoint                                   | Method  | Description                                        |
|--------------------------------------------|---------|----------------------------------------------------|
| `/api/v1/products/categories`              | GET     | Get all unique product categories                  |
| `/api/v1/products`                         | GET     | Get products, filter by category if provided       |
| `/api/v1/product/{id}`                     | GET     | Get product by ID                                  |
| `/api/v1/product/{id}/price`               | PATCH   | Adjust the price of a product                      |
| `/api/v1/createProduct`                    | POST    | Create a new product                               |
| `/api/v1/updateProduct/{id}`               | PUT     | Update an existing product                         |
| `/api/v1/product/{id}/categories`          | POST    | Add categories to a product                        |
| `/api/v1/product/{id}/inventory`           | GET     | Get inventory details for a specific product       |
| `/api/v1/createProductInventory/{id}`      | POST    | Create or update inventory for a product           |
| `/api/v1/product/{id}/adjustInventory`     | POST    | Adjust inventory and log the change                |
| `/api/v1/product/{id}/inventory/audit`     | GET     | Get audit trail of inventory transactions          |

## Security
* **Password Encryption**: User passwords are encrypted using secure hashing algorithms to ensure safe storage.
* **JWT Authentication**: JSON Web Tokens (JWT) are used for secure user authentication and session management.

# Customer Support Platform

Customer support and ticket management platform built with React, ASP.NET Core Web API, SQL Server, and Docker.

## Overview

This project simulates a real-world support system with authentication, dashboard metrics, ticket management, and paginated data access.

## Tech Stack

### Frontend
- React
- TypeScript
- Vite
- Axios
- React Router

### Backend
- ASP.NET Core Web API
- .NET 10
- Entity Framework Core
- SQL Server 2022
- JWT Authentication
- BCrypt
- Swagger

### Environment
- WSL Ubuntu
- Docker Desktop
- Docker Compose

## Highlights

- Full-stack application with React frontend and ASP.NET Core Web API backend
- JWT authentication with protected routes
- SQL Server running in Docker with persistent volume
- WSL-based Linux development environment

## Current Features

- JWT-based authentication
- protected frontend routes
- dashboard summary
- ticket listing
- filtering and sorting
- pagination with `pageNumber/pageSize` and `limit/offset`
- ticket comments
- ticket assignment
- ticket status history
- SQL Server running in Docker with persistent volume

## Run Locally

### Database
From the project root:

```bash
docker compose up -d
```

### Backend
From `backend`:

```bash
dotnet run --project ./CustomerSupport.API/CustomerSupport.API.csproj
```

API:
- http://localhost:5132/swagger

### Frontend
From `frontend`:

```bash
npm install
npm run dev
```

Frontend:
- http://localhost:5173

## Development Credentials

- Admin: `admin@customersupport.com` / `admin123`
- Agent: `agent@customersupport.com` / `admin123`
- Customer: `customer@customersupport.com` / `admin123`

## Roadmap

- frontend ticket filters
- improved login error handling
- backend API containerization
- Docker healthcheck
- UI refinements
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
- backend configuration via environment variables for local secrets

## Current Features

- JWT-based authentication
- protected frontend routes
- dashboard summary
- ticket listing
- filtering and sorting
- pagination with `pageNumber/pageSize` and `limit/offset`
- visual status and priority badges
- ticket comments
- ticket assignment
- ticket status history
- SQL Server running in Docker with persistent volume
- automatic database migration on backend startup

## Run Locally

### 1. Create your local environment file

From the project root:

```bash
cp .env.example .env
```

Update `.env` with your local development values.

### 2. Start the database

From the project root:

```bash
set -a
source .env
set +a
docker compose up -d
```

### 3. Run the backend

From `backend`, in the same shell session used to load `.env`:

```bash
dotnet run --project ./CustomerSupport.API/CustomerSupport.API.csproj
```

API:
- http://localhost:5132/swagger

### 4. Run the frontend

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

## Notes

- Local secrets are stored in `.env`, which is ignored by Git.
- The repository includes `.env.example` as a configuration template.
- The backend applies migrations automatically on startup.

## Roadmap

- improved login error handling
- frontend ticket creation
- backend API containerization
- UI refinements
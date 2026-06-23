# Customer Support Platform

[![CI](https://github.com/dmarinhoDKR/customer-support-platform/actions/workflows/ci.yml/badge.svg)](https://github.com/dmarinhoDKR/customer-support-platform/actions/workflows/ci.yml)
[![CD](https://github.com/dmarinhoDKR/customer-support-platform/actions/workflows/cd.yml/badge.svg)](https://github.com/dmarinhoDKR/customer-support-platform/actions/workflows/cd.yml)

<p align="center">
  <img src="https://img.shields.io/badge/C%23-512BD4?style=for-the-badge&logo=csharp&logoColor=white" />
  <img src="https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" />
  <img src="https://img.shields.io/badge/ASP.NET%20Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" />
  <img src="https://img.shields.io/badge/REST%20API-005571?style=for-the-badge" />
  <img src="https://img.shields.io/badge/Swagger-85EA2D?style=for-the-badge&logo=swagger&logoColor=black" />
  <img src="https://img.shields.io/badge/OpenAPI-6BA539?style=for-the-badge&logo=openapiinitiative&logoColor=white" />
  <img src="https://img.shields.io/badge/JWT-000000?style=for-the-badge&logo=jsonwebtokens&logoColor=white" />
  <img src="https://img.shields.io/badge/Entity%20Framework%20Core-512BD4?style=for-the-badge" />
  <img src="https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white" />
  <img src="https://img.shields.io/badge/React-20232A?style=for-the-badge&logo=react&logoColor=61DAFB" />
  <img src="https://img.shields.io/badge/TypeScript-3178C6?style=for-the-badge&logo=typescript&logoColor=white" />
  <img src="https://img.shields.io/badge/Vite-646CFF?style=for-the-badge&logo=vite&logoColor=white" />
  <img src="https://img.shields.io/badge/CSS3-1572B6?style=for-the-badge&logo=css3&logoColor=white" />
  <img src="https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white" />
  <img src="https://img.shields.io/badge/GitHub%20Actions-2088FF?style=for-the-badge&logo=githubactions&logoColor=white" />
  <img src="https://img.shields.io/badge/CI%2FCD-0A0A0A?style=for-the-badge" />
  <img src="https://img.shields.io/badge/API%20Testing-FF6C37?style=for-the-badge&logo=postman&logoColor=white" />
  <img src="https://img.shields.io/badge/Role--Based%20Access-2E8B57?style=for-the-badge" />
  <img src="https://img.shields.io/badge/Dashboard-1E90FF?style=for-the-badge" />
  <img src="https://img.shields.io/badge/Ticket%20Management-6A5ACD?style=for-the-badge" />
</p>

Customer Support Platform is a full-stack ticket management application built with React, ASP.NET Core Web API, SQL Server, and Docker. The project includes JWT authentication, dashboard metrics, ticket creation, filtering, detailed ticket views, comments, assignment, and status history, simulating a real-world customer support workflow.

## Overview

This project simulates a real-world customer support workflow with authentication, dashboard metrics, ticket creation, ticket details, comments, status history, and paginated data access.

## Screenshots

### Login
Authentication screen for platform access.

![Login page](docs/screenshots/login-page.png)

### Dashboard
Overview of ticket metrics and user session information.

![Dashboard page](docs/screenshots/dashboard-page.png)

### Tickets
Ticket listing with filtering, pagination, and ticket creation form.

![Tickets page](docs/screenshots/tickets-page.png)

### Ticket Details
Detailed ticket view with metadata, comments, and status history.

![Ticket details page](docs/screenshots/ticket-details-page.png)

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
- Ticket creation, listing, filtering, and detailed ticket views
- Ticket comments, assignment, and status history
- SQL Server running in Docker with persistent volume
- WSL-based Linux development environment
- Backend configuration via environment variables for local secrets
- GitHub Actions CI pipeline for build validation and CD workflow for release artifact preparation
- Postman collection for manual API testing and QA workflows

## Current Features

- JWT-based authentication
- Protected frontend routes
- Dashboard summary
- Ticket creation
- Ticket listing
- Filtering and sorting
- Pagination with `pageNumber/pageSize` and `limit/offset`
- Visual status and priority badges
- Ticket details page
- Ticket comments
- Ticket assignment
- Ticket status history
- SQL Server running in Docker with persistent volume
- GitHub Actions CI/CD workflows for build validation and release artifact generation
- Automatic database migration on backend startup

## API Testing

A Postman collection is included to support manual API validation, authentication flow checks, and endpoint testing across the platform.

Postman collection:
- `docs/postman/CustomerSupport.postman_collection.json`

Covered flows:
- Authentication
- Dashboard summary
- Ticket listing
- Ticket creation
- Ticket lookup by id
- Ticket status update
- Ticket assignment
- Ticket comments
- Ticket status history

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

From the `backend` folder, in the same shell session used to load `.env`:

```bash
cd backend
dotnet run --project ./CustomerSupport.API/CustomerSupport.API.csproj

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

- Improved login error handling
- Backend API containerization
- UI refinements

# 🍽️ Food Delivery Backend

Backend study project for a **Food Delivery Service**.
The project replicates the functionality of an existing API specification and demonstrates backend development best practices together with software engineering documentation.

---

## 📌 Project Overview

This project implements the backend of a **Food Delivery Service** with the following features:

* User registration & authentication (JWT-based)
* Profile management
* Menu browsing with filtering & sorting
* Dish ratings (available to users who ordered the dish)
* Shopping cart
* Order creation, order details, and delivery confirmation

The project is divided into two parts:

1. **Backend Development** – creation of the backend application and API.
2. **Software Engineering Documentation** – proper use of Git workflow, task/bug tracking, time logs, and wiki pages.

---

## 🏗 Architecture

The solution follows a **3-layered architecture**:

* **WebAPI** → REST endpoints, authentication, request/response handling
* **BusinessLogic** → core services, business rules, validation
* **DataAccess** → database models, migrations, repositories

---

## 🔀 Git Workflow

Branching strategy:

* `main` → stable production-ready branch
* `develop` → integration branch for new features
* `feature/*` → new feature branches (e.g. `feature/auth`, `feature/cart`)
* `bugfix/*` → bug fixes

Pull Requests (PRs) must be created from feature/bugfix branches → `develop`.
After testing, `develop` is merged → `main`.

---

## 📂 Repository Structure

```
/FoodDeliveryBackend
│
├── WebAPI/              # Controllers, API endpoints
├── BusinessLogic/       # Services, validation, core rules
├── DataAccess/          # Entities, migrations, DbContext
│
├── README.md            # Project overview (this file)
├── TASK_LOG.md          # Log of completed tasks & bugs
└── .github/ISSUE_TEMPLATE/
    ├── bug_report.md
    └── feature_request.md
```

---

## 🚀 Getting Started

### 1. Prerequisites

* .NET 6+ / Node.js (depending on framework chosen for implementation)
* SQL Server / PostgreSQL / SQLite
* Git

### 2. Clone Repository

```bash
git clone https://github.com/ulasfinance/introtosoftware.git
cd food-delivery-backend
```

### 3. Install Dependencies

```bash
dotnet restore   # if using .NET
# or
npm install      # if using Node.js
```

### 4. Run Migrations

```bash
dotnet ef database update
```

### 5. Start Application

```bash
dotnet run
```

---

## 🧪 Testing

* Unit tests will be placed under `/tests` (to be added later).
* Postman collection for endpoints will be provided.

---

## 📋 Task & Bug Tracking

* Tasks and bugs are logged in **GitHub Issues**.
* Each issue includes:

  * Feature/Bug description
  * Estimated time vs actual time
  * Branch and commit references

A full history of tasks is maintained in [`TASK_LOG.md`](TASK_LOG.md).

---

## 📖 Documentation

All project documentation is maintained in the **Wiki**:

* Architecture diagrams
* API endpoint descriptions
* Task and bug history
* Setup guide

---

## 👥 Authors

* Ulas
* For **Backend Development** & **Intro to Software Engineering** coursework

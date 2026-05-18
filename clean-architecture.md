# Clean Architecture

## Overview

Clean Architecture (Robert C. Martin, "Uncle Bob") organizes code into concentric layers where dependencies point **inward only** — outer layers depend on inner layers, never the reverse.

```
+--------------------------------------------------+
|                                                  |
|   Infrastructure / Frameworks & Drivers          |
|   +------------------------------------------+  |
|   |                                          |  |
|   |   Interface Adapters                     |  |
|   |   +----------------------------------+   |  |
|   |   |                                  |   |  |
|   |   |   Application / Use Cases        |   |  |
|   |   |   +---------------------------+  |   |  |
|   |   |   |                           |  |   |  |
|   |   |   |   Domain / Entities       |  |   |  |
|   |   |   |                           |  |   |  |
|   |   |   +---------------------------+  |   |  |
|   |   |                                  |   |  |
|   |   +----------------------------------+   |  |
|   |                                          |  |
|   +------------------------------------------+  |
|                                                  |
+--------------------------------------------------+

          Dependency rule: ---->
          All arrows point inward
```

---

## Layers

### 1. Domain (Entities)
**Innermost layer. No dependencies on anything else.**

- Core business objects and rules
- Plain data structures / value objects
- Business invariants enforced here (not in a framework)
- Completely framework-agnostic

```
domain/
  entities/
    User.ts
    Order.ts
  value-objects/
    Email.ts
    Money.ts
  exceptions/
    DomainException.ts
```

### 2. Application (Use Cases)
**Orchestrates domain objects to fulfill a specific business action.**

- One use case = one business operation (e.g., `CreateUser`, `PlaceOrder`)
- Defines **interfaces** (ports) for everything it needs from the outside world
- Does not know about HTTP, SQL, or any framework
- Depends only on: Domain layer

```
application/
  use-cases/
    CreateUser.ts
    PlaceOrder.ts
  ports/                  ← interfaces defined here, implemented in Infrastructure
    UserRepository.ts
    EmailService.ts
  dtos/
    CreateUserDTO.ts
```

### 3. Interface Adapters
**Converts data between the format most convenient for use cases vs. external agents.**

- Controllers — translate HTTP requests into use-case inputs
- Presenters — translate use-case output into HTTP responses / view models
- Repository implementations (using the port interfaces from Application)
- Does not contain business logic

```
adapters/
  controllers/
    UserController.ts
  presenters/
    UserPresenter.ts
  repositories/
    SqlUserRepository.ts   ← implements UserRepository port
```

### 4. Infrastructure (Frameworks & Drivers)
**Outermost layer. All I/O lives here.**

- Web framework wiring (Express, FastAPI, Spring, etc.)
- Database clients / ORM configuration
- External service SDKs (email, payment, storage)
- Dependency injection container
- Configuration / environment

```
infrastructure/
  database/
    db.ts
    migrations/
  http/
    server.ts
    routes.ts
  di/
    container.ts
  config/
    env.ts
```

---

## Dependency Rule

| Layer | May depend on |
|---|---|
| Domain | Nothing |
| Application | Domain |
| Interface Adapters | Application, Domain |
| Infrastructure | All layers (wires them together) |

---

## Key Concepts

### Ports & Adapters (Hexagonal Architecture)
The Application layer defines **ports** (interfaces). Infrastructure provides **adapters** (concrete implementations). This makes the core testable without a real database or network.

```
Application defines:
  interface UserRepository {
    findById(id: string): Promise<User>
    save(user: User): Promise<void>
  }

Infrastructure provides:
  class PostgresUserRepository implements UserRepository { ... }
  class InMemoryUserRepository implements UserRepository { ... }  // for tests
```

### Dependency Inversion
High-level policy (use cases) must not depend on low-level details (SQL, HTTP). Both depend on abstractions (interfaces). Concrete implementations are injected at startup via a DI container.

### Data Transfer Objects (DTOs)
Raw input/output data crossing layer boundaries. DTOs prevent domain objects from leaking into outer layers and vice versa.

---

## Typical Request Flow

```
HTTP Request
    │
    ▼
Controller (Interface Adapters)
    │  maps request → DTO
    ▼
Use Case (Application)
    │  calls domain logic
    │  calls repository port
    ▼
Repository Port (Application interface)
    │  implemented by ↓
    ▼
SQL Repository (Infrastructure)
    │
    ▼
Database
```

---

## Benefits

- **Testability** — domain and use cases are pure functions with no I/O; swap real DB for in-memory in tests
- **Framework independence** — swap Express for Fastify, PostgreSQL for MongoDB without touching business logic
- **Independent deployability** — layers can evolve at different rates
- **Clear ownership** — business rules never scatter into controllers or SQL queries

## Trade-offs

- More boilerplate upfront (ports, DTOs, mappers)
- Overkill for very small or CRUD-only services
- Team must understand and enforce the dependency rule consistently

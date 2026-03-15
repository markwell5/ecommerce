# Ecommerce

[![.NET](https://github.com/markwell5/ecommerce/actions/workflows/dotnet-core.yml/badge.svg)](https://github.com/markwell5/ecommerce/actions/workflows/dotnet-core.yml)

A microservice ecommerce platform built with .NET 8, demonstrating event-driven architecture, saga orchestration, and modern cloud-native patterns.

## Architecture

The platform uses an event-driven microservice architecture with RabbitMQ (via MassTransit) as the message broker. Each service owns its own PostgreSQL database and communicates asynchronously through domain events and commands. Services also expose gRPC endpoints for synchronous inter-service calls where needed.

```
                    ┌──────────────┐
                    │   RabbitMQ   │
                    │  (MassTransit)│
                    └──────┬───────┘
       ┌──────────┬────────┼────────┬──────────┬──────────┐
       ▼          ▼        ▼        ▼          ▼          ▼
  ┌─────────┐ ┌────────┐ ┌───────┐ ┌──────┐ ┌────────┐ ┌──────┐
  │ Product │ │ Order  │ │ Stock │ │ User │ │Payment │ │ Cart │
  │ :5001   │ │ :5002  │ │ :5003 │ │:5004 │ │ :5005  │ │:5006 │
  └────┬────┘ └───┬────┘ └───┬───┘ └──┬───┘ └───┬────┘ └──┬───┘
       ▼          ▼          ▼        ▼         ▼         │
   product_db  order_db   stock_db  user_db  payment_db   │
       └──────────┴──────────┴────────┴─────────┘         │
                       PostgreSQL                      Redis
                            │
                       ┌────┴────┐
                       │PgBouncer│
                       └─────────┘

  ┌─────────────────────── Observability ───────────────────────┐
  │  Serilog → Seq (:8081)    OpenTelemetry → Jaeger (:16686)  │
  │  Prometheus (:9090) → Grafana (:3000)                      │
  └─────────────────────────────────────────────────────────────┘
```

### Services

| Service | Port | Database | Description |
|---------|------|----------|-------------|
| **Product Service** | 5001 | product_db | Product catalogue with full-text search, Redis caching, and category support |
| **Order Service** | 5002 | order_db | Order lifecycle orchestration via MassTransit saga state machine with event sourcing |
| **Stock Service** | 5003 | stock_db | Inventory management with pessimistic locking and low-stock alerts |
| **User Service** | 5004 | user_db | User registration, JWT authentication, profile and address management |
| **Payment Service** | 5005 | payment_db | Stripe integration with resilient gateway (Polly retry + circuit breaker) |
| **Cart Service** | 5006 | Redis | Shopping cart backed by Redis with product validation via gRPC |
| **Data Seeder** | — | — | Console app that seeds initial product and stock data |

### Tech Stack

| Category | Technology |
|----------|-----------|
| Runtime | .NET 8 |
| Databases | PostgreSQL 16 (per-service), Redis 7 |
| Connection Pooling | PgBouncer |
| Messaging | RabbitMQ via MassTransit 8.x |
| CQRS | MediatR with pipeline behaviours |
| ORM | Entity Framework Core 8 |
| Validation | FluentValidation (MediatR pipeline) |
| Mapping | AutoMapper |
| Auth | JWT Bearer tokens (issued by User Service) |
| API Versioning | Asp.Versioning (URL segment + header) |
| Resilience | Polly 8 (retry, circuit breaker, timeout) |
| Rate Limiting | ASP.NET Core sliding window (per-IP) |
| Idempotency | Custom idempotency filter for write endpoints |
| Logging | Serilog → Seq |
| Tracing | OpenTelemetry → Jaeger |
| Metrics | OpenTelemetry → Prometheus → Grafana |
| Testing | xUnit, FluentAssertions, NSubstitute, Testcontainers, PactNet |
| CI | GitHub Actions (format check, build, test) |
| Containers | Docker Compose with profiles |

## Order Saga Flow

The order service uses a MassTransit state machine saga with event sourcing to orchestrate the order lifecycle across services:

```
  PlaceOrder
      │
      ▼
 [ReservingStock] ──StockReservationFailed──▶ [Rejected]
      │
 StockReserved
      │
      ▼
   [Paying] ──PaymentFailed──▶ ReleaseStock ──▶ [PaymentFailed]
      │
 PaymentSucceeded
      │
      ▼
 [Confirmed] ──Ship──▶ [Shipped] ──Deliver──▶ [Delivered] ──Return──▶ [Returned]
                                                                    └─▶ ReleaseStock

 Any state can receive CancelOrder:
   Confirmed → ReleaseStock + RefundPayment → [Cancelled]
   ReservingStock/Paying → ReleaseStock → [Cancelled]
```

All state transitions are persisted as events in the `OrderEvents` table (event sourcing), with a denormalised read model in the `Orders` table.

## API Reference

### Product Service (:5001)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/v1/products` | No | List products (paginated, sortable) |
| GET | `/api/v1/products/{id}` | No | Get product by ID |
| GET | `/api/v1/products/search` | No | Full-text search with filters |
| POST | `/api/v1/products` | Yes | Create product |
| PUT | `/api/v1/products/{id}` | Yes | Update product |
| DELETE | `/api/v1/products/{id}` | Yes | Delete product |

### Order Service (:5002)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/v1/orders` | Yes | Place order (returns 202) |
| GET | `/api/v1/orders/{id}` | Yes | Get order by ID |
| POST | `/api/v1/orders/{id}/cancel` | Yes | Cancel order |
| POST | `/api/v1/orders/{id}/ship` | Yes | Ship order |
| POST | `/api/v1/orders/{id}/deliver` | Yes | Mark delivered |
| POST | `/api/v1/orders/{id}/return` | Yes | Return order |

### Stock Service (:5003)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/v1/stock/{productId}` | No | Get stock for product |
| PUT | `/api/v1/stock/{productId}` | Yes | Update stock quantity |

### User Service (:5004)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/v1/auth/register` | No | Register new user |
| POST | `/api/v1/auth/login` | No | Login (returns JWT) |
| POST | `/api/v1/auth/refresh` | No | Refresh token |
| GET | `/api/v1/users/me` | Yes | Get current user profile |
| PUT | `/api/v1/users/me` | Yes | Update profile |
| PUT | `/api/v1/users/me/password` | Yes | Change password |
| GET | `/api/v1/users/me/addresses` | Yes | List addresses |
| POST | `/api/v1/users/me/addresses` | Yes | Add address |
| PUT | `/api/v1/users/me/addresses/{id}` | Yes | Update address |
| DELETE | `/api/v1/users/me/addresses/{id}` | Yes | Delete address |

### Payment Service (:5005)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/v1/payments/{orderId}` | Yes | Get payment by order |
| GET | `/api/v1/payments/customer/{customerId}` | Yes | Get customer payments |
| POST | `/api/v1/payments/{paymentId}/refund` | Yes | Refund payment |

### Cart Service (:5006)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/v1/cart/{cartId}` | No | Get cart |
| POST | `/api/v1/cart/{cartId}/items` | No | Add item to cart |
| PUT | `/api/v1/cart/{cartId}/items/{productId}` | No | Update item quantity |
| DELETE | `/api/v1/cart/{cartId}/items/{productId}` | No | Remove item |
| DELETE | `/api/v1/cart/{cartId}` | No | Clear cart |

All services also expose:
- `GET /health` — Health check
- `GET /metrics` — Prometheus metrics
- `GET /swagger` — OpenAPI docs (development only)

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://docs.docker.com/get-docker/) and Docker Compose

### Running with Docker Compose

| Command | What it starts |
|---------|---------------|
| `docker compose up -d` | Infrastructure only (PostgreSQL, PgBouncer, RabbitMQ, Redis, Seq) |
| `docker compose --profile app up -d` | Full stack — all services + infrastructure |
| `docker compose --profile tools up -d` | Infrastructure + dev tools (Prometheus, Grafana, Jaeger, pgAdmin) |
| `docker compose --profile app --profile tools up -d` | Everything |
| `docker compose down -v` | Tear down including volumes |

### Local Development

Start infrastructure, then run services locally:

```bash
# Start infrastructure
docker compose up -d

# Install git hooks (format checking on commit)
bash scripts/install-hooks.sh

# Run individual services
dotnet run --project product-service/Product.Service
dotnet run --project order-service/Order.Service
dotnet run --project stock-service/Stock.Service
dotnet run --project user-service/User.Service
dotnet run --project payment-service/Payment.Service
dotnet run --project cart-service/Cart.Service
```

### Seed Data

After services are running:

```bash
dotnet run --project data-seeder/DataSeeder
```

### Run Tests

```bash
# All tests (unit, integration, contract)
dotnet test

# Specific test project
dotnet test product-service/Product.Integration.Tests
```

Integration tests use **Testcontainers** — they spin up PostgreSQL and Redis containers automatically. No external dependencies needed.

## Useful URLs

| URL | Description |
|-----|-------------|
| http://localhost:5001/swagger | Product Service API docs |
| http://localhost:5002/swagger | Order Service API docs |
| http://localhost:5003/swagger | Stock Service API docs |
| http://localhost:5004/swagger | User Service API docs |
| http://localhost:5005/swagger | Payment Service API docs |
| http://localhost:5006/swagger | Cart Service API docs |
| http://localhost:8081 | Seq log viewer |
| http://localhost:15672 | RabbitMQ management (guest / guest) |
| http://localhost:9090 | Prometheus (tools profile) |
| http://localhost:3000 | Grafana dashboards (tools profile, admin / admin) |
| http://localhost:16686 | Jaeger tracing (tools profile) |
| http://localhost:8080 | pgAdmin (tools profile, admin@local.dev / admin) |

## Project Structure

```
├── cart-service/           # Cart Service (Redis-backed)
├── order-service/          # Order Service (saga orchestrator)
├── payment-service/        # Payment Service (Stripe)
├── product-service/        # Product Service (catalogue)
├── stock-service/          # Stock Service (inventory)
├── user-service/           # User Service (auth + profiles)
├── data-seeder/            # Data seeding console app
├── domain/
│   ├── Ecommerce.Events/   # Shared domain events and messages
│   └── Ecommerce.Model/    # Shared domain models
├── shared/
│   ├── Ecommerce.Shared.Infrastructure/  # Cross-cutting concerns
│   └── Ecommerce.Shared.Protos/          # gRPC proto definitions
├── tests/
│   └── Ecommerce.Contract.Tests/  # Pact consumer/provider tests
├── tools/
│   ├── prometheus/         # Prometheus config
│   └── grafana/            # Grafana dashboards and provisioning
├── scripts/                # DB init, git hooks, pgbouncer config
└── docker-compose.yml
```

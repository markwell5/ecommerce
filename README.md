# Ecommerce

A microservice ecommerce platform using data from Instacart.

## Architecture

The platform uses an event-driven microservice architecture with RabbitMQ (via MassTransit) as the message broker. Each service owns its own PostgreSQL database and communicates asynchronously through domain events and commands.

```
                        ┌─────────────┐
                        │  RabbitMQ   │
                        └──────┬──────┘
              ┌────────────────┼────────────────┐
              ▼                ▼                ▼
     ┌────────────────┐ ┌───────────────┐ ┌───────────────┐
     │ Product Service│ │ Order Service │ │ Stock Service │
     │   :5001        │ │   :5002       │ │   :5003       │
     └───────┬────────┘ └───────┬───────┘ └───────┬───────┘
             ▼                  ▼                  ▼
         product_db          order_db           stock_db
              └────────────────┼────────────────┘
                          PostgreSQL
```

### Services

| Service | Port | Description |
|---------|------|-------------|
| **Product Service** | 5001 | CRUD for product catalogue. Publishes `ProductCreated`/`Updated`/`Deleted` events. |
| **Order Service** | 5002 | Orchestrates order lifecycle via MassTransit state machine saga with event sourcing. |
| **Stock Service** | 5003 | Manages inventory. Consumes `ReserveStock`/`ReleaseStock` from the Order saga. |
| **Data Seeder** | — | Console app that seeds initial product and stock data. |

### Tech Stack

- **.NET 8** — All services
- **PostgreSQL 16** — Persistence (each service has its own database)
- **RabbitMQ** — Message broker (via MassTransit 8.x)
- **MediatR** — CQRS command/query handling
- **Entity Framework Core** — ORM and migrations
- **FluentValidation** — Request validation via MediatR pipeline
- **AutoMapper** — Entity-to-DTO mapping
- **Serilog + Seq** — Structured logging and log viewer
- **Docker Compose** — Container orchestration

### Order Saga Flow

```
PlaceOrder → [Placed] → ReserveStock → [ReservingStock]
  StockReserved → ProcessPayment → [Paying]
    PaymentSucceeded → [Confirmed] → Ship → Deliver → Return
    PaymentFailed → ReleaseStock → [PaymentFailed]
  StockReservationFailed → [Rejected]
  CancelOrder → ReleaseStock → [Cancelled]
```

## Running the Application

### Full stack (Docker)

```bash
docker compose up -d
```

This starts all services, PostgreSQL, RabbitMQ, and Seq.

### Local development

Start infrastructure only:

```bash
docker compose -f docker-compose.local.yml up -d
```

Then run individual services:

```bash
dotnet run --project product-service/Product.Service
dotnet run --project order-service/Order.Service
dotnet run --project stock-service/Stock.Service
```

### Seed data

After services are running:

```bash
dotnet run --project data-seeder/DataSeeder
```

### Useful URLs

| URL | Description |
|-----|-------------|
| http://localhost:5001/swagger | Product Service API docs |
| http://localhost:5002/swagger | Order Service API docs |
| http://localhost:5003/swagger | Stock Service API docs |
| http://localhost:8081 | Seq log viewer |
| http://localhost:15672 | RabbitMQ management (guest/guest) |

## Future Developments

1. Auth Service — user and service-to-service authentication
1. gRPC for inter-service communication
1. GraphQL API gateway layer
1. React/Node.js frontend with Elasticsearch
1. Recommendation service (ML-based product suggestions)

using System;
using System.Text.Json;
using Ecommerce.Events.Order.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;
using Order.Application.Entities;

namespace Order.Application.Saga
{
    public class OrderStateMachine : MassTransitStateMachine<OrderSagaState>
    {
        public OrderStateMachine(ILogger<OrderStateMachine> logger)
        {
            InstanceState(x => x.CurrentState);

            Event(() => OrderPlaced, x => x.CorrelateById(ctx => ctx.Message.OrderId));
            Event(() => StockReservedEvent, x => x.CorrelateById(ctx => ctx.Message.OrderId));
            Event(() => StockReservationFailedEvent, x => x.CorrelateById(ctx => ctx.Message.OrderId));
            Event(() => PaymentSucceededEvent, x => x.CorrelateById(ctx => ctx.Message.OrderId));
            Event(() => PaymentFailedEvent, x => x.CorrelateById(ctx => ctx.Message.OrderId));
            Event(() => OrderCancelledEvent, x => x.CorrelateById(ctx => ctx.Message.OrderId));
            Event(() => OrderShippedEvent, x => x.CorrelateById(ctx => ctx.Message.OrderId));
            Event(() => OrderDeliveredEvent, x => x.CorrelateById(ctx => ctx.Message.OrderId));
            Event(() => OrderReturnedEvent, x => x.CorrelateById(ctx => ctx.Message.OrderId));

            Initially(
                When(OrderPlaced)
                    .Then(context =>
                    {
                        context.Saga.CustomerId = context.Message.CustomerId;
                        context.Saga.TotalAmount = context.Message.TotalAmount;
                        context.Saga.ItemsJson = context.Message.ItemsJson;
                        context.Saga.CouponCode = context.Message.CouponCode;
                        context.Saga.DiscountAmount = context.Message.DiscountAmount;
                        context.Saga.CreatedAt = DateTime.UtcNow;

                        var db = context.GetPayload<OrderDbContext>();
                        AppendEvent(db, context.Saga.CorrelationId, "OrderPlaced",
                            JsonSerializer.Serialize(new { context.Message.CustomerId, context.Message.TotalAmount }));
                        UpsertReadModel(db, context.Saga, "Placed");

                        logger.LogInformation("Order {OrderId} placed for customer {CustomerId}", context.Saga.CorrelationId, context.Message.CustomerId);
                    })
                    .Send(context => new ReserveStock
                    {
                        OrderId = context.Saga.CorrelationId,
                        ItemsJson = context.Saga.ItemsJson
                    })
                    .TransitionTo(ReservingStock));

            During(ReservingStock,
                When(StockReservedEvent)
                    .Then(context =>
                    {
                        context.Saga.UpdatedAt = DateTime.UtcNow;
                        var db = context.GetPayload<OrderDbContext>();
                        AppendEvent(db, context.Saga.CorrelationId, "StockReserved", "{}");
                        UpsertReadModel(db, context.Saga, "Paying");

                        logger.LogInformation("Stock reserved for order {OrderId}", context.Saga.CorrelationId);
                    })
                    .Send(context => new ProcessPayment
                    {
                        OrderId = context.Saga.CorrelationId,
                        Amount = context.Saga.TotalAmount,
                        CustomerId = context.Saga.CustomerId
                    })
                    .TransitionTo(Paying),
                When(StockReservationFailedEvent)
                    .Then(context =>
                    {
                        context.Saga.UpdatedAt = DateTime.UtcNow;
                        var db = context.GetPayload<OrderDbContext>();
                        AppendEvent(db, context.Saga.CorrelationId, "StockReservationFailed",
                            JsonSerializer.Serialize(new { context.Message.Reason }));
                        UpsertReadModel(db, context.Saga, "Rejected");

                        logger.LogWarning("Stock reservation failed for order {OrderId}: {Reason}", context.Saga.CorrelationId, context.Message.Reason);
                    })
                    .TransitionTo(Rejected),
                When(OrderCancelledEvent)
                    .Then(context =>
                    {
                        context.Saga.UpdatedAt = DateTime.UtcNow;
                        var db = context.GetPayload<OrderDbContext>();
                        AppendEvent(db, context.Saga.CorrelationId, "OrderCancelled", "{}");
                        UpsertReadModel(db, context.Saga, "Cancelled");

                        logger.LogInformation("Order {OrderId} cancelled during stock reservation", context.Saga.CorrelationId);
                    })
                    .TransitionTo(Cancelled));

            During(Paying,
                When(PaymentSucceededEvent)
                    .Then(context =>
                    {
                        context.Saga.UpdatedAt = DateTime.UtcNow;
                        var db = context.GetPayload<OrderDbContext>();
                        AppendEvent(db, context.Saga.CorrelationId, "PaymentSucceeded", "{}");
                        UpsertReadModel(db, context.Saga, "Confirmed");

                        logger.LogInformation("Payment succeeded for order {OrderId}", context.Saga.CorrelationId);
                    })
                    .TransitionTo(Confirmed),
                When(PaymentFailedEvent)
                    .Then(context =>
                    {
                        context.Saga.UpdatedAt = DateTime.UtcNow;
                        var db = context.GetPayload<OrderDbContext>();
                        AppendEvent(db, context.Saga.CorrelationId, "PaymentFailed",
                            JsonSerializer.Serialize(new { context.Message.Reason }));
                        UpsertReadModel(db, context.Saga, "PaymentFailed");

                        logger.LogWarning("Payment failed for order {OrderId}: {Reason}", context.Saga.CorrelationId, context.Message.Reason);
                    })
                    .Send(context => new ReleaseStock
                    {
                        OrderId = context.Saga.CorrelationId,
                        ItemsJson = context.Saga.ItemsJson
                    })
                    .TransitionTo(PaymentFailed),
                When(OrderCancelledEvent)
                    .Then(context =>
                    {
                        context.Saga.UpdatedAt = DateTime.UtcNow;
                        var db = context.GetPayload<OrderDbContext>();
                        AppendEvent(db, context.Saga.CorrelationId, "OrderCancelled", "{}");
                        UpsertReadModel(db, context.Saga, "Cancelled");

                        logger.LogInformation("Order {OrderId} cancelled during payment", context.Saga.CorrelationId);
                    })
                    .Send(context => new ReleaseStock
                    {
                        OrderId = context.Saga.CorrelationId,
                        ItemsJson = context.Saga.ItemsJson
                    })
                    .TransitionTo(Cancelled));

            During(Confirmed,
                When(OrderShippedEvent)
                    .Then(context =>
                    {
                        context.Saga.UpdatedAt = DateTime.UtcNow;
                        var db = context.GetPayload<OrderDbContext>();
                        AppendEvent(db, context.Saga.CorrelationId, "OrderShipped", "{}");
                        UpsertReadModel(db, context.Saga, "Shipped");

                        logger.LogInformation("Order {OrderId} shipped", context.Saga.CorrelationId);
                    })
                    .TransitionTo(Shipped),
                When(OrderCancelledEvent)
                    .Then(context =>
                    {
                        context.Saga.UpdatedAt = DateTime.UtcNow;
                        var db = context.GetPayload<OrderDbContext>();
                        AppendEvent(db, context.Saga.CorrelationId, "OrderCancelled", "{}");
                        UpsertReadModel(db, context.Saga, "Cancelled");

                        logger.LogInformation("Order {OrderId} cancelled", context.Saga.CorrelationId);
                    })
                    .Send(context => new ReleaseStock
                    {
                        OrderId = context.Saga.CorrelationId,
                        ItemsJson = context.Saga.ItemsJson
                    })
                    .Send(context => new RefundPayment
                    {
                        OrderId = context.Saga.CorrelationId
                    })
                    .TransitionTo(Cancelled));

            During(Shipped,
                When(OrderDeliveredEvent)
                    .Then(context =>
                    {
                        context.Saga.UpdatedAt = DateTime.UtcNow;
                        var db = context.GetPayload<OrderDbContext>();
                        AppendEvent(db, context.Saga.CorrelationId, "OrderDelivered", "{}");
                        UpsertReadModel(db, context.Saga, "Delivered");

                        logger.LogInformation("Order {OrderId} delivered", context.Saga.CorrelationId);
                    })
                    .TransitionTo(Delivered));

            During(Delivered,
                When(OrderReturnedEvent)
                    .Then(context =>
                    {
                        context.Saga.UpdatedAt = DateTime.UtcNow;
                        var db = context.GetPayload<OrderDbContext>();
                        AppendEvent(db, context.Saga.CorrelationId, "OrderReturned", "{}");
                        UpsertReadModel(db, context.Saga, "Returned");

                        logger.LogInformation("Order {OrderId} returned", context.Saga.CorrelationId);
                    })
                    .Send(context => new ReleaseStock
                    {
                        OrderId = context.Saga.CorrelationId,
                        ItemsJson = context.Saga.ItemsJson
                    })
                    .TransitionTo(Returned));
        }

        public State ReservingStock { get; private set; }
        public State Paying { get; private set; }
        public State Confirmed { get; private set; }
        public State Rejected { get; private set; }
        public State PaymentFailed { get; private set; }
        public State Cancelled { get; private set; }
        public State Shipped { get; private set; }
        public State Delivered { get; private set; }
        public State Returned { get; private set; }

        public Event<PlaceOrder> OrderPlaced { get; private set; }
        public Event<StockReserved> StockReservedEvent { get; private set; }
        public Event<StockReservationFailed> StockReservationFailedEvent { get; private set; }
        public Event<PaymentSucceeded> PaymentSucceededEvent { get; private set; }
        public Event<PaymentFailed> PaymentFailedEvent { get; private set; }
        public Event<CancelOrder> OrderCancelledEvent { get; private set; }
        public Event<ShipOrder> OrderShippedEvent { get; private set; }
        public Event<DeliverOrder> OrderDeliveredEvent { get; private set; }
        public Event<ReturnOrder> OrderReturnedEvent { get; private set; }

        private static void AppendEvent(OrderDbContext db, Guid orderId, string eventName, string payload)
        {
            db.OrderEvents.Add(new OrderEvent
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                EventName = eventName,
                Payload = payload,
                OccurredAt = DateTime.UtcNow
            });
        }

        private static void UpsertReadModel(OrderDbContext db, OrderSagaState saga, string status)
        {
            var existing = db.Orders.Find(saga.CorrelationId);
            if (existing == null)
            {
                db.Orders.Add(new Entities.Order
                {
                    OrderId = saga.CorrelationId,
                    CustomerId = saga.CustomerId,
                    Status = status,
                    TotalAmount = saga.TotalAmount,
                    ItemsJson = saga.ItemsJson,
                    CouponCode = saga.CouponCode,
                    DiscountAmount = saga.DiscountAmount,
                    CreatedAt = saga.CreatedAt,
                    UpdatedAt = saga.UpdatedAt
                });
            }
            else
            {
                existing.Status = status;
                existing.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}

using System;
using MassTransit;

namespace Order.Application.Entities
{
    public class OrderSagaState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        public string CustomerId { get; set; }
        public decimal TotalAmount { get; set; }
        public string ItemsJson { get; set; }
        public string CouponCode { get; set; }
        public decimal DiscountAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

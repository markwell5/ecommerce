using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Events.Return;
using Ecommerce.Model.Return.Response;
using Ecommerce.Shared.Protos;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Return.Application.Policies;

namespace Return.Application.Commands
{
    public class ResolveReturnCommand : IRequest<ReturnResponse>
    {
        public long ReturnRequestId { get; set; }
        public string Resolution { get; set; } = string.Empty; // full_refund, partial_refund, exchange
        public decimal RefundAmount { get; set; }
        public long? ExchangeProductId { get; set; }
        public string ExchangeProductName { get; set; } = string.Empty;
    }

    public class ResolveReturnCommandHandler : IRequestHandler<ResolveReturnCommand, ReturnResponse>
    {
        private readonly ReturnDbContext _dbContext;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IMapper _mapper;
        private readonly OrderGrpc.OrderGrpcClient _orderClient;
        private readonly ProductGrpc.ProductGrpcClient _productClient;
        private readonly ILogger<ResolveReturnCommandHandler> _logger;

        public ResolveReturnCommandHandler(
            ReturnDbContext dbContext,
            IPublishEndpoint publishEndpoint,
            IMapper mapper,
            OrderGrpc.OrderGrpcClient orderClient,
            ProductGrpc.ProductGrpcClient productClient,
            ILogger<ResolveReturnCommandHandler> logger)
        {
            _dbContext = dbContext;
            _publishEndpoint = publishEndpoint;
            _mapper = mapper;
            _orderClient = orderClient;
            _productClient = productClient;
            _logger = logger;
        }

        public async Task<ReturnResponse> Handle(ResolveReturnCommand command, CancellationToken cancellationToken)
        {
            var ret = await _dbContext.ReturnRequests
                .FirstOrDefaultAsync(r => r.Id == command.ReturnRequestId, cancellationToken);

            if (ret == null) return null;
            if (ret.Status != "Approved" && ret.Status != "Received")
                throw new InvalidOperationException($"Cannot resolve return in status '{ret.Status}'");

            var restockingFee = ReturnPolicy.CalculateRestockingFee(ret.Reason, ret.CreatedAt, command.RefundAmount);
            var finalRefund = command.RefundAmount - restockingFee;

            ret.Resolution = command.Resolution;
            ret.RefundAmount = finalRefund;
            ret.RestockingFee = restockingFee;
            ret.Status = command.Resolution == "exchange" ? "Exchanged" : "Refunded";
            ret.ResolvedAt = DateTime.UtcNow;
            ret.UpdatedAt = DateTime.UtcNow;

            if (command.Resolution == "exchange")
            {
                var exchangeProductId = command.ExchangeProductId ?? ret.ProductId;
                var exchangeProductName = command.ExchangeProductName;

                if (string.IsNullOrEmpty(exchangeProductName))
                {
                    var product = await _productClient.GetProductAsync(
                        new GetProductRequest { Id = exchangeProductId },
                        cancellationToken: cancellationToken);
                    exchangeProductName = product.Name;
                }

                var exchangeOrder = await _orderClient.PlaceOrderAsync(new PlaceOrderGrpcRequest
                {
                    CustomerId = ret.CustomerId,
                    Items =
                    {
                        new OrderLineItemGrpc
                        {
                            ProductId = exchangeProductId,
                            ProductName = exchangeProductName,
                            Quantity = ret.Quantity,
                            UnitPrice = "0.00"
                        }
                    }
                }, cancellationToken: cancellationToken);

                ret.ExchangeProductId = exchangeProductId;
                ret.ExchangeProductName = exchangeProductName;
                ret.ExchangeOrderId = Guid.Parse(exchangeOrder.OrderId);

                _logger.LogInformation(
                    "Exchange order {ExchangeOrderId} created for return {ReturnId}",
                    exchangeOrder.OrderId, ret.Id);

                await _dbContext.SaveChangesAsync(cancellationToken);

                await _publishEndpoint.Publish(new ExchangeOrderCreated
                {
                    ReturnRequestId = ret.Id,
                    OriginalOrderId = ret.OrderId,
                    ExchangeOrderId = Guid.Parse(exchangeOrder.OrderId),
                    CustomerId = ret.CustomerId,
                    ExchangeProductId = exchangeProductId,
                    Quantity = ret.Quantity
                }, cancellationToken);
            }
            else
            {
                await _dbContext.SaveChangesAsync(cancellationToken);

                if (command.Resolution is "full_refund" or "partial_refund")
                {
                    await _publishEndpoint.Publish(new RefundRequested
                    {
                        ReturnRequestId = ret.Id,
                        OrderId = ret.OrderId,
                        CustomerId = ret.CustomerId,
                        Amount = finalRefund
                    }, cancellationToken);
                }
            }

            return _mapper.Map<ReturnResponse>(ret);
        }
    }
}

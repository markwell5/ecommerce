using System;
using System.Threading;
using System.Threading.Tasks;
using Ecommerce.Model.Payment.Response;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Payment.Application.Services;

namespace Payment.Application.Commands
{
    public class RefundPaymentCommand : IRequest<PaymentResponse>
    {
        public RefundPaymentCommand(long paymentId)
        {
            PaymentId = paymentId;
        }

        public long PaymentId { get; }
    }

    public class RefundPaymentCommandHandler : IRequestHandler<RefundPaymentCommand, PaymentResponse>
    {
        private readonly PaymentDbContext _dbContext;
        private readonly IPaymentGateway _paymentGateway;
        private readonly IMapper _mapper;

        public RefundPaymentCommandHandler(PaymentDbContext dbContext, IPaymentGateway paymentGateway, IMapper mapper)
        {
            _dbContext = dbContext;
            _paymentGateway = paymentGateway;
            _mapper = mapper;
        }

        public async Task<PaymentResponse> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
        {
            var payment = await _dbContext.Payments
                .FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken);

            if (payment == null)
                return null;

            if (payment.Status != "Succeeded")
                return null;

            var result = await _paymentGateway.CreateRefundAsync(payment.StripePaymentIntentId, payment.Amount);

            var refund = new Entities.Refund
            {
                PaymentId = payment.Id,
                StripeRefundId = result.RefundId,
                Amount = payment.Amount,
                Reason = "Manual refund",
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Refunds.Add(refund);

            payment.Status = "Refunded";
            payment.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return _mapper.Map<PaymentResponse>(payment);
        }
    }
}

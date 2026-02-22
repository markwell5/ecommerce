using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Model.Payment.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Payment.Application.Queries
{
    public class GetPaymentByOrderQuery : IRequest<PaymentResponse>
    {
        public GetPaymentByOrderQuery(Guid orderId)
        {
            OrderId = orderId;
        }

        public Guid OrderId { get; }
    }

    public class GetPaymentByOrderQueryHandler : IRequestHandler<GetPaymentByOrderQuery, PaymentResponse>
    {
        private readonly PaymentDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetPaymentByOrderQueryHandler(PaymentDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<PaymentResponse> Handle(GetPaymentByOrderQuery request, CancellationToken cancellationToken)
        {
            var payment = await _dbContext.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.OrderId == request.OrderId, cancellationToken);

            if (payment == null)
                return null;

            return _mapper.Map<PaymentResponse>(payment);
        }
    }
}

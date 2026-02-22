using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Model.Payment.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Payment.Application.Queries
{
    public class GetPaymentsByCustomerQuery : IRequest<List<PaymentResponse>>
    {
        public GetPaymentsByCustomerQuery(string customerId)
        {
            CustomerId = customerId;
        }

        public string CustomerId { get; }
    }

    public class GetPaymentsByCustomerQueryHandler : IRequestHandler<GetPaymentsByCustomerQuery, List<PaymentResponse>>
    {
        private readonly PaymentDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetPaymentsByCustomerQueryHandler(PaymentDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<List<PaymentResponse>> Handle(GetPaymentsByCustomerQuery request, CancellationToken cancellationToken)
        {
            var payments = await _dbContext.Payments
                .AsNoTracking()
                .Where(p => p.CustomerId == request.CustomerId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<PaymentResponse>>(payments);
        }
    }
}

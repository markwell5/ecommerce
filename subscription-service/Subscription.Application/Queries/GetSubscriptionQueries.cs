using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Model.Subscription.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Subscription.Application.Queries
{
    public record GetSubscriptionQuery(long SubscriptionId) : IRequest<SubscriptionResponse>;
    public record GetSubscriptionsByCustomerQuery(string CustomerId) : IRequest<List<SubscriptionResponse>>;
    public record GetUpcomingRenewalsQuery(int Days) : IRequest<List<SubscriptionResponse>>;

    public class GetSubscriptionQueryHandler : IRequestHandler<GetSubscriptionQuery, SubscriptionResponse>
    {
        private readonly SubscriptionDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetSubscriptionQueryHandler(SubscriptionDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<SubscriptionResponse> Handle(GetSubscriptionQuery request, CancellationToken cancellationToken)
        {
            var subscription = await _dbContext.Subscriptions
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == request.SubscriptionId, cancellationToken);

            return subscription == null ? null : _mapper.Map<SubscriptionResponse>(subscription);
        }
    }

    public class GetSubscriptionsByCustomerQueryHandler : IRequestHandler<GetSubscriptionsByCustomerQuery, List<SubscriptionResponse>>
    {
        private readonly SubscriptionDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetSubscriptionsByCustomerQueryHandler(SubscriptionDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<List<SubscriptionResponse>> Handle(GetSubscriptionsByCustomerQuery request, CancellationToken cancellationToken)
        {
            var subscriptions = await _dbContext.Subscriptions
                .AsNoTracking()
                .Where(s => s.CustomerId == request.CustomerId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<SubscriptionResponse>>(subscriptions);
        }
    }

    public class GetUpcomingRenewalsQueryHandler : IRequestHandler<GetUpcomingRenewalsQuery, List<SubscriptionResponse>>
    {
        private readonly SubscriptionDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetUpcomingRenewalsQueryHandler(SubscriptionDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<List<SubscriptionResponse>> Handle(GetUpcomingRenewalsQuery request, CancellationToken cancellationToken)
        {
            var cutoff = DateTime.UtcNow.AddDays(request.Days);
            var subscriptions = await _dbContext.Subscriptions
                .AsNoTracking()
                .Where(s => s.Status == "Active" && s.NextRenewalAt <= cutoff)
                .OrderBy(s => s.NextRenewalAt)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<SubscriptionResponse>>(subscriptions);
        }
    }
}

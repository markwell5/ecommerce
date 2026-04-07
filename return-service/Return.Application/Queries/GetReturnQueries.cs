using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Model.Return.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Return.Application.Queries
{
    public record GetReturnQuery(long ReturnRequestId) : IRequest<ReturnResponse>;
    public record GetReturnByRmaQuery(string RmaNumber) : IRequest<ReturnResponse>;
    public record GetReturnsByOrderQuery(Guid OrderId) : IRequest<List<ReturnResponse>>;
    public record GetReturnsByCustomerQuery(string CustomerId) : IRequest<List<ReturnResponse>>;

    public class GetReturnQueryHandler : IRequestHandler<GetReturnQuery, ReturnResponse>
    {
        private readonly ReturnDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetReturnQueryHandler(ReturnDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<ReturnResponse> Handle(GetReturnQuery request, CancellationToken cancellationToken)
        {
            var ret = await _dbContext.ReturnRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == request.ReturnRequestId, cancellationToken);

            return ret == null ? null : _mapper.Map<ReturnResponse>(ret);
        }
    }

    public class GetReturnByRmaQueryHandler : IRequestHandler<GetReturnByRmaQuery, ReturnResponse>
    {
        private readonly ReturnDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetReturnByRmaQueryHandler(ReturnDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<ReturnResponse> Handle(GetReturnByRmaQuery request, CancellationToken cancellationToken)
        {
            var ret = await _dbContext.ReturnRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RmaNumber == request.RmaNumber, cancellationToken);

            return ret == null ? null : _mapper.Map<ReturnResponse>(ret);
        }
    }

    public class GetReturnsByOrderQueryHandler : IRequestHandler<GetReturnsByOrderQuery, List<ReturnResponse>>
    {
        private readonly ReturnDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetReturnsByOrderQueryHandler(ReturnDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<List<ReturnResponse>> Handle(GetReturnsByOrderQuery request, CancellationToken cancellationToken)
        {
            var returns = await _dbContext.ReturnRequests
                .AsNoTracking()
                .Where(r => r.OrderId == request.OrderId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<ReturnResponse>>(returns);
        }
    }

    public class GetReturnsByCustomerQueryHandler : IRequestHandler<GetReturnsByCustomerQuery, List<ReturnResponse>>
    {
        private readonly ReturnDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetReturnsByCustomerQueryHandler(ReturnDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<List<ReturnResponse>> Handle(GetReturnsByCustomerQuery request, CancellationToken cancellationToken)
        {
            var returns = await _dbContext.ReturnRequests
                .AsNoTracking()
                .Where(r => r.CustomerId == request.CustomerId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<ReturnResponse>>(returns);
        }
    }
}

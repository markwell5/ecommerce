using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Model.Return.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Return.Application.Commands
{
    public record RejectReturnCommand(long ReturnRequestId, string AdminNotes) : IRequest<ReturnResponse>;

    public class RejectReturnCommandHandler : IRequestHandler<RejectReturnCommand, ReturnResponse>
    {
        private readonly ReturnDbContext _dbContext;
        private readonly IMapper _mapper;

        public RejectReturnCommandHandler(ReturnDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<ReturnResponse> Handle(RejectReturnCommand command, CancellationToken cancellationToken)
        {
            var ret = await _dbContext.ReturnRequests
                .FirstOrDefaultAsync(r => r.Id == command.ReturnRequestId, cancellationToken);

            if (ret == null) return null;
            if (ret.Status != "Requested")
                throw new InvalidOperationException($"Cannot reject return in status '{ret.Status}'");

            ret.Status = "Rejected";
            ret.AdminNotes = command.AdminNotes;
            ret.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return _mapper.Map<ReturnResponse>(ret);
        }
    }
}

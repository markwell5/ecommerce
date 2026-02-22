using MediatR;
using Microsoft.EntityFrameworkCore;

namespace User.Application.Commands
{
    public record DeleteAddressCommand(Guid UserId, long AddressId) : IRequest<bool>;

    public class DeleteAddressCommandHandler : IRequestHandler<DeleteAddressCommand, bool>
    {
        private readonly UserDbContext _dbContext;

        public DeleteAddressCommandHandler(UserDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(DeleteAddressCommand command, CancellationToken cancellationToken)
        {
            var address = await _dbContext.Addresses
                .FirstOrDefaultAsync(a => a.Id == command.AddressId && a.UserId == command.UserId, cancellationToken);

            if (address == null)
                return false;

            _dbContext.Addresses.Remove(address);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}

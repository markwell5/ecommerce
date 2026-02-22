using AutoMapper;
using Ecommerce.Model.User.Request;
using Ecommerce.Model.User.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;
using User.Application.Entities;

namespace User.Application.Commands
{
    public record AddAddressCommand(Guid UserId, AddressRequest Request) : IRequest<AddressResponse>;

    public class AddAddressCommandHandler : IRequestHandler<AddAddressCommand, AddressResponse>
    {
        private readonly UserDbContext _dbContext;
        private readonly IMapper _mapper;

        public AddAddressCommandHandler(UserDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<AddressResponse> Handle(AddAddressCommand command, CancellationToken cancellationToken)
        {
            if (command.Request.IsDefault)
            {
                var existingDefaults = await _dbContext.Addresses
                    .Where(a => a.UserId == command.UserId && a.IsDefault)
                    .ToListAsync(cancellationToken);

                foreach (var addr in existingDefaults)
                    addr.IsDefault = false;
            }

            var address = _mapper.Map<Address>(command.Request);
            address.UserId = command.UserId;

            _dbContext.Addresses.Add(address);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return _mapper.Map<AddressResponse>(address);
        }
    }
}

using AutoMapper;
using Ecommerce.Model.User.Request;
using Ecommerce.Model.User.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;
using User.Application.Entities;

namespace User.Application.Commands
{
    public record UpdateAddressCommand(Guid UserId, long AddressId, AddressRequest Request) : IRequest<AddressResponse>;

    public class UpdateAddressCommandHandler : IRequestHandler<UpdateAddressCommand, AddressResponse>
    {
        private readonly UserDbContext _dbContext;
        private readonly IMapper _mapper;

        public UpdateAddressCommandHandler(UserDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<AddressResponse> Handle(UpdateAddressCommand command, CancellationToken cancellationToken)
        {
            var address = await _dbContext.Addresses
                .FirstOrDefaultAsync(a => a.Id == command.AddressId && a.UserId == command.UserId, cancellationToken);

            if (address == null)
                return null;

            if (command.Request.IsDefault && !address.IsDefault)
            {
                var existingDefaults = await _dbContext.Addresses
                    .Where(a => a.UserId == command.UserId && a.IsDefault && a.Id != command.AddressId)
                    .ToListAsync(cancellationToken);

                foreach (var addr in existingDefaults)
                    addr.IsDefault = false;
            }

            address.Line1 = command.Request.Line1;
            address.Line2 = command.Request.Line2;
            address.City = command.Request.City;
            address.County = command.Request.County;
            address.PostCode = command.Request.PostCode;
            address.Country = command.Request.Country;
            address.IsDefault = command.Request.IsDefault;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return _mapper.Map<AddressResponse>(address);
        }
    }
}

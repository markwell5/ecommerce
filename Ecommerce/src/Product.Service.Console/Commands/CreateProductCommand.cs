using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Product.Service.Commands
{
    public class CreateProductCommand : IRequest<int>
    {
        public CreateProductCommand(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
    {
        public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            Console.WriteLine(request.Name);

            return new Random().Next(1, 100);
        }
    }
}

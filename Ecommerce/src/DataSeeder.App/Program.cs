using Core.Domain;
using Core.Domain.Commands;
using System;

namespace DataSeeder.App
{
    class Program
    {
        static void Main(string[] args)
        {
            var publisher = new Publisher("");

            publisher.Publish(Topics.CreateProduct, new CreateProduct("book")).Wait();
            publisher.Publish(Topics.CreateProduct, new CreateProduct("diary")).Wait();
            publisher.Publish(Topics.CreateProduct, new CreateProduct("magazine")).Wait();
        }
    }
}

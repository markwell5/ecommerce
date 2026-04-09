using System;
using System.Threading;
using System.Threading.Tasks;

namespace Return.Application.Carriers
{
    public class StubCarrierAdapter : ICarrierAdapter
    {
        public Task<ShippingLabel> GenerateReturnLabelAsync(string rmaNumber, string carrier, CancellationToken cancellationToken = default)
        {
            var trackingNumber = $"{carrier.ToUpper()}-RTN-{Guid.NewGuid().ToString()[..8].ToUpper()}";

            return Task.FromResult(new ShippingLabel
            {
                TrackingNumber = trackingNumber,
                LabelUrl = $"https://labels.example.com/{carrier}/{trackingNumber}.pdf",
                Carrier = carrier
            });
        }

        public Task<ShipmentStatus> GetShipmentStatusAsync(string trackingNumber, string carrier, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ShipmentStatus
            {
                TrackingNumber = trackingNumber,
                Status = "InTransit",
                Location = "Distribution Center"
            });
        }
    }
}

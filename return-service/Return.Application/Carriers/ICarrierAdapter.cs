using System.Threading;
using System.Threading.Tasks;

namespace Return.Application.Carriers
{
    public class ShippingLabel
    {
        public string TrackingNumber { get; set; } = string.Empty;
        public string LabelUrl { get; set; } = string.Empty;
        public string Carrier { get; set; } = string.Empty;
    }

    public class ShipmentStatus
    {
        public string TrackingNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }

    public interface ICarrierAdapter
    {
        Task<ShippingLabel> GenerateReturnLabelAsync(string rmaNumber, string carrier, CancellationToken cancellationToken = default);
        Task<ShipmentStatus> GetShipmentStatusAsync(string trackingNumber, string carrier, CancellationToken cancellationToken = default);
    }
}

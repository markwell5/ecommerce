namespace Ecommerce.Model.User.Request
{
    public class AddressRequest
    {
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string PostCode { get; set; }
        public string Country { get; set; }
        public bool IsDefault { get; set; }
    }
}

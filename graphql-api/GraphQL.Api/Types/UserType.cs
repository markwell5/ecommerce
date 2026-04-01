namespace GraphQL.Api.Types;

public class User
{
    public string Id { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string CreatedAt { get; set; } = default!;
}

public class Address
{
    public long Id { get; set; }
    public string Line1 { get; set; } = default!;
    public string? Line2 { get; set; }
    public string City { get; set; } = default!;
    public string County { get; set; } = default!;
    public string PostCode { get; set; } = default!;
    public string Country { get; set; } = default!;
    public bool IsDefault { get; set; }
}

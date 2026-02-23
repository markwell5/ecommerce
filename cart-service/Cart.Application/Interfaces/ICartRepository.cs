using Cart.Application.Models;

namespace Cart.Application.Interfaces;

public interface ICartRepository
{
    Task<Models.Cart?> GetCartAsync(string cartId);
    Task SaveCartAsync(Models.Cart cart);
    Task DeleteCartAsync(string cartId);
}

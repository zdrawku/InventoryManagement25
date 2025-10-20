using InventoryManagement2025.Models;
using InventoryManagement2025.Services.Models;

namespace InventoryManagement2025.Services
{
    public interface ITokenService
    {
        TokenResult CreateToken(User user);
    }
}

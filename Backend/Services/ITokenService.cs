using Backend.Models;

namespace Backend.Services
{
    public interface ITokenService
    {
        string CreateAccessToken(UserModel user);
    }
}

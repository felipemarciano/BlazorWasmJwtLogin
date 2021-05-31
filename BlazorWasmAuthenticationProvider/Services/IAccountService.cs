using BlazorWasmAuthenticationProvider.Models;
using System.Threading.Tasks;

namespace BlazorWasmAuthenticationProvider.Services
{
    public interface IAccountService
    {
        Task<bool> LoginAsync(LoginModel model);
        Task<bool> LogoutAsync();
    }
}

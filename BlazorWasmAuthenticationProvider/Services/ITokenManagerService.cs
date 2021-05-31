using System.Threading.Tasks;

namespace BlazorWasmAuthenticationProvider.Services
{
    public interface ITokenManagerService
    {
        Task<string> GetTokenAsync();
    }
}

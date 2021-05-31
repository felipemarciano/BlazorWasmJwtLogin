using Blazored.LocalStorage;
using BlazorWasmAuthenticationProvider.Auth;
using BlazorWasmAuthenticationProvider.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BlazorWasmAuthenticationProvider.Services
{
    public class AccountService : IAccountService
    {
        private readonly AuthenticationStateProvider _customAuthenticationProvider;
        private readonly ILocalStorageService _localStorageService;
        private readonly HttpClient _httpClient;
        private readonly NavigationManager _navigationManager;

        public AccountService(ILocalStorageService localStorageService,
            AuthenticationStateProvider customAuthenticationProvider,
            IHttpClientFactory httpClientFactory,
            NavigationManager navigationManager)
        {
            _localStorageService = localStorageService;
            _customAuthenticationProvider = customAuthenticationProvider;
            _httpClient = httpClientFactory.CreateClient("ServerAPI.AnonymousAPI");
            _navigationManager = navigationManager;
        }
        public async Task<bool> LoginAsync(LoginModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/account/login", model);
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }
            AuthResponse authData = await response.Content.ReadFromJsonAsync<AuthResponse>();
            await _localStorageService.SetItemAsync("token", authData.Token);
            (_customAuthenticationProvider as CustomAuthenticationProvider).Notify();
            _navigationManager.NavigateTo(model.ReturnUrl ?? "");
            return true;
        }

        public async Task<bool> LogoutAsync()
        {
            await _localStorageService.RemoveItemAsync("token");
            (_customAuthenticationProvider as CustomAuthenticationProvider).Notify();
            _navigationManager.NavigateTo("/Login");
            return true;
        }
    }
}

using BlazorWasmAuthenticationProvider.Helpers;
using BlazorWasmAuthenticationProvider.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazorWasmAuthenticationProvider.Auth
{
    public class CustomAuthenticationProvider : AuthenticationStateProvider
    {
        private readonly ITokenManagerService _tokenManagerService;
        private readonly NavigationManager _navigationManager;

        public CustomAuthenticationProvider(ITokenManagerService tokenManagerService, NavigationManager navigationManager)
        {
            _tokenManagerService = tokenManagerService;
            _navigationManager = navigationManager;
        }
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            string token = await _tokenManagerService.GetTokenAsync();

            if (string.IsNullOrEmpty(token))
            {
                var anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity() { }));

                var returnUrl = new Uri(_navigationManager.Uri).PathAndQuery;
                _navigationManager.NavigateTo($"/login?returnUrl={returnUrl}");

                return anonymous;
            }

            var userClaimPrincipal = new ClaimsPrincipal(new ClaimsIdentity(JwtParser.ParseClaimsFromJwt(token), "Authentication"));
            var loginUser = new AuthenticationState(userClaimPrincipal);
            return loginUser;
        }

        public void Notify()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}

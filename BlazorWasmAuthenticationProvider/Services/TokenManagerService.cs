using Blazored.LocalStorage;
using BlazorWasmAuthenticationProvider.Helpers;
using BlazorWasmAuthenticationProvider.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazorWasmAuthenticationProvider.Services
{
    public class TokenManagerService : ITokenManagerService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorageService;
        public TokenManagerService(ILocalStorageService localStorageService, IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ServerAPI.AnonymousAPI");
            _localStorageService = localStorageService;
        }

        private bool ValidateTokenExpiration(string token)
        {
            List<Claim> claims = JwtParser.ParseClaimsFromJwt(token).ToList();
            if (claims?.Count == 0)
            {
                return false;
            }
            string expirationSeconds = claims.Where(_ => _.Type.ToLower() == "exp").Select(_ => _.Value).FirstOrDefault();
            if (string.IsNullOrEmpty(expirationSeconds))
            {
                return false;
            }

            var exprationDate = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(expirationSeconds));
            if (exprationDate < DateTime.UtcNow)
            {
                return false;
            }
            return true;
        }

        private async Task<string> RefreshTokenEndPoint(TokenModel tokenModel)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/account/refreshtoken", tokenModel.Token);
            if (!response.IsSuccessStatusCode)
            {
                return string.Empty;
            }
            AuthResponse authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            await _localStorageService.SetItemAsync("token", authResponse.Token);

            return authResponse.Token;
        }

        public async Task<string> GetTokenAsync()
        {
            string token = await _localStorageService.GetItemAsync<string>("token");
            if (string.IsNullOrEmpty(token))
            {
                return string.Empty;
            }

            if (ValidateTokenExpiration(token))
            {
                return token;
            }

            TokenModel tokenModel = new() { Token = token };
            return await RefreshTokenEndPoint(tokenModel);
        }
    }
}

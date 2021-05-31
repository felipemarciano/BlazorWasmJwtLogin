using BlazorWasmAuthenticationProvider.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorWasmAuthenticationProvider.Helpers
{
    public class TokenHandler : DelegatingHandler
    {
        private readonly ITokenManagerService _tokenManagerService;
        private readonly NavigationManager _navigationManager;

        public TokenHandler(
            ITokenManagerService tokenManagerService, NavigationManager navigationManager)
        {
            _tokenManagerService = tokenManagerService;
            _navigationManager = navigationManager;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string token = await _tokenManagerService.GetTokenAsync();

            if (string.IsNullOrEmpty(token))
            {
                var returnUrl = new Uri(_navigationManager.Uri).PathAndQuery;
                _navigationManager.NavigateTo($"/login?returnUrl={returnUrl}");

                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                response.Content = new StringContent("");
                return response;
            }

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}

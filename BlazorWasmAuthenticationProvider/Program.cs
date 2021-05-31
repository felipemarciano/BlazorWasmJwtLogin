using Blazored.LocalStorage;
using BlazorWasmAuthenticationProvider.Auth;
using BlazorWasmAuthenticationProvider.Helpers;
using BlazorWasmAuthenticationProvider.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorWasmAuthenticationProvider
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddHttpClient("ServerAPI.AnonymousAPI",
              client => client.BaseAddress = new Uri("https://localhost:5011"));

            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationProvider>();
            builder.Services.AddAuthorizationCore();
            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<ITokenManagerService, TokenManagerService>();

            builder.Services.AddScoped<TokenHandler>();

            builder.Services.AddHttpClient("ServerAPI",
                  client => client.BaseAddress = new Uri("https://localhost:5011"))
                .AddHttpMessageHandler<TokenHandler>();

            builder.Services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>()
                    .CreateClient("ServerAPI"));

            builder.Services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>()
                  .CreateClient("ServerAPI.AnonymousAPI"));

            await builder.Build().RunAsync();
        }
    }
}

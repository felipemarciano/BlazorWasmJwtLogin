using BlazorWasmAuthenticationProvider.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorWasmAuthenticationProvider.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : Controller
    {
        private readonly IConfiguration _config;
        private readonly double _expiresTokenMinutes;
        private readonly double _expiresRefreshTokenMinutes;
        private readonly IDistributedCache _cache;

        public AccountController(IConfiguration config, IDistributedCache cache)
        {
            _config = config;
            _cache = cache;
            _expiresTokenMinutes = 1;
            _expiresRefreshTokenMinutes = 1;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginModel login)
        {
            IActionResult response = Unauthorized();
            var user = AuthenticateUser(login);

            if (user != null)
            {
                var tokenString = await GenerateJSONWebTokenAsync(user);
                response = Ok(new { token = tokenString });
            }

            return response;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("refreshtoken")]
        public async Task<IActionResult> RefreshTokenAsync([FromBody] string token)
        {
            IActionResult response = Ok(new { token = "" });

            var validToken = await _cache.GetStringAsync(token);

            if (validToken == null) return response;

            var jwtSecurityTokenOld = new JwtSecurityToken(validToken);

            var newToken = await GenerateRefreshTokenAsync(jwtSecurityTokenOld);

            return Ok(new { token = newToken });
        }

        private async Task<string> GenerateJSONWebTokenAsync(LoginModel userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, userInfo.Email),
                new Claim(JwtRegisteredClaimNames.Email, userInfo.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var jwtSecurityToken = new JwtSecurityToken(_config["Jwt:Issuer"],
                _config["Jwt:Issuer"],
                claims,
                expires: DateTime.Now.AddMinutes(_expiresTokenMinutes),
                signingCredentials: credentials);

            var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

            var options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(_expiresRefreshTokenMinutes));
            await _cache.SetStringAsync(token, JsonSerializer.Serialize(token), options);

            return token;
        }

        private async Task<string> GenerateRefreshTokenAsync(JwtSecurityToken jwtSecurityToken)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var newJwtSecurityToken = new JwtSecurityToken(_config["Jwt:Issuer"],
                _config["Jwt:Issuer"],
                jwtSecurityToken.Claims,
                expires: DateTime.Now.AddMinutes(_expiresTokenMinutes),
                signingCredentials: credentials);

            var newToken = new JwtSecurityTokenHandler().WriteToken(newJwtSecurityToken);
            var options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(_expiresRefreshTokenMinutes));
            await _cache.SetStringAsync(newToken, JsonSerializer.Serialize(newToken), options);

            return newToken;
        }

        private LoginModel AuthenticateUser(LoginModel login)
        {
            LoginModel user = null;

            if (login.Email == "teste@teste.com")
            {
                user = new LoginModel { Email = "teste@teste.com" };
            }

            return user;
        }
    }
}


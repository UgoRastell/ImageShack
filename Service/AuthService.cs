using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class AuthService
    {
        private readonly IJSRuntime _jsRuntime;

        public AuthService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<string> GetTokenAsync()
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            return token;
        }

        public async Task<ClaimsPrincipal> GetUserAsync()
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Token is null or empty.");
                return new ClaimsPrincipal(new ClaimsIdentity());
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
                var user = new ClaimsPrincipal(identity);
                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading JWT token: {ex.Message}");
                return new ClaimsPrincipal(new ClaimsIdentity());
            }
        }

        public async Task<string> GetUserEmailAsync()
        {
            var user = await GetUserAsync();
            var emailClaim = user.FindFirst(ClaimTypes.Email);
            Console.WriteLine($"User email: {emailClaim?.Value}");
            return emailClaim?.Value;
        }

        public async Task<bool> IsUserAuthenticatedAsync()
        {
            var user = await GetUserAsync();
            return user.Identity.IsAuthenticated;
        }

        public async Task LogoutAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
        }

        public async Task<Guid?> GetUserIdAsync()
        {
            var user = await GetUserAsync();
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }
            return null;
        }
    }
}

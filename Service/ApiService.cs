using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Models.ViewModel;

namespace Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<string> LoginAsync(LoginViewModel loginModel)
        {
            var loginUrl = new Uri(_httpClient.BaseAddress, "Login/login");
            Console.WriteLine($"Login URL: {loginUrl}");

            var response = await _httpClient.PostAsJsonAsync(loginUrl, loginModel);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<LoginResult>();
            return result.Token;
        }

        public async Task<HttpResponseMessage> RegisterUserAsync(UserRegistrationViewModel userRegistration)
        {
            var response = await _httpClient.PostAsJsonAsync("Login/register", userRegistration);
            return response;
        }

        private class LoginResult
        {
            public string Token { get; set; }
        }
    }
}

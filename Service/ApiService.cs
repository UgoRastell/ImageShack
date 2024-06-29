using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Models;
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

        public async Task<HttpResponseMessage> DeleteUserAsync(Guid userId)
        {
            return await _httpClient.DeleteAsync($"Login/DeleteUser/{userId}");
        }


        public async Task<List<ImageViewModel>> GetPublicImagesAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<ImageViewModel>>("Image/GetPublicImages");
        }

        public async Task<List<ImageViewModel>> GetUserImagesAsync(Guid userId)
        {
            return await _httpClient.GetFromJsonAsync<List<ImageViewModel>>($"Image/GetUserImages/{userId}");
        }

        public async Task<HttpResponseMessage> ToggleImageVisibilityAsync(Guid imageId)
        {
            return await _httpClient.PostAsync($"Image/ToggleImageVisibility/{imageId}", null);
        }

        public async Task<HttpResponseMessage> UploadImageAsync(MultipartFormDataContent content)
        {
            return await _httpClient.PostAsync("Image/UploadImage", content);
        }

        public async Task<HttpResponseMessage> DeleteImageAsync(Guid imageId)
        {
            return await _httpClient.DeleteAsync($"Image/DeleteImage/{imageId}");
        }


        private class LoginResult
        {
            public string Token { get; set; }
        }
    }
}

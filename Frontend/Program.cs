using Blazored.Toast;
using Frontend;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7120") });

builder.Services.AddScoped<AuthService>();

builder.Services.AddScoped<ApiService>();

builder.Services.AddBlazoredToast();

await builder.Build().RunAsync();

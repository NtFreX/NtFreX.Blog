using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using BlazorDownloadFile;
using Blazored.LocalStorage;

namespace NtFreX.Blog.Client
{
    public class Program
    {
        public static string TwoFactorUserTokenName = "usertoken";

        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => {
                var localStorage = sp.GetRequiredService<ILocalStorageService>();
                return new HttpClient(new JwtTokenHttpHandler(localStorage))
                {
                    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
                };
            });
            builder.Services.AddBlazorDownloadFile();
            builder.Services.AddBlazoredLocalStorage();

            await builder.Build().RunAsync();
        }
    }
}

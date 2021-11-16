using System.Net.Http;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using System.Threading;
using System.Net.Http.Headers;

namespace NtFreX.Blog.Client
{
    public class JwtTokenHttpHandler : DelegatingHandler
    {
        private readonly ILocalStorageService localStorageService;

        public const string SessionStorageKey = "token";

        public JwtTokenHttpHandler(ILocalStorageService localStorageService)
        {
            InnerHandler = new HttpClientHandler();

            this.localStorageService = localStorageService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await localStorageService.GetItemAsStringAsync(SessionStorageKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await base.SendAsync(request, cancellationToken);
        }
    }
}

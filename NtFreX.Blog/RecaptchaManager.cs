using Microsoft.Extensions.Logging;
using NtFreX.Blog.Configuration;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace NtFreX.Blog
{
    public class RecaptchaManager
    {
        private readonly ConfigPreloader config;
        private readonly ILogger<RecaptchaManager> logger;

        public RecaptchaManager(ConfigPreloader config, ILogger<RecaptchaManager> logger)
        {
            this.config = config;
            this.logger = logger;
        }

        public async Task<bool> ValidateReCaptchaResponseAsync(string encodedResponse)
        {
            logger.LogTrace("Checking recaptcha");

            if (string.IsNullOrEmpty(BlogConfiguration.ReCaptchaSiteKey))
            {
                logger.LogDebug("No recaptcha site key is configured");
                return true;
            }

            if (string.IsNullOrEmpty(encodedResponse))
            {
                logger.LogDebug("The given recaptcha response is null or empty");
                return false;
            }

            var secret = config.Get(ConfigNames.RecaptchaSecret);
            if (string.IsNullOrEmpty(secret))
            {
                logger.LogWarning("No recaptcha site secret is configured but recaptcha is enabled");
                return false;
            }

            try
            {
                var client = new HttpClient();
                var googleReply = await client.GetStreamAsync($"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={encodedResponse}");
                var parsed = await JsonSerializer.DeserializeAsync<RecaptchaResponse>(googleReply);
               
                logger.LogInformation("The recaptcha was " + (parsed.success ? "valid" : "invalid"));

                return parsed.success;
            }
            catch (Exception exce)
            {
                logger.LogError(exce, "The given recaptcha could not be validated");
                return false;
            }
        }

        private class RecaptchaResponse
        {
            public bool success { get; set; }
        }
    }
}

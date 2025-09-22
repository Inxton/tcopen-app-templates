using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace x_template_xHmi.Wpf.SynologyStatus
{
    public class Synology
    {
        private readonly string nasIpAddress;
        private readonly string username;
        private readonly string password;
        private readonly HttpClient client;

        public Synology(string ip, string user, string pass)
        {
            nasIpAddress = ip;
            username = user;
            password = pass;

            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            client = new HttpClient(httpClientHandler);
        }

        public async Task<string> ReadHealthStatus()
        {
            string systemHealth = "unknown";

            try
            {
                // Login
                var loginParams = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("api", "SYNO.API.Auth"),
                    new KeyValuePair<string, string>("version", "6"),
                    new KeyValuePair<string, string>("method", "login"),
                    new KeyValuePair<string, string>("account", username),
                    new KeyValuePair<string, string>("passwd", password),
                    new KeyValuePair<string, string>("session", "Core"),
                    new KeyValuePair<string, string>("format", "sid")
                });

                if (string.IsNullOrWhiteSpace(nasIpAddress))
                {
                    // Option 1: silently skip
                    return null;
                }
                var loginResponse = await client.PostAsync($"https://{nasIpAddress}/webapi/auth.cgi", loginParams);
                var loginContentBytes = await loginResponse.Content.ReadAsByteArrayAsync();
                string loginContent = Encoding.UTF8.GetString(loginContentBytes);

                var loginResult = JObject.Parse(loginContent);
                if (loginResult["success"].Value<bool>())
                {
                    string sid = loginResult["data"]["sid"].Value<string>();

                    // API path a verzia
                    var (apiPath, apiVersion) = await GetApiInfo("SYNO.Core.System.SystemHealth");

                    var healthUri = $"https://{nasIpAddress}/webapi/{apiPath}?api=SYNO.Core.System.SystemHealth&version={apiVersion}&method=get&_sid={sid}";
                    var healthResponse = await client.GetAsync(healthUri);
                    var healthContentBytes = await healthResponse.Content.ReadAsByteArrayAsync();
                    string healthContent = Encoding.UTF8.GetString(healthContentBytes);

                    var healthResult = JObject.Parse(healthContent);
                    if (healthResult["success"].Value<bool>())
                    {
                        var ruleObj = healthResult["data"]["rule"] as JObject;
                        systemHealth = ParseSystemHealthFromRule(ruleObj);
                    }

                    // Logout
                    var logoutParams = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("api", "SYNO.API.Auth"),
                        new KeyValuePair<string, string>("version", "6"),
                        new KeyValuePair<string, string>("method", "logout"),
                        new KeyValuePair<string, string>("_sid", sid)
                    });

                    await client.PostAsync($"https://{nasIpAddress}/webapi/auth.cgi", logoutParams);
                }
            }
            catch (Exception ex)
            {
                // Optional log
                // Console.WriteLine($"Exception: {ex.Message}");
            }

            return systemHealth;
        }

        private async Task<(string path, int version)> GetApiInfo(string apiName)
        {
            string uri = $"https://{nasIpAddress}/webapi/query.cgi?api=SYNO.API.Info&version=1&method=query&query={apiName}";
            var response = await client.GetAsync(uri);
            var contentBytes = await response.Content.ReadAsByteArrayAsync();
            string content = Encoding.UTF8.GetString(contentBytes);

            var result = JObject.Parse(content);
            var data = result["data"]?[apiName];
            if (data != null)
            {
                string path = data["path"]?.Value<string>() ?? "entry.cgi";
                int version = data["maxVersion"]?.Value<int>() ?? 1;
                return (path, version);
            }

            return ("entry.cgi", 1);
        }

        private string ParseSystemHealthFromRule(JObject ruleObject)
        {
            if (ruleObject == null)
                return "unknown";

            string code = "unknown";

            var description = ruleObject["description"];
            if (description != null)
            {
                var descriptionParams = description["description_params"] as JArray;
                if (descriptionParams != null && descriptionParams.Count > 0)
                {
                    code = descriptionParams[0].ToString();
                }
            }

            switch (code)
            {
                case "widget:system_ok":
                    return "healthy";
                case "widget:system_warning":
                    return "warning";
                case "widget:system_danger":
                    return "critical";
                default:
                    return "unknown";
            }
        }
    }
}

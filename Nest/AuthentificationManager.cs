using Constellation.Host;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;

namespace Nest
{
    // Use to generate the AccessToken
    internal static class AuthentificationManager
    {
        private const string requestTokenUrl = "https://home.nest.com/login/oauth2?client_id={0}&state={1}";
        private const string accessTokenUrl = "https://api.home.nest.com/oauth2/access_token?code={0}&client_id={1}&client_secret={2}&grant_type=authorization_code";

        public static string GetAccessToken()
        {
            string authorizationUrl = string.Format(requestTokenUrl, PackageHost.GetSettingValue<string>("ClientId"), "ConstellationToken" + new Random().Next(1000, 9999).ToString());

            using (var process = Process.Start(authorizationUrl))
            {
                PackageHost.WriteWarn("Awaiting response, please accept on the Works with Nest page to continue and Type your PIN code.");

                string pinCode = Console.ReadLine();

                string url = string.Format(accessTokenUrl,
                 pinCode, PackageHost.GetSettingValue<string>("ClientId"), PackageHost.GetSettingValue<string>("ClientSecret"));

                using (var httpClient = new HttpClient())
                {
                    using (var response = httpClient.PostAsync(url, content: null).Result)
                    {
                        var accessTokenObject = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
                        return (accessTokenObject as dynamic).access_token;
                    }
                }
            }
        }
    }
}

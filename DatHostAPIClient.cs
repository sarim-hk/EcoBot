using Microsoft.Extensions.Logging;
using System;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;


namespace EcoBot
{
    class DatHostAPIClient
    {
        private ILogger Logger;
        private string _authInfo;

        public DatHostAPIClient(string email, string password, ILogger logger)
        {
            Logger = logger;
            _authInfo = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{email}:{password}"));
        }

        public async Task<string?> BaseRequest(string apiURL)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _authInfo);

                try
                {
                    HttpResponseMessage response = await client.GetAsync(apiURL);

                    if (response.IsSuccessStatusCode)
                    {
                        string content = await response.Content.ReadAsStringAsync();
                        return content;
                    }
                    else
                    {
                        Logger.LogError($"{response.StatusCode} - {response.ReasonPhrase}");
                        return null;
                    }
                }
                
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                    return null;
                }
            }
        }

    }
}

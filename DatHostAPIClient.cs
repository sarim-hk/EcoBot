using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace EcoBot {
    class DatHostAPIClient {
        private ILogger _logger;
        private string? _serverID;
        private string? _authInfo;

        public DatHostAPIClient(ILogger logger) {
            _logger = logger;
        }

        public void SetAuthDetails(string email, string password, string serverID) {
            _serverID = serverID;
            _authInfo = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{email}:{password}"));
        }

        private async Task<string?> BaseGetRequest(string apiURL) {
            using (HttpClient client = new HttpClient()) {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _authInfo);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try {
                    HttpResponseMessage response = await client.GetAsync(apiURL);

                    if (response.IsSuccessStatusCode) {
                        string content = await response.Content.ReadAsStringAsync();
                        return content;
                    }
                    else {
                        _logger.LogError($"{response.StatusCode} - {response.ReasonPhrase} - {response.Content}");
                        return null;
                    }
                }
                catch (Exception ex) {
                    _logger.LogError(ex.StackTrace);
                    return null;
                }
            }
        }

        private async Task<HttpStatusCode> BasePostRequest(string apiURL, Dictionary<string, string>? parameters = null) {
            using (HttpClient client = new HttpClient()) {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _authInfo);

                var content = new FormUrlEncodedContent(parameters ?? new Dictionary<string, string>());

                try {
                    HttpResponseMessage response = await client.PostAsync(apiURL, content);

                    // Log error information if not successful
                    if (!response.IsSuccessStatusCode) {
                        _logger.LogError($"{response.StatusCode} - {response.ReasonPhrase}");
                    }

                    return response.StatusCode;
                }
                catch (Exception ex) {
                    _logger.LogError(ex.StackTrace);
                    return HttpStatusCode.InternalServerError; // Default to 500 for exceptions
                }
            }
        }

        public async Task<RconDetails?> GetRconDetails() {
            string? response = await BaseGetRequest($"https://dathost.net/api/0.1/game-servers/{_serverID}");
            if (string.IsNullOrEmpty(response)) return null;

            try {
                using JsonDocument doc = JsonDocument.Parse(response);
                string? rawIP = doc.RootElement.GetProperty("raw_ip").GetString();
                int? port = doc.RootElement.GetProperty("ports").GetProperty("game").GetInt32();
                string? rconPassword = doc.RootElement.GetProperty("cs2_settings").GetProperty("rcon").GetString();

                if (string.IsNullOrEmpty(rawIP) || !port.HasValue || string.IsNullOrEmpty(rconPassword)) {
                    _logger.LogError($"Some fetched RCON details are missing: {rawIP} {port} {rconPassword}");
                    return null;
                }

                var rconDetails = new RconDetails(rawIP, port.Value, rconPassword);
                return rconDetails;

            }
            catch (Exception ex) {
                _logger.LogError(ex.StackTrace);
                return null;
            }
        }

        public async Task<int?> GetServerStatus() {
            string? response = await BaseGetRequest($"https://dathost.net/api/0.1/game-servers/{_serverID}");
            if (string.IsNullOrEmpty(response)) return null;

            try {
                using JsonDocument doc = JsonDocument.Parse(response);

                bool? isServerBooting = doc.RootElement.GetProperty("booting").GetBoolean();
                bool? isServerOn = doc.RootElement.GetProperty("on").GetBoolean();

                if (isServerBooting != null && isServerBooting == true) {
                    return -1;
                }

                if (isServerOn != null) {
                    return Convert.ToInt32(isServerOn);
                }

                return null;
            }
            catch (Exception ex) {
                _logger.LogError(ex.StackTrace);
                return null;
            }
        }

        public async Task<bool> StartServer() {
            var parameters = new Dictionary<string, string> { { "allow_host_reassignment", "false" } };
            HttpStatusCode statusCode = await BasePostRequest($"https://dathost.net/api/0.1/game-servers/{_serverID}/start", parameters);

            // Return true only if request was successful (2xx status code)
            return statusCode >= HttpStatusCode.OK && statusCode < HttpStatusCode.MultipleChoices;
        }

        public async Task<bool> StopServer() {
            HttpStatusCode statusCode = await BasePostRequest($"https://dathost.net/api/0.1/game-servers/{_serverID}/stop");

            // Return true only if request was successful (2xx status code)
            return statusCode >= HttpStatusCode.OK && statusCode < HttpStatusCode.MultipleChoices;
        }

    }
}

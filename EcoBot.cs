using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EcoBot
{
    public class EcoBot
    {
        private static IConfigurationRoot? Config;
        private static ILogger Logger = LoggerFactory.Create(builder => {builder.AddConsole().AddDebug().SetMinimumLevel(LogLevel.Information);}).CreateLogger<EcoBot>();
        private static DiscordSocketClient? _client;

        private static bool CreateConfig()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            string? token = config["Discord:Token"];
            if (string.IsNullOrEmpty(token))
            {
                Logger.LogCritical("No Discord Token in config.");
                return false;
            }

            Config = config;
            return true;

        }

        public static async Task Main()
        {
            if (!CreateConfig()) return;

            _client = new DiscordSocketClient();
            await _client.LoginAsync(TokenType.Bot, Config!["Discord:Token"]);
            await _client.StartAsync();
            await Task.Delay(-1);
        }


    }
}

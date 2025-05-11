using Discord;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;

namespace EcoBot
{

    public enum LogType
    {
        Critical,
        Error,
        Information,
        Debug
    }

    public class EcoBot
    {
        private static IConfigurationRoot Config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .Build();

        private static ILogger Logger = LoggerFactory.Create(builder => { builder.AddConsole().AddDebug().SetMinimumLevel(LogLevel.Information); }).CreateLogger<EcoBot>();
        private static DiscordSocketClient _client = new DiscordSocketClient();

        private static bool ValidateConfig()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .Build();

            string? discordToken = config["Discord:Token"];
            if (string.IsNullOrEmpty(discordToken))
            {
                Logger.LogCritical("No Discord Token in config.");
                return false;
            }

            string? discordGuildID = config["Discord:GuildID"];
            if (string.IsNullOrEmpty(discordGuildID) || (discordGuildID == "-1"))
            {
                Logger.LogCritical("No Discord Guild ID in config.");
                return false;
            }

            LogAsync("Config validated successfully.", LogType.Information);
            return true;
        }

        public static async Task Main()
        {
            if (!ValidateConfig()) return;

            _client.Ready += ReadyAsync;
            _client.SlashCommandExecuted += SlashCommandHandler;

            await _client.LoginAsync(TokenType.Bot, Config["Discord:Token"]!);
            await _client.StartAsync();
            await Task.Delay(Timeout.Infinite);
        }

        private static Task LogAsync(string logMessage, LogType logType)
        {
            switch (logType)
            {
                case LogType.Critical:
                    Logger.LogCritical(logMessage);
                    break;

                case LogType.Error:
                    Logger.LogError(logMessage);
                    break;

                case LogType.Information:
                    Logger.LogInformation(logMessage);
                    break;

                case LogType.Debug:
                    Logger.LogInformation(logMessage);
                    break;
            }

            return Task.CompletedTask;
        }

        private static async Task ReadyAsync()
        {
            var guild = _client.GetGuild(ulong.Parse(Config["Discord:GuildID"]!));
            await LogAsync($"Client is ready at {guild.Name}", LogType.Information);

            var guildCommand = new SlashCommandBuilder();
            guildCommand.WithName("startserver");

            try
            {
                await guild.CreateApplicationCommandAsync(guildCommand.Build());
            }
            catch (HttpException ex)
            {
                await LogAsync(ex.ToString(), LogType.Critical);
            }
        }

        private static async Task SlashCommandHandler(SocketSlashCommand command)
        {
            try
            {
                await command.RespondAsync("server started (theoretically)");
            }
            catch ( Exception ex )
            {
                await LogAsync(ex.ToString(), LogType.Critical);
            }
        }

    }
}

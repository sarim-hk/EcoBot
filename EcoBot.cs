using Discord;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EcoBot {

    public partial class EcoBot {

        private static IConfigurationRoot Config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .Build();

        private static ILogger _logger = LoggerFactory.Create(builder => { builder.AddConsole().AddDebug().SetMinimumLevel(LogLevel.Information); }).CreateLogger<EcoBot>();
        private static DiscordSocketClient _discordClient = new DiscordSocketClient();
        private static DatHostAPIClient _datHostClient = new DatHostAPIClient(_logger);


        public static async Task Main() {
            if (!ValidateConfig()) return;

            _discordClient.Ready += ReadyAsync;
            _discordClient.SlashCommandExecuted += SlashCommandHandler;

            await _discordClient.LoginAsync(TokenType.Bot, Config["Discord:Token"]!);
            await _discordClient.StartAsync();
            await Task.Delay(Timeout.Infinite);
        }

        private static async Task ReadyAsync() {

            _datHostClient.SetAuthDetails(Config["DatHost:EmailAddress"]!, Config["DatHost:Password"]!, Config["DatHost:ServerID"]!);

            var guild = _discordClient.GetGuild(ulong.Parse(Config["Discord:GuildID"]!));
            var startServerCommandBuild = new SlashCommandBuilder().WithName("startserver").WithDescription("Start the eco server.");
            var restartServerCommandBuild = new SlashCommandBuilder().WithName("restartserver").WithDescription("Restart the eco server.");
            var stopServerCommandBuild = new SlashCommandBuilder().WithName("stopserver").WithDescription("Stop the eco server.");

            try {
                await guild.CreateApplicationCommandAsync(startServerCommandBuild.Build());
                await guild.CreateApplicationCommandAsync(restartServerCommandBuild.Build());
                await guild.CreateApplicationCommandAsync(stopServerCommandBuild.Build());
            }
            catch (HttpException ex) {
                _logger.LogCritical(ex.StackTrace);
            }

            _logger.LogInformation($"Client is ready at {guild.Name}");

        }

        private static async Task SlashCommandHandler(SocketSlashCommand command) {
            switch (command.Data.Name) {
                case "startserver":
                    await HandleStartServerCommand(command);
                    break;
                case "stopserver":
                    await HandleStopServerCommand(command);
                    break;
            }

        }

    }
}

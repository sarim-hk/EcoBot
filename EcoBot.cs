using Discord;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EcoBot
{

    public partial class EcoBot
    {

        private static IConfigurationRoot Config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .Build();

        private static ILogger Logger = LoggerFactory.Create(builder => { builder.AddConsole().AddDebug().SetMinimumLevel(LogLevel.Information); }).CreateLogger<EcoBot>();
        private static DiscordSocketClient _client = new DiscordSocketClient();
        private static DatHostAPIClient DatHostClient = new DatHostAPIClient(Logger);


        public static async Task Main()
        {
            if (!ValidateConfig()) return;

            _client.Ready += ReadyAsync;
            _client.SlashCommandExecuted += SlashCommandHandler;

            await _client.LoginAsync(TokenType.Bot, Config["Discord:Token"]!);
            await _client.StartAsync();
            await Task.Delay(Timeout.Infinite);
        }

        private static async Task ReadyAsync()
        {

            DatHostClient.SetAuthDetails(Config["DatHost:EmailAddress"]!, Config["DatHost:Password"]!, Config["DatHost:ServerID"]!);

            var guild = _client.GetGuild(ulong.Parse(Config["Discord:GuildID"]!));
            var guildCommand = new SlashCommandBuilder().WithName("startserver").WithDescription("Start the eco server.");

            try
            {
                await guild.CreateApplicationCommandAsync(guildCommand.Build());
            }
            catch (HttpException ex)
            {
                Logger.LogCritical(ex.StackTrace);
            }

            Logger.LogInformation($"Client is ready at {guild.Name}");

        }

        private static async Task SlashCommandHandler(SocketSlashCommand command)
        {
            switch (command.Data.Name)
            {
                case "startserver":
                    await HandleStartServerCommand(command);
                    break;
            }

        }

    }
}

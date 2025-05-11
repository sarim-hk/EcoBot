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

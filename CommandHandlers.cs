using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using RconSharp;
using System.Text.RegularExpressions;

namespace EcoBot
{
    public partial class EcoBot
    {
        private static async Task HandleStartServerCommand(SocketSlashCommand command)
        {
            try
            {
                RconDetails? rconDetails = await _datHostClient.GetRconDetails();
                if (rconDetails == null) return;

                var rconClient = new CS2RconClient(rconDetails, _logger);
                
                int? playerCount = await rconClient.GetPlayerCount();
                if (!playerCount.HasValue) return;

                string description;
                switch (playerCount)
                {
                    case 0:
                        description = $"The server is already on, but nobody is online. Did you mean /restartserver?";
                        break;
                    case 1:
                        description = $"The server is already on, with 1 player online.";
                        break;
                    default:
                        description = $"The server is already on, with {playerCount} players online.";
                        break;
                }

                var embed = CreateEmbed(
                    title: "Start Server",
                    description: description,
                    ResponseType.Information
                    );

                await command.RespondAsync(embed: embed.Build());
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.StackTrace);
            }
        }


    }
}

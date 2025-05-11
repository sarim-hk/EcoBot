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
                RconDetails? rconDetails = await DatHostClient.GetRconDetails();
                if (rconDetails == null) return;

                var rconClient = new CS2RconClient(rconDetails, Logger);
                
                int? playerCount = await rconClient.GetPlayerCount();
                if (!playerCount.HasValue) return;

                var embed = CreateEmbed(
                    title: "Start Server",
                    description: $"The server is already on, with {playerCount} players online.",
                    ResponseType.Information
                    );

                await command.RespondAsync(embed: embed.Build());
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex.StackTrace);
            }
        }


    }
}

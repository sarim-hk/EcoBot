using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace EcoBot
{
    public partial class EcoBot
    {
        private static async Task HandleStartServerCommand(SocketSlashCommand command)
        {
            try {

                // Let discord know that you've actually received the command and that you're gonna start processing stuff
                await command.DeferAsync();

                // Get the server status (-1 = booting,  0 = off, 1 = on)
                int? serverStatus = await _datHostClient.GetServerStatus();
                if (serverStatus == null) {
                    var nullStatusEmbed = CreateEmbed(
                        title: "Start Server",
                        description: "An error occurred while getting the current server status.",
                        ResponseType.Error
                        );
                    await command.FollowupAsync(embed: nullStatusEmbed.Build());
                    return;
                }

                // If the server is off, turn it on. If it's already booting, inform the user. If it's on, proceed to next code block.
                Discord.EmbedBuilder startServerEmbed;
                switch (serverStatus) {
                    case 0:
                        bool startServerResponse = await _datHostClient.StartServer();
                        if (startServerResponse == true) {
                            startServerEmbed = CreateEmbed(
                                title: "Start Server",
                                description: "The server will start shortly.",
                                ResponseType.Success
                                );
                        }
                        else
                        {
                            startServerEmbed = CreateEmbed(
                                title: "Start Server",
                                description: "An error occurred while starting the server.",
                                ResponseType.Error
                                );
                        }

                        await command.FollowupAsync(embed: startServerEmbed.Build());
                        return;

                    case -1:
                        startServerEmbed = CreateEmbed(
                            title: "Start Server",
                            description: "The server is currently booting.",
                            ResponseType.Information
                            );
                        await command.FollowupAsync(embed: startServerEmbed.Build());
                        return;
                }

                // Otherwise, lets get the rcon details which we'll use to check if there are people in the server or if its empty
                RconDetails? rconDetails = await _datHostClient.GetRconDetails();
                if (rconDetails == null) {
                    var nullRconDetailsEmbed = CreateEmbed(
                        title: "Start Server",
                        description: "An error occurred while getting the server RCON details.",
                        ResponseType.Error
                        );
                    await command.FollowupAsync(embed: nullRconDetailsEmbed.Build());
                    return;
                }

                // Get the player count
                var rconClient = new CS2RconClient(rconDetails, _logger);
                int? playerCount = await rconClient.GetPlayerCount();
                if (!playerCount.HasValue) {
                    var nullPlayerCountEmbed = CreateEmbed(
                        title: "Start Server",
                        description: "An error occurred while getting the player count.",
                        ResponseType.Error
                        );
                    await command.FollowupAsync(embed: nullPlayerCountEmbed.Build());
                    return;
                }

                // Change the embed description based on the player count
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

                var serverAlreadyOnEmbed = CreateEmbed(
                    title: "Start Server",
                    description: description,
                    ResponseType.Information
                    );
                await command.FollowupAsync(embed: serverAlreadyOnEmbed.Build());
            }

            catch (Exception ex)
            {
                _logger.LogCritical(ex.StackTrace);
            }

        }
    }
}

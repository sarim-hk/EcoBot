using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EcoBot
{
    public partial class EcoBot
    {
        private static async Task HandleStartServerCommand(SocketSlashCommand command)
        {
            try {
                EmbedBuilder startServerEmbed;

                // Let discord know that you've actually received the command and that you're gonna start processing stuff
                await command.DeferAsync();

                // Get the server status (-1 = booting,  0 = off, 1 = on)
                int? serverStatus = await _datHostClient.GetServerStatus();
                if (serverStatus == null) {
                    startServerEmbed = CreateEmbed(
                        title: "Start Server",
                        description: "An error occurred while getting the current server status.",
                        ResponseType.Error
                        );
                    await command.FollowupAsync(embed: startServerEmbed.Build());
                    return;
                }

                // If the server is off, turn it on. If it's already booting, inform the user. If it's on, proceed to next code block.
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
                    startServerEmbed = CreateEmbed(
                        title: "Start Server",
                        description: "An error occurred while getting the server RCON details.",
                        ResponseType.Error
                        );
                    await command.FollowupAsync(embed: startServerEmbed.Build());
                    return;
                }

                // Get the player count
                var rconClient = new CS2RconClient(rconDetails, _logger);
                int? playerCount = await rconClient.GetPlayerCount();
                if (!playerCount.HasValue) {
                    startServerEmbed = CreateEmbed(
                        title: "Start Server",
                        description: "An error occurred while getting the player count.",
                        ResponseType.Error
                        );
                    await command.FollowupAsync(embed: startServerEmbed.Build());
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

                startServerEmbed = CreateEmbed(
                    title: "Start Server",
                    description: description,
                    ResponseType.Information
                    );
                await command.FollowupAsync(embed: startServerEmbed.Build());
            }

            catch (Exception ex)
            {
                _logger.LogCritical(ex.StackTrace);
            }

        }

        private static async Task HandleStopServerCommand(SocketSlashCommand command) {
            try {
                EmbedBuilder stopServerEmbed;

                // Let discord know that you've actually received the command and that you're gonna start processing stuff
                await command.DeferAsync();

                // Get the server status (-1 = booting,  0 = off, 1 = on)
                int? serverStatus = await _datHostClient.GetServerStatus();
                if (serverStatus == null) {
                    stopServerEmbed = CreateEmbed(
                        title: "Stop Server",
                        description: "An error occurred while getting the current server status.",
                        ResponseType.Error
                        );
                    await command.FollowupAsync(embed: stopServerEmbed.Build());
                    return;
                }

                // If the server is off, inform the user it's already off. If it's booting, inform the user. If it's on, proceed to next code block.
                switch (serverStatus) {
                    case 0:
                        stopServerEmbed = CreateEmbed(
                            title: "Stop Server",
                            description: "The server is already off.",
                            ResponseType.Information
                            );
                        await command.FollowupAsync(embed: stopServerEmbed.Build());
                        return;

                    case -1:
                        stopServerEmbed = CreateEmbed(
                            title: "Stop Server",
                            description: "The server is currently booting.",
                            ResponseType.Information
                            );
                        await command.FollowupAsync(embed: stopServerEmbed.Build());
                        return;
                }

                // Otherwise, lets get the rcon details which we'll use to check if there are people in the server or if its empty
                RconDetails? rconDetails = await _datHostClient.GetRconDetails();
                if (rconDetails == null) {
                    stopServerEmbed = CreateEmbed(
                        title: "Stop Server",
                        description: "An error occurred while getting the server RCON details.",
                        ResponseType.Error
                        );
                    await command.FollowupAsync(embed: stopServerEmbed.Build());
                    return;
                }

                // Get the player count
                var rconClient = new CS2RconClient(rconDetails, _logger);
                int? playerCount = await rconClient.GetPlayerCount();
                if (!playerCount.HasValue) {
                    stopServerEmbed = CreateEmbed(
                        title: "Stop Server",
                        description: "An error occurred while getting the player count.",
                        ResponseType.Error
                        );
                    await command.FollowupAsync(embed: stopServerEmbed.Build());
                    return;
                }

                // Change the embed description and response type based on the player count
                string description;
                ResponseType responseType;
                switch (playerCount) {
                    case 0:
                        bool stopServerResponse = await _datHostClient.StopServer();
                        if (stopServerResponse == true) {
                            description = "The server is being stopped.";
                            responseType = ResponseType.Success;
                        }
                        else {
                            description = "An error occurred while stopping the server.";
                            responseType = ResponseType.Error;
                        }
                        break;
                    case 1:
                        description = $"The server can't be stopped, 1 player is online.";
                        responseType = ResponseType.Information;
                        break;
                    default:
                        description = $"The server can't be stopped, there are {playerCount} players online.";
                        responseType = ResponseType.Information;
                        break;
                }

                stopServerEmbed = CreateEmbed(
                    title: "Stop Server",
                    description: description,
                    responseType
                    );
                await command.FollowupAsync(embed: stopServerEmbed.Build());
            }

            catch (Exception ex) {
                _logger.LogCritical(ex.StackTrace);
            }

        }
    }
}

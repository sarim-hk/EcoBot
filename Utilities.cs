using Discord;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EcoBot
{

    enum ResponseType
    {
        Information,
        Success,
        Error
    }

    class RconDetails
    {
        public string RawIP;
        public int Port;
        public string RconPassword;

        public RconDetails(string rawIP, int port, string rconPassword)
        {
            RawIP = rawIP;
            Port = port;
            RconPassword = rconPassword;
        }
    }

    public partial class EcoBot
    {
        /// <summary> Validates the configuration values from the application's JSON config files. </summary>
        /// <returns> <c>true</c> if all configuration values are present and valid; otherwise, <c>false</c>. </returns>
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

            string? dathostServerID = config["Dathost:ServerID"];
            if (string.IsNullOrEmpty(dathostServerID))
            {
                Logger.LogCritical("No Dathost Server ID in config.");
                return false;
            }

            string? dathostEmail = config["Dathost:EmailAddress"];
            if (string.IsNullOrEmpty(dathostEmail))
            {
                Logger.LogCritical("No Dathost Email Address in config.");
                return false;
            }

            string? dathostPassword = config["Dathost:Password"];
            if (string.IsNullOrEmpty(dathostPassword))
            {
                Logger.LogCritical("No Dathost Password in config.");
                return false;
            }

            Logger.LogInformation("Config validated successfully.");
            return true;
        }

        private static EmbedBuilder CreateEmbed(string title, string description, ResponseType responseType)
        {

            Color colour;
            switch(responseType)
            {
                case ResponseType.Information:
                    colour = Color.Blue;
                    break;
                case ResponseType.Success:
                    colour = Color.Green;
                    break;
                case ResponseType.Error:
                    colour = Color.Red;
                    break;
                default:
                    colour = Color.DarkerGrey;
                    break;
            }

            var embed = new EmbedBuilder
            {
                Title = title,
                Description = description,
                Color = colour,
            };

            return embed;
        }

    }

}

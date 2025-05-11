using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EcoBot
{
    public enum LogType
    {
        Critical,
        Error,
        Information,
        Debug
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

            LogAsync("Config validated successfully.", LogType.Information);
            return true;
        }

        /// <summary> Async wrapper around logger. </summary>
        /// <param name="logType"> Enum that maps to log types: Critical, Error, Information, Debug. </param>
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


    }

}

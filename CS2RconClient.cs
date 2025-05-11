using Microsoft.Extensions.Logging;
using RconSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EcoBot
{
    class CS2RconClient
    {
        private RconDetails _rconDetails;
        private ILogger Logger;

        public CS2RconClient(RconDetails rconDetails, ILogger logger)
        {
            _rconDetails = rconDetails;
            Logger = logger;
        }

        public async Task<int?> GetPlayerCount()
        {
            var rconClient = RconClient.Create(_rconDetails.RawIP, _rconDetails.Port);
            await rconClient.ConnectAsync();

            if (await rconClient.AuthenticateAsync(_rconDetails.RconPassword))
            {
                var status = await rconClient.ExecuteCommandAsync("status");
                var match = Regex.Match(status, @"players\s*:\s*(\d+)\s+humans");

                if (match.Success)
                {
                    int humanCount = int.Parse(match.Groups[1].Value);
                    return humanCount;
                }
                else
                {
                    Logger.LogError("Could not find the player count in status output.");
                    return null;
                }
            }
            else
            {
                Logger.LogError("Server did not like our rcon auth.");
                return null;
            }

        }

    }
}

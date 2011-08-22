using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using Terraria_Server;
using System.Threading;
using Terraria_Server.Collections;
using Terraria_Server.Commands;
using Terraria_Server.Misc;
using Terraria_Server.Logging;
using Terraria_Server.RemoteConsole;
using Terraria_Server.WorldMod;
using Terraria_Server.Definitions;
using Terraria_Server.Plugin;

namespace House.Commands
{
    class Commands
    {
        public static void house(Server server, ISender sender, ArgumentList args)
        {
            String cmd;
            if (args.TryGetString(0, out cmd))
            {
                switch (cmd.ToUpper())
                {
                    case "START":
                        House.plugin.CreateHouseNodeForPlayer(sender.Name);
                        server.GetPlayerByName(sender.Name).PluginData["starthouse"] = true;
                        break;
                    case "END":
                        server.GetPlayerByName(sender.Name).PluginData["endhouse"] = true;
                        break;
                    default:
                        server.GetPlayerByName(sender.Name).PluginData["starthouse"] = true;
                        break;
                }
            }
        }
    }
}

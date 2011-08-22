using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using Terraria_Server;
using Terraria_Server.Plugin;
using Terraria_Server.Collections;
using Terraria_Server.Commands;
using Terraria_Server.Events;
using Terraria_Server.Logging;

namespace House
{
    public class House : Plugin
    {
        public static House plugin;

        public override void Load()
        {
            Name = "House";
            Description = "A plugin to allow players to define a safe area";
            Author = "amarriner";
            Version = "0.1";
            TDSMBuild = 31;

            plugin = this;
        }

        public override void Enable()
        {
            Program.tConsole.WriteLine(base.Name + " enabled.");
        }

        public override void Disable()
        {
            Program.tConsole.WriteLine(base.Name + " disabled.");
        }
    }
}

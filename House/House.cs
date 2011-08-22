using System;
using System.Collections;
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

using System.Xml;

namespace House
{
    public class House : Plugin
    {
        public static House plugin;
        public Properties properties;
        public int maxArea;

        public String xmlFilename = "house.xml";
        public String xmlNamespace = "housePlugin";

        public XmlDocument houseXML;

        public override void Load()
        {
            Name = "House";
            Description = "A plugin to allow players to define a safe area";
            Author = "amarriner";
            Version = "0.1";
            TDSMBuild = 31;

            plugin = this;

            this.registerHook(Hooks.PLAYER_TILECHANGE);

            string pluginFolder = Statics.PluginPath + Path.DirectorySeparatorChar + "House";
            CreateDirectory(pluginFolder);

            properties = new Properties(pluginFolder + Path.DirectorySeparatorChar + "house.properties");
            properties.Load();
            properties.Save();
            maxArea = properties.MaxArea;

            AddCommand("house")
                .WithAccessLevel(AccessLevel.PLAYER)
                .WithDescription("House Commands")
                .WithHelpText("/house")
                .Calls(Commands.Commands.house);

            xmlFilename = pluginFolder + Path.DirectorySeparatorChar + xmlFilename;
            houseXML = new XmlDocument();
//            try
//            {
                houseXML.Load(xmlFilename);
                //XmlNodeList nodeList = houseXML.GetElementsByTagName("houses");
                //nodeList[0].AppendChild(houseXML.CreateNode(System.Xml.XmlNodeType.Element, "house", "housePlugin"));
                //houseXML.Save(pluginFolder + Path.DirectorySeparatorChar + xmlFilename);
//            }
//            catch
//            {
//                Program.tConsole.WriteLine("Missing " + xmlFilename + " file in " + pluginFolder + "! Disabling Plugin!");
//                Disable();
//            }
        }

        public override void Enable()
        {
            Program.tConsole.WriteLine(base.Name + " enabled.");
        }

        public override void Disable()
        {
            Program.tConsole.WriteLine(base.Name + " disabled.");
        }

        private static void CreateDirectory(string dirPath)
        {
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
        }

        public override void onPlayerTileChange(PlayerTileChangeEvent Event)
        {
            Player player = Server.GetPlayerByName(Event.Sender.Name);
            bool starthouse = player.PluginData.ContainsKey("starthouse") ? (bool)player.PluginData["starthouse"] : false;
            bool endhouse = player.PluginData.ContainsKey("endhouse") ? (bool)player.PluginData["endhouse"] : false;
            if (starthouse)
            {
                Event.Cancelled = true;
                Program.tConsole.WriteLine("Starting house at " + player.Position.X + "," + player.Position.Y);
                player.PluginData["starthouse"] = false;
                UpdateCoordsForPlayer(player.Name, "topleft", player.Position.X.ToString() + "," + player.Position.Y.ToString());
            }

            else if (endhouse)
            {
                Event.Cancelled = true;
                Program.tConsole.WriteLine("Ending house at " + player.Position.X + "," + player.Position.Y);
                player.PluginData["endhouse"] = false;
                UpdateNodeForPlayer(player.Name, "bottomright", player.Position.X.ToString() + "," + player.Position.Y.ToString());
            }
            base.onPlayerTileChange(Event);
        }

        public XmlNode GetPlayerHouseNode(string PlayerName)
        {
            XmlNode current;
            XmlNodeList houses = houseXML.GetElementsByTagName("house");
            IEnumerator ienum = houses.GetEnumerator();
            while (ienum.MoveNext())
            {
                current = (XmlNode)ienum.Current;
                if (current.Attributes["id"].Value == PlayerName)
                {
                    return current;
                }
            }
            return null;
        }

        public XmlNode CreateHouseNodeForPlayer(string PlayerName)
        {
            if (GetPlayerHouseNode(PlayerName) == null)
            {
                XmlNodeList houses = houseXML.GetElementsByTagName("houses");
                XmlNode newNode = houseXML.CreateNode(XmlNodeType.Element, "house", xmlNamespace);
                XmlAttribute newAttribute = House.plugin.houseXML.CreateAttribute("id");
                newAttribute.Value = PlayerName;
                newNode.Attributes.Append(newAttribute);
                houses[0].AppendChild(newNode);
                houseXML.Save(House.plugin.xmlFilename);
                return newNode;
            }

            return null;
        }

        public void UpdateNodeForPlayer(string PlayerName, string NodeName, string Value)
        {
            bool foundNode = false;
            XmlNode houseNode = GetPlayerHouseNode(PlayerName);
            if (houseNode == null)
            {
                houseNode = CreateHouseNodeForPlayer(PlayerName);
            }

            XmlNode current;
            IEnumerator ienum = houseNode.ChildNodes.GetEnumerator();
            while (ienum.MoveNext())
            { 
                current = (XmlNode)ienum.Current;
                if (current.Name == NodeName)
                {
                    foundNode = true;
                    current.Value = Value;
                }
            }

            if (!foundNode)
            {
                XmlNode newNode = houseXML.CreateNode(XmlNodeType.Element, NodeName, xmlNamespace);
                newNode.InnerXml = Value;
                houseNode.AppendChild(newNode);
            }

            houseXML.Save(xmlFilename);
        }
    }
}

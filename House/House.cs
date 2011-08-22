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

        private Terraria_Server.Misc.Color chatColor = new Terraria_Server.Misc.Color(100, 200, 100);

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
            this.registerHook(Hooks.PLAYER_CHEST);

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
            houseXML.Load(xmlFilename);
        }

        public override void Enable()
        {
            Program.tConsole.WriteLine(base.Name + " enabled.");
        }

        public override void Disable()
        {
            Program.tConsole.WriteLine(base.Name + " disabled.");
            houseXML.Save(xmlFilename);
        }

        private static void CreateDirectory(string dirPath)
        {
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
        }

        public override void onPlayerOpenChest(PlayerChestOpenEvent Event)
        {
            Player player = Server.GetPlayerByName(Event.Sender.Name);
            if (IsInsideAnotherHouse(player.Name, (int)player.Position.X / 16, (int)player.Position.Y / 16))
            {
                Event.Cancelled = true;
                player.sendMessage("You cannot open this chest, it's inside someone else's house", chatColor);
            }
            base.onPlayerOpenChest(Event);
        }

        public override void onPlayerTileChange(PlayerTileChangeEvent Event)
        {
            Player player = Server.GetPlayerByName(Event.Sender.Name);
            bool starthouse = player.PluginData.ContainsKey("starthouse") ? (bool)player.PluginData["starthouse"] : false;
            bool endhouse = player.PluginData.ContainsKey("endhouse") ? (bool)player.PluginData["endhouse"] : false;
            if (starthouse || endhouse)
            {
                String NodeName = starthouse ? "topleft" : "bottomright";
                String cornerDesc = starthouse ? "top-left" : "bottom-right";
                Event.Cancelled = true;

                if (!IsInsideAnotherHouse(player.Name, (int)Event.Position.X, (int)Event.Position.Y))
                {
                    UpdateCoordsForPlayer(player.Name, NodeName, (int)Event.Position.X, (int)Event.Position.Y);
                    player.sendMessage("You've set the " + cornerDesc + " corner of your house", chatColor);
                    player.PluginData["starthouse"] = false;
                    player.PluginData["endhouse"] = false;
                }
                else
                {
                    player.sendMessage("You're inside another player's house, you cannot set your " + cornerDesc + " here");
                }
            } 

            if (IsInsideAnotherHouse(player.Name, (int)Event.Position.X, (int)Event.Position.Y) && ! starthouse && ! endhouse)
            {
                Event.Cancelled = true;
                player.sendMessage("You're trying to build inside someone's house--this is not allowed", chatColor);
            }

            base.onPlayerTileChange(Event);
        }

        public bool IsInsideAnotherHouse(string PlayerName, int x, int y)
        {
            XmlNode current;
            XmlNodeList houses = houseXML.GetElementsByTagName("house");
            IEnumerator ienum = houses.GetEnumerator();
            while (ienum.MoveNext())
            {
                current = (XmlNode)ienum.Current;
                if (current.Attributes["id"].Value != PlayerName)
                {
                    if (x >= Int32.Parse(current["topleft"]["x"].InnerXml) && x <= Int32.Parse(current["bottomright"]["x"].InnerXml) &&
                        y >= Int32.Parse(current["topleft"]["y"].InnerXml) && y <= Int32.Parse(current["bottomright"]["y"].InnerXml))
                    {
                        return true;
                    }
                }
            }

            return false;
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

        public void UpdateCoordsForPlayer(string PlayerName, string NodeName, int x, int y)
        {
            bool foundNode = false, foundX = false, foundY = false;
            XmlNode houseNode = GetPlayerHouseNode(PlayerName);
            XmlNode xNode, yNode, current, inner;

            if (houseNode == null)
            {
                houseNode = CreateHouseNodeForPlayer(PlayerName);
            }

            xNode = houseXML.CreateNode(XmlNodeType.Element, "x", xmlNamespace);
            xNode.InnerXml = x.ToString();
            yNode = houseXML.CreateNode(XmlNodeType.Element, "y", xmlNamespace);
            yNode.InnerXml = y.ToString();

            IEnumerator ienum = houseNode.ChildNodes.GetEnumerator();
            while (ienum.MoveNext())
            {
                current = (XmlNode)ienum.Current;

                if (current.Name == NodeName)
                {
                    foundNode = true;

                    IEnumerator innerIenum = current.ChildNodes.GetEnumerator();
                    while (innerIenum.MoveNext())
                    {
                        inner = (XmlNode)innerIenum.Current;
                        if (inner.Name == "x")
                        {
                            foundX = true;
                        }

                        if (inner.Name == "y")
                        {
                            foundY = true;
                        }
                    }
                }
            }

            if (!foundNode)
            {
                current = houseXML.CreateNode(XmlNodeType.Element, NodeName, xmlNamespace);
                current.AppendChild(xNode);
                current.AppendChild(yNode);
                houseNode.AppendChild(current);
            }

            else
            {
                if (!foundX)
                {
                    houseNode[NodeName].AppendChild(xNode);
                }
                else
                {
                    houseNode[NodeName]["x"].InnerXml = x.ToString();
                }

                if (!foundY)
                {
                    houseNode[NodeName].AppendChild(yNode);
                }
                else
                {
                    houseNode[NodeName]["y"].InnerXml = y.ToString();
                }
            }
        }
    }
}

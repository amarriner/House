using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Drawing;

using Terraria_Server;
using Terraria_Server.Plugin;
using Terraria_Server.Collections;
using Terraria_Server.Commands;
using Terraria_Server.Events;
using Terraria_Server.Logging;

using System.Xml;

namespace House
{
    public struct PlayerHouseCoords
    {
        public Point TopLeft, BottomRight;
    }

    public struct PlayerHouses
    {
        public string PlayerName;
        public List<PlayerHouseCoords> Houses;
        public bool LockChests;
        public bool LockSigns;
        public bool LockDoors;

        public PlayerHouses(string PlayerName)
        {
            this.PlayerName = PlayerName;
            this.LockChests = false;
            this.LockSigns = false;
            this.LockDoors = false;
            this.Houses = new List<PlayerHouseCoords>();
        }
    }

    public class House : Plugin
    {
        public static House plugin;
        public Properties properties;
        public int maxArea;

        public String xmlFilename = "house.xml";
        public String xmlNamespace = "housePlugin";

        private Terraria_Server.Misc.Color chatColor = new Terraria_Server.Misc.Color(100, 200, 100);

        public XmlDocument houseXML = new XmlDocument();
        public List<PlayerHouses> playerHouses = new List<PlayerHouses>();
        public string pluginFolder;

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

            AddCommand("house")
                .WithAccessLevel(AccessLevel.PLAYER)
                .WithDescription("House Commands")
                .WithHelpText("/house")
                .Calls(Commands.Commands.house);
        }

        public override void Enable()
        {
            pluginFolder = Statics.PluginPath + Path.DirectorySeparatorChar + "House";
            CreateDirectory(pluginFolder);

            properties = new Properties(pluginFolder + Path.DirectorySeparatorChar + "house.properties");
            properties.Load();
            properties.Save();
            maxArea = properties.MaxArea;

            LoadHouseData();

            Program.tConsole.WriteLine(base.Name + " enabled.");
        }

        public override void Disable()
        {
            SaveHouseData();
            Program.tConsole.WriteLine(base.Name + " disabled.");
        }

        private void LoadHouseData()
        {
            XmlNode playerNode, houseNode, dataNode;
            XmlNodeList playerNodes;
            IEnumerator playerEnum, houseEnum, dataEnum;
            PlayerHouses playerHouse;
            PlayerHouseCoords playerHouseCoords;

            xmlFilename = pluginFolder + Path.DirectorySeparatorChar + xmlFilename;
            houseXML.Load(xmlFilename);

            playerNodes = houseXML.GetElementsByTagName("player");
            playerEnum = playerNodes.GetEnumerator();
            while (playerEnum.MoveNext())
            {
                // Set player information
                playerNode = (XmlNode)playerEnum.Current;
                playerHouse = new PlayerHouses();
                playerHouse.PlayerName = playerNode.Attributes["id"].Value;
                playerHouse.Houses = new List<PlayerHouseCoords>();

                // Loop through player data
                dataEnum = playerNode.ChildNodes.GetEnumerator();
                while (dataEnum.MoveNext())
                {
                    // Set House Information
                    dataNode = (XmlNode)dataEnum.Current;

                    switch (dataNode.Name.ToUpper())
                    {
                        case "HOUSES":
                            playerHouseCoords = new PlayerHouseCoords();

                            // Loop through house data
                            houseEnum = dataNode.ChildNodes.GetEnumerator();
                            while (houseEnum.MoveNext())
                            {
                                houseNode = (XmlNode)houseEnum.Current;
                                playerHouseCoords.TopLeft.X = Int32.Parse(houseNode["topleft"]["x"].InnerXml);
                                playerHouseCoords.TopLeft.Y = Int32.Parse(houseNode["topleft"]["y"].InnerXml);
                                playerHouseCoords.BottomRight.X = Int32.Parse(houseNode["bottomright"]["x"].InnerXml);
                                playerHouseCoords.BottomRight.Y = Int32.Parse(houseNode["bottomright"]["y"].InnerXml);
                            }

                            playerHouse.Houses.Add(playerHouseCoords);
                            break;
                        case "LOCKCHESTS":
                            playerHouse.LockChests = Boolean.Parse(dataNode.InnerXml);
                            break;
                        case "LOCKDOORS":
                            playerHouse.LockDoors = Boolean.Parse(dataNode.InnerXml);
                            break;
                        case "LOCKSIGNS":
                            playerHouse.LockSigns = Boolean.Parse(dataNode.InnerXml);
                            break;
                    }
                }

                playerHouses.Add(playerHouse);
            }
        }

        public void SaveHouseData()
        {
            houseXML = new XmlDocument();
            XmlNode dec = houseXML.CreateNode(XmlNodeType.XmlDeclaration, "xml", xmlNamespace);
            houseXML.AppendChild(dec);
            XmlNode playersNode = houseXML.CreateNode(XmlNodeType.Element, "players", xmlNamespace);
            XmlNode playerNode, houses, node, coord, x, y;
            XmlAttribute playerAttribute;

            foreach (PlayerHouses playerHouse in playerHouses)
            {
                playerAttribute = houseXML.CreateAttribute("id");
                playerAttribute.Value = playerHouse.PlayerName;
                playerNode = houseXML.CreateNode(XmlNodeType.Element, "player", xmlNamespace);
                playerNode.Attributes.Append(playerAttribute);

                node = houseXML.CreateNode(XmlNodeType.Element, "lockchests", xmlNamespace);
                node.InnerXml = playerHouse.LockChests.ToString();
                playerNode.AppendChild(node);
                node = houseXML.CreateNode(XmlNodeType.Element, "lockcdoors", xmlNamespace);
                node.InnerXml = playerHouse.LockDoors.ToString();
                playerNode.AppendChild(node);
                node = houseXML.CreateNode(XmlNodeType.Element, "locksigns", xmlNamespace);
                node.InnerXml = playerHouse.LockSigns.ToString();
                playerNode.AppendChild(node);

                houses = houseXML.CreateNode(XmlNodeType.Element, "houses", xmlNamespace);
                foreach (PlayerHouseCoords coords in playerHouse.Houses)
                {
                    node = houseXML.CreateNode(XmlNodeType.Element, "house", xmlNamespace);

                    coord = houseXML.CreateNode(XmlNodeType.Element, "topleft", xmlNamespace);
                    x = houseXML.CreateNode(XmlNodeType.Element, "x", xmlNamespace);
                    x.InnerXml = coords.TopLeft.X.ToString();
                    y = houseXML.CreateNode(XmlNodeType.Element, "y", xmlNamespace);
                    y.InnerXml = coords.TopLeft.Y.ToString();
                    coord.AppendChild(x);
                    coord.AppendChild(y);
                    node.AppendChild(coord);

                    coord = houseXML.CreateNode(XmlNodeType.Element, "bottomright", xmlNamespace);
                    x = houseXML.CreateNode(XmlNodeType.Element, "x", xmlNamespace);
                    x.InnerXml = coords.BottomRight.X.ToString();
                    y = houseXML.CreateNode(XmlNodeType.Element, "y", xmlNamespace);
                    y.InnerXml = coords.BottomRight.Y.ToString();
                    coord.AppendChild(x);
                    coord.AppendChild(y);
                    node.AppendChild(coord);

                    houses.AppendChild(node);
                }
                playerNode.AppendChild(houses);
                playersNode.AppendChild(playerNode);
            }

            houseXML.AppendChild(playersNode);
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
                    UpdateCoordsForPlayer(player.Name, (int)Event.Position.X, (int)Event.Position.Y);
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
            foreach (PlayerHouses playerHouse in playerHouses)
            {
                if (playerHouse.PlayerName != PlayerName)
                {
                    foreach (PlayerHouseCoords houseCoords in playerHouse.Houses)
                    {
                        if (x >= houseCoords.TopLeft.X && x <= houseCoords.BottomRight.X &&
                            y >= houseCoords.TopLeft.Y && y <= houseCoords.BottomRight.Y)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public bool CreatePlayerHouse(string PlayerName, int houseIndex = 0)
        {
            foreach (PlayerHouses i in playerHouses)
            {
                if (i.PlayerName == PlayerName)
                {
                    return false;
                }
            }

            PlayerHouses playerHouse = new PlayerHouses(PlayerName);
            playerHouses.Add(playerHouse);
            return true;
        }

        public int GetPlayerHouseIndex(string PlayerName)
        {
            for (int i = 0; i < playerHouses.Count; i++)
            {
                if (playerHouses[i].PlayerName == PlayerName)
                {
                    return i;
                }
            }

            return -1;
        }

        public void UpdateCoordsForPlayer(string PlayerName, int x, int y, int houseIndex = 0)
        {
            Player player = Server.GetPlayerByName(PlayerName);
            bool starthouse = player.PluginData.ContainsKey("starthouse") ? (bool)player.PluginData["starthouse"] : false;
            bool endhouse = player.PluginData.ContainsKey("endhouse") ? (bool)player.PluginData["endhouse"] : false;

            CreatePlayerHouse(PlayerName);
            int playerIndex = GetPlayerHouseIndex(PlayerName);

            PlayerHouseCoords tempCoords;

            if (playerHouses[playerIndex].Houses.Count == houseIndex)
            {
                tempCoords = new PlayerHouseCoords();
                playerHouses[playerIndex].Houses.Add(tempCoords);
            }
            else
            {
                tempCoords = playerHouses[playerIndex].Houses[houseIndex];
            }

            if (starthouse)
            {
                tempCoords.TopLeft.X = x;
                tempCoords.TopLeft.Y = y;
                playerHouses[playerIndex].Houses[houseIndex] = tempCoords;
            }
            else if (endhouse)
            {
                tempCoords.BottomRight.X = x;
                tempCoords.BottomRight.Y = y;
                playerHouses[playerIndex].Houses[houseIndex] = tempCoords;
            }
        }
    }
}

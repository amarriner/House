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
        public string HouseName;
        public Point TopLeft, BottomRight;
        public List<string> Allowed;

        public PlayerHouseCoords(string HouseName = null)
        {
            this.HouseName = HouseName;
            this.TopLeft = new Point();
            this.BottomRight = new Point();
            this.Allowed = new List<string>();
        }
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
        public static int CHECK_CHEST_LOCK = 1;
        public static int CHECK_DOOR_LOCK = 2;
        public static int CHECK_SIGN_LOCK = 3;
        public static int CHECK_ALLOWED = 4;

        public static House plugin;
        public Properties properties;
        public int maxArea;
        public int minHeight;
        public int maxHeight;
        public int maxHouses;
        public bool playersCanTeleport;
        public bool playersCanMakeHouses;

        public String xmlFilename = "house.xml";
        public String xmlNamespace = "housePlugin";

        public Terraria_Server.Misc.Color chatColor = new Terraria_Server.Misc.Color(100, 200, 100);

        public XmlDocument houseXML = new XmlDocument();
        public List<PlayerHouses> playerHouses = new List<PlayerHouses>();
        public string pluginFolder;

        public double lastTime;

        public override void Load()
        {
            Name = "House";
            Description = "A plugin to allow players to define a safe area";
            Author = "amarriner";
            Version = "0.3.3";
            TDSMBuild = 31;

            plugin = this;

            this.registerHook(Hooks.PLAYER_TILECHANGE);
            this.registerHook(Hooks.PLAYER_CHEST);
            this.registerHook(Hooks.PLAYER_EDITSIGN);
            this.registerHook(Hooks.DOOR_STATECHANGE);
            this.registerHook(Hooks.TIME_CHANGED);

            AddCommand("h")
                .WithAccessLevel(AccessLevel.PLAYER)
                .WithDescription("House Commands, type /house ? for help")
                .WithHelpText("/house <command> <parameter> <parameter>")
                .Calls(Commands.Commands.house);

            AddCommand("house")
                .WithAccessLevel(AccessLevel.PLAYER)
                .WithDescription("House Commands, type /house ? for help")
                .WithHelpText("/house <command> <parameter> <parameter>")
                .Calls(Commands.Commands.house);
        }

        public override void Enable()
        {
            lastTime = Server.time;

            pluginFolder = Statics.PluginPath + Path.DirectorySeparatorChar + "House";
            CreateDirectory(pluginFolder);

            properties = new Properties(pluginFolder + Path.DirectorySeparatorChar + "house.properties");
            properties.Load();
            properties.Save();
            maxArea = properties.MaxArea;
            minHeight = properties.MinHeight;
            maxHeight = properties.MaxHeight;
            maxHouses = properties.MaxHouses;
            playersCanTeleport = properties.PlayersCanTeleport;
            playersCanMakeHouses = properties.PlayersCanMakeHouses;

            LoadHouseData();

            Program.tConsole.WriteLine(base.Name + " enabled.");
        }

        public override void Disable()
        {
            SaveHouseData();
            Program.tConsole.WriteLine(base.Name + " disabled.");
        }

        public override void onDoorStateChange(DoorStateChangeEvent Event)
        {
            /*
            Player player = Server.GetPlayerByName(Event.Sender.Name);
            if (IsInsideAnotherHouse(player.Name, (int)Event.X, (int)Event.Y, CHECK_DOOR_LOCK) &&
                !player.isInOpList())
            {
                Event.Cancelled = true;
                player.sendMessage("You cannot open or close this door, it's locked and inside someone else's house", chatColor);
            }
            base.onDoorStateChange(Event);
             */
        }

        public override void onPlayerEditSign(PlayerEditSignEvent Event)
        {
            Player player = Server.GetPlayerByName(Event.Sender.Name);
            if (IsInsideAnotherHouse(player.Name, (int)Event.Sign.x, (int)Event.Sign.y, CHECK_SIGN_LOCK) &&
                !player.Op)
            {
                Event.Cancelled = true;
                player.sendMessage("You cannot edit this sign, it's locked and inside someone else's house", chatColor);
            }
            base.onPlayerEditSign(Event);
        }

        public override void onPlayerOpenChest(PlayerChestOpenEvent Event)
        {
            Player player = Server.GetPlayerByName(Event.Sender.Name);
            if (IsInsideAnotherHouse(player.Name, (int)Server.chest[Event.ID].x, (int)Server.chest[Event.ID].y, CHECK_CHEST_LOCK) &&
                !player.Op)
            {
                Event.Cancelled = true;
                player.sendMessage("You cannot open this chest, it's locked and inside someone else's house", chatColor);
            }
            base.onPlayerOpenChest(Event);
        }

        public override void onPlayerTileChange(PlayerTileChangeEvent Event)
        {
            Player player = Server.GetPlayerByName(Event.Sender.Name);
            bool starthouse = player.PluginData.ContainsKey("starthouse") ? (bool)player.PluginData["starthouse"] : false;
            bool endhouse = player.PluginData.ContainsKey("endhouse") ? (bool)player.PluginData["endhouse"] : false;
            bool check = player.PluginData.ContainsKey("check") ? (bool)player.PluginData["check"] : false;
            int houseIndex = player.PluginData.ContainsKey("houseIndex") ? (int)player.PluginData["houseIndex"] : -1;
            string houseName = player.PluginData.ContainsKey("houseName") ? (string)player.PluginData["houseName"] : null;

            if (starthouse || endhouse)
            {
                String NodeName = starthouse ? "topleft" : "bottomright";
                String cornerDesc = starthouse ? "top-left" : "bottom-right";
                Event.Cancelled = true;

                if (GetHouseNameImInside(player) == null)
                {
                    UpdateCoordsForPlayer(player.Name, (int)Event.Position.X, (int)Event.Position.Y, houseIndex);
                    player.sendMessage("You've set the " + cornerDesc + " corner of house " + houseName, chatColor);
                    player.PluginData["starthouse"] = false;
                    player.PluginData["endhouse"] = false;
                }
                else
                {
                    player.sendMessage("You're inside another house, you cannot set your " + cornerDesc + " here");
                }
            } 

            else if (IsInsideAnotherHouse(player.Name, (int)Event.Position.X, (int)Event.Position.Y) && ! starthouse && ! endhouse &&
                !player.Op)
            {
                Event.Cancelled = true;
                player.sendMessage("You're trying to build inside someone's house--this is not allowed", chatColor);
            }

            else if (check)
            {
                Event.Cancelled = true;
                player.PluginData["check"] = false;
                player.sendMessage("The block you just clicked on is at " + (int)Event.Position.X + "," + (int)Event.Position.Y, chatColor);
            }

            base.onPlayerTileChange(Event);
        }

        public string GetMyHouseNameImInside(string PlayerName)
        {
                Player player = Server.GetPlayerByName(PlayerName);
                int playerHouseIndex = GetPlayerHouseIndex(PlayerName);

                if (playerHouseIndex < 0)
                    return null;

                foreach (PlayerHouseCoords playerHouseCoord in playerHouses[playerHouseIndex].Houses)
                {
                    if (player.Position.X / 16 >= playerHouseCoord.TopLeft.X && player.Position.X / 16 <= playerHouseCoord.BottomRight.X &&
                        player.Position.Y / 16 >= playerHouseCoord.TopLeft.Y && player.Position.Y / 16 <= playerHouseCoord.BottomRight.Y)
                        return playerHouseCoord.HouseName;
                }

            return null;
        }

        public string GetHouseNameImInside(Player player)
        {
            foreach (PlayerHouses playerHouse in playerHouses)
            {
                foreach (PlayerHouseCoords playerHouseCoord in playerHouse.Houses)
                {
                    if (player.Position.X / 16 >= playerHouseCoord.TopLeft.X && player.Position.X / 16 <= playerHouseCoord.BottomRight.X &&
                        player.Position.Y / 16 >= playerHouseCoord.TopLeft.Y && player.Position.Y / 16 <= playerHouseCoord.BottomRight.Y)
                        return playerHouse.PlayerName + "'s house " + playerHouseCoord.HouseName;
                }
            }

            return null;
        }

        public bool IsInsideAnotherHouse(string PlayerName, int x, int y, int check = 0)
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
                            for (int i = 0; i < houseCoords.Allowed.Count; i++)
                            {
                                if (houseCoords.Allowed[i] == PlayerName)
                                {
                                    return false;
                                }
                            }

                            if (check == House.CHECK_CHEST_LOCK && !playerHouse.LockChests)
                                return false;

                            if (check == House.CHECK_DOOR_LOCK && !playerHouse.LockDoors)
                                return false;

                            if (check == House.CHECK_SIGN_LOCK && !playerHouse.LockSigns)
                                return false;

                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public override void onTimeChange(TimeChangedEvent Event)
        {
            if (Math.Abs(Event.GetTime - lastTime) > 10000)
            {
                Program.tConsole.WriteLine("Saving house.xml");
                SaveHouseData();
                lastTime = Event.GetTime;
            }
 	        base.onTimeChange(Event);
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

        public int GetTotalPlayerHouses(string PlayerName)
        {
            int index = GetPlayerHouseIndex(PlayerName);
            if (index >= 0)
                return playerHouses[GetPlayerHouseIndex(PlayerName)].Houses.ToArray().Length;
            else
                return -1;
        }

        public int GetHouseCoordsIndexByName(string PlayerName, string HouseName)
        {
            int playerHouseIndex = GetPlayerHouseIndex(PlayerName);
            if (playerHouseIndex < 0)
                return -1;

            for (int i = 0; i < playerHouses[playerHouseIndex].Houses.ToArray().Length; i++)
            {
                if (playerHouses[GetPlayerHouseIndex(PlayerName)].Houses[i].HouseName == HouseName)
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
                tempCoords = new PlayerHouseCoords((string)player.PluginData["houseName"]);
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

            validateHouse(PlayerName, houseIndex);
        }

        public void validateHouse(string PlayerName, int houseIndex)
        {
            PlayerHouses playerHouse = playerHouses[GetPlayerHouseIndex(PlayerName)];
            PlayerHouseCoords playerHouseCoords = playerHouse.Houses[houseIndex];

            if (!((playerHouseCoords.TopLeft.X == 0 && playerHouseCoords.TopLeft.Y == 0) || 
                  (playerHouseCoords.BottomRight.X == 0 && playerHouseCoords.BottomRight.Y == 0)))
            {
                // Check for top left/bottom right reversed
                if (playerHouseCoords.TopLeft.X > playerHouseCoords.BottomRight.X ||
                    playerHouseCoords.TopLeft.Y > playerHouseCoords.BottomRight.Y)
                {
                    Server.GetPlayerByName(PlayerName).sendMessage("Top right corner is greater than bottom left, deleting house " +
                        playerHouseCoords.BottomRight.X + "," + playerHouseCoords.BottomRight.Y, chatColor);
                    playerHouse.Houses.RemoveAt(houseIndex);
                }

                // Check area
                int houseArea = (playerHouseCoords.BottomRight.X - playerHouseCoords.TopLeft.X) *
                    (playerHouseCoords.BottomRight.Y - playerHouseCoords.TopLeft.Y);
                if (houseArea > maxArea)
                {
                    Server.GetPlayerByName(PlayerName).sendMessage("Your house exceeds the maximum area, deleting house", chatColor);
                    playerHouse.Houses.RemoveAt(houseIndex);
                }

                // Check min height
                if (playerHouseCoords.BottomRight.Y < minHeight)
                {
                    Server.GetPlayerByName(PlayerName).sendMessage("Your house is below the minimum depth level, deleting house", chatColor);
                    playerHouse.Houses.RemoveAt(houseIndex);
                }

                // Check max height
                if (playerHouseCoords.BottomRight.Y > maxHeight)
                {
                    Server.GetPlayerByName(PlayerName).sendMessage("Your house is above the maximum depth level, deleting house", chatColor);
                    playerHouse.Houses.RemoveAt(houseIndex);
                }
            }
        }

        private void LoadHouseData()
        {
            XmlNode playerNode, houseNode, dataNode;
            XmlNodeList playerNodes;
            IEnumerator playerEnum, houseEnum, dataEnum;
            PlayerHouses playerHouse;
            PlayerHouseCoords playerHouseCoords;

            xmlFilename = pluginFolder + Path.DirectorySeparatorChar + xmlFilename;
            if (!File.Exists(xmlFilename))
            {
                houseXML = new XmlDocument();
                XmlNode node = houseXML.CreateNode(XmlNodeType.XmlDeclaration, "xml", xmlNamespace);
                houseXML.AppendChild(node);
                node = houseXML.CreateNode(XmlNodeType.Element, "players", xmlNamespace);
                houseXML.AppendChild(node);
                houseXML.Save(xmlFilename);
                Program.tConsole.WriteLine(houseXML.InnerText);
            }

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
                            // Loop through house data
                            int houseCount = 0;
                            houseEnum = dataNode.ChildNodes.GetEnumerator();
                            while (houseEnum.MoveNext())
                            {
                                string houseName = null;
                                houseNode = (XmlNode)houseEnum.Current;
                                for (int i = 0; i < houseNode.Attributes.Count; i++)
                                {
                                    if (houseNode.Attributes[i].Name.ToUpper() == "NAME")
                                        houseName = houseNode.Attributes[i].Value;
                                }
                                if (houseName == null)
                                    houseName = "house" + houseCount;

                                playerHouseCoords = new PlayerHouseCoords(houseName);

                                playerHouseCoords.TopLeft.X = Int32.Parse(houseNode["topleft"]["x"].InnerXml);
                                playerHouseCoords.TopLeft.Y = Int32.Parse(houseNode["topleft"]["y"].InnerXml);
                                playerHouseCoords.BottomRight.X = Int32.Parse(houseNode["bottomright"]["x"].InnerXml);
                                playerHouseCoords.BottomRight.Y = Int32.Parse(houseNode["bottomright"]["y"].InnerXml);

                                for (int i = 0; i < houseNode.ChildNodes.Count; i++)
                                {
                                    if (houseNode.ChildNodes[i].Name.ToUpper() == "ALLOW")
                                    {
                                        for (int j = 0; j < houseNode.ChildNodes[i].ChildNodes.Count; j++)
                                        {
                                            playerHouseCoords.Allowed.Add(houseNode.ChildNodes[i].ChildNodes[j].InnerXml);
                                        }
                                    }
                                }

                                playerHouse.Houses.Add(playerHouseCoords);

                                houseCount++;
                            }

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
            XmlAttribute attr, playerAttribute;

            foreach (PlayerHouses playerHouse in playerHouses)
            {
                playerAttribute = houseXML.CreateAttribute("id");
                playerAttribute.Value = playerHouse.PlayerName;
                playerNode = houseXML.CreateNode(XmlNodeType.Element, "player", xmlNamespace);
                playerNode.Attributes.Append(playerAttribute);

                node = houseXML.CreateNode(XmlNodeType.Element, "lockchests", xmlNamespace);
                node.InnerXml = playerHouse.LockChests.ToString();
                playerNode.AppendChild(node);
                node = houseXML.CreateNode(XmlNodeType.Element, "lockdoors", xmlNamespace);
                node.InnerXml = playerHouse.LockDoors.ToString();
                playerNode.AppendChild(node);
                node = houseXML.CreateNode(XmlNodeType.Element, "locksigns", xmlNamespace);
                node.InnerXml = playerHouse.LockSigns.ToString();
                playerNode.AppendChild(node);

                houses = houseXML.CreateNode(XmlNodeType.Element, "houses", xmlNamespace);
                foreach (PlayerHouseCoords coords in playerHouse.Houses)
                {
                    node = houseXML.CreateNode(XmlNodeType.Element, "house", xmlNamespace);
                    attr = houseXML.CreateAttribute("name");
                    attr.Value = coords.HouseName;
                    node.Attributes.Append(attr);

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

                    XmlNode allow, allowed;
                    allow = houseXML.CreateNode(XmlNodeType.Element, "allow", xmlNamespace);
                    IEnumerator allowIenum = coords.Allowed.GetEnumerator();
                    while (allowIenum.MoveNext())
                    {
                        allowed = houseXML.CreateNode(XmlNodeType.Element, "allowed", xmlNamespace);
                        allowed.InnerXml = (string)allowIenum.Current;
                        allow.AppendChild(allowed);
                    }
                    node.AppendChild(allow);

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
    }
}

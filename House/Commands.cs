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
        public static void DeleteHouse(string PlayerName, string HouseName, Player Deleter)
        {
            int playerIndex = House.plugin.GetPlayerHouseIndex(PlayerName);
            if (playerIndex < 0)
            {
                Deleter.sendMessage("Invalid player", House.plugin.chatColor);
            }
            else
            {
                int playerHouseIndex = House.plugin.GetPlayerHouseIndex(PlayerName);
                if (playerHouseIndex < 0)
                {
                    if (PlayerName == Deleter.Name)
                    {
                        Deleter.sendMessage("You have no houses to delete", House.plugin.chatColor);
                    }
                    else
                    {
                        Deleter.sendMessage("There are no houses to delete for " + PlayerName, House.plugin.chatColor);
                    }
                }
                else
                {
                    int coordsIndex = House.plugin.GetHouseCoordsIndexByName(PlayerName, HouseName);
                    if (coordsIndex < 0)
                        Deleter.sendMessage("There is no house called " + HouseName, House.plugin.chatColor);
                    else
                    {
                        House.plugin.playerHouses[playerHouseIndex].Houses.RemoveAt(coordsIndex);
                        Deleter.sendMessage("You've deleted the house called " + HouseName, House.plugin.chatColor);
                    }
                }
            }
        }

        public static void TeleportToHouse(string PlayerName, string HouseName, Player Teleporter)
        {
            int playerIndex = House.plugin.GetPlayerHouseIndex(PlayerName);
            if (playerIndex < 0)
            {
                Teleporter.sendMessage("Invalid player", House.plugin.chatColor);
            }
            else
            {
                if (House.plugin.properties.PlayersCanTeleport || Teleporter.Op)
                {
                    int houseIndex = House.plugin.GetHouseCoordsIndexByName(PlayerName, HouseName);
                    if (houseIndex < 0)
                    {
                        if (PlayerName == Teleporter.Name)
                            Teleporter.sendMessage("You don't have a house called " + HouseName, House.plugin.chatColor);
                        else
                            Teleporter.sendMessage(PlayerName + " doesn't have a house called " + HouseName, House.plugin.chatColor);
                    }
                    else
                    {
                        Teleporter.sendMessage("Teleporting to " + HouseName, House.plugin.chatColor);
                        PlayerHouseCoords pHC = House.plugin.playerHouses[House.plugin.GetPlayerHouseIndex(PlayerName)].Houses[houseIndex];
                        Teleporter.teleportTo(((pHC.TopLeft.X * 16) + (pHC.BottomRight.X * 16)) / 2,
                                          ((pHC.TopLeft.Y * 16) + (pHC.BottomRight.Y * 16)) / 2);
                    }
                }
                else
                    Teleporter.sendMessage("Only OPs can teleport to houses", House.plugin.chatColor);
            }
        }

        public static void house(Server server, ISender sender, ArgumentList args)
        {
            String cmd, param, param2, houseName;
            int value, houseIndex;
            Player player = server.GetPlayerByName(sender.Name);
            PlayerHouses playerHouse;
            int playerHouseIndex;
            if (args.TryGetString(0, out cmd))
            {
                switch (cmd.ToUpper())
                {
                    // HELP
                    case "H":
                    case "?":
                    case "HELP":
                        if (args.TryGetString(1, out param))
                        {
                            switch (param.ToUpper())
                            {
                                case "?":
                                case "H":
                                case "HELP":
                                    player.sendMessage("/house help <command>", House.plugin.chatColor);
                                    player.sendMessage("Retrieves help on house commands", House.plugin.chatColor);
                                    player.sendMessage("Abbreviations: ?, h", House.plugin.chatColor);
                                    break;
                                case "A":
                                case "ALLOW":
                                    player.sendMessage("/house allow <house> <player>", House.plugin.chatColor);
                                    player.sendMessage("Allow <player> access to build in <house>", House.plugin.chatColor);
                                    player.sendMessage("Abbreviations: a", House.plugin.chatColor);
                                    break;
                                case "DS":
                                case "DISALLOW":
                                    player.sendMessage("/house disallow <house> <player>", House.plugin.chatColor);
                                    player.sendMessage("Disallow <player> access to build in <house>", House.plugin.chatColor);
                                    player.sendMessage("Abbreviations: ds", House.plugin.chatColor);
                                    break;
                                case "C":
                                case "CHECK":
                                    player.sendMessage("/house check", House.plugin.chatColor);
                                    player.sendMessage("Gives you the coordinates of the next block you use the pickaxe on", House.plugin.chatColor);
                                    player.sendMessage("Abbreviations: c", House.plugin.chatColor);
                                    break;
                                case "D":
                                case "DELETE":
                                    player.sendMessage("/house delete <housename>", House.plugin.chatColor);
                                    player.sendMessage("Deletes the house called <housename>", House.plugin.chatColor);
                                    player.sendMessage("Abbreviations: d", House.plugin.chatColor);
                                    break;
                                case "P":
                                case "PROPERTIES":
                                    player.sendMessage("/house properties", House.plugin.chatColor);
                                    player.sendMessage("Lists the properties and limits set by OPs", House.plugin.chatColor);
                                    player.sendMessage("Abbreviations: p", House.plugin.chatColor);
                                    break;
                                case "S":
                                case "SET":
                                    player.sendMessage("/house set <property> <value>", House.plugin.chatColor);
                                    player.sendMessage("Sets the given property to the given value", House.plugin.chatColor);
                                    player.sendMessage("Valid properties are: MaxArea, MaxHeight, MinHeight, MaxHouses,", House.plugin.chatColor);
                                    player.sendMessage("PlayersCanTeleport, PlayersCanMakeHouses", House.plugin.chatColor);
                                    player.sendMessage("Must be OP to run this", House.plugin.chatColor);
                                    player.sendMessage("Abbreviations: s", House.plugin.chatColor);
                                    break;
                                case "T":
                                case "TELEPORT":
                                    player.sendMessage("/teleport <housename>", House.plugin.chatColor);
                                    player.sendMessage("Teleport to your house called <housename>", House.plugin.chatColor);
                                    if (!House.plugin.properties.PlayersCanTeleport)
                                        player.sendMessage("Must be OP to run this", House.plugin.chatColor);
                                    player.sendMessage("Abbreviations: t", House.plugin.chatColor);
                                    break;
                                case "TL":
                                case "TOPLEFT":
                                case "START":
                                    player.sendMessage("/house start <housename>", House.plugin.chatColor);
                                    player.sendMessage("Sets the top left coordinates of the house called <housename>", House.plugin.chatColor);
                                    player.sendMessage("to next block you use the pickaxe on", House.plugin.chatColor);
                                    player.sendMessage("Abbreviations: tl, topleft", House.plugin.chatColor);
                                    break;
                                case "BR":
                                case "BOTTOMRIGHT":
                                case "END":
                                    player.sendMessage("/house end <housename>", House.plugin.chatColor);
                                    player.sendMessage("Sets the bottom right coordinates of the house called <housename>", House.plugin.chatColor);
                                    player.sendMessage("to next block you use the pickaxe on", House.plugin.chatColor);
                                    player.sendMessage("Abbreviations: br, bottomright", House.plugin.chatColor);
                                    break;
                                case "I":
                                case "LIST":
                                    player.sendMessage("/house list", House.plugin.chatColor);
                                    player.sendMessage("Lists the names of the houses you have created", House.plugin.chatColor);
                                    player.sendMessage("Abbreviations: i", House.plugin.chatColor);
                                    break;
                                case "L":
                                case "LOCK":
                                    player.sendMessage("/house lock <object>", House.plugin.chatColor);
                                    player.sendMessage("Locks all instances of the given object in your house", House.plugin.chatColor);
                                    player.sendMessage("Valid objects are: CHESTS, DOORS, and SIGNS", House.plugin.chatColor);
                                    player.sendMessage("Abbreviations: l", House.plugin.chatColor);
                                    break;
                                case "OD":
                                case "OPDELETE":
                                    player.sendMessage("/house opdelete <player> <house>", House.plugin.chatColor);
                                    player.sendMessage("Deletes <player>'s <house>", House.plugin.chatColor);
                                    player.sendMessage("Must be OP to run this", House.plugin.chatColor);
                                    player.sendMessage("Abbreviations: od", House.plugin.chatColor);
                                    break;
                                case "OI":
                                case "OPLIST":
                                    player.sendMessage("/house oplist <player>", House.plugin.chatColor);
                                    player.sendMessage("Lists all houses for <player>", House.plugin.chatColor);
                                    player.sendMessage("Must be OP to run this", House.plugin.chatColor);
                                    player.sendMessage("Abbreviations: oi", House.plugin.chatColor);
                                    break;
                                case "OT":
                                case "OPTELEPORT":
                                    player.sendMessage("/house opteleport <player> <house>", House.plugin.chatColor);
                                    player.sendMessage("Teleports to <player>'s <house>", House.plugin.chatColor);
                                    player.sendMessage("Must be OP to run this", House.plugin.chatColor);
                                    player.sendMessage("Abbreviations: oi", House.plugin.chatColor);
                                    break;
                                case "OW":
                                case "OPWHICH":
                                    player.sendMessage("/house opwhich", House.plugin.chatColor);
                                    player.sendMessage("Returns the house you're standing in", House.plugin.chatColor);
                                    player.sendMessage("Must be OP to run this", House.plugin.chatColor);
                                    player.sendMessage("Abbreviations: ow", House.plugin.chatColor);
                                    break;
                                case "UL":
                                case "UNLOCK":
                                    player.sendMessage("/house unlock <object>", House.plugin.chatColor);
                                    player.sendMessage("Unlocks all instances of the given object in your house", House.plugin.chatColor);
                                    player.sendMessage("Valid objects are: CHESTS, DOORS, and SIGNS", House.plugin.chatColor);
                                    player.sendMessage("Abbreviations: ul", House.plugin.chatColor);
                                    break;
                                case "W":
                                case "WHICH":
                                    player.sendMessage("/house which", House.plugin.chatColor);
                                    player.sendMessage("Returns the name of your house that you're in", House.plugin.chatColor);
                                    player.sendMessage("Abbreviations: w", House.plugin.chatColor);
                                    break;
                                default:
                                    player.sendMessage("No such command!", House.plugin.chatColor);
                                    break;
                            }
                        }
                        else
                        {
                            player.sendMessage("VALID HOUSE COMMANDS:", House.plugin.chatColor);
                            player.sendMessage("/house allow, check, delete, disallow, end, list, lock, properties, set, start, unlock, which", House.plugin.chatColor);
                            player.sendMessage("Run /house ? <command> for more details on a particular command", House.plugin.chatColor);
                        }
                        break;

                    // ALLOW
                    case "A":
                    case "ALLOW":
                        if (args.TryGetString(1, out param))
                        {
                            houseIndex = House.plugin.GetHouseCoordsIndexByName(player.Name, param);
                            if (houseIndex >= 0)
                            {
                                if (args.TryGetString(2, out param2))
                                {
                                    bool foundPlayer = false;
                                    int playerIndex = House.plugin.GetPlayerHouseIndex(player.Name);
                                    foreach (string playerName in House.plugin.playerHouses[playerIndex].Houses[houseIndex].Allowed)
                                    {
                                        if (playerName == param2)
                                            foundPlayer = true;
                                    }

                                    if (!foundPlayer)
                                    {
                                        House.plugin.playerHouses[playerIndex].Houses[houseIndex].Allowed.Add(param2);
                                        player.sendMessage("Allowed " + param2 + " to " + param, House.plugin.chatColor);
                                    }
                                }
                                else
                                {
                                    player.sendMessage("You must specify a player to allow", House.plugin.chatColor);
                                }
                            }
                            else
                            {
                                player.sendMessage("You don't have a house called " + param, House.plugin.chatColor);
                            }
                        }
                        else
                        {
                            player.sendMessage("You must specify a house to allow a player into", House.plugin.chatColor);
                        }
                        break;

                    // CHECK
                    case "C":
                    case "CHECK":
                        player.PluginData["check"] = true;
                        break;

                    // PROPERTIES
                    case "P":
                    case "PROPERTIES":
                        player.sendMessage("Max Area: " + House.plugin.maxArea, House.plugin.chatColor);
                        player.sendMessage("Max Houses: " + House.plugin.maxHouses, House.plugin.chatColor);
                        player.sendMessage("Min Height: " + House.plugin.minHeight, House.plugin.chatColor);
                        player.sendMessage("Max Height: " + House.plugin.maxHeight, House.plugin.chatColor);
                        player.sendMessage("Players Can Make Houses: " + House.plugin.playersCanMakeHouses.ToString(), House.plugin.chatColor);
                        player.sendMessage("Players Can Teleport: " + House.plugin.playersCanTeleport.ToString(), House.plugin.chatColor);
                        break;

                    // DELETE
                    case "D":
                    case "DELETE":
                        if (args.TryGetString(1, out param))
                            DeleteHouse(player.Name, param, player);
                        else
                            player.sendMessage("You must supply a house name to delete", House.plugin.chatColor);
                        break;

                    // DISALLOW
                    case "DS":
                    case "DISALLOW":
                        if (args.TryGetString(1, out param))
                        {
                            houseIndex = House.plugin.GetHouseCoordsIndexByName(player.Name, param);
                            if (houseIndex >= 0)
                            {
                                if (args.TryGetString(2, out param2))
                                {
                                    bool foundPlayer = false;
                                    int playerIndex = House.plugin.GetPlayerHouseIndex(player.Name);
                                    for (int i = 0; i < House.plugin.playerHouses[playerIndex].Houses[houseIndex].Allowed.Count; i++)
                                    {
                                        if (House.plugin.playerHouses[playerIndex].Houses[houseIndex].Allowed[i] == param2)
                                        {
                                            foundPlayer = true;
                                            House.plugin.playerHouses[playerIndex].Houses[houseIndex].Allowed.RemoveAt(i);
                                            player.sendMessage("You've disallowed " + param2 + " from house " + param, House.plugin.chatColor);
                                        }
                                    }

                                    if (!foundPlayer)
                                        player.sendMessage("The player " + param2 + " is not currently allowed to house " + param, House.plugin.chatColor);
                                }
                                else
                                {
                                    player.sendMessage("You must specify a player to disallow", House.plugin.chatColor);
                                }
                            }
                            else
                            {
                                player.sendMessage("You don't have a house called " + param, House.plugin.chatColor);
                            }
                        }
                        else
                        {
                            player.sendMessage("You must specify a house to disallow a player from", House.plugin.chatColor);
                        }
                        break;

                    // TOP LEFT
                    case "TL":
                    case "TOPLEFT":
                    case "START":
                        if (House.plugin.playersCanMakeHouses || player.Op)
                        {
                            player.PluginData["starthouse"] = true;
                            if (args.TryGetString(1, out param))
                            {
                                houseIndex = House.plugin.GetHouseCoordsIndexByName(player.Name, param);
                                if (houseIndex >= 0)
                                    player.PluginData["houseIndex"] = houseIndex;
                                else
                                {
                                    int totalHouses = House.plugin.GetTotalPlayerHouses(player.Name);
                                    if (totalHouses >= House.plugin.maxHouses)
                                        player.sendMessage("You cannot create another house, you have reached the max house limit", House.plugin.chatColor);
                                    else
                                    {
                                        player.sendMessage("Break the block where you want the top left corner of your house to be", House.plugin.chatColor);
                                        player.PluginData["houseIndex"] = totalHouses >= 0 ? totalHouses : 0;
                                        player.PluginData["houseName"] = param;
                                    }
                                }
                            }
                            else
                            {
                                player.sendMessage("You must specify a name for your house", House.plugin.chatColor);
                            }
                        }
                        else
                            player.sendMessage("Players aren't allowed to make houses", House.plugin.chatColor);
                        break;

                    // BOTTOM RIGHT
                    case "BR":
                    case "BOTTOMRIGHT":
                    case "END":
                        if (House.plugin.playersCanMakeHouses || player.Op)
                        {
                            player.PluginData["endhouse"] = true;
                            if (args.TryGetString(1, out param))
                            {
                                houseIndex = House.plugin.GetHouseCoordsIndexByName(player.Name, param);
                                if (houseIndex >= 0)
                                    player.PluginData["houseIndex"] = houseIndex;
                                else
                                {
                                    int totalHouses = House.plugin.GetTotalPlayerHouses(player.Name);
                                    if (totalHouses >= House.plugin.maxHouses)
                                        player.sendMessage("You cannot create another house, you have reached the max house limit", House.plugin.chatColor);
                                    else
                                    {
                                        player.sendMessage("Break the block where you want the bottom right corner of your house to be", House.plugin.chatColor);
                                        player.PluginData["houseIndex"] = totalHouses >= 0 ? totalHouses : 0;
                                        player.PluginData["houseName"] = param;
                                    }
                                }
                            }
                            else
                            {
                                player.sendMessage("You must specify a name for your house", House.plugin.chatColor);
                            }
                        }
                        else
                            player.sendMessage("Players aren't allowed to make houses", House.plugin.chatColor);
                        break;

                    // OPDELETE
                    case "OD":
                    case "OPDELETE":
                        if (player.Op)
                        {
                            if (args.TryGetString(1, out param))
                            {
                                if (args.TryGetString(2, out param2))
                                    DeleteHouse(param, param2, player);
                                else
                                    player.sendMessage("You must supply a house name to delete", House.plugin.chatColor);
                            }
                            else
                                player.sendMessage("You must supply a player name", House.plugin.chatColor);
                        }
                        break;

                    // OPLIST
                    case "OI":
                    case "OPLIST":
                        if (player.Op)
                        {
                            if (args.TryGetString(1, out param))
                            {
                                playerHouseIndex = House.plugin.GetPlayerHouseIndex(param);
                                if (playerHouseIndex < 0)
                                    player.sendMessage("No houses for player " + param, House.plugin.chatColor);
                                else
                                {
                                    foreach (PlayerHouseCoords playerHouseCoord in House.plugin.playerHouses[playerHouseIndex].Houses)
                                    {
                                        player.sendMessage(playerHouseCoord.HouseName + " at " +
                                            "(" + playerHouseCoord.TopLeft.X + "," + playerHouseCoord.TopLeft.Y + ")" +
                                            "(" + playerHouseCoord.BottomRight.X + "," + playerHouseCoord.BottomRight.Y + ")", House.plugin.chatColor);
                                    }
                                }
                            }
                            else
                                player.sendMessage("You must supply a player name", House.plugin.chatColor);
                        }
                        break;

                    // OPTELEPORT
                    case "OT":
                    case "OPTELEPORT":
                        if (player.Op)
                        {
                            if (args.TryGetString(1, out param))
                            {
                                if (args.TryGetString(2, out param2))
                                    TeleportToHouse(param, param2, player);
                                else
                                    player.sendMessage("You must supply a house to teleport to", House.plugin.chatColor);
                            }
                            else
                                player.sendMessage("You must supply a player name", House.plugin.chatColor);
                        }
                        break;

                    // OPWHICH
                    case "OW":
                    case "OPWHICH":
                        if (player.Op)
                        {
                            houseName = House.plugin.GetHouseNameImInside(player);
                            if (houseName == null)
                                player.sendMessage("You're not inside any of your houses", House.plugin.chatColor);
                            else
                                player.sendMessage("You're inside the house called " + houseName, House.plugin.chatColor);
                        }
                        break;

                    // SET
                    case "S":
                    case "SET":
                        if (!player.Op)
                            player.sendMessage("Only ops can use this command!", House.plugin.chatColor);
                        else
                        {
                            if (args.TryGetString(1, out param))
                            {
                                switch (param.ToUpper())
                                {
                                    case "MAXAREA":
                                        if (args.TryGetInt(2, out value))
                                        {
                                            House.plugin.properties.MaxArea = value;
                                            House.plugin.maxArea = value;
                                            House.plugin.properties.Save();
                                            player.sendMessage("You updated MaxArea to " + value, House.plugin.chatColor);
                                        }
                                        else
                                            player.sendMessage("Must specify an integer value for MaxArea!", House.plugin.chatColor);
                                        break;

                                    case "MAXHOUSES":
                                        if (args.TryGetInt(2, out value))
                                        {
                                            House.plugin.properties.MaxHouses = value;
                                            House.plugin.maxHouses = value;
                                            House.plugin.properties.Save();
                                            player.sendMessage("You updated MaxHouses to " + value, House.plugin.chatColor);
                                        }
                                        else
                                            player.sendMessage("Must specify an integer value for MaxHouses!", House.plugin.chatColor);
                                        break;

                                    case "MINHEIGHT":
                                        if (args.TryGetInt(2, out value))
                                        {
                                            House.plugin.properties.MinHeight = value;
                                            House.plugin.minHeight = value;
                                            House.plugin.properties.Save();
                                            player.sendMessage("You updated MinHeight to " + value, House.plugin.chatColor);
                                        }
                                        else
                                            player.sendMessage("Must specify an integer value for MinHeight!", House.plugin.chatColor);
                                        break;

                                    case "MAXHEIGHT":
                                        if (args.TryGetInt(2, out value))
                                        {
                                            House.plugin.properties.MaxHeight = value;
                                            House.plugin.maxHeight = value;
                                            House.plugin.properties.Save();
                                            player.sendMessage("You updated MaxHeight to " + value, House.plugin.chatColor);
                                        }
                                        else
                                            player.sendMessage("Must specify an integer value for MaxHeight!", House.plugin.chatColor);
                                        break;

                                    case "PLAYERSCANTELEPORT":
                                        if (args.TryGetString(2, out param2))
                                        {
                                            if (param2.ToUpper() == "TRUE" || param2.ToUpper() == "FALSE")
                                            {
                                                House.plugin.properties.PlayersCanTeleport = Boolean.Parse(param2);
                                                House.plugin.playersCanTeleport = Boolean.Parse(param2);
                                                House.plugin.properties.Save();
                                                player.sendMessage("You updated PlayersCanTeleport to " + param2, House.plugin.chatColor);
                                            }
                                            else
                                            {
                                                player.sendMessage("The playerscanteleport property must be either true or false", House.plugin.chatColor);
                                            }
                                        }
                                        else
                                        {
                                            player.sendMessage("The playerscanteleport property must be either true or false", House.plugin.chatColor);
                                        }
                                        break;

                                    case "PLAYERSCANMAKEHOUSES":
                                        if (args.TryGetString(2, out param2))
                                        {
                                            if (param2.ToUpper() == "TRUE" || param2.ToUpper() == "FALSE")
                                            {
                                                House.plugin.properties.PlayersCanMakeHouses = Boolean.Parse(param2);
                                                House.plugin.playersCanMakeHouses = Boolean.Parse(param2);
                                                House.plugin.properties.Save();
                                                player.sendMessage("You updated PlayersCanMakeHouses to " + param2, House.plugin.chatColor);
                                            }
                                            else
                                            {
                                                player.sendMessage("The playerscanmakehouses property must be either true or false", House.plugin.chatColor);
                                            }
                                        }
                                        else
                                        {
                                            player.sendMessage("The playerscanmakehouses property must be either true or false", House.plugin.chatColor);
                                        }
                                        break;

                                    default:
                                        player.sendMessage("Invalid set parameter!", House.plugin.chatColor);
                                        break;
                                }
                            }
                            else
                                player.sendMessage("Invalid set parameter!", House.plugin.chatColor);
                        }
                        break;

                    // LIST
                    case "I":
                    case "LIST":
                        playerHouseIndex = House.plugin.GetPlayerHouseIndex(player.Name);
                        if (playerHouseIndex < 0)
                            player.sendMessage("You don't have any houses yet", House.plugin.chatColor);
                        else
                        {
                            foreach (PlayerHouseCoords playerHouseCoord in House.plugin.playerHouses[playerHouseIndex].Houses)
                            {
                                player.sendMessage(playerHouseCoord.HouseName, House.plugin.chatColor);
                            }
                        }
                        break;

                    // LOCK
                    case "L":
                    case "LOCK":
                        playerHouse = House.plugin.playerHouses[House.plugin.GetPlayerHouseIndex(player.Name)];
                        if (args.TryGetString(1, out param))
                        {
                            houseIndex = House.plugin.GetHouseCoordsIndexByName(player.Name, param);
                            if (houseIndex >= 0)
                            {
                                if (args.TryGetString(2, out param2))
                                {
                                    PlayerHouseCoords pHC = playerHouse.Houses[houseIndex];
                                    switch (param2.ToUpper())
                                    {
                                        case "C":
                                        case "CHEST":
                                        case "CHESTS":
                                            pHC.LockChests = true;
                                            playerHouse.Houses[houseIndex] = pHC;
                                            House.plugin.playerHouses[House.plugin.GetPlayerHouseIndex(player.Name)] = playerHouse;
                                            player.sendMessage("You locked all chests in your house " + param, House.plugin.chatColor);
                                            break;
                                        case "D":
                                        case "DOOR":
                                        case "DOORS":
                                            pHC.LockDoors = true;
                                            playerHouse.Houses[houseIndex] = pHC;
                                            House.plugin.playerHouses[House.plugin.GetPlayerHouseIndex(player.Name)] = playerHouse;
                                            player.sendMessage("You locked all doors in your house " + param, House.plugin.chatColor);
                                            break;
                                        case "S":
                                        case "SIGN":
                                        case "SIGNS":
                                            pHC.LockSigns = true;
                                            playerHouse.Houses[houseIndex] = pHC;
                                            House.plugin.playerHouses[House.plugin.GetPlayerHouseIndex(player.Name)] = playerHouse;
                                            player.sendMessage("You locked all signs in your house " + param, House.plugin.chatColor);
                                            break;
                                        default:
                                            player.sendMessage("Invalid lock parameter!", House.plugin.chatColor);
                                            break;
                                    }
                                }
                                else
                                    player.sendMessage("You must supply a keyword (CHESTS, DOORS, SIGNS)", House.plugin.chatColor);
                            }
                            else
                                player.sendMessage("No such house", House.plugin.chatColor);
                        }
                        else
                        {
                            player.sendMessage("You must supply a house name", House.plugin.chatColor);
                        }
                        break;

                    // TELEPORT
                    case "T":
                    case "TELEPORT":
                        if (args.TryGetString(1, out param))
                            TeleportToHouse(player.Name, param, player);
                        else
                            player.sendMessage("You must supply a house name to teleport to", House.plugin.chatColor);
                        break;

                    // WHICH
                    case "W":
                    case "WHICH":
                        houseName = House.plugin.GetMyHouseNameImInside(player.Name);
                        if (houseName == null)
                            player.sendMessage("You're not inside any of your houses", House.plugin.chatColor);
                        else
                            player.sendMessage("You're inside the house called " + houseName, House.plugin.chatColor);
                        break;

                    // UNLOCK
                    case "UL":
                    case "UNLOCK":
                        playerHouse = House.plugin.playerHouses[House.plugin.GetPlayerHouseIndex(player.Name)];
                        if (args.TryGetString(1, out param))
                        {
                            houseIndex = House.plugin.GetHouseCoordsIndexByName(player.Name, param);
                            if (houseIndex >= 0)
                            {
                                if (args.TryGetString(2, out param2))
                                {
                                    PlayerHouseCoords pHC = playerHouse.Houses[houseIndex];
                                    switch (param2.ToUpper())
                                    {
                                        case "C":
                                        case "CHEST":
                                        case "CHESTS":
                                            pHC.LockChests = false;
                                            playerHouse.Houses[houseIndex] = pHC;
                                            House.plugin.playerHouses[House.plugin.GetPlayerHouseIndex(player.Name)] = playerHouse;
                                            player.sendMessage("You unlocked all chests in your house " + param, House.plugin.chatColor);
                                            break;
                                        case "D":
                                        case "DOOR":
                                        case "DOORS":
                                            pHC.LockDoors = false;
                                            playerHouse.Houses[houseIndex] = pHC;
                                            House.plugin.playerHouses[House.plugin.GetPlayerHouseIndex(player.Name)] = playerHouse;
                                            player.sendMessage("You unlocked all doors in your house " + param, House.plugin.chatColor);
                                            break;
                                        case "S":
                                        case "SIGN":
                                        case "SIGNS":
                                            pHC.LockSigns = false;
                                            playerHouse.Houses[houseIndex] = pHC;
                                            House.plugin.playerHouses[House.plugin.GetPlayerHouseIndex(player.Name)] = playerHouse;
                                            player.sendMessage("You unlocked all signs in your house " + param, House.plugin.chatColor);
                                            break;
                                        default:
                                            player.sendMessage("Invalid unlock parameter!", House.plugin.chatColor);
                                            break;
                                    }
                                }
                                else
                                    player.sendMessage("You must supply a keyword (CHESTS, DOORS, SIGNS)", House.plugin.chatColor);
                            }
                            else
                                player.sendMessage("No such house", House.plugin.chatColor);
                        }
                        else
                        {
                            player.sendMessage("You must supply a house name", House.plugin.chatColor);
                        }
                        break;
                    default:
                        player.sendMessage("Invalid house command", House.plugin.chatColor);
                        break;
                }
            }
        }
    }
}

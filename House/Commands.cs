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
            String cmd, param;
            int value, houseIndex;
            Player player = server.GetPlayerByName(sender.Name);
            PlayerHouses playerHouse;
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
                                    player.sendMessage("Valid properties are: MaxArea, MaxHeight, MinHeight, MaxHouses", House.plugin.chatColor);
                                    player.sendMessage("Must be OP to run this", House.plugin.chatColor);
                                    player.sendMessage("Abbreviations: s", House.plugin.chatColor);
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
                            player.sendMessage("/house check, delete, end, list, lock, properties, set, start, unlock", House.plugin.chatColor);
                            player.sendMessage("Run /house ? <command> for more details on a particular command", House.plugin.chatColor);
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
                        break;

                    // DELETE
                    case "D":
                    case "DELETE":
                        int playerHouseIndex = House.plugin.GetPlayerHouseIndex(player.Name);
                        if (playerHouseIndex < 0)
                            throw new CommandError("You don't have any houses to delete");
                        else
                        {
                            if (args.TryGetString(1, out param))
                            {
                                int coordsIndex = House.plugin.GetHouseCoordsIndexByName(player.Name, param);
                                if (coordsIndex < 0)
                                    throw new CommandError("You do not have a house called " + param);
                                else
                                {
                                    House.plugin.playerHouses[playerHouseIndex].Houses.RemoveAt(coordsIndex);
                                    player.sendMessage("You've deleted the house called " + param, House.plugin.chatColor);
                                }   
                            }
                            else
                                throw new CommandError("You must supply the name of the house you want to delete");
                        }
                        break;

                    // TOP LEFT
                    case "TL":
                    case "TOPLEFT":
                    case "START":
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
                                    throw new CommandError("You cannot create another house, you have reached the max house limit");
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
                            throw new CommandError("You must specify a name for your house");
                        }
                        break;

                    // BOTTOM RIGHT
                    case "BR":
                    case "BOTTOMRIGHT":
                    case "END":
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
                                    throw new CommandError("You cannot create another house, you have reached the max house limit");
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
                            throw new CommandError("You must specify a name for your house");
                        }
                        break;

                    // SET
                    case "S":
                    case "SET":
                        if (!player.isInOpList())
                            throw new CommandError("Only ops can use this command!");
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
                                            throw new CommandError("Must specify an integer value for MaxArea!");
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
                                            throw new CommandError("Must specify an integer value for MaxHouses!");
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
                                            throw new CommandError("Must specify an integer value for MinHeight!");
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
                                            throw new CommandError("Must specify an integer value for MaxHeight!");
                                        break;
                                    default:
                                        throw new CommandError("Invalid set parameter!");
                                }
                            }
                            else
                                throw new CommandError("Invalid set parameter!");
                        }
                        break;

                    // LIST
                    case "I":
                    case "LIST":
                        playerHouseIndex = House.plugin.GetPlayerHouseIndex(player.Name);
                        if (playerHouseIndex < 0)
                            throw new CommandError("You don't have any houses yet");
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
                            switch (param.ToUpper())
                            {
                                case "C":
                                case "CHEST":
                                case "CHESTS":
                                    playerHouse.LockChests = true;
                                    House.plugin.playerHouses[House.plugin.GetPlayerHouseIndex(player.Name)] = playerHouse;
                                    player.sendMessage("You locked all chests in your house", House.plugin.chatColor);
                                    break;
                                case "D":
                                case "DOOR":
                                case "DOORS":
                                    playerHouse.LockDoors = true;
                                    House.plugin.playerHouses[House.plugin.GetPlayerHouseIndex(player.Name)] = playerHouse;
                                    player.sendMessage("You locked all doors in your house", House.plugin.chatColor);
                                    break;
                                case "S":
                                case "SIGN":
                                case "SIGNS":
                                    playerHouse.LockSigns = true;
                                    House.plugin.playerHouses[House.plugin.GetPlayerHouseIndex(player.Name)] = playerHouse;
                                    player.sendMessage("You locked all signs in your house", House.plugin.chatColor);
                                    break;
                                default:
                                    throw new CommandError("Invalid lock parameter!");
                            }
                        }
                        else
                        {
                            throw new CommandError("Invalid lock parameter");
                        }
                        break;

                    // WHICH
                    case "W":
                    case "WHICH":
                        string houseName = House.plugin.GetHouseNameImInside(player.Name);
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
                            switch (param.ToUpper())
                            {
                                case "C":
                                case "CHEST":
                                case "CHESTS":
                                    playerHouse.LockChests = false;
                                    House.plugin.playerHouses[House.plugin.GetPlayerHouseIndex(player.Name)] = playerHouse;
                                    player.sendMessage("You unlocked all chests in your house", House.plugin.chatColor);
                                    break;
                                case "D":
                                case "DOOR":
                                case "DOORS":
                                    playerHouse.LockDoors = false;
                                    House.plugin.playerHouses[House.plugin.GetPlayerHouseIndex(player.Name)] = playerHouse;
                                    player.sendMessage("You unlocked all doors in your house", House.plugin.chatColor);                                    break;
                                case "S":
                                case "SIGN":
                                case "SIGNS":
                                    playerHouse.LockSigns = false;
                                    House.plugin.playerHouses[House.plugin.GetPlayerHouseIndex(player.Name)] = playerHouse;
                                    player.sendMessage("You unlocked all signs in your house", House.plugin.chatColor);
                                    break;
                                default:
                                    throw new CommandError("Invalid lock parameter!");
                            }
                        }
                        else
                        {
                            throw new CommandError("Invalid lock parameter");
                        }
                        break;
                    default:
                        throw new CommandError("Invalid house command");
                }
            }
        }
    }
}

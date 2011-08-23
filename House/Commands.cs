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
            int value;
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
                                case "S":
                                case "SET":
                                    player.sendMessage("/house set <property> <value>", House.plugin.chatColor);
                                    player.sendMessage("Sets the given property to the given value", House.plugin.chatColor);
                                    player.sendMessage("Valid properties are: MaxArea, MaxHeight, MinHeight", House.plugin.chatColor);
                                    player.sendMessage("Must be OP to run this", House.plugin.chatColor);
                                    player.sendMessage("Abbreviations: s", House.plugin.chatColor);
                                    break;
                                case "TL":
                                case "TOPLEFT":
                                case "START":
                                    player.sendMessage("/house start", House.plugin.chatColor);
                                    player.sendMessage("Sets the coordinates of the next block you use the pickaxe on", House.plugin.chatColor);
                                    player.sendMessage("to the top left corner of your house", House.plugin.chatColor);
                                    player.sendMessage("Abbreviations: tl, topleft", House.plugin.chatColor);
                                    break;
                                case "BR":
                                case "BOTTOMRIGHT":
                                case "END":
                                    player.sendMessage("/house end", House.plugin.chatColor);
                                    player.sendMessage("Sets the coordinates of the next block you use the pickaxe on", House.plugin.chatColor);
                                    player.sendMessage("to the bottom right corner of your house", House.plugin.chatColor);
                                    player.sendMessage("Abbreviations: br, bottomright", House.plugin.chatColor);
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
                                default:
                                    player.sendMessage("No such command!", House.plugin.chatColor);
                                    break;
                            }
                        }
                        else
                        {
                            player.sendMessage("VALID HOUSE COMMANDS:", House.plugin.chatColor);
                            player.sendMessage("/house check, set, start, end, lock, unlock", House.plugin.chatColor);
                            player.sendMessage("Run /house ? <command> for more details on a particular command", House.plugin.chatColor);
                        }
                        break;

                    // CHECK
                    case "C":
                    case "CHECK":
                        player.PluginData["check"] = true;
                        break;

                    // TOP LEFT
                    case "TL":
                    case "TOPLEFT":
                    case "START":
                        player.PluginData["starthouse"] = true;
                        break;

                    // BOTTOM RIGHT
                    case "BR":
                    case "BOTTOMRIGHT":
                    case "END":
                        player.PluginData["endhouse"] = true;
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

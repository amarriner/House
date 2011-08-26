HOUSE

This plugin allows every player to designate houses for themselves. A house is a rectangular region that other players (except OPs) cannot build in. A properties file allows server admins (or OPs while the server is running) to alter the maximum area a player house can be, how many houses a player may have, how high in the world it can be, and how low in the world it can be.

It also allows players to lock and unlock chests, doors, and/or signs that are within their house rectangle. Locked chests cannot be opened by other players, locked doors cannot be opened by other players, and locked signs cannot be edited by other players (well they can be edited, but they don't save). Doors are not locking at the moment

This plugin probably works best with something like the Restrict plugin where each username is unique. Also, the houses aren't immune to explosives so using rakAntiGrief or something similar is probably wise as well. Looks like explosives are prevented from damaging houses as well.

There's one command with several options:

/house allow <house> <player> - Allow <player> the ability to build in <house>
/house check - Use the pickaxe on a block and get the coordinates of that block
/house delete <house> - Deletes the given player's house
/house disallow <house> <player> - Remove <player> from allowed list in <house>
/house start <house> - Use the pickaxe on a block to set the top left corner of your house
/house end <house> - Use the pickaxe on a block to set the bottom right corner of your house
/house help - Prints help text
/house list - Lists the player's current houses
/house lock <object> - Lock all instances of <object> within your house
/house opdelete <player> <house> - Delete <player>'s <house> (requires OP)
/house oplist <player> - List all <player>'s houses (requires OP)
/house opteleport <player> <house> - Teleport to <player>'s <house> (requires OP)
/house opwhich - Display the house you're in regardless of owner (requires OP)
/house properties - Lists current property settings
/house set <property> <value> - Set any of the properties to a value (requires OP)
/house teleport <house> - Teleport to your house called <house> (optionally requires OP)
/house teleportset <house> - Set the point where you'll teleport to inside <house>
/house unlock <object> - Unlock all instances of <object> within your house
/house which - Returns the name of the player's house they are currently inside of

There are a few properties that can be set at runtime via an OP or in a properties file:
MaxArea - The largest sized rectangle allowed for houses (length * width)
MaxHeight - The lowest level houses can be created at
MinHeight - The highest level houses can be created at
MaxHouses - The number of houses a player can create
PlayersCanMakeHouses - Whether players or just OPs can create houses
PlayersCanTeleport - Whether players or just OPs can teleport to their houses

There's some other things I'll probably work on after an initial round of bug squashing: using something other than a rectangle to define a house, and attempting to get doors working properly. However, I'm at a point where I would like to have some testing done just for a change of pace and to see where things lie.

This plugin is very early on so just let me know whatever bugs you come across and I'll do my best to work on them. Another learning attempt for me, and really a way to solve an issue I've been having. Hope others may have use for this, too. 

A NOTE ABOUT UPGRADING!
If you're upgrading from one version of the plugin to another, it's wise to backup your house.xml file before running the server with the new version. For the most part I try to make each version backwards compatible with prior versions of the XML layout, but of course there could be bugs. Ideally you'd delete the house.xml file entirely before running the new version, but naturally that deletes everyone's houses so that's not always practical. So when in doubt, back up! Generally any changes could be made by hand if absolutely necessary.

SOURCE
https://github.com/amarriner/House

DOWNLOAD
http://awbw.amarriner.com/terraria/House.zip

CHANGELOG
0.3.5
Disallows liquid flow from within a house (can still flow in from outside!)

0.3.4
Moved locks to be on a per house basis rather than every house for a player
Changed the way houses being made are stored by the plugin to attempt to avoid invalid houses being created
Added teleportset command to allow players to set the point to which they'll teleport to inside a given house

0.3.3
Fixed opwhere command
Fixed house building bug

0.3.2
Added four admin/op functions oplist, opdelete, opteleport, opwhich
Fixed a bug where you could create a house inside another of your own houses

0.3.1
Fixed a problem where commands were allowed for users in the OpList, even if they weren't logged in as OP

0.3
Players can teleport to their houses
Teleport can be restricted to OPs only via a property
Players can allow other players access to build in their houses
The ability to make houses can be restricted to OPs only via a property

0.2
Multiple houses per player
Houses now require a name
New property called maxhouses that limits the number of houses a player might have
New commands delete, list, properties, and which

0.1.1
Fixed server malfunction error which occurred when attempting to access null object
Fixed issue where when bottom right wasn't set, the plugin always thought the house was invalid
Made it so that the house.xml file doesn't need to be present when the plugin runs. It will be created if it's missing

0.1
Initial Release
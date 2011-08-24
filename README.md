HOUSE
A plugin for TDSM to allow users to designate a safe area
author: amarriner

This plugin allows every player to designate a house for themselves. A house is a rectangular region that other players (except OPs) cannot build in. A properties file allows server admins (or OPs while the server is running) to alter the maximum area a player house can be, how high in the world it can be, and how low in the world it can be.

It also allows players to lock and unlock chests, doors, and/or signs that are within their house rectangle. Locked chests cannot be opened by other players, locked doors cannot be opened by other players, and locked signs cannot be edited by other players (well they can be edited, but they don't save). Doors are not locking at the moment

The zip file contains an xml file that MUST be present when the server starts or it'll crash (I'll fix this in the future). This plugin probably works best with something like the Restrict plugin where each username is unique. Also, the houses aren't immune to explosives so using rakAntiGrief or something similar is probably wise as well.

There's one command with several options:

/house check - Use the pickaxe on a block and get the coordinates of that block
/house start - Use the pickaxe on a block to set the top left corner of your house
/house end - Use the pickaxe on a block to set the bottom right corner of your house
/house lock <object> - Lock all instances of <object> within your house
/house unlock <object> - Unlock all instances of <object> within your house
/house set <property> <value> - Set any of the properties to a value (requires OP)
/house help

There's some other things I'll probably work on after an initial round of bug squashing. There's the groundwork for multiple houses for a player (max houses property would be set by admin or OP), using something other than a rectangle to define a house, and attempting to get doors working properly. However, I'm at a point where I would like to have some testing done just for a change of pace and to see where things lie.

This plugin is very early on so just let me know whatever bugs you come across and I'll do my best to work on them. Another learning attempt for me, and really a way to solve an issue I've been having. Hope others may have use for this, too. 
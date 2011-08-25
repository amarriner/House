using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria_Server.Misc;

namespace House
{
    public class Properties : PropertiesFile
    {
        private const int DEFAULT_MAX_HOUSES = 1;
        private const int DEFAULT_MAX_AREA = 1000;
        private const int DEFAULT_MIN_HEIGHT = 175;
        private const int DEFAULT_MAX_HEIGHT = 1000;
        private const bool DEFAULT_PLAYERS_CAN_TELEPORT = false;
        private const bool DEFAULT_PLAYERS_CAN_MAKE_HOUSES = true;

        private const String MAX_HOUSES = "maxhouses";
        private const String MAX_AREA = "maxarea";
        private const String MIN_HEIGHT = "minheight";
        private const String MAX_HEIGHT = "maxheight";
        private const String PLAYERS_CAN_TELEPORT = "playerscanteleport";
        private const String PLAYERS_CAN_MAKE_HOUSES = "playerscanmakehouses";

        public Properties(String propertiesPath) : base(propertiesPath) { }

        public int MaxArea
        {
            get
            {
                return getValue(MAX_AREA, DEFAULT_MAX_AREA);
            }

            set
            {
                setValue(MAX_AREA, value);
            }
        }

        public int MaxHouses
        {
            get
            {
                return getValue(MAX_HOUSES, DEFAULT_MAX_HOUSES);
            }

            set
            {
                setValue(MAX_HOUSES, value);
            }
        }

        public int MinHeight
        {
            get
            {
                return getValue(MIN_HEIGHT, DEFAULT_MIN_HEIGHT);
            }
            set
            {
                setValue(MIN_HEIGHT, value);
            }
        }

        public int MaxHeight
        {
            get
            {
                return getValue(MAX_HEIGHT, DEFAULT_MAX_HEIGHT);
            }
            set
            {
                setValue(MAX_HEIGHT, value);
            }
        }

        public bool PlayersCanTeleport
        {
            get
            {
                return getValue(PLAYERS_CAN_TELEPORT, DEFAULT_PLAYERS_CAN_TELEPORT);
            }
            set
            {
                setValue(PLAYERS_CAN_TELEPORT, value);
            }
        }

        public bool PlayersCanMakeHouses
        {
            get
            {
                return getValue(PLAYERS_CAN_MAKE_HOUSES, DEFAULT_PLAYERS_CAN_MAKE_HOUSES);
            }
            set
            {
                setValue(PLAYERS_CAN_MAKE_HOUSES, value);
            }
        }
    }
}

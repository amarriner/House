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

        private const String MAX_HOUSES = "maxhouses";
        private const String MAX_AREA = "maxarea";
        private const String MIN_HEIGHT = "minheight";
        private const String MAX_HEIGHT = "maxheight";

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
    }
}

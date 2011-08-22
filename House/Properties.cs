using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria_Server.Misc;

namespace House
{
    public class Properties : PropertiesFile
    {
        private const int DEFAULT_MAX_AREA = 1000;

        private const String MAX_AREA = "maxarea";

        public Properties(String propertiesPath) : base(propertiesPath) { }

        public int MaxArea
        {
            get
            {
                return getValue(MAX_AREA, DEFAULT_MAX_AREA);
            }
        }
    }
}

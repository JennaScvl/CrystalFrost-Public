using System.Collections;
using System.Collections.Generic;

namespace CrystalFrost.Config
{
    public class GridConfig
    {
        public const string subsectionName = "Grid";

        public string LoginURI { get; set; } = OpenMetaverse.Settings.AGNI_LOGIN_SERVER;
    }
}

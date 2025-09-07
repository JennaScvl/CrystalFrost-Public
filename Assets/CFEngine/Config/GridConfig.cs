using System.Collections;
using System.Collections.Generic;

namespace CrystalFrost.Config
{
    /// <summary>
    /// Contains configuration settings for grid-related functionalities.
    /// </summary>
    public class GridConfig
    {
        /// <summary>
        /// The name of the configuration subsection for grid settings.
        /// </summary>
        public const string subsectionName = "Grid";

        /// <summary>
        /// Gets or sets the login URI for the grid.
        /// </summary>
        public string LoginURI { get; set; } = OpenMetaverse.Settings.AGNI_LOGIN_SERVER;
    }
}

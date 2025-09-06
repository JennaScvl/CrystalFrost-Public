using Microsoft.Extensions.Logging;
using OpenMetaverse;
using System.IO;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace CrystalFrost
{
    /// <summary>
    /// Defines a piece of code that create and configure a
    /// OpenMetaverse.GridClient.
    /// </summary>
    public interface IGridClientFactory
    {
        GridClient BuildGridClient();
    }

    /// <summary>
    /// A Default implementation of IGridClientFactory
    /// </summary>
    public class GridClientFactory : IGridClientFactory
    {
        private readonly ILogger _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="GridClientFactory"/> class.
        /// </summary>
        /// <param name="log">The logger for recording messages.</param>
        public GridClientFactory(ILogger<GridClientFactory> log)
        {
            _log = log;
        }

        /// <summary>
        /// Creates and configures a <see cref="GridClient"/> instance with default settings.
        /// </summary>
        /// <returns>A new <see cref="GridClient"/> instance.</returns>
        public GridClient BuildGridClient()
        {
            var client = new GridClient();
            client.Settings.SEND_AGENT_UPDATES = true;
            client.Settings.ALWAYS_REQUEST_OBJECTS = true;
            client.Settings.ALWAYS_DECODE_OBJECTS = true;
            client.Settings.OBJECT_TRACKING = true;
            client.Settings.USE_HTTP_TEXTURES = true;

            //todo Get cacheDir from configuration.
            var cacheDir = Path.Combine(Application.persistentDataPath, "cache");

            _log.GridClientCacheDirSet(cacheDir);

            client.Settings.ASSET_CACHE_DIR = cacheDir;
            client.Settings.SEND_PINGS = true;
            //client.Settings.ENABLE_CAPS = true;
            client.Settings.ENABLE_SIMSTATS = true;
            client.Settings.STORE_LAND_PATCHES = true;
            client.Settings.ENABLE_SIMSTATS = true;
            client.Settings.USE_INTERPOLATION_TIMER = false;
            client.Settings.SEND_AGENT_THROTTLE = true;
            client.Settings.SEND_AGENT_UPDATES = true;
            //client.Settings.TRACK_UTILIZATION = false;
			client.Settings.MULTIPLE_SIMS = true;

            return client;
        }
    }
}

using Microsoft.Extensions.Configuration;

using CrystalFrost.Config;
using Microsoft.Extensions.Options;
namespace CrystalFrost.Logging
{
    public static class LoggingConfiguration
    {
        /* COMMENTED OUT COMMENT: Code below is from the original LoggingConfiguration.cs file.
        // for now logging configuration is comming from this in memory declaration.
        // In the future It could come from a config file on disk or something.
        private static readonly Dictionary<string, string> ConfigData = new()
        {
            // set our default level to Information.
            ["Logging:LogLevel:Default"] = "Information",
            // this next line sets the GridClientFactory to 'Debug' level.
            // The factory is not chatty, and this lets us see logging is working by loging one thing
            ["Logging:LogLevel:CrystalFrost.GridClientFactory"] = "Information",
			// this next controlls capturing log messages from the LibMetaverse Library code.
			["Logging:LogLevel:CrystalFrost.Logging.LMVLogger"] = "Information",
        };

        private static IConfigurationRoot _root = default!;

        private static IConfigurationRoot BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(ConfigData)
                .Build();
        }
        */

        /// <summary>
        /// Gets the logging configuration section from the configuration root.
        /// This gets the raw "Logging" section from the configuration as it is
        /// passed directly into the logging subsystem.
        /// This allows someone to add  their own section specific logging parameters
        /// without having to change code.
        /// </summary>
        /// <returns></returns>
        public static IConfigurationSection GetLoggingConfiguration()
        {
            return Services.GetConfigSection("Logging");
        }
    }
}

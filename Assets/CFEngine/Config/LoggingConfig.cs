using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CrystalFrost.Config
{
    /// <summary>
    /// Contains configuration settings for the logging subsystem.
    /// </summary>
    public class LoggingConfig
    {
        /// <summary>
        /// Provides default logging level settings for various components.
        /// </summary>
        public static Dictionary<string,string> DefaultValues = new Dictionary<string, string>()
        {
            { "Logging:LogLevel:Default", "Information" },
            { "Logging:LogLevel:CrystalFrost.GridClientFactory", "Information" },
            { "Logging:LogLevel:CrystalFrost:Logging:LMVLogger", "Information" }
        };

        /// <summary>
        /// The name of the configuration subsection for logging.
        /// </summary>
        public const string subsectionName = "Logging";

        /// <summary>
        /// Gets or sets the directory where log files will be stored.
        /// </summary>
        public string LogDirectory { get; set; } = "./Logs";

        /// <summary>
        /// Defines the default log levels for different parts of the application.
        /// </summary>
        public object LogLevel = new {
            Default = "Information",
            CrystalFrost = new {
                GridClientFactory = "Information",
                Logging = new {
                    LMVLogger = "Information"
                }
            }
        };

    }
}

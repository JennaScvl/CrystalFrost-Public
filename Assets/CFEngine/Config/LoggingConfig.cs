using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CrystalFrost.Config
{
    public class LoggingConfig
    {
        // Logging config is handled differently than other config sections since
        // this configuraion is passed directly into the logging system.
        // This creates the presets that can be modified by the user.
        public static Dictionary<string,string> DefaultValues = new Dictionary<string, string>()
        {
            { "Logging:LogLevel:Default", "Information" },
            { "Logging:LogLevel:CrystalFrost.GridClientFactory", "Information" },
            { "Logging:LogLevel:CrystalFrost:Logging:LMVLogger", "Information" }
        };

        public const string subsectionName = "Logging";

        public string LogDirectory { get; set; } = "./Logs";

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

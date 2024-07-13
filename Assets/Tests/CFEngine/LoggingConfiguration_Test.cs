using System;

using NUnit.Framework;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using CrystalFrost.Config;
using CrystalFrost.Logging;

namespace CrystalFrostEngine.Tests
{
    public class LoggingConfigration_Test
    {
        // A Test behaves as an ordinary method
        [Test]
        public void LoggingConfigration_DefaultInfo()
        {
            ConfigRoot_Test.SetupConfigAndServiceProvider(out IServiceProvider serviceProvider, out IConfigurationRoot configs);

            // Default loglevel specified in the configuration
            string defaultLogLevel = configs["Logging:LogLevel:Default"];

            Assert.AreEqual(defaultLogLevel, "Information");
        }

        [Test]
        public void LoggingConfiguration_EnvironmentVar()
        {
            Environment.SetEnvironmentVariable("CF_Logging:LogLevel:Default", "Debug");

            ConfigRoot_Test.SetupConfigAndServiceProvider(out IServiceProvider serviceProvider, out IConfigurationRoot configs);

            // Remember to remove the evnironment variable or later tests will be affected
            Environment.SetEnvironmentVariable("CF_Logging:LogLevel:Default", null);

            // Default loglevel specified in the configuration
            string defaultLogLevel = configs["Logging:LogLevel:Default"];

            // var logConfig = serviceProvider.GetService<IOptions<LoggingConfig>>().Value;
            Assert.AreEqual(defaultLogLevel, "Debug");
        }
    }
}

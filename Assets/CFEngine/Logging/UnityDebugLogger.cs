using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace CrystalFrost.Logging
{
    /// <summary>
    /// An ILogger that outputs via UnityEngine.Debug
    /// </summary>
    public class UnityDebugLogger : ILogger
    {
        /// <summary>
        /// Holds the desired level for this category.
        /// </summary>
        private readonly LogLevel _logLevel;

        public UnityDebugLogger(string categoryName, IConfiguration configuration)
        {
            // Look in the configuration for a log level section,
            // and in there look for value with out category name.
            // if that value exists use it for our level.
            // if a value with a name matching our category was not found.
            // use the default category.
            // if there is no default category use 'Information' as the level.
            var logLevelSection = configuration.GetSection("LogLevel");
            var level = logLevelSection[categoryName];
            level ??= logLevelSection["Default"];
            level ??= "Information";

            // convert the string from the configuration to the enum.
            // defaulting to information if there are problems.
            _logLevel = Enum.TryParse<LogLevel>(level, out var parsed)
                ? parsed
                : LogLevel.Information;

            //System.Diagnostics.Debug.WriteLine(categoryName + " LogLevel: " + _logLevel);
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            // no need for logging scopes just yet.
            return default;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= _logLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel)) return; // don't bother if the level is too low.

            // only create the log string now that its been 
            // decided to actually log something.
            // this prevents allocations of strings that might not get
            // logged, and this is where we squeeze performance out of the logging code,
            // by not allocating and garbage collecting strings unless we really need to.
            var message = formatter(state, exception);

            System.Diagnostics.Debug.Write(logLevel);
            System.Diagnostics.Debug.WriteLine(message);

            // Dump the message out via UnityEngine.Debug
            switch(logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                case LogLevel.Information:
                    UnityEngine.Debug.Log(message);
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(message);
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                default:
                    UnityEngine.Debug.LogError(message);
                    break;
            }
        }
    }
}

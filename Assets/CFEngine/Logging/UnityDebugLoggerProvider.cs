using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

namespace CrystalFrost.Logging
{
    /// <summary>
    /// A logger provider that creates loggers that log via
    /// UnityEngine.Debug.
    /// </summary>
    public class UnityDebugLoggerProvider : ILoggerProvider
    {
        // keep a reference to the configuration, it gets used
        // each time a logger is created so that logger can configure
        // itself.
        private readonly IConfigurationSection _configuration;

        // keep a dictionary of previously created loggers.
        // the key is the 'categoryname' which in practice is the Class Name.
        private readonly ConcurrentDictionary<string, UnityDebugLogger> _loggers =
            new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="UnityDebugLoggerProvider"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to use for the loggers.</param>
        public UnityDebugLoggerProvider(IConfiguration configuration)
        {
            // look for a subsection in the logging section we were provided.
            // if that is there use it, otherwise default to the generic logging
            // configuration
            _configuration = configuration.GetSection("UnityDebugLogger");
            if (!_configuration.Exists())
            {
                _configuration = (IConfigurationSection)configuration;
            }
        }

        public ILogger CreateLogger(string categoryName)
        {
            // if this categoryName/class name has been seen before
            // reuse it. Otherwise create a new one.
            return _loggers.GetOrAdd(
                categoryName,
                name => new UnityDebugLogger(name, _configuration));
        }

        public void Dispose()
        {
            // this provide doesn't use any unmanaged resources,
            // but ILoggerProvider inherits IDisposable, so we must implement
            // Dispose(), and this call to suppress finalize prevents compiler hints.
            GC.SuppressFinalize(this);
        }
    }
}

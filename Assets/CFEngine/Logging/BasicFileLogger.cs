using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace CrystalFrost.Logging
{
	/// <summary>
	/// Implements a simple file logger that writes log messages to a file.
	/// </summary>
	public class BasicFileLogger : ILogger
	{
		/// <summary>
		/// Holds the desired level for this category.
		/// </summary>
		private readonly LogLevel _logLevel;
		private readonly ILogFileWriter _writer;

		/// <summary>
		/// Initializes a new instance of the <see cref="BasicFileLogger"/> class.
		/// </summary>
		/// <param name="categoryName">The category name for messages produced by the logger.</param>
		/// <param name="configuration">The application configuration.</param>
		/// <param name="writer">The file writer to use for logging.</param>
		public BasicFileLogger(string categoryName, IConfiguration configuration,
			ILogFileWriter writer)
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

			_writer = writer;
		}

		/// <summary>
		/// Begins a logical operation scope.
		/// </summary>
		/// <typeparam name="TState">The type of the state to begin scope for.</typeparam>
		/// <param name="state">The identifier for the scope.</param>
		/// <returns>A disposable object that ends the logical operation scope on disposal.</returns>
		public IDisposable BeginScope<TState>(TState state) where TState : notnull
		{
			// no need for logging scopes just yet.
			return default;
		}

		/// <summary>
		/// Checks if the given <paramref name="logLevel"/> is enabled.
		/// </summary>
		/// <param name="logLevel">The level to be checked.</param>
		/// <returns>True if enabled; otherwise, false.</returns>
		public bool IsEnabled(LogLevel logLevel)
		{
			return logLevel >= _logLevel;
		}

		/// <summary>
		/// Writes a log entry.
		/// </summary>
		/// <typeparam name="TState">The type of the object to be written.</typeparam>
		/// <param name="logLevel">The entry will be written on this level.</param>
		/// <param name="eventId">The id of the event.</param>
		/// <param name="state">The entry to be written. Can be also an object.</param>
		/// <param name="exception">The exception related to this entry.</param>
		/// <param name="formatter">A function to create a <c>string</c> message of the <paramref name="state"/> and <paramref name="exception"/>.</param>
		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			_writer.Enqeue(eventId.Id, logLevel, formatter(state, exception));
		}
	}
}

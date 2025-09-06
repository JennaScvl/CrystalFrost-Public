using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

namespace CrystalFrost.Logging
{
	/// <summary>
	/// A provider for creating <see cref="BasicFileLogger"/> instances.
	/// </summary>
	public class BasicFileLoggerProvider : ILoggerProvider
	{
		private readonly IConfigurationSection _configuration;
		private readonly LogFileWriter _writer;
		private readonly ConcurrentDictionary<string, BasicFileLogger> _loggers = new(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Initializes a new instance of the <see cref="BasicFileLoggerProvider"/> class.
		/// </summary>
		/// <param name="configuration">The configuration to use for the loggers.</param>
		public BasicFileLoggerProvider(IConfiguration configuration) 
		{
			_configuration = configuration.GetSection("BasicFileLogger");
			_writer = new LogFileWriter();
		}

		/// <summary>
		/// Creates a new <see cref="BasicFileLogger"/> instance.
		/// </summary>
		/// <param name="categoryName">The category name for messages produced by the logger.</param>
		/// <returns>A new <see cref="BasicFileLogger"/> instance.</returns>
		public ILogger CreateLogger(string categoryName)
		{
			return _loggers.GetOrAdd(categoryName, name => new BasicFileLogger(name, _configuration, _writer));
		}

		/// <summary>
		/// Releases all resources used by the <see cref="BasicFileLoggerProvider"/> object.
		/// </summary>
		public void Dispose()
		{
			_loggers.Clear();
			GC.SuppressFinalize(this);
		}
	}
}

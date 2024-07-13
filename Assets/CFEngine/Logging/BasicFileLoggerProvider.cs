using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

namespace CrystalFrost.Logging
{
	public class BasicFileLoggerProvider : ILoggerProvider
	{
		private readonly IConfigurationSection _configuration;
		private readonly LogFileWriter _writer;
		private readonly ConcurrentDictionary<string, BasicFileLogger> _loggers = new(StringComparer.OrdinalIgnoreCase);


		public BasicFileLoggerProvider(IConfiguration configuration) 
		{
			_configuration = configuration.GetSection("BasicFileLogger");
			_writer = new LogFileWriter();
		}

		public ILogger CreateLogger(string categoryName)
		{
			return _loggers.GetOrAdd(categoryName, name => new BasicFileLogger(name, _configuration, _writer));
		}

		public void Dispose()
		{
			_loggers.Clear();
			GC.SuppressFinalize(this);
		}
	}
}

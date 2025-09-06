using Microsoft.Extensions.Logging;
using System;

namespace CrystalFrost.Logging
{
	/// <summary>
	/// Defines an interface for a logger that captures messages from the OpenMetaverse library.
	/// </summary>
	public interface ILMVLogger : IDisposable { }

	/// <summary>
	/// Implements a logger that captures and forwards log messages from the OpenMetaverse library.
	/// </summary>
	public class LMVLogger : ILMVLogger
	{
		private readonly ILogger<LMVLogger> _log;

		/// <summary>
		/// Initializes a new instance of the <see cref="LMVLogger"/> class.
		/// </summary>
		/// <param name="log">The logger to forward messages to.</param>
		public LMVLogger(ILogger<LMVLogger> log)
		{
			_log = log;
			OpenMetaverse.Logger.OnLogMessage += OpenMetaverseLogger_OnLogMessage;
		}

		private void OpenMetaverseLogger_OnLogMessage(object message, OpenMetaverse.Helpers.LogLevel level)
		{
			switch (level)
			{
				case OpenMetaverse.Helpers.LogLevel.Debug:
					_log.LMV_Debug((string)message);
					break;
				case OpenMetaverse.Helpers.LogLevel.Info:
					_log.LMV_Information((string)message);
					break;
				case OpenMetaverse.Helpers.LogLevel.Warning:
					_log.LMV_Warning((string)message);
					break;
				case OpenMetaverse.Helpers.LogLevel.Error:
					_log.LMV_Error((string)message);
					break;
				default:
					break;
			}
		}

		/// <summary>
		/// Releases all resources used by the <see cref="LMVLogger"/> object.
		/// </summary>
		public void Dispose()
		{
			OpenMetaverse.Logger.OnLogMessage -= OpenMetaverseLogger_OnLogMessage;
			GC.SuppressFinalize(this);
		}
	}
}

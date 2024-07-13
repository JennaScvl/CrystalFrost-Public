using Microsoft.Extensions.Logging;
using System;

namespace CrystalFrost.Logging
{
	public interface ILMVLogger : IDisposable { }

	public class LMVLogger : ILMVLogger
	{
		private readonly ILogger<LMVLogger> _log;

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

		public void Dispose()
		{
			OpenMetaverse.Logger.OnLogMessage -= OpenMetaverseLogger_OnLogMessage;
			GC.SuppressFinalize(this);
		}
	}
}

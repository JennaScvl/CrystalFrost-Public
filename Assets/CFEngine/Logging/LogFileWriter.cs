using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalFrost.Logging
{
	/// <summary>
	/// Defines an interface for a log file writer.
	/// </summary>
	public interface ILogFileWriter
	{
		/// <summary>
		/// Enqueues a log message to be written to the file.
		/// </summary>
		/// <param name="eventId">The event ID associated with the log entry.</param>
		/// <param name="level">The level of the log entry.</param>
		/// <param name="message">The log message.</param>
		void Enqeue(int eventId, LogLevel level, string message);
	}

	/// <summary>
	/// Implements a log file writer that asynchronously writes log messages to a file.
	/// </summary>
	public class LogFileWriter : ILogFileWriter
	{
		private readonly string _filename;
		private Task writerTask;
		private bool taskrunning = false;

		private static readonly SemaphoreSlim _semaphore = new(1, 1);

		private class Entry
		{
			public LogLevel Level;
			public string Message;
			public DateTime Occurred;
			public int EventId;
		}

		private readonly ConcurrentQueue<Entry> _entries = new();

		/// <summary>
		/// Initializes a new instance of the <see cref="LogFileWriter"/> class.
		/// </summary>
		public LogFileWriter()
		{
			var logDir = Path.Combine(UnityEngine.Application.persistentDataPath, "logs");
			if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);

			_filename = Path.Combine(logDir, DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".log");
		}

		private void WriterThread()
		{
			_semaphore.Wait();
			try
			{


				using var filestream = new FileStream(_filename, FileMode.Append, FileAccess.Write);
				using var streamwriter = new StreamWriter(filestream, Encoding.UTF8);

				while (_entries.TryDequeue(out Entry entry))
				{
					if (entry is null) continue;
					streamwriter.Write(entry.Occurred.ToString("s"));
					streamwriter.Write(" ");
					streamwriter.Write(LevelString(entry.Level));
					streamwriter.Write(" ");
					streamwriter.Write(entry.EventId.ToString());
					streamwriter.Write(" ");
					streamwriter.WriteLine(entry.Message);
					streamwriter.Flush();
				}
				taskrunning = false;
			}
			finally
			{
				_semaphore.Release();
			}
		}

		private static string LevelString(LogLevel level)
		{
			return level switch
			{
				LogLevel.Trace => "TRCE",
				LogLevel.Debug => "DBUG",
				LogLevel.Information => "INFO",
				LogLevel.Warning => "WARN",
				LogLevel.Error => "FAIL",
				LogLevel.Critical => "CRIT",
				_ => "????",
			};
		}

		/// <summary>
		/// Enqueues a log message to be written to the file and starts the writer task if not running.
		/// </summary>
		/// <param name="eventId">The event ID associated with the log entry.</param>
		/// <param name="level">The level of the log entry.</param>
		/// <param name="message">The log message.</param>
		public void Enqeue(int eventId, LogLevel level, string message)
		{
			_entries.Enqueue(new Entry
			{
				Occurred = DateTime.Now,
				Message = message,
				Level = level,
				EventId = eventId,
			});

			if (writerTask != null && writerTask.IsFaulted)
			{
				throw writerTask.Exception;
			}

			if (!taskrunning)
			{
				_semaphore.Wait();
				if (!taskrunning)
				{
					taskrunning = true;
					writerTask = Task.Run(WriterThread);
				}
				_semaphore.Release();
			}
		}
	}
}

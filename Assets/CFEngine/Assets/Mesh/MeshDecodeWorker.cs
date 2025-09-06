using CrystalFrost.Config;
using CrystalFrost.Lib;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace CrystalFrost.Assets.Mesh
{
	/// <summary>
	/// Defines an interface for a background worker that decodes mesh assets.
	/// </summary>
	public interface IMeshDecodeWorker : IDisposable
	{

	}

	/// <summary>
	/// A background worker that decodes downloaded mesh assets.
	/// </summary>
	public class MeshDecodeWorker : BackgroundWorker, IMeshDecodeWorker
	{
		private readonly MeshConfig _meshConfig;
		private readonly IDownloadedMeshQueue _downloadedMeshQueue;
		private readonly IDecodedMeshQueue _readyMeshQueue;
		private readonly IMeshDecoder _meshDecoder;

		/// <summary>
		/// Initializes a new instance of the <see cref="MeshDecodeWorker"/> class.
		/// </summary>
		/// <param name="log">The logger for recording messages.</param>
		/// <param name="runningIndicator">The provider for shutdown signals.</param>
		/// <param name="downloadedMeshQueue">The queue for downloaded meshes.</param>
		/// <param name="readyMeshQueue">The queue for decoded meshes.</param>
		/// <param name="meshDecoder">The mesh decoder.</param>
		/// <param name="meshConfig">The mesh configuration.</param>
		public MeshDecodeWorker(
			ILogger<MeshDecodeWorker> log,
			IProvideShutdownSignal runningIndicator,
			IDownloadedMeshQueue downloadedMeshQueue,
			IDecodedMeshQueue readyMeshQueue,
			IMeshDecoder meshDecoder,
			IOptions<MeshConfig> meshConfig)
			: base("MeshDecode", 0, log, runningIndicator)
		{
			_meshConfig = meshConfig.Value;
			_downloadedMeshQueue = downloadedMeshQueue;
			_downloadedMeshQueue.ItemEnqueued += DownloadedMeshQueue_ItemEnqueued;
			_readyMeshQueue = readyMeshQueue;
			_readyMeshQueue.ItemDequeued += ReadyMeshQueue_ItemDequeued;
			_meshDecoder = meshDecoder;
		}

		private void ReadyMeshQueue_ItemDequeued(MeshRequest obj)
		{
			CheckForWork();
		}

		private void DownloadedMeshQueue_ItemEnqueued(MeshRequest obj)
		{
			CheckForWork();
		}

		protected override Task<bool> DoWork()
		{
			return Task.Run(() => DoWorkImpl());
		}

		private bool DoWorkImpl()
		{
			if (_downloadedMeshQueue.Count == 0) return false;
			if (!_downloadedMeshQueue.TryDequeue(out var request)) return true;
			if (request is null) return true;
			// decode something
			_meshDecoder.Decode(request);
			return _downloadedMeshQueue.Count > 0;
		}

		protected override bool OutputIsBacklogged()
		{
			return _readyMeshQueue.Count > _meshConfig.MaxReadyMeshes;
		}

		public override void Dispose()
		{
			_downloadedMeshQueue.ItemEnqueued -= DownloadedMeshQueue_ItemEnqueued;
			_readyMeshQueue.ItemDequeued -= ReadyMeshQueue_ItemDequeued;
			base.Dispose();
		}
	}
}

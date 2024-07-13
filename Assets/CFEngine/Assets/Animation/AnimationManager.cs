using Microsoft.Extensions.Logging;
using OpenMetaverse;
using System;

namespace CrystalFrost.Assets.Animation
{

	public interface IAnimationManager : IDisposable
	{
		public void RequestAnimation(Primitive primitive, UUID animationId);
	}

	public class AnimationManager : IAnimationManager
	{
		private readonly ILogger<AnimationManager> _log;
		private readonly IAnimationRequestQueue _requestQueue;
		private readonly IAnimationDownloadWorker _downloadWorker;
		private readonly IAnimationDecodeWorker _decodeWorker;
		private readonly IAnimationCacheWorker _animationCache;

		public AnimationManager(ILogger<AnimationManager> log,
			IAnimationRequestQueue requestQueue,
			IAnimationDownloadWorker downloadWorker,
			IAnimationDecodeWorker decodeWorker,
			IAnimationCacheWorker animationCache)
		{
			this._log = log;
			this._requestQueue = requestQueue;
			this._downloadWorker = downloadWorker;
			this._decodeWorker = decodeWorker;
			this._animationCache = animationCache;
		}

		public void RequestAnimation(Primitive primitive, UUID animationId)
		{
			//_log.LogInformation($"Request AnimationId: {animationId}");
			AnimationRequest request = new AnimationRequest
			{
				Primitive = primitive,
				UUID = animationId
			};
			this._requestQueue.Enqueue(request);

		}

		void IDisposable.Dispose()
		{
			_animationCache.Dispose();
			_decodeWorker.Dispose();
			_downloadWorker.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}


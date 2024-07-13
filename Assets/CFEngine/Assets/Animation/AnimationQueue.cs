using CrystalFrost.Lib;

namespace CrystalFrost.Assets.Animation
{
	/// <summary>
	/// Represents a queue of decoded Animations that are ready for
	/// the main thread to pick up and read into Graphics memory.
	/// </summary>
	public interface IDecodedAnimationQueue : IConcurrentQueue<AnimationRequest> { }

	public class DecodedAnimationQueue : AbstractedConcurrentQueue<AnimationRequest>, IDecodedAnimationQueue { }

	/// <summary>
	/// A queue for Animations that have been download but not cached.
	/// </summary>
	public interface IDownloadedAnimationCacheQueue : IConcurrentQueue<AnimationRequest> { }

	public class DownloadedAnimationCacheQueue : AbstractedConcurrentQueue<AnimationRequest>, IDownloadedAnimationCacheQueue { }


	/// <summary>
	/// A queue for Animations that have been dowloaded, but have not been converted.
	/// </summary>
	public interface IDownloadedAnimationQueue : IConcurrentQueue<AnimationRequest> { }

	public class DownloadedAnimationQueue : AbstractedConcurrentQueue<AnimationRequest>, IDownloadedAnimationQueue { }

	/// <summary>
	/// A queue for Animations that have not been downloaded.
	/// </summary>
	public interface IAnimationDownloadRequestQueue : IConcurrentQueue<AnimationRequest> { }

	public class AnimationDownloadRequestQueue : AbstractedConcurrentQueue<AnimationRequest>, IAnimationDownloadRequestQueue { }

	/// <summary>
	/// A queue for Animations that have been requested (from cache or downloads)
	/// </summary>
	public interface IAnimationRequestQueue : IConcurrentQueue<AnimationRequest> { }

	public class AnimationRequestQueue : AbstractedConcurrentQueue<AnimationRequest>, IAnimationRequestQueue { }

}
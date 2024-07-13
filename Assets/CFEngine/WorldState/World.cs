using Microsoft.Extensions.Logging;

namespace CrystalFrost.WorldState
{
	/// <summary>
	/// A Root Object for the simulation data.
	/// </summary>
	public interface IWorld
	{
		/// <summary>
		/// A collection of all known objects.
		/// </summary>
		IAllSimObject AllObjects { get; }

		/// <summary>
		///  A collection of known (nearyby?) regions.
		/// </summary>
		IAllRegions Regions { get; }
	}

	public class World : IWorld
	{
		private readonly ILogger _log;

		public IAllSimObject AllObjects { get; private set; }
		public IAllRegions Regions { get; private set; }

		public World(ILogger<World> log,
			IAllSimObject worldObjects,
			IAllRegions regions)
		{
			_log = log;
			AllObjects = worldObjects;
			Regions = regions;
		}
	}
}

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

		/// <summary>
		/// Initializes a new instance of the <see cref="World"/> class.
		/// </summary>
		/// <param name="log">The logger for recording messages.</param>
		/// <param name="worldObjects">The collection of all simulation objects.</param>
		/// <param name="regions">The collection of all regions.</param>
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

using Microsoft.Extensions.Logging;
using OpenMetaverse;
using System.Collections.Concurrent;

namespace CrystalFrost.WorldState
{
	public interface IAllRegions
	{
		/// <summary>
		/// Creates or returns the Region Object that is related to a Simulator
		/// </summary>
		/// <param name="simulator">A Simulator, typicalle an update event argument.</param>
		/// <returns>The region in the world model that is related to the simulator </returns>
		Region AddOrUpdate(Simulator simulator);

		/// <summary>
		/// Gets an existing region, or returns null
		/// </summary>
		/// <param name="regionID"></param>
		/// <returns></returns>
		Region GetOrDefault(UUID regionID);
	}

	public class AllRegions : IAllRegions
	{
		private ConcurrentDictionary<UUID, Region> _regions = new();
		private ILogger<AllRegions> _log;

		public AllRegions(ILogger<AllRegions> log)
		{
			_log = log;
		}

		public Region AddOrUpdate(Simulator simulator)
		{
			return _regions.AddOrUpdate(
				simulator.ID,
				(id) => NewRegionFromSimulator(simulator),
				(id, existing) => UpdateRegionFromSimulator(existing, simulator));
		}

		public Region GetOrDefault(UUID regionID)
		{
			return _regions.ContainsKey(regionID) ? _regions[regionID] : null;
		}

		private Region UpdateRegionFromSimulator(Region existing, Simulator simulator)
		{
			// I wonder how often this actually happens?
			existing.Name = simulator.Name;

			// add more interesting things about the region here.
			return existing;
		}

		private Region NewRegionFromSimulator(Simulator simulator)
		{
			_log.NewRegion(simulator.RegionID, simulator.Name);
			return new Region
			{
				RegionId = simulator.RegionID,
				Name = simulator.Name,
				Handle = simulator.Handle,
				// add more interesting things about the region here.
			};
		}
	}
}

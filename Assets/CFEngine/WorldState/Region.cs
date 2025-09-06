using OpenMetaverse;
using System.Collections.Generic;

namespace CrystalFrost.WorldState
{
	/// <summary>
	/// Represents a region in the virtual world, containing its properties and objects.
	/// </summary>
	public class Region
	{
		/// <summary>
		/// The name of the region.
		/// </summary>
		public string Name;
		/// <summary>
		/// The unique identifier of the region.
		/// </summary>
		public UUID RegionId;
		/// <summary>
		/// A list of simulation objects within this region.
		/// </summary>
		public List<SimObject> Objects = new();
		/// <summary>
		/// The handle of the region.
		/// </summary>
		public ulong Handle;
	}
}

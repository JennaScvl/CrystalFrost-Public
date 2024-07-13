using OpenMetaverse;
using System.Collections.Generic;

namespace CrystalFrost.WorldState
{
	public class Region
	{
		public string Name;
		public UUID RegionId;
		public List<SimObject> Objects = new();
		public ulong Handle;
	}
}

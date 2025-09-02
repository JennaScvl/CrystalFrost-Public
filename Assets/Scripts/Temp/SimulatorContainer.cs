using OpenMetaverse;
using UnityEngine;

namespace Temp
{
	/// <summary>
	/// Keeps pointers, terrain, and details of individual regions.
	/// Takes the events from the region and updates terrain, objects, and avatars.
	/// </summary>
	public class SimulatorContainer
	{
		// Handle to the OpenMetaverse Simulator object
		public Simulator sim;

		// Terrain that is built an managed for this region. Includes building mesh and splats
		public SimTerrain terrain;

		// Region's location in the simulator grid
		public uint x;
		public uint y;

		// Region size in meters
		public uint sizeX;
		public uint sizeY;

		public float age = 0f;

		// The region's graphics location in the scene
		public Transform transform
		{
			get { return terrain.Transform; }
		}

		public SimulatorContainer(Simulator sim, uint x, uint y)
		{
			this.sim = sim;
			this.sizeX = sim.SizeX;
			this.sizeY = sim.SizeY;
			this.x = x;
			this.y = y;

			terrain = new SimTerrainTiles(sim);
			/*  Used to use heightmap for smaller regions, but Tiles is better for both
            if (sizeX > 256 || sizeY > 256)
            {
                terrain = new SimTerrainTiles(sim);
            }
            else
            {
                terrain = new SimTerrainHeightMap(sim);
            }
            */
			Debug.Log($"SimulatorContainer: creation: name={sim.Name}, loc={x}/{y}, sz={sizeX}/{sizeY}");
		}
	}

}
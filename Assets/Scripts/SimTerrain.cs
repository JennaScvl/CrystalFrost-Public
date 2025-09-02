using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Temp;
using UnityEngine;

using OMV = OpenMetaverse;

interface ISimTerrain
{
    Transform Transform { get; }
    public bool is256 { get; }
    public bool isModified { get; }
    public float MinHeight { get; }
    public float MaxHeight { get; }
    public float SizeX { get; }
    public float SizeY { get; }
    // Called with terrain height updates from the simulator
    public void TerrainPatchUpdate(OMV.LandPatchReceivedEventArgs pLandPatchEvent);
    // Called once a frame to update the terrain
    public void TerrainUpdate(SimulatorContainer pSimContainer);
    // Set the graphics position of the terrain
    public void SetTerrainRelativePosition(OMV.Simulator pSim);

}

/// <summary>
/// Base class for terrain implementations.
/// Mostly holds properties that are common to the various terrain implementations.
/// </summary>
public abstract class SimTerrain : ISimTerrain
{

    protected OMV.Simulator _sim;

    // Return the graphics location of the terrain
    public abstract Transform Transform { get; }
    
    // Whether the terrain is a default 256x256 terrain
    public bool is256 { get; protected set; } = false;
    // Whether the terrain has been modified
    public bool isModified { get; protected set; } = false;

    public float MinHeight { get; private set; }
    public float MaxHeight { get; private set; }

    public float SizeX { get { return _sim.SizeX; } }
    public float SizeY { get { return _sim.SizeY; } }

    public SimTerrain(OMV.Simulator pSim)
    {
        _sim = pSim;

        // public flag saying if this is a default 256x256 terrain
        is256 = _sim.SizeX == 256 && _sim.SizeY == 256;

    }

    public abstract void SetTerrainRelativePosition(OMV.Simulator pSim);

    public abstract void TerrainPatchUpdate(OMV.LandPatchReceivedEventArgs pLandPatchEvent);

    public abstract void TerrainUpdate(SimulatorContainer pSimContainer);

    // Handle an edit terrain height message from the simulator.
	public void EditTerrainHeight(uint handle, int absX, int absY, float[,] heightData)
	{
        // TODO: OLD code that needs to be updated.
        /*
		if (!terrainDictionary.ContainsKey(handle))
		{
			Debug.LogWarning($"Handle {handle} not found in the terrain dictionary.");
			return;
		}

		int terrainX = (int)(absX * 0.00390625f);
		int terrainY = (int)(absY * 0.00390625f);
		int localX = absX & 255;
		int localY = absY & 255;

		Terrain[,] terrains = terrainDictionary[handle];
		Terrain terrain = terrains[terrainX, terrainY];
		TerrainData terrainData = terrain.terrainData;

		terrainData.SetHeights(localX, localY, heightData);

		// Check edges and sync accordingly
		if (localX == 0 || localY == 0 || localX + 16 == 256 || localY + 16 == 256)
		{
			CloneEdgeHeight(terrain);
			SyncMainEdgeHeight(terrain);
		}
        */
	}

    /*
	public ulong GetNorth(ulong handle) => (handle & 0xFFFFFFFF00000000) | ((uint)handle + 256);
	public ulong GetSouth(ulong handle) => (handle & 0xFFFFFFFF00000000) | ((uint)handle - 256);
	public ulong GetEast(ulong handle) => (((ulong)((uint)(handle >> 32) + 256)) << 32) | (uint)handle;
	public ulong GetWest(ulong handle) => (((ulong)((uint)(handle >> 32) - 256)) << 32) | (uint)handle;
	private void StitchTerrains()
	{
		foreach (KeyValuePair<ulong, SimManager.SimulatorContainer> kvp in ClientManager.simManager.simulators)
		{
			Terrain northTerrain = null;
			Terrain eastTerrain = null;
			Terrain southTerrain = null;
			Terrain westTerrain = null;

			if (ClientManager.simManager.simulators.ContainsKey(GetNorth(kvp.Value.sim.Handle)))
			{
				Debug.Log($"TERRAIN: {kvp.Value.sim.Handle} has North neighbor");
				northTerrain = ClientManager.simManager.simulators[GetNorth(kvp.Value.sim.Handle)].terrain;
				northTerrain.SetNeighbors(northTerrain.leftNeighbor, northTerrain.topNeighbor, northTerrain.rightNeighbor, ClientManager.simManager.simulators[kvp.Value.sim.Handle].terrain);
				//northTerrain.terrainData.SyncHeightmap();
			}
			if (ClientManager.simManager.simulators.ContainsKey(GetEast(kvp.Value.sim.Handle)))
			{
				Debug.Log($"TERRAIN: {kvp.Value.sim.Handle} has East neighbor");
				eastTerrain = ClientManager.simManager.simulators[GetEast(kvp.Value.sim.Handle)].terrain;
				eastTerrain.SetNeighbors(ClientManager.simManager.simulators[kvp.Value.sim.Handle].terrain, eastTerrain.topNeighbor, eastTerrain.rightNeighbor, eastTerrain.bottomNeighbor);
				//eastTerrain.terrainData.SyncHeightmap();
			}
			if (ClientManager.simManager.simulators.ContainsKey(GetSouth(kvp.Value.sim.Handle)))
			{
				Debug.Log($"TERRAIN: {kvp.Value.sim.Handle} has South neighbor");
				southTerrain = ClientManager.simManager.simulators[GetSouth(kvp.Value.sim.Handle)].terrain;
				southTerrain.SetNeighbors(southTerrain.leftNeighbor, ClientManager.simManager.simulators[kvp.Value.sim.Handle].terrain, southTerrain.rightNeighbor, southTerrain.bottomNeighbor);
				//southTerrain.terrainData.SyncHeightmap();
			}
			if (ClientManager.simManager.simulators.ContainsKey(GetWest(kvp.Value.sim.Handle)))
			{
				Debug.Log($"TERRAIN: {kvp.Value.sim.Handle} has West neighbor");
				westTerrain = ClientManager.simManager.simulators[GetWest(kvp.Value.sim.Handle)].terrain;
				westTerrain.SetNeighbors(westTerrain.leftNeighbor, westTerrain.topNeighbor, ClientManager.simManager.simulators[kvp.Value.sim.Handle].terrain, westTerrain.bottomNeighbor);
				//westTerrain.terrainData.SyncHeightmap();
			}

			ClientManager.simManager.simulators[kvp.Value.sim.Handle].terrain.SetNeighbors(westTerrain, northTerrain, eastTerrain, southTerrain);
			CloneEdgeHeight(ClientManager.simManager.simulators[kvp.Value.sim.Handle].terrain);
			SyncMainEdgeHeight(ClientManager.simManager.simulators[kvp.Value.sim.Handle].terrain);
		}
	}

	private void SyncEdgeHeight(Terrain mainTerrain)
	{
		int resolution = mainTerrain.terrainData.heightmapResolution;
		float[,] mainHeights = mainTerrain.terrainData.GetHeights(0, 0, resolution, resolution);

		// Sync left neighbor
		Terrain leftNeighbor = mainTerrain.leftNeighbor;
		if (leftNeighbor != null)
		{
			float[,] neighborHeights = leftNeighbor.terrainData.GetHeights(0, 0, resolution, resolution);
			for (int y = 0; y < resolution; y++)
			{
				neighborHeights[y, resolution - 1] = mainHeights[y, 0];
			}
			leftNeighbor.terrainData.SetHeights(0, 0, neighborHeights);
			leftNeighbor.terrainData.SyncHeightmap();
		}

		// Sync bottom neighbor
		Terrain bottomNeighbor = mainTerrain.bottomNeighbor;
		if (bottomNeighbor != null)
		{
			float[,] neighborHeights = bottomNeighbor.terrainData.GetHeights(0, 0, resolution, resolution);
			for (int x = 0; x < resolution; x++)
			{
				neighborHeights[resolution - 1, x] = mainHeights[0, x];
			}
			bottomNeighbor.terrainData.SetHeights(0, 0, neighborHeights);
			bottomNeighbor.terrainData.SyncHeightmap();
		}
	}
    */

	private void CloneEdgeHeight(Terrain mainTerrain)
	{
		int resolution = mainTerrain.terrainData.heightmapResolution;
		float[,] heights = mainTerrain.terrainData.GetHeights(0, 0, resolution, resolution);

		// Copy the right-1 column to the right edge
		for (int y = 0; y < resolution; y++)
		{
			heights[y, resolution - 1] = heights[y, resolution - 2];
		}

		// Copy the top-1 row to the top edge
		for (int x = 0; x < resolution; x++)
		{
			heights[resolution - 1, x] = heights[resolution - 2, x];
		}

		mainTerrain.terrainData.SetHeights(0, 0, heights);
	}

	private void SyncMainEdgeHeight(Terrain mainTerrain)
	{
		int resolution = mainTerrain.terrainData.heightmapResolution;
		float[,] mainHeights = mainTerrain.terrainData.GetHeights(0, 0, resolution, resolution);

		// Sync with bottom neighbor
		Terrain bottomNeighbor = mainTerrain.bottomNeighbor;
		if (bottomNeighbor != null)
		{
			float[,] neighborHeights = bottomNeighbor.terrainData.GetHeights(0, 0, resolution, resolution);
			for (int x = 0; x < resolution; x++)
			{
				mainHeights[0, x] = neighborHeights[resolution - 1, x]; // Bottom edge of main conforming to top edge of neighbor
			}
		}

		// Sync with left neighbor
		Terrain leftNeighbor = mainTerrain.leftNeighbor;
		if (leftNeighbor != null)
		{
			float[,] neighborHeights = leftNeighbor.terrainData.GetHeights(0, 0, resolution, resolution);
			for (int y = 0; y < resolution; y++)
			{
				mainHeights[y, 0] = neighborHeights[y, resolution - 1]; // Left edge of main conforming to right edge of neighbor
			}
		}

		mainTerrain.terrainData.SetHeights(0, 0, mainHeights);
		mainTerrain.terrainData.SyncHeightmap();
	}

	public float QuadLerp(float v00, float v01, float v10, float v11, float xPercent, float yPercent)
	{
		//float abu = Mathf.Lerp(a, b, u);
		//float dcu = Mathf.Lerp(d, c, u);
		//return Mathf.Lerp(abu, dcu, v);
		return Mathf.Lerp(Mathf.Lerp(v00, v01, xPercent), Mathf.Lerp(v10, v11, xPercent), yPercent);

	}

	float PerlinTurbulence2(OMV.Vector2 v, float freq)
	{
		float t;
		OMV.Vector2 vec;

		for (t = 0; freq >= 1; freq *= 0.5f)
		{
			vec.X = freq * v.X;
			vec.Y = freq * v.Y;
			t += Mathf.PerlinNoise(vec.X, vec.Y) / freq;
		}
		return t;
	}



    /*
    private void OLDCODE(OMV.SimConnectedEventArgs e, float x, float y, float relx, float rely)
    {

        if(e.Simulator.SizeX > 256 || e.Simulator.SizeY > 256)
        {
            //ClientManager.client.Terrain.LandPatchReceived -= new EventHandler<LandPatchReceivedEventArgs>(ClientManager.simManager.TerrainEventHandler);
            if (ClientManager.simManager.simulators.TryAdd(e.Simulator.Handle, new SimManager.SimulatorContainer(e.Simulator, x, y)))
            {
                ClientManager.simManager.simulators[e.Simulator.Handle].material.SetTexture("_MainTex", (Texture2D)ClientManager.assetManager.RequestTexture(e.Simulator.TerrainDetail0));
                ClientManager.simManager.simulators[e.Simulator.Handle].material.SetTexture("_Splat2", (Texture2D)ClientManager.assetManager.RequestTexture(e.Simulator.TerrainDetail1));
                ClientManager.simManager.simulators[e.Simulator.Handle].material.SetTexture("_Splat3", (Texture2D)ClientManager.assetManager.RequestTexture(e.Simulator.TerrainDetail2));
                ClientManager.simManager.simulators[e.Simulator.Handle].material.SetTexture("_Splat4", (Texture2D)ClientManager.assetManager.RequestTexture(e.Simulator.TerrainDetail3));
                //ClientManager.simManager.simulators[e.Simulator.Handle].material.SetTexture("_SplatMap", );
            }
            return;
        }

        // Create new TerrainData
        TerrainData terrainData = new TerrainData();
        terrainData.heightmapResolution = (int)e.Simulator.SizeX;//terrainPrefab.terrainData.heightmapResolution; // or your desired resolution
        terrainData.alphamapResolution = (int)e.Simulator.SizeY;
        uint sizeX = e.Simulator.SizeX;
        uint sizeY = e.Simulator.SizeY;
        //sizeX = e.Simulator.
        terrainData.size = new UnityEngine.Vector3((float)sizeX, 256, (float)sizeY);

        TerrainLayer[] terrainLayers = new TerrainLayer[4];
        for (int i = 0; i < 4; i++)
        {
            terrainLayers[i] = new TerrainLayer();
            terrainLayers[i].tileSize = new UnityEngine.Vector2(12, 12);
            // Configure the layer, e.g., terrainLayers[i].diffuseTexture = someTexture;
        }
        terrainData.terrainLayers = terrainLayers;
        // Create the Terrain GameObject
        GameObject terrainObject = Terrain.CreateTerrainGameObject(terrainData);

        // Optionally, set the position, parent, or any other properties
        terrainObject.transform.position = new UnityEngine.Vector3(relx, 0f, rely);
        Terrain terrain = terrainObject.GetComponent<Terrain>();

        //terrain.terrainData.



        ClientManager.simManager.simulators.TryAdd(e.Simulator.Handle, new SimManager.SimulatorContainer(e.Simulator, terrain, x, y));


        terrain = ClientManager.simManager.simulators[e.Simulator.Handle].terrain;
        terrain.name = $"Simulator: {e.Simulator.Name}";
        terrain.terrainData.terrainLayers[0].diffuseTexture = (Texture2D)ClientManager.assetManager.RequestTexture(e.Simulator.TerrainDetail0);
        terrain.terrainData.terrainLayers[1].diffuseTexture = (Texture2D)ClientManager.assetManager.RequestTexture(e.Simulator.TerrainDetail1);
        terrain.terrainData.terrainLayers[2].diffuseTexture = (Texture2D)ClientManager.assetManager.RequestTexture(e.Simulator.TerrainDetail2);
        terrain.terrainData.terrainLayers[3].diffuseTexture = (Texture2D)ClientManager.assetManager.RequestTexture(e.Simulator.TerrainDetail3);
    }
        */

		// Assuming you have cached meshFilters in an array
		//MeshFilter[,] meshFilters;


    /*
	public Dictionary<uint, Terrain[,]> terrainDictionary = new Dictionary<uint, Terrain[,]>();
	public Terrain[,] GetTerrainGrid(uint handle)
	{
		if (terrainDictionary.ContainsKey(handle))
		{
			return terrainDictionary[handle];
		}
		else
		{
			Debug.LogWarning($"Handle {handle} not found in the terrain dictionary.");
			return null;
		}
	}
    */
}

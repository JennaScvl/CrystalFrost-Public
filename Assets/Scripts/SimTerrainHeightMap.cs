using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;

using OMV = OpenMetaverse;
using UnityEngine;
using System.Text;



/// <summary>
/// All the code to manage the terrain for a simulator.
/// An instance of this class is created when the SimulatorContainer is created.
/// It initially creates a default terrain but then accepts the terrain
/// update packets from the simulator and updates the terrain accordingly.
/// 
/// Terrain heights are updated on receiving TerrainPatchUpdate messages which
/// give updates for 16x16 meter areas. These are queued up and processed
/// between frames.
/// 
/// The terrain texture (splat) is generated once all the terrain heights have
/// been received. This is done by calling the TerrainApplyTerrainSplatting.
/// 
/// So, there are several optimizations done. If there terrain is default size
/// (256x256 denoted by .is256 being true) then the Unity mesh is updated as 
/// the terrain heights are received. If the terrain is not default size then
/// the Unity mesh is not updated until all the terrain heights have been received.
/// 
/// </summary>

public class SimTerrainHeightMap : SimTerrain
{
    // The Unity terrain object that is built from the terrain info from the simulator
    private Terrain UnityTerrain;
    public override Transform Transform { get { return UnityTerrain.transform; } }

    // The terrain heights kept to set with TerrainData.SetHeights().
    // This means values are 0.0..1.0 and are scaled by the heightmapResolution.
    private float[,] _heights;

    // variables used to control the processing of terrain updates and splat generation
    protected bool isComplete = false;
    protected float age = 0f;
    // If the terrain texture has been generated
    protected bool isSplatGenerated = false;

	public SimTerrainHeightMap(OMV.Simulator pSim) : base(pSim)
	{
        _heights = new float[(int)_sim.SizeX, (int)_sim.SizeY];

        // Create a flat terrain for the region.
        CreateDefaultTerrain();

        // Forces generation of terrain texture (splat) and update of Unity terrain mesh
        isSplatGenerated = false;
        isModified = true;

        Debug.Log($"SimTerrain: creation: {_sim.Name} {_sim.SizeX}x{_sim.SizeY}, is256={is256}");
	}

    /// <summary>
    /// Creates a flat terrain for the region.
    /// </summary>
    public const int TERRAIN_HEIGHT_DEFAULT = 25;
    public const int TERRAIN_HEIGHT_MIN = 0;
    public const int TERRAIN_HEIGHT_MAX = 4096;
    private void CreateDefaultTerrain()
    {

		// Create a Unity terrain with the default size and height.
        // TerrainData is an odd cat in that the terrain heights are actually
        // stored as 16 bit ints that are scaled in-world by the heightmapResolution.
        // Since OpenSim terrain heights are floats from 0 to TERRAIN_HEIGHT_MAX, the
        // entries must be scaled.
        // This is complicated by TerrainData.SetHeights() taking a float[,] array containing
        // values 0.0..1.0. So scaling is done in two places.
        int terrainResolution = (int)_sim.SizeX; // assuming the region is square
		TerrainData terrainData = new TerrainData
		{
			heightmapResolution = terrainResolution,
            alphamapResolution = terrainResolution,
			size = new UnityEngine.Vector3((float)_sim.SizeX, TERRAIN_HEIGHT_MAX, (float)_sim.SizeY)
		};

		TerrainLayer[] terrainLayers = new TerrainLayer[4];
        OMV.UUID[] layerTextureNames = new OMV.UUID[] { _sim.TerrainDetail0, _sim.TerrainDetail1, _sim.TerrainDetail2, _sim.TerrainDetail3 };
        for (int i = 0; i < 4; i++)
        {
            var layer = new TerrainLayer
            {
                tileSize = new UnityEngine.Vector2(12, 12),
                diffuseTexture = (Texture2D)ClientManager.assetManager.RequestTexture(layerTextureNames[i])
            };
            terrainLayers[i] = layer;
        }
        terrainData.terrainLayers = terrainLayers;
        // Create the Terrain GameObject
        GameObject terrainObject = Terrain.CreateTerrainGameObject(terrainData);

        UnityTerrain = terrainObject.GetComponent<Terrain>();
        UnityTerrain.name = $"Simulator: {_sim.Name}";

        SetTerrainRelativePosition(_sim);

        // Set the height array to the default height
        const float scaledHeight = TERRAIN_HEIGHT_DEFAULT / TERRAIN_HEIGHT_MAX;
        for (int y = 0; y < _sim.SizeY; y++)
        {
            for (int x = 0; x < _sim.SizeX; x++)
            {
                _heights[x, y] = scaledHeight;
            }
        }   
    }

    /// <summary>
    /// Displayed terrains get a transform that is relative to the main simulator.
    /// This code gets the current sim and calculates the relative position of this terrain.
    /// </summary>
    public override void SetTerrainRelativePosition(OMV.Simulator pSim) {
        // The terrain positions is set relative to the simulator origin
        OMV.Utils.LongToUInts(ClientManager.client.Network.CurrentSim.Handle, out uint _x, out uint _y);
        OMV.Utils.LongToUInts(pSim.Handle, out uint x, out uint y);
        uint relx = x - _x;
        uint rely = y - _y;

        UnityTerrain.transform.position = new UnityEngine.Vector3(relx, 0f, rely);
    }


    /// <summary>
    /// Received a terrain update from the simulator.
    /// These come in a bunch when a simulator starts so the might need to
    /// be queued up have processing spread out.
    /// </summary>
	public ConcurrentQueue<OMV.LandPatchReceivedEventArgs> terrainPatchUpdates = new();

    /// <summary>
    /// Have received a terrain land patch update (for a 16x16 area).
    /// Queue the update to a work queue and process between frames.
    /// </summary>
    /// <param name="pLandPatchEvent"></param>
    public override void TerrainPatchUpdate(OMV.LandPatchReceivedEventArgs pLandPatchEvent)
    {
        // Debug.Log($"TerrainPatchUpdate: {pLandPatchEvent.Simulator.Name} {pLandPatchEvent.X},{pLandPatchEvent.Y}");
		terrainPatchUpdates.Enqueue(pLandPatchEvent);
    }

    /// <summary>
    /// Called once per frame to do any updates to the terrain.
    /// Note that this is called outside the context of an enclosing SimTerrain
    /// so the simulator passed is the one used.
    /// </summary>
	public override void TerrainUpdate(SimManager.SimulatorContainer pSimContainer)
	{
        // Debug.Log($"SimTerrainHeightMap.TerrainUpdate: {pSimContainer.sim.Name}");
        SimTerrainHeightMap terrain = pSimContainer.terrain as SimTerrainHeightMap;

        if (terrain == null)
        {
            // The terrain didn't change to a height map terrain. It's not us.
            // Debug.Log($"SimTerrainHeightMap.TerrainUpdate: terrain is null");
            return;
        }

        if (terrainPatchUpdates.TryDequeue(out OMV.LandPatchReceivedEventArgs patchUpdate))
        {
            // Since the terrain is being updated, remember that the terrain texture needs regen
            isSplatGenerated = false;
            isModified = true;
            SetHeightMapHeights(patchUpdate.X, patchUpdate.Y, patchUpdate.HeightMap);
            terrain.UnityTerrain.terrainData.SetHeights(0, 0, terrain._heights);
            terrain.UnityTerrain.terrainData.SyncHeightmap();
        }
        else
        {
            // No update to process. Do the terrain texture generation if needed.
            if (!isSplatGenerated)
            {
                // If the terrain is modified and the splat is not generated then generate the splat
                // Debug.Log($"TerrainUpdate: {pSimContainer.sim.Name} splat");
                TerrainApplyTerrainSplatting(pSimContainer);
                isSplatGenerated = true;
            }
            isModified = false;
            return;
        }

        // ============================================================
        /*

		terrainPatchUpdates.TryDequeue(out e);
		if (e == null)
		{
            //Debug.Log($"Terrain: {kvp.Value.sim.Name} {kvp.Value.age}");
            if(terrain.age >= 10f && !terrain.isComplete)
            {
                if (!terrain.is256)
                {
                    //kvp.Value.terrain.terrainData.SetHeights(0, 0, kvp.Value.heights);
                    TerrainApplyTerrainSplatting(pSimContainer);
                    terrain.isComplete = true;
                }
                else
                {
                    // This causes the unity terrain mesh to be built with all the updates
                    terrain.UnityTerrain.terrainData.SetHeights(0, 0, terrain.heights);
                    // Create the texture for the terrain
                    TerrainApplyTerrainSplatting(pSimContainer);
                    terrain.isComplete = true;
                }
            }
		}
		else
		{
			ulong handle = e.Simulator.Handle;
			if (!terrain.is256)
			{
				terrain.UpdateVertexHeights(e.X * 16, e.Y * 16, e.HeightMap);
				if (terrain.isComplete)TerrainApplyTerrainSplatting(pSimContainer);
				return;
			}
			//simulators[e.Simulator.Handle].age >= 5f
			if (!terrain.isComplete)
			{
				Debug.Log($"Terrain {e.Simulator.Name}: COMPLETE");
				terrain.UnityTerrain.terrainData.SetHeights(0, 0, terrain.heights);
				terrain.UnityTerrain.terrainData.SyncHeightmap();
				terrain.isComplete = true;

			}
			else
			{
				float[,] terrainHeight = new float[16, 16];
				//float[,,] terrainSplats = new float[16, 16, 4];
				int i, j, x, y;
				x = (e.X * 16);
				y = (e.Y * 16);

				float height;
				for (j = 0; j < 16; j++)
				{
					for (i = 0; i < 16; i++)
					{
						height = (e.HeightMap[j * 16 + i]);
						terrainHeight[j, i] = height * 0.0039215686274509803921568627451f;


					}
				}

				TerrainApplyTerrainSplatting(pSimContainer);
				pSimContainer.terrain.UnityTerrain.terrainData.SetHeightsDelayLOD(x, y, terrainHeight);
				pSimContainer.terrain.UnityTerrain.terrainData.SyncHeightmap();
				// simContainer.updated = true;

				// if (e.X == 0 || e.Y == 0 || e.X + 16 >= (e.Simulator.SizeX - 1) || e.Y + 16 >= (e.Simulator.SizeX - 2))
				// {
				// 	stitch = true;
				// }
			}
		}
            */

	}


    private void SetHeightMapHeights(int x, int y, float[] newheights)
    {
        age = 0f;
        x *= 16;
        y *= 16;
        //Debug.Log($"Terrain SetHeights: {x},{y}");
        for (int j = 0; j < 16; j++)
        {
            for (int i = 0; i < 16; i++)
            {
                // float height = (newheights[j * 16 + i]) * 0.0039215686274509803921568627451f;
                float height = (newheights[j * 16 + i]);
                _heights[y + j, x + i] = height / TERRAIN_HEIGHT_MAX;
            }
        }
    }

    /*
	private Vector2 _vec = new Vector2();
	private float[,] _splatValues;

	public static void TerrainApplyTerrainSplatting(SimManager.SimContainer pSimContainer)
	{
		Debug.Log($"Applying Terrain Splats {simulator.Name}");
		TerrainData terrainData = terrain.terrainData;
		int resolution = terrainData.heightmapResolution;

		if (_splatValues == null || _splatValues.GetLength(0) != resolution || _splatValues.GetLength(1) != resolution)
		{
			_splatValues = new float[resolution, resolution];
		}

		int i, j, x, y;

		float[,] heights = simulators[simulator.Handle].heights;//terrainData.GetHeights(0, 0, resolution, resolution);

		//float[,] terrainHeight = new float[16, 16];
		float[,,] terrainSplats = new float[simulator.SizeX, simulator.SizeY, 4];

		float swLow = simulator.TerrainStartHeight00;
		float swHigh = simulator.TerrainHeightRange00;
		float nwLow = simulator.TerrainStartHeight01;
		float nwHigh = simulator.TerrainHeightRange01;
		float seLow = simulator.TerrainStartHeight10;
		float seHigh = simulator.TerrainHeightRange10;
		float neLow = simulator.TerrainStartHeight11;
		float neHigh = simulator.TerrainHeightRange11;
		float interpolated_start_height = 0;
		float interpolated_height_range = 0;
		//Vector2 vec;
		float low_freq;
		float high_freq;
		//float noise;

		float value;

		float verticalblend;
		float dist;
		float modheight;
		int RegionSize = 256; //this will need to change for varregions
		int diff = 256;//simulators[e.Simulator.Handle].terrain.terrainData.hei.GetLength(0) / RegionSize;

		float noisemult = .2222222f;
		float turbmult = 2f;

		for (y = 0; y < simulator.SizeY; y++)
		{
			for (x = 0; x < simulator.SizeX; x++)
			{
				float height = heights[x, y]*256;
				Debug.Log($"Terrain Splat Height: {x},{y} = {height}");
				float globalXpercent = x * 0.0039215686274509803921568627451f;
				float globalYpercent = y * 0.0039215686274509803921568627451f;

				Vector2 global_position = new Vector2(x,y);
				//Debug.Log($"TERRAINSPLAT: e.X={global_position}");
				// vec = global_position * 0.20319f;
				// low_freq = perlin_noise2(vec.x * 0.222222f, vec.y * 0.222222f) * 6.5f;
				// high_freq = perlin_turbulence2(vec.x, vec.y, 2) * 2.25f;
				// noise = (low_freq + high_freq) * 2;

				int newX = Mathf.FloorToInt((float)x * 0.0039215686274509803921568627451f);/// diff;
				int newY = Mathf.FloorToInt((float)y * 0.0039215686274509803921568627451f);/// diff;

				Vector3 vec = new Vector3
				(
					newX * 0.20319f,
					newY * 0.20319f,
					height * 0.25f
				);

				float lowFreq = Mathf.PerlinNoise(global_position.x * noisemult, global_position.y * noisemult) * 6.5f;
				float highFreq = perlin_turbulence2(global_position.x, global_position.y, turbmult) * 2.25f;//Perlin.turbulence2(vec.x, vec.y, 2f) * 2.25f;
																											//Debug.Log($"TERRAINSPLAT: noise lowFreq={lowFreq} highFreq={highFreq}");
				float noise = (lowFreq + highFreq) * 2f;
				//Debug.Log($"TERRAINSPLAT: noise={noise}");
				//interpolated_start_height = QuadLerp(swLow, nwLow, seLow, neLow, globalYpercent, globalXpercent);
				//interpolated_height_range = QuadLerp(swHigh, nwHigh, seHigh, neHigh, globalYpercent, globalXpercent);
				interpolated_start_height = Bilinear(
					simulator.TerrainStartHeight00,
					simulator.TerrainStartHeight01,
					simulator.TerrainStartHeight10,
					simulator.TerrainStartHeight11,
					global_position.x, global_position.y);
				//interpolated_start_height = Mathf.Clamp(interpolated_start_height, 0f, 255f);
				interpolated_height_range = Bilinear(
					simulator.TerrainHeightRange00,
					simulator.TerrainHeightRange01,
					simulator.TerrainHeightRange10,
					simulator.TerrainHeightRange11,
					global_position.x, global_position.y);

				float startHeight = interpolated_start_height;
				float heightRange = interpolated_height_range;
				//interpolated_height_range = Mathf.Clamp(interpolated_height_range, 0f, 255f);

				value = Mathf.Clamp((height + noise - interpolated_start_height) * 4 / interpolated_height_range, 0, 3f);
				//modheight = height + value;//* value;

				dist = (interpolated_height_range - interpolated_start_height);

				verticalblend = ((value - interpolated_start_height) / dist);
				float layer = ((height + noise - startHeight) / heightRange) * 4f;

				if (Single.IsNaN(layer))
					layer = 0f;
				layer = Mathf.Clamp(layer, 0f, 3f);

				int layeri = Mathf.CeilToInt(layer);
				layeri = Mathf.RoundToInt(value);


				terrainSplats[x, y, layeri] = 1f - (layer - layeri);
				if (layeri < 3) terrainSplats[x, y, layeri + 1] = (layer - layeri);

			}
		}

		terrain.terrainData.SetAlphamaps(0,0, terrainSplats);

		// Continue with the application of splat values as texture weights, or use them as needed
	}
    */

    /// <summary>
    /// Compute the terrain covering texture (splat) from the terrain heights.
    /// </summary>
    /// <param name="pSimContainer"></param>
	public static void TerrainApplyTerrainSplatting(SimManager.SimulatorContainer pSimContainer)
	{
		Debug.Log($"Applying Terrain Splats {pSimContainer.sim.Name}");
        SimTerrainHeightMap terrain = pSimContainer.terrain as SimTerrainHeightMap;

        if (terrain == null)
        {
            // The terrain didn't change to a height map terrain. It's not us.
            // Debug.Log($"TerrainApplyTerrainSplatting: terrain is null");
            return;
        }

		float[,] heights = terrain._heights;
		float[,,] terrainSplats = new float[pSimContainer.sizeX, pSimContainer.sizeY, 4];

		for (int y = 0; y < pSimContainer.sizeY; y++)
		{
			for (int x = 0; x < pSimContainer.sizeX; x++)
			{
				OMV.Vector2 global_position = new OMV.Vector2(x, y);
				float height = SimTerrainHeightMap.CalculateHeight(heights, x, y);
				float lowFreq, highFreq, noise;
				SimTerrainHeightMap.CalculateNoise(global_position, out lowFreq, out highFreq, out noise);

				float interpolated_start_height, interpolated_height_range;
				SimTerrainHeightMap.InterpolateHeights(pSimContainer.sim, global_position, out interpolated_start_height, out interpolated_height_range);

				SimTerrainHeightMap.ApplyLayer(ref terrainSplats, x, y, height, noise, interpolated_start_height, interpolated_height_range);
			}
		}

		terrain.UnityTerrain.terrainData.SetAlphamaps(0, 0, terrainSplats);
	}

	public static float Bilinear(float v00, float v01, float v10, float v11, float xPercent, float yPercent)
	{
		return Mathf.Lerp(Mathf.Lerp(v00, v01, xPercent), Mathf.Lerp(v10, v11, xPercent), yPercent);
	}

	private static float CalculateHeight(float[,] heights, int x, int y)
	{
		return heights[x, y] * TERRAIN_HEIGHT_MAX;
	}

	private static void CalculateNoise(OMV.Vector2 global_position, out float lowFreq, out float highFreq, out float noise)
	{
		float noisemult = .2222222f;
		float turbmult = 2f;

		lowFreq = Mathf.PerlinNoise(global_position.X * noisemult, global_position.Y * noisemult) * 6.5f;
		highFreq = perlin_turbulence2(global_position.X, global_position.Y, turbmult) * 2.25f;
		noise = (lowFreq + highFreq) * 2f;
	}

	private static void InterpolateHeights(OMV.Simulator simulator, OMV.Vector2 global_position, out float interpolated_start_height, out float interpolated_height_range)
	{
		interpolated_start_height = Bilinear(
			simulator.TerrainStartHeight00,
			simulator.TerrainStartHeight01,
			simulator.TerrainStartHeight10,
			simulator.TerrainStartHeight11,
			global_position.X, global_position.Y);

		interpolated_height_range = Bilinear(
			simulator.TerrainHeightRange00,
			simulator.TerrainHeightRange01,
			simulator.TerrainHeightRange10,
			simulator.TerrainHeightRange11,
			global_position.X, global_position.Y);
	}

	private static void ApplyLayer(ref float[,,] terrainSplats, int x, int y, float height, float noise, float interpolated_start_height, float interpolated_height_range)
	{
		float value = Mathf.Clamp((height + noise - interpolated_start_height) * 4 / interpolated_height_range, 0, 3f);
		float dist = (interpolated_height_range - interpolated_start_height);
		float verticalblend = ((value - interpolated_start_height) / dist);
		float layer = ((height + noise - interpolated_start_height) / interpolated_height_range) * 4f;
		layer = Mathf.Clamp(layer, 0f, 3f);
		int layeri = Mathf.RoundToInt(value);

		terrainSplats[x, y, layeri] = 1f - (layer - layeri);
		if (layeri < 3) terrainSplats[x, y, layeri + 1] = (layer - layeri);
	}

    /* OLD VERSION
	private static void ApplyLayer(ulong handle, int x, int y, float height, float noise, float interpolated_start_height, float interpolated_height_range)
	{
		float value = Mathf.Clamp((height + noise - interpolated_start_height) * 4 / interpolated_height_range, 0, 3f);
		float layer = ((height + noise - interpolated_start_height) / interpolated_height_range) * 4f;
		layer = Mathf.Clamp(layer, 0f, 3f);
		int layeri = Mathf.RoundToInt(value);

		Color newColor = new Color(0, 0, 0, 0);
		float amount = 1f - (layer - layeri);
		float inverseAmount = layer - layeri;

		switch (layeri)
		{
			case 0:
				newColor.r = amount;
				newColor.g = inverseAmount;
				break;
			case 1:
				newColor.g = amount;
				newColor.b = inverseAmount;
				break;
			case 2:
				newColor.b = amount;
				newColor.a = inverseAmount;
				break;
			case 3:
				newColor.a = amount;
				//if (layeri < 3) newColor.b = inverseAmount;
				break;
		}
		simulators[handle].splatMap.SetPixel(x, y, newColor);
	}
    */

    /* OLD VERSION
	public void ApplyTerrainSplatting(Texture2D texture, Simulator simulator)
	{
		Debug.Log($"Applying Terrain Splats {simulator.Name}");
		int resolution = texture.width; // Assuming the texture is square

		float[,] heights = simulators[simulator.Handle].heights;
		Color[] colors = new Color[resolution * resolution];
		//TerrainData terrainData = terrain.terrainData;
		//int resolution = (int)simulator.SizeX;//terrainData.heightmapResolution;

		if (_splatValues == null || _splatValues.GetLength(0) != resolution || _splatValues.GetLength(1) != resolution)
		{
			_splatValues = new float[resolution, resolution];
		}

		int i, j, x, y;

		//float[,] heights = simulators[simulator.Handle].heights;//terrainData.GetHeights(0, 0, resolution, resolution);

		//float[,] terrainHeight = new float[16, 16];
		float[,,] terrainSplats = new float[simulator.SizeX, simulator.SizeY, 4];

		float swLow = simulator.TerrainStartHeight00;
		float swHigh = simulator.TerrainHeightRange00;
		float nwLow = simulator.TerrainStartHeight01;
		float nwHigh = simulator.TerrainHeightRange01;
		float seLow = simulator.TerrainStartHeight10;
		float seHigh = simulator.TerrainHeightRange10;
		float neLow = simulator.TerrainStartHeight11;
		float neHigh = simulator.TerrainHeightRange11;
		float interpolated_start_height = 0;
		float interpolated_height_range = 0;
		//Vector2 vec;
		float low_freq;
		float high_freq;
		//float noise;

		float value;

		float verticalblend;
		float dist;
		float modheight;
		int RegionSize = 256; //this will need to change for varregions
		int diff = 256;//simulators[e.Simulator.Handle].terrain.terrainData.hei.GetLength(0) / RegionSize;

		float noisemult = .2222222f;
		float turbmult = 2f;


		for (y = 0; y < simulator.SizeY; y++)
		{
			for (x = 0; x < simulator.SizeX; x++)
			{
				float height = heights[x, y] * 256;
				Debug.Log($"Terrain Splat Height: {x},{y} = {height}");
				float globalXpercent = x * 0.0039215686274509803921568627451f;
				float globalYpercent = y * 0.0039215686274509803921568627451f;

				Vector2 global_position = new Vector2(x, y);
				//Debug.Log($"TERRAINSPLAT: e.X={global_position}");
                // vec = global_position * 0.20319f;
				// low_freq = perlin_noise2(vec.x * 0.222222f, vec.y * 0.222222f) * 6.5f;
				// high_freq = perlin_turbulence2(vec.x, vec.y, 2) * 2.25f;
				// noise = (low_freq + high_freq) * 2;

				int newX = Mathf.FloorToInt((float)x * 0.0039215686274509803921568627451f);/// diff;
				int newY = Mathf.FloorToInt((float)y * 0.0039215686274509803921568627451f);/// diff;

				Vector3 vec = new Vector3
				(
					newX * 0.20319f,
					newY * 0.20319f,
					height * 0.25f
				);

				float lowFreq = Mathf.PerlinNoise(global_position.x * noisemult, global_position.y * noisemult) * 6.5f;
				float highFreq = perlin_turbulence2(global_position.x, global_position.y, turbmult) * 2.25f;//Perlin.turbulence2(vec.x, vec.y, 2f) * 2.25f;
																											//Debug.Log($"TERRAINSPLAT: noise lowFreq={lowFreq} highFreq={highFreq}");
				float noise = (lowFreq + highFreq) * 2f;
				//Debug.Log($"TERRAINSPLAT: noise={noise}");
				//interpolated_start_height = QuadLerp(swLow, nwLow, seLow, neLow, globalYpercent, globalXpercent);
				//interpolated_height_range = QuadLerp(swHigh, nwHigh, seHigh, neHigh, globalYpercent, globalXpercent);
				interpolated_start_height = Bilinear(
					simulator.TerrainStartHeight00,
					simulator.TerrainStartHeight01,
					simulator.TerrainStartHeight10,
					simulator.TerrainStartHeight11,
					global_position.x, global_position.y);
				//interpolated_start_height = Mathf.Clamp(interpolated_start_height, 0f, 255f);
				interpolated_height_range = Bilinear(
					simulator.TerrainHeightRange00,
					simulator.TerrainHeightRange01,
					simulator.TerrainHeightRange10,
					simulator.TerrainHeightRange11,
					global_position.x, global_position.y);

				float startHeight = interpolated_start_height;
				float heightRange = interpolated_height_range;
				//interpolated_height_range = Mathf.Clamp(interpolated_height_range, 0f, 255f);

				value = Mathf.Clamp((height + noise - interpolated_start_height) * 4 / interpolated_height_range, 0, 3f);
				//modheight = height + value;//* value;

				dist = (interpolated_height_range - interpolated_start_height);

				verticalblend = ((value - interpolated_start_height) / dist);
				float layer = ((height + noise - startHeight) / heightRange) * 4f;

				if (Single.IsNaN(layer))
					layer = 0f;
				layer = Mathf.Clamp(layer, 0f, 3f);


				int layeri = Mathf.CeilToInt(layer);
				layeri = Mathf.RoundToInt(value);

				Color color = new Color();
				color.r = layeri == 0 ? 1f - (layer - layeri) : 0;
				color.g = layeri == 1 ? 1f - (layer - layeri) : 0;
				color.b = layeri == 2 ? 1f - (layer - layeri) : 0;
				color.a = layeri == 3 ? 1f - (layer - layeri) : 0;

				if (layeri < 3)
				{
					if (layeri + 1 == 1) color.g = (layer - layeri);
					if (layeri + 1 == 2) color.b = (layer - layeri);
					if (layeri + 1 == 3) color.a = (layer - layeri);
				}

				colors[y * resolution + x] = color;
			}
		}

		texture.SetPixels(colors);
		texture.Apply(); // Apply changes to the texture
	}
    */

	// Simple Perlin noise function
	private static float perlin_noise2(float x, float y)
	{
		return Mathf.PerlinNoise(x, y);
	}

	// Perlin turbulence function
	private static float perlin_turbulence2(float x, float y, float freq)
	{
		float t;
		OMV.Vector2 vec = OMV.Vector2.Zero;

		for (t = 0f; freq >= 1f; freq *= 0.5f)
		{
			vec.X = freq * x;
			vec.Y = freq * y;
			t += Mathf.PerlinNoise(vec.X, vec.Y) / freq;
		}
		return t;
	}

	int SAMPLE_SIZE = 1024;
	int B = 1024;
	int BM = 1023;
	int N = 0x1000;

	int[] p = new int[2050];
	float[,] g3 = new float[2050, 3];
	float[,] g2 = new float[2050, 2];
	float[] g1 = new float[2050];
	public float noise2(float x, float y)
	{
		int bx0, bx1, by0, by1, b00, b10, b01, b11;
		float rx0, rx1, ry0, ry1, sx, sy, a, b, t, u, v;
		int i, j;

		t = x + N;
		bx0 = ((int)t) & BM;
		bx1 = (bx0 + 1) & BM;
		rx0 = t - (int)t;
		rx1 = rx0 - 1f;

		t = y + N;
		by0 = ((int)t) & BM;
		by1 = (by0 + 1) & BM;
		ry0 = t - (int)t;
		ry1 = ry0 - 1f;

		i = p[bx0];
		j = p[bx1];

		b00 = p[i + by0];
		b10 = p[j + by0];
		b01 = p[i + by1];
		b11 = p[j + by1];

		sx = s_curve(rx0);
		sy = s_curve(ry0);

		u = rx0 * g2[b00, 0] + ry0 * g2[b00, 1];
		v = rx1 * g2[b10, 0] + ry0 * g2[b10, 1];
		a = OMV.Utils.Lerp(u, v, sx);

		u = rx0 * g2[b01, 0] + ry1 * g2[b01, 1];
		v = rx1 * g2[b11, 0] + ry1 * g2[b11, 1];
		b = OMV.Utils.Lerp(u, v, sx);

		return OMV.Utils.Lerp(a, b, sy);
	}

	private static float s_curve(float t)
	{
		return t * t * (3f - 2f * t);
	}


}

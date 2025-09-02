using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Temp;
using OMV = OpenMetaverse;

using UnityEngine;

public class SimTerrainTiles : SimTerrain
{
    private GameObject TerrainObject;
    public override Transform Transform { get { return TerrainObject.transform; } }

    private MeshFilter[,] _meshFilters;
    private Texture2D _splatMap;    // The texture that is build as the region coverer
    private Material _splatMaterial;
    private float[,] _heights;

	public SimTerrainTiles(OMV.Simulator pSim) : base(pSim)
	{
        // Keeps the heights of the terrain for meter centered grid
        _heights = new float[pSim.SizeX, pSim.SizeY];

        // Create a flat terrain for the region.
        CreateDefaultTerrain();

        isModified = true;

        Debug.Log($"SimTerrainTiles: creation: {_sim.Name} {_sim.SizeX}x{_sim.SizeY}, is256={is256}");
	}

    private void CreateDefaultTerrain()
    {
        CreateTerrainTileStorage();
    }

    public override void SetTerrainRelativePosition(OMV.Simulator pSim)
    {
		// The terrain positions is set relative to the simulator origin
        OMV.Utils.LongToUInts(ClientManager.client.Network.CurrentSim.Handle, out uint _x, out uint _y);
        OMV.Utils.LongToUInts(pSim.Handle, out uint x, out uint y);
        uint relx = x - _x;
        uint rely = y - _y;

        TerrainObject.transform.position = new UnityEngine.Vector3(relx, 0f, rely);
    }
    /// <summary>
    /// Received a terrain update from the simulator.
    /// These come in a bunch when a simulator starts so the might need to
    /// be queued up have processing spread out.
    /// </summary>
	public ConcurrentQueue<OMV.LandPatchReceivedEventArgs> terrainPatchUpdates = new();

    public override void TerrainPatchUpdate(OMV.LandPatchReceivedEventArgs pLandPatchEvent)
    {
		terrainPatchUpdates.Enqueue(pLandPatchEvent);
    }

	public override void TerrainUpdate(SimulatorContainer pSimContainer)
    {
        SimTerrainTiles terrain = pSimContainer.terrain as SimTerrainTiles;
        if (terrain == null)
        {
            // The terrain didn't change to a tile terrain. It's not us.
            // Debug.Log($"SimTerrainTiles.TerrainUpdate: terrain is null");
            return;
        }
        if (terrainPatchUpdates.TryDequeue(out OMV.LandPatchReceivedEventArgs patchUpdate))
        {
            // Debug.Log($"SimTerrainTiles.TerrainUpdate: patchUpdate: {patchUpdate.X},{patchUpdate.Y}");
            // Since the terrain is being updated, remember that the terrain texture needs regen
            isModified = true;
            SetHeightMapHeights(patchUpdate.X, patchUpdate.Y, patchUpdate.HeightMap);
            terrain.UpdateVertexHeights(patchUpdate.X * 16, patchUpdate.Y * 16, patchUpdate.HeightMap);
        }
        else
        {
            // Debug.Log($"SimTerrainTiles.TerrainUpdate: no patchUpdate");
            terrain.TerrainApplyTerrainSplatting(pSimContainer, terrain);
        }
    }

    // Update the array of heights for the terrain with a 16x16 patch of heights
    private void SetHeightMapHeights(int x, int y, float[] newheights)
    {
        x *= 16;
        y *= 16;
        for (int j = 0; j < 16; j++)
        {
            for (int i = 0; i < 16; i++)
            {
                float height = (newheights[j * 16 + i]);
                _heights[y + j, x + i] = height;
            }
        }
    }

    /// <summary>
    /// Implement terrain as 256x256 "tiles" that use a shader to compute the covering texture.
    /// Each tile is implemented as GameObject containing a MeshFilter and they are all
    /// parented to a GameObject that is the terrain.
    /// </summary>
    private void CreateTerrainTileStorage()
    {
        Mesh[] meshes = GenerateGrid((uint)SizeX, (uint)SizeY);
        int numTilesX = Mathf.CeilToInt((int)SizeX / 256);
        int numTilesY = Mathf.CeilToInt((int)SizeY / 256);
        var renderers = new Renderer[numTilesX, numTilesY];
        _meshFilters = new MeshFilter[numTilesX, numTilesY];

        var material = new UnityEngine.Material(Shader.Find("CFEngine/SimpleFourSplatTerrain"));

        // Set the default white texture for the material's properties
        Texture2D defaultWhiteTexture = Texture2D.whiteTexture;
        _splatMap = new Texture2D((int)SizeX, (int)SizeY, TextureFormat.RGBA32, false);

        // material.SetTexture("_MainTex", defaultWhiteTexture);
        // material.SetTexture("_Splat2", defaultWhiteTexture);
        // material.SetTexture("_Splat3", defaultWhiteTexture);
        // material.SetTexture("_Splat4", defaultWhiteTexture);
        material.SetTexture("_MainTex", (Texture2D)ClientManager.assetManager.RequestTexture(_sim.TerrainDetail0));
        material.SetTexture("_Splat2", (Texture2D)ClientManager.assetManager.RequestTexture(_sim.TerrainDetail1));
        material.SetTexture("_Splat3", (Texture2D)ClientManager.assetManager.RequestTexture(_sim.TerrainDetail2));
        material.SetTexture("_Splat4", (Texture2D)ClientManager.assetManager.RequestTexture(_sim.TerrainDetail3));

        material.SetTexture("_SplatMap", _splatMap);

        _splatMaterial = material;

        TerrainObject = new GameObject($"Sim: {_sim.Name}");    // top level object
        for (int tileY = 0; tileY < numTilesY; tileY++)
        {
            for (int tileX = 0; tileX < numTilesX; tileX++)
            {
                GameObject tile = new GameObject($"{_sim.Name}_Tile_{tileX}_{tileY}");
                MeshFilter meshFilter = tile.AddComponent<MeshFilter>();
                meshFilter.mesh = meshes[tileY * numTilesX + tileX];
                Renderer renderer = tile.AddComponent<MeshRenderer>();
                renderer.material = material; // set shared material
                renderers[tileX, tileY] = renderer;
                _meshFilters[tileX, tileY] = meshFilter;
                tile.transform.parent = TerrainObject.transform;
                tile.transform.localPosition = new UnityEngine.Vector3(tileX * 255, 0, tileY * 255);
            }
        }
    }

    static int TILE_SIZE = 256;
    static int TILE_MASK = TILE_SIZE - 1;

    /// <summary>
    /// Generate a grid of meshes for the terrain.
    /// Each of the meshes handles a 256x256 region of the terrain.
    /// </summary>
    /// <param name="sizeX">X dimension of whole region in meters</param>
    /// <param name="sizeY">Y dimension of whole region in meters</param>
    /// <returns>2D array of meshes</returns>
    public static Mesh[] GenerateGrid(uint sizeX, uint sizeY)
    {
        // Calculate the number of tiles
        int numTilesX = Mathf.CeilToInt(sizeX / TILE_SIZE);
        int numTilesY = Mathf.CeilToInt(sizeY / TILE_SIZE);

        // Create mesh array
        Mesh[] meshes = new UnityEngine.Mesh[numTilesX * numTilesY];

        for (int y = 0; y < numTilesY; y++)
        {
            for (int x = 0; x < numTilesX; x++)
            {
                meshes[y * numTilesX + x] = GenerateTile(x, y, numTilesX, numTilesY);
            }
        }

        return meshes;
    }

    public static Mesh GenerateTile(int tileX, int tileY, int numTilesX, int numTilesY)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[TILE_SIZE * TILE_SIZE];
        Vector2[] uv = new Vector2[TILE_SIZE * TILE_SIZE];
        Vector2[] uv2 = new Vector2[TILE_SIZE * TILE_SIZE]; // Second UV channel
        int[] triangles = new int[(TILE_SIZE - 1) * (TILE_SIZE - 1) * 6];

        float uvTileSizeX = 1f / numTilesX; // UV size for individual tile along X axis
        float uvTileSizeY = 1f / numTilesY; // UV size for individual tile along Y axis

        for (int y = 0; y < TILE_SIZE; y++)
        {
            for (int x = 0; x < TILE_SIZE; x++)
            {
                int index = y * TILE_SIZE + x;
                vertices[index] = new Vector3(x, 0, y);
                uv[index] = new Vector2(x / 12f, y / 12f); // Tiling every 12 meters for the main texture

                // Calculate UV2 based on tile's position in the grid
                uv2[index] = new Vector2((x / (float)TILE_SIZE + tileX) * uvTileSizeX, (y / (float)TILE_SIZE + tileY) * uvTileSizeY);
            }
        }

        int triangleIndex = 0;
        for (int y = 0; y < TILE_SIZE - 1; y++)
        {
            for (int x = 0; x < TILE_SIZE - 1; x++)
            {
                int index = y * TILE_SIZE + x;
                triangles[triangleIndex++] = index;
                triangles[triangleIndex++] = index + TILE_SIZE;
                triangles[triangleIndex++] = index + 1;
                triangles[triangleIndex++] = index + 1;
                triangles[triangleIndex++] = index + TILE_SIZE;
                triangles[triangleIndex++] = index + TILE_SIZE + 1;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.uv2 = uv2; // Second UV channel
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    private void UpdateVertexHeights(int vertexX, int vertexY, float[] newHeights)
    {
        int tileX = vertexX >> 8;
        int tileY = vertexY >> 8;
        int localVertexX = vertexX & 0xFF;
        int localVertexY = vertexY & 0xFF;

        //Debug.Log($"{tileX},{tileY}");

        Mesh mesh = _meshFilters[tileX, tileY].mesh;
        Vector3[] vertices = mesh.vertices;

        int endX = localVertexX + 16;
        int endY = localVertexY + 16;
        int heightIndex = 0;
        int baseIndex = localVertexY * 256;

        for (int y = localVertexY; y < endY; y++, baseIndex += 256)
        {
            int index = baseIndex + localVertexX;
            for (int x = localVertexX; x < endX; x++, index++)
            {
                vertices[index].y = newHeights[heightIndex++];
            }
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals(); // Consider manual update or removal if possible
        mesh.RecalculateBounds();
    }

    // There have been updates to the terrain some compute the splat texture that covers the terrain.
    public void TerrainApplyTerrainSplatting(SimulatorContainer pSimContainer, SimTerrainTiles pTerrain)
    {
        // Debug.Log($"Applying Terrain Splats {pSimContainer.sim.Name}");

        float[,] heights = pTerrain._heights;
        int sX = (int)pTerrain.SizeX;
        int sY = (int)pTerrain.SizeY;

        for (int y = 0; y < sY; y++)
        {
            for (int x = 0; x < sX; x++)
            {
                Vector2 global_position = new Vector2(x, y);
                float height = CalculateHeight(heights, x, y);
                float lowFreq, highFreq, noise;
                CalculateNoise(global_position, out lowFreq, out highFreq, out noise);

                float interpolated_start_height, interpolated_height_range;
                InterpolateHeights(pSimContainer.sim, global_position,
                            out interpolated_start_height, out interpolated_height_range);

                ApplyLayer(x, y, height, noise, interpolated_start_height, interpolated_height_range);
            }
        }

        //terrain.terrainData.SetAlphamaps(0, 0, terrainSplats);
    }
    private float CalculateHeight(float[,] heights, int x, int y)
    {
        return heights[x, y] * 256;
    }

    private void CalculateNoise(Vector2 global_position, out float lowFreq, out float highFreq, out float noise)
    {
        float noisemult = .2222222f;
        float turbmult = 2f;

        lowFreq = Mathf.PerlinNoise(global_position.x * noisemult, global_position.y * noisemult) * 6.5f;
        highFreq = perlin_turbulence2(global_position.x, global_position.y, turbmult) * 2.25f;
        noise = (lowFreq + highFreq) * 2f;
    }

    private void InterpolateHeights(OMV.Simulator simulator, Vector2 global_position,
                        out float interpolated_start_height, out float interpolated_height_range)
    {
        interpolated_start_height = Bilinear(
            simulator.TerrainStartHeight00,
            simulator.TerrainStartHeight01,
            simulator.TerrainStartHeight10,
            simulator.TerrainStartHeight11,
            global_position.x, global_position.y);

        interpolated_height_range = Bilinear(
            simulator.TerrainHeightRange00,
            simulator.TerrainHeightRange01,
            simulator.TerrainHeightRange10,
            simulator.TerrainHeightRange11,
            global_position.x, global_position.y);
    }
    public static float Bilinear(float v00, float v01, float v10, float v11, float xPercent, float yPercent)
    {
        return Mathf.Lerp(Mathf.Lerp(v00, v01, xPercent), Mathf.Lerp(v10, v11, xPercent), yPercent);
    }
    // Simple Perlin noise function

    float perlin_noise2(float x, float y)
    {
        return Mathf.PerlinNoise(x, y);
    }

    // Perlin turbulence function
    float perlin_turbulence2(float x, float y, float freq)
    {
        float t;
        Vector2 vec = Vector2.zero;

        for (t = 0f; freq >= 1f; freq *= 0.5f)
        {
            vec.x = freq * x;
            vec.y = freq * y;
            t += Mathf.PerlinNoise(vec.x, vec.y) / freq;
        }
        return t;
    }

    /// <summary>
    /// The material for the terrain uses _SplatMap to determine the color of each pixel.
    /// The formaula for the color is:
    ///    _MainTex*color.r + _Splat2*color.g + _Splat3*color.b + _Splat4*color.a
    /// where 'color' is from _splatMap.
    /// This function computes the value for _splatMap for a given pixel based on the height and noise.
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="height"></param>
    /// <param name="noise"></param>
    /// <param name="interpolated_start_height"></param>
    /// <param name="interpolated_height_range"></param>
    private void ApplyLayer(int x, int y, float height, float noise, float interpolated_start_height, float interpolated_height_range)
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

        _splatMap.SetPixel(x, y, newColor);
    }


}

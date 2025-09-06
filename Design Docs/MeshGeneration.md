### **Feature Guide: Avatar & Object Mesh Generation**

**Version:** 1.0
**Author:** Jules

#### **1. Overview**

**What are Meshes?**
In 3D graphics, a "mesh" is the collection of vertices, edges, and faces that define the shape of an object. In the context of Crystal Frost and Second Life, every object you see—from a simple cube to a complex avatar—is represented by a mesh. This pipeline is responsible for taking the abstract description of an object provided by the server and turning it into a visible 3D model in Unity.

**Types of Meshes in Second Life:**
*   **Standard Primitives (Prims):** These are basic shapes (cubes, spheres, cylinders, etc.) that can be modified with parameters like cutting, twisting, and tapering. The mesh for these is generated procedurally.
*   **Mesh Assets:** These are custom models created in external 3D modeling software (like Blender) and uploaded to the virtual world.
*   **Rigged/Skinned Meshes:** This is a special type of mesh asset, primarily used for avatars and attachments. It includes a "skeleton" of joints (or bones) that allow the mesh to deform realistically during animation.

**The Crystal Frost Mesh Pipeline:**
Similar to the texture pipeline, the mesh generation process is handled by a multi-threaded, queue-based system to avoid freezing the main application thread. The core of this system uses the powerful `OpenMetaverse.Rendering.PrimMesher` library from LibreMetaverse to do the heavy lifting of procedural mesh generation.

The high-level process is:
1.  The `StateManager` identifies a new object (`Primitive`) that needs to be displayed.
2.  It calls the `MeshManager` to request a mesh for this primitive.
3.  The `MeshManager` creates a `MeshRequest` and enqueues it.
4.  If the primitive uses a mesh asset, a `MeshDownloadWorker` downloads the asset data.
5.  A `MeshDecodeWorker` dequeues the request and passes it to the `MeshDecoder`.
6.  The `MeshDecoder` uses the `PrimMesher` library to generate the vertex data (vertices, normals, UVs, indices).
7.  The decoded data is stored in a `DecodedMeshData` object and placed in the `ReadyMeshes` queue.
8.  The main thread's `UnityRenderManager` dequeues the ready mesh and uses the data to construct a Unity `Mesh` object, which is then assigned to a `GameObject` to be rendered.

#### **2. Architecture & Key Components**

**Key Classes:**
*   `MeshManager`: The public entry point for requesting a mesh for a given `Primitive`.
*   `MeshDecodeWorker`: The background service that orchestrates the decoding process.
*   `MeshDecoder`: The core class that contains the logic for converting a `Primitive` object into raw mesh data. It acts as a wrapper around the LibreMetaverse `PrimMesher`.
*   `MeshRequest`: A data object that holds all information about a single mesh request, including the input `Primitive` and the output `DecodedMeshData`.
*   `DecodedMeshData`: A custom data structure that holds all the data needed to construct one or more Unity `Mesh` objects (vertices, normals, UVs, indices, bone weights, etc.). It can contain multiple `RawMeshData` objects, one for each submesh.
*   `RawMeshData`: Holds the final vertex data for a single submesh.
*   `OpenMetaverse.Rendering.RiggedMesh`: A powerful class from the LibreMetaverse library that takes a `Primitive` and generates its 3D mesh data. The name is slightly misleading as it handles both rigged and non-rigged objects.

#### **3. File Locations**

*   **Service Registration:** `Assets/CFEngine/Services.cs`
*   **Mesh Manager:** `Assets/CFEngine/Assets/Mesh/MeshManager.cs`
*   **Decode Worker:** `Assets/CFEngine/Assets/Mesh/MeshDecodeWorker.cs`
*   **Mesh Decoder:** `Assets/CFEngine/Assets/Mesh/MeshDecoder.cs`

#### **4. Detailed Implementation Walkthrough**

**Step A: Requesting a Mesh (`MeshManager.cs`)**
The process begins when `MeshManager.RequestMesh()` is called. This method bundles the `Primitive` data and the target `GameObject` into a `MeshRequest` object and places it on a queue.

**Step B: The Decoding Worker (`MeshDecodeWorker.cs`)**
The `MeshDecodeWorker` runs on a background thread. Its `DoWorkImpl()` method waits for a `MeshRequest` to appear in the `_downloadedMeshQueue`.

```csharp
// Inside MeshDecodeWorker.DoWorkImpl()
private bool DoWorkImpl()
{
    if (_downloadedMeshQueue.Count == 0) return false;
    if (!_downloadedMeshQueue.TryDequeue(out var request)) return true;

    // The key call: pass the request to the injected IMeshDecoder
    // which is our MeshDecoder class.
    _meshDecoder.Decode(request);

    return _downloadedMeshQueue.Count > 0;
}
```

**Step C: The Core Decoding (`MeshDecoder.cs`)**
This is where the actual mesh generation happens. The `Decode` method is the entry point.

1.  **Generate the High-Level Mesh:** The first and most important call is to `RiggedMesh.TryDecodeFromAsset`. This is a static method from LibreMetaverse's `PrimMesher` library. It takes the `Primitive` object and generates a `RiggedMesh` object, which is a high-level representation of the 3D model, including all its faces (submeshes), vertices, and (if applicable) skeleton information.

    ```csharp
    // Inside MeshDecoder.TranscodeFacetedMeshAtDetailLevel()
    if (!RiggedMesh.TryDecodeFromAsset(prim, assetMesh, detailLevel, out RiggedMesh fmesh))
    {
        Debug.LogWarning($"Unable to decode {detailLevel} detail mesh UUID: {request.UUID}");
        return;
    }
    ```

2.  **Process Each Face (Submesh):** A single primitive can have multiple faces, each with a different texture. The code iterates through each `Face` in the generated `fmesh`. Each of these `Face` objects will become a separate "submesh" in the final Unity `Mesh`.

3.  **Generate Raw Vertex Data:** For each `Face`, the code calls `face.ToRawMeshData()`. This method converts the face's vertices, normals, and texture coordinates into simple arrays (`Vector3[]`, `Vector2[]`, etc.) that Unity can understand. It has a separate path for skinned meshes to include bone weights.

4.  **Transform UV Coordinates:** After generating the basic UVs, the code loops through them to apply the texture transformations defined on the `TextureEntryFace` (tiling, offset, and rotation). This ensures the texture is mapped correctly onto the submesh.

    ```csharp
    // Inside the loop over faces in MeshDecoder
    for (var i = 0; i < rmd.uvs.Length; i++)
    {
        // Center the UVs around (0,0) for rotation
        float tX = rmd.uvs[i].x - 0.5f;
        float tY = rmd.uvs[i].y - 0.5f;

        // Apply rotation
        rmd.uvs[i].x = (tX * cosineAngle + tY * sinAngle) * repeatU + textureEntryFace.OffsetU + 0.5f;
        rmd.uvs[i].y = (-tX * sinAngle + tY * cosineAngle) * repeatV + (1f - textureEntryFace.OffsetV) + 0.5f;
    }
    ```
    *(Note: This rotation logic is a known issue and will be addressed in a separate task as per the roadmap.)*

5.  **Enqueue the Result:** The generated `RawMeshData` for each face is added to the `request.DecodedMesh.meshData` list. Finally, the entire `request` object (which now contains the decoded mesh data) is enqueued into the `_readyMeshQueue`.

**Step D: Creating the Unity Mesh**
The `UnityRenderManager` (not detailed here) is responsible for dequeuing from the `ReadyMeshes` queue on the main thread. It will then take the `DecodedMeshData`, create a new `UnityEngine.Mesh`, and assign the vertices, normals, uvs, and triangles to it. If there are multiple `RawMeshData` objects, it will create a mesh with multiple submeshes.

#### **5. Testing Strategy**

**Unit Testing:**
Directly unit testing the `MeshDecoder` is complex because it requires constructing a valid `Primitive` object, which can have dozens of properties. A more effective approach for testing the core logic is integration testing.

**Integration Testing in Unity:**
A new developer can create a test scene to verify the entire mesh pipeline.
1.  Create a new, empty scene.
2.  Create a C# script (`MeshTest.cs`).
3.  In the script's `Start()` method, get the `IMeshManager` from the `Services` container.
4.  Create a `Primitive` object programmatically. Start with a simple default cube.
    ```csharp
    Primitive myPrim = new Primitive();
    myPrim.PrimData.Shape = PrimShape.Box;
    // ... set other properties like position and scale.
    ```
5.  Create a new `GameObject` to hold the mesh.
6.  Call `meshManager.RequestMesh()` with your new prim and `GameObject`.
7.  In the script's `Update()` method, monitor the `ReadyMeshes` queue.
8.  When your mesh appears, dequeue it. The `DecodedMeshData` will be attached to the `MeshRequest` object.
9.  Get the `MeshFilter` component on your `GameObject`.
10. Create a new `UnityEngine.Mesh` and populate it from the `DecodedMeshData`.
    ```csharp
    var unityMesh = new UnityEngine.Mesh();
    // Assuming one submesh for a simple cube
    var rawData = decodedMesh.meshData[0];
    unityMesh.vertices = rawData.vertices;
    unityMesh.normals = rawData.normals;
    unityMesh.uv = rawData.uvs;
    unityMesh.triangles = rawData.indices;
    meshFilter.mesh = unityMesh;
    ```
11. Run the scene. You should see the cube you defined in code appear in the game view. You can then experiment with different prim shapes and parameters.

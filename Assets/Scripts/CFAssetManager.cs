using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using OpenMetaverse;
using OpenMetaverse.Assets;
using OpenMetaverse.Rendering;
using CrystalFrost.Assets;
using UnityEditor;
using OMVVector3 = OpenMetaverse.Vector3;
using Vector3 = UnityEngine.Vector3;
using OMVVector2 = OpenMetaverse.Vector2;
using Vector2 = UnityEngine.Vector2;
using Material = UnityEngine.Material;
using Mesh = UnityEngine.Mesh;
using CrystalFrost.Lib;
using CrystalFrost.Extensions;
using Microsoft.Extensions.Logging;
using CrystalFrost.Assets.Mesh;
using Unity.VisualScripting;

namespace CrystalFrost
{
	public class CFAssetManager
	{
		//it's faster to multiply by this than to divide by 255
		//to derive the float value from 0-255 pixel values
		//private float byteMult = 0.003921568627451f;

		public SimManager simManager;

		private readonly IAssetManager _assetManager;
		private readonly ITransformTexCoords _transformTextureCoords;
		private readonly ILogger<CFAssetManager> _log;

		public CFAssetManager()
		{
			_log = Services.GetService<ILogger<CFAssetManager>>();
			_transformTextureCoords = Services.GetService<ITransformTexCoords>();
			_assetManager = Services.GetService<IAssetManager>();
		}

		public class MeshQueueItem
		{
			public UUID uuid;
			public List<RawMeshData> meshData = new();
		}

		public ConcurrentQueue<MeshQueueItem> concurrentMeshQueue = new();

		public class SLMeshData
		{
			public Mesh[] meshHighest;
			public Mesh[] meshHigh;
			public Mesh[] meshMedium;
		}

		public Dictionary<UUID, SLMeshData> meshCache = new();
		public Dictionary<UUID, AudioClip> sounds = new();
		public Dictionary<UUID, List<Renderer>> materials = new();
		public Dictionary<UUID, int> componentsDict = new();
		public List<MeshRenderer> fullbrights = new();

		public Material zeroMaterial;
		public class MaterialContainer
		{
			public bool ready = false;
			public Material materialOpaque;
			public Material materialAlpha;
			public Material materialOpaqueFullbright;
			public Material materialAlphaFullbright;
			public Texture2D texture;
			public uint components;
			public UUID uuid;

			//attempting to rewrite this to branchless
			/*public Material GetMaterial(Color color, float glow, bool fullbright)
			{
				if (components == 3)
				{
					return GetMaterialOpaque(color, glow, fullbright);
				}
				else// if(components == 4)
				{
					return GetMaterialOpaque(color, glow, fullbright);
				}
			}*/

			//attempted branchless refactor
			public Material GetMaterial(Color color, float glow, bool fullbright)
			{
				// An array of function delegates
				Func<Color, float, bool, Material>[] functions = new Func<Color, float, bool, Material>[]
				{
					GetMaterialOpaque,
					GetMaterialAlpha
				};

				// Using the value of components as an index into the array
				return functions[components - 3](color, glow, fullbright);
			}


			public Material GetMaterialOpaque(Color color, float glow, bool fullbright)
			{
				Material mat = fullbright ? ResourceCache.opaqueFullbrightMaterial : ResourceCache.opaqueMaterial;

				if (materialOpaque == null)
				{
					materialOpaque = Material.Instantiate(mat);
#if UNITY_EDITOR
					materialOpaque.name = $"opaque {uuid}";
#endif
					materialOpaque.SetTexture(ClientManager.DiffuseName, texture);
				}

				if (components == 4 || color.a < 0.999f) return GetMaterialAlpha(color, glow, fullbright);

				bool colorChange = color.r < 0.999f || color.g < 0.999f || color.b < 0.999f || glow > 0.001f || fullbright;

				if (colorChange)
				{
					mat = Material.Instantiate(materialOpaque);
					mat.SetColor(ClientManager.ColorName, color);
					if (fullbright || glow > 0.001f)
					{
						Color emissiveColor = color * (fullbright ? 1.0001f : ((1f + glow) * 2f));
						mat.SetColor(ClientManager.EmissiveColorName, emissiveColor);
						mat.SetTexture(ClientManager.EmissiveMapName, texture);
					}
					return mat;
				}

				return materialOpaque;
			}
			public Material GetMaterialAlpha(Color color, float glow, bool fullbright)
			{
				Material mat = fullbright ? ResourceCache.alphaFullbrightMaterial : ResourceCache.alphaMaterial;

				if (materialAlpha == null)
				{
					materialAlpha = Material.Instantiate(mat);
#if UNITY_EDITOR
					materialAlpha.name = $"alpha {uuid}";
#endif
					materialAlpha.SetTexture(ClientManager.DiffuseName, texture);
				}

				bool colorChange = color.r < 0.999f || color.g < 0.999f || color.b < 0.999f || color.a < 0.999f || glow > 0.001f || fullbright;

				if (colorChange)
				{
					mat = Material.Instantiate(materialAlpha);
					mat.SetColor(ClientManager.ColorName, color);
					if (fullbright || glow > 0.001f)
					{
						Color emissiveColor = color * (fullbright ? 1.0001f : ((1f + glow) * 2f));
						mat.SetColor(ClientManager.EmissiveColorName, emissiveColor);
						mat.SetTexture(ClientManager.EmissiveMapName, texture);
					}
					return mat;
				}

				return materialAlpha;
			}


			public MaterialContainer(UUID uuid, Texture2D texture, uint components)
			{
				this.uuid = uuid;
				this.texture = texture;
				this.components = components;
			}
		}

		public Dictionary<UUID, MaterialContainer> materialContainer = new();

		//request non-fullbright texture from the server
		/*		public void RequestTexture(UUID uuid, Renderer rendr, Color color, float glow, bool fullbright)
				{
					if (uuid == UUID.Zero) return;
					if (!materials.ContainsKey(uuid))
					{
						materials.Add(uuid, new List<Renderer>());
					}
					if (!materials[uuid].Contains(rendr))
					{
						materials[uuid].Add(rendr);
					}

					DissolveIn dis = rendr.gameObject.GetComponent<DissolveIn>();
					//Don't bother requesting a texture if it's already cached in memory;
					if (materialContainer.ContainsKey(uuid))
					{
						rendr.sharedMaterial = materialContainer[uuid].GetMaterial(color, glow, fullbright);
						if (materialContainer[uuid].ready) return;
					}
					else
					{
						materialContainer.Add(uuid, new MaterialContainer(uuid, Texture2D.Instantiate(Texture2D.whiteTexture), 3));
						if (dis == null)
						{
							rendr.sharedMaterial = materialContainer[uuid].GetMaterial(color, glow, fullbright);
						}
						else
						{
							dis.newMat = materialContainer[uuid].GetMaterial(color, glow, fullbright);
						}
					}
					materialContainer[uuid].ready = true;
					_assetManager.Textures.RequestImage(uuid);
				}*/

		public Material RequestTexture(UUID uuid, Renderer rendr, int subMeshIndex, Color color, float glow, bool fullbright)
		{

			if (uuid == UUID.Zero) return null;

			if (!materials.ContainsKey(uuid))
			{
				materials.Add(uuid, new List<Renderer>());
			}

			if (!materials[uuid].Contains(rendr))
			{
				materials[uuid].Add(rendr);
			}

			// DissolveIn dis = rendr.gameObject.GetComponent<DissolveIn>();

			if (!materialContainer.ContainsKey(uuid))
			{
				materialContainer.Add(uuid, new MaterialContainer(uuid, Texture2D.Instantiate(Texture2D.whiteTexture), 3));
			}
			Material newMaterial = newMaterial = materialContainer[uuid].GetMaterial(color, glow, fullbright);

			// Apply the new material to the specific submesh
			Material[] mats = rendr.materials;
			if (subMeshIndex < mats.Length)
			{
				mats[subMeshIndex] = newMaterial;
				rendr.materials = mats;
			}

			if (!materialContainer[uuid].ready)
			{
				DebugStatsManager.AddStateUpdate(DebugStatsType.TextureDownloadRequest, uuid.ToString());
				_assetManager.Textures.RequestImage(uuid);
				materialContainer[uuid].ready = true;
			}

			return newMaterial;
		}


		//request terrain texture from the server
		public Texture2D RequestTexture(UUID uuid)
		{
			if (uuid == UUID.Zero) return materialContainer[uuid].texture;
			if (!materials.ContainsKey(uuid)) materials.Add(uuid, new List<Renderer>());
			//materials[uuid].Add(rendr);
			//Don't bother requesting a texture if it's already cached in memory;
			if (materialContainer.ContainsKey(uuid))
			{
				return materialContainer[uuid].texture;
			}
			else
			{
				materialContainer.Add(uuid, new MaterialContainer(uuid, Texture2D.Instantiate(Texture2D.whiteTexture), 3));
				//return materialContainer[uuid].GetMaterial();
			}

			_assetManager.Textures.RequestImage(uuid);

			return materialContainer[uuid].texture;
		}




		public class SculptData
		{
			public GameObject gameObject;
			public Primitive prim;
		}

		//request sculpt texture from server
		public void RequestSculpt(GameObject gameObject, Primitive prim)
		{
			SculptData sculptdata = new()
			{
				gameObject = gameObject,
				prim = prim,
			};

			//store the gameObject and prim data for the object that requested the mesh
			//so that it can be applied once the data is ready
			requestedMeshes.TryAdd(prim.Sculpt.SculptTexture, new List<SculptData>());
			requestedMeshes[prim.Sculpt.SculptTexture].Add(sculptdata);

			_ = System.Threading.Tasks.Task.Run(() =>
			{
				ClientManager.client.Assets.RequestImage(prim.Sculpt.SculptTexture, CallbackSculptTexture);
			});
		}

		//callback that receives sculpt texture from the server,
		//then processes it into a mesh for use in Unity
		//if multithreaded sculpts are enabled, it adds data to a
		//ConcurrentQueue to be processed on the main thread
		public void CallbackSculptTexture(TextureRequestState state, AssetTexture assetTexture)
		{
			if (state != TextureRequestState.Finished) return;

#if !UNITY_ANDROID && !UNITY_IOS && !UNITY_EDITOR_OSX
			UUID id = assetTexture.AssetID;

			MeshmerizerR mesher = new();

			//FIXME Replace this decode with the native code DLL version
			try
			{
				var _ = assetTexture.Decode();
			}
			catch (Exception ex)
			{
				_log.LogError("Exception Decoding Sculpt Texture. " + ex.ToString());
				throw;
			}

			FacetedMesh fmesh;
			Primitive prim;
			try
			{
				// Call a method that might throw an exception
				if (!requestedMeshes.TryGetValue(id, out var sculptDataList)) return;
				if (sculptDataList.Count < 1) return;
				prim = sculptDataList[0].prim;
				fmesh = mesher.GenerateFacetedSculptMesh(requestedMeshes[id][0].prim, assetTexture.Image.ExportBitmap(), DetailLevel.Highest);
			}
			catch (Exception e)
			{
				Debug.Log(e);
				return;
				// Catch all exception cases individually
			}

			int j;

			foreach (SimManager.MeshRequestData mrd in simManager.meshRequests)
			{
				if (simManager.scenePrims.ContainsKey(mrd.localID))
				{
					simManager.scenePrims[mrd.localID].renderers = new Renderer[48];
				}

			}

			for (j = 0; j < fmesh.Faces.Count; j++)
			{

				if (fmesh.Faces[j].Vertices.Count == 0)
				{
					continue;
				}

				var item = new MeshQueueItem()
				{ uuid = prim.Sculpt.SculptTexture };

				for (j = 0; j < fmesh.Faces.Count; j++)
				{
					Primitive.TextureEntryFace textureEntryFace = prim.Textures.GetFace((uint)j);

					var face = fmesh.Faces[j];
					_transformTextureCoords.TransformTexCoords(face.Vertices, face.Center, textureEntryFace, prim.Scale);
					RawMeshData rmd = face.ToRawMeshData();
					item.meshData.Add(rmd);
				}
				concurrentMeshQueue.Enqueue(item);
				requestedMeshes.TryRemove(id, out var _);
			}
#endif
		}

		//Reinitialize a texture for use as a shaded texture
		//multi-threaded version.         
		public void MainThreadTextureReinitialize(byte[] bytes, UUID uuid, int width, int height, int components)
		{
			DebugStatsManager.AddStateUpdate(DebugStatsType.DecodedTextureProcess, uuid.ToString());

			if (components == 3)
			{
				materialContainer[uuid].texture.Reinitialize(width, height, TextureFormat.RGB24, false);
				//materialContainer[uuid].texture.Reinitialize(width, height, TextureFormat.BGRA32, false);
			}
			else
			{
				materialContainer[uuid].texture.Reinitialize(width, height, TextureFormat.RGBA32, false);
				//materialContainer[uuid].texture.Reinitialize(width, height, TextureFormat.ARGB32, false);                
				//materialContainer[uuid].texture.Reinitialize(width, height, TextureFormat.BGRA32, false);
			}

			materialContainer[uuid].texture.SetPixelData(bytes, 0);
			materialContainer[uuid].texture.name = $"{uuid} Comp:{components}";
			materialContainer[uuid].texture.Apply();
			materialContainer[uuid].texture.Compress(false);
			materialContainer[uuid].components = (uint)components;
			int i;

			List<Renderer> removeMaterials = new();
			DissolveIn dis;
			if (components == 4)
			{
				for (i = 0; i < materials[uuid].Count; i++)
				{
					if (materials[uuid][i] == null) continue;

					dis = materials[uuid][i].GetComponent<DissolveIn>();

					Primitive.TextureEntryFace textureEntryFace;
					PrimInfo pi = materials[uuid][i].GetComponent<PrimInfo>();
					if (!ClientManager.simManager.scenePrims.ContainsKey(pi.localID))
					{
						removeMaterials.Add(materials[uuid][i]);
						continue;
					}

					textureEntryFace = ClientManager.simManager.scenePrims[pi.localID].prim.Textures.GetFace((uint)pi.face);

					if (ClientManager.simManager.scenePrims.ContainsKey(pi.localID))
					{
						materials[uuid][i].name += " alpha";
						if (dis == null)
						{
							materials[uuid][i].sharedMaterial = materialContainer[uuid].GetMaterialAlpha(textureEntryFace.RGBA.ToUnity(), textureEntryFace.Glow, textureEntryFace.Fullbright);
						}
						else
						{
							dis.newMat = materialContainer[uuid].GetMaterialAlpha(textureEntryFace.RGBA.ToUnity(), textureEntryFace.Glow, textureEntryFace.Fullbright);
						}
					}

				}
				foreach (Renderer r in removeMaterials)
				{
					materials[uuid].Remove(r);
				}
				//Resources.UnloadUnusedAssets();
			}
		}

		public void RequestMesh2(GameObject gameObject, Primitive primitive, UUID uuid, GameObject meshHolder)
		{
			if (gameObject.IsDestroyed())
			{
				// log warning?
				return;
			}
			if (meshHolder.IsDestroyed())
			{
				// log warning?
				return;
			}

			_assetManager.Meshes.RequestMesh(gameObject, primitive, uuid, meshHolder);
		}

		public void RequestAnimation(Primitive primitive, UUID animationId)
		{
			_assetManager.AnimationManager.RequestAnimation(primitive,animationId);
		}

		private readonly ConcurrentDictionary<UUID, List<SculptData>> requestedMeshes = new();
	}
}

using System;
using OpenMetaverse;
using UnityEngine;
using Material = UnityEngine.Material;

// todo, material property block
// we need to run with game object and use mesh renderer to change property block
namespace Temp
{
	
		public class MaterialContainer
		{
			public bool ready = false;
			public Material materialOpaque;
			private MaterialPropertyBlock _opaqueBlock;
			
			public Material materialAlpha;
			private MaterialPropertyBlock _alphaBlock;
			
			public Material materialOpaqueFullbright;
			private MaterialPropertyBlock _opaqueFBBlock;
			public Material materialAlphaFullbright;
			private MaterialPropertyBlock _alphaFBBlock;
			
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

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ResourceCache
{
	public static readonly GameObject cubePrefab = Resources.Load<GameObject>("Cube");
	public static readonly UnityEngine.Material alphaMaterial = Resources.Load<UnityEngine.Material>(ClientManager.MaterialNameModifier + "Alpha Material");
	public static readonly UnityEngine.Material alphaFullbrightMaterial = Resources.Load<UnityEngine.Material>(ClientManager.MaterialNameModifier + "Alpha Fullbright Material");
	public static readonly UnityEngine.Material opaqueMaterial = Resources.Load<UnityEngine.Material>(ClientManager.MaterialNameModifier + "Opaque Material");
	public static readonly UnityEngine.Material opaqueFullbrightMaterial = Resources.Load<UnityEngine.Material>(ClientManager.MaterialNameModifier + "Opaque Fullbright Material");
	public static readonly GameObject empty = Resources.Load<GameObject>("Empty");
	public static readonly GameObject pointLight = Resources.Load<GameObject>("Point Light");
	public static readonly UnityEngine.Material additiveParticleMaterial = Resources.Load<Material>("Additive ParticleMaterial");
	public static readonly UnityEngine.Material particleMaterial = Resources.Load<Material>("ParticleMaterial");
	public static readonly Transform nameplate = Resources.Load<Transform>("Nameplate");

}

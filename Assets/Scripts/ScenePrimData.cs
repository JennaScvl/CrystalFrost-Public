using System;
using System.Collections.Generic;
using System.Linq; // used for Sum of array
using UnityEngine;
using OpenMetaverse;
using OpenMetaverse.Rendering;
#if useHDRP
using UnityEngine.Rendering.HighDefinition;
#endif
using CrystalFrost;
#if USE_KWS
using KWS;
using static KWS.WaterSystem;
#endif
#if USE_FUNLY_SKY
using Funly.SkyStudio;
#endif
using static Bunny.HUDHelper;
using OMVVector3 = OpenMetaverse.Vector3;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Mesh = UnityEngine.Mesh;
using Material = UnityEngine.Material;
using Quaternion = UnityEngine.Quaternion;
using CrystalFrost.Lib;
using CrystalFrost.Extensions;
using CrystalFrost.Assets.Mesh;
using CrystalFrost.Config;
using UnityEngine.Networking;
using YYClass;

public class ScenePrimData
{
	public Vector3 pos;
	public Primitive prim;

	public int ConstructionHash;
	public UUID uuid;
	public string name;
	public GameObject obj;

	public List<GameObject> faces = new();
	public GameObject meshHolder;
	public Renderer[] renderers;// = new List<Renderer[];
	public Transform nameTag;
	public TMPro.TMP_Text text;

	public Vector3 velocity;
	public Vector3 omega;
#if USE_KWS
	public WaterSystem water;
	public GameObject waterBox;
#endif
	public bool isWater = false;
	public bool particles = false;

	public bool parentIsHUD = false;

	public bool interactsWithWater = false;
	public GameObject waterInteractor;

	public ScenePrimData(GameObject go, Primitive p)
	{
		ClientManager.simManager.scenePrimIndexUUID.TryAdd(p.ID, p.LocalID);
		obj = go;
		prim = p;
		obj.name = $"Object {prim.LocalID}";
		meshHolder = SimManager.Instantiate<GameObject>(ResourceCache.empty);
		//cameraRoot.gameObject.GetComponent<CFOcclusion>().agedMeshHolders.Add(meshHolder, Time.realtimeSinceStartupAsDouble);
		meshHolder.AddComponent<LODGroup>();
		meshHolder.transform.parent = obj.transform;
		meshHolder.transform.SetLocalPositionAndRotation(Vector3.zero,Quaternion.identity);
		meshHolder.name = $"mesh Holder {prim.LocalID}";
		velocity = Vector3.zero;
		omega = Vector3.zero;
	}
}

public static class ScenePrimDataExtensions
{
	public static void MakeWater(this ScenePrimData spd)
	{

#if USE_KWS

		if (spd.isWater) return;
		//Debug.Log("MakeWater");
		spd.isWater = true;

		foreach (Renderer renderer in spd.renderers)
		{
			if (renderer != null) renderer.enabled = false;
		}

		var waterBox = spd.waterBox;
		var prim = spd.prim;
		var water = spd.water;
		var meshHolder = spd.meshHolder;

		waterBox = SimManager.Instantiate(Resources.Load<GameObject>("WaterBox"));
		waterBox.name = $"WaterBox: {prim.LocalID}";
		//waterBox.GetComponent<Renderer>().enabled = false;
		//MeshFilter meshF = waterBox.GetComponent<MeshFilter>();
		//Mesh mesh = new Mesh();// Instantiate<Mesh>(meshF.mesh);
		Vector3 scale = prim.Scale.ToVector3();

		waterBox.transform.position = meshHolder.transform.position + new Vector3(0f, scale.y * 0.5f, 0f);
		waterBox.transform.rotation = meshHolder.transform.rotation * Quaternion.Euler(0, 0, 0f);


		//Add WaterSystem to box.
		water = waterBox.GetComponent<WaterSystem>();


		//Color Settings

		//generate water color from color of prim's face 0
		//then rotate the hue and reduce the value for turbidity color
		Color waterColor = prim.Textures.GetFace(0).RGBA.ToUnity();
		Color turbidityColor = Color.white;
		float hue; float sat; float val;
		Color.RGBToHSV(waterColor, out hue, out sat, out val);
		turbidityColor = Color.HSVToRGB(Mathf.Repeat(hue - 0.05f, 1.0f), sat, Mathf.Clamp(val * 0.5f, 0.0f, 1f));
		turbidityColor.a = 1f;
		waterColor.a = 1f;
		spd.SizeWater(scale);


		water.Settings.WaterColor = waterColor;
		water.Settings.TurbidityColor = turbidityColor;
		water.Settings.Transparent = (25f - (prim.Textures.GetFace(0).RGBA.ToUnity().a * 25f)) * 0.5f;
		water.Settings.Turbidity = 0.142f;
		//water.Settings.WaterMeshType = WaterMeshTypeEnum.FiniteBox;


		//Waves Settings
		//water.Settings.ShowExpertWavesSettings = false;
		water.Settings.FFT_SimulationSize = FFT_GPU.SizeSetting.Size_512;
		//water.Settings.UseMultipleSimulations = false;
		water.Settings.WindSpeed = 0.6f;
		water.Settings.WindRotation = 341f;
		water.Settings.WindTurbulence = 0.25f;
		water.Settings.TimeScale = 0.9f;

		//Reflection Settings
		water.Settings.ReflectionProfile = WaterProfileEnum.High;
		//water.Settings.ReflectionMode = ReflectionModeEnum.ScreenSpaceReflection;
		water.Settings.CubemapUpdateInterval = 6;
		water.Settings.CubemapCullingMask = LayerMask.GetMask(new string[] { "Default", "Terrain", "Glow", "Transparent", "Transparent FX", "Ignore Raycast" });//KWS_Settings.water.Settings.DefaultCubemapCullingMask;
		water.Settings.CubemapReflectionResolutionQuality = CubemapReflectionResolutionQualityEnum.High;
		//water.Settings.PlanarReflectionResolutionQuality = PlanarReflectionResolutionQualityEnum.Medium;
		water.Settings.ScreenSpaceReflectionResolutionQuality = ScreenSpaceReflectionResolutionQualityEnum.High;
		//water.Settings.UsePlanarCubemapReflection = true;
		//water.Settings.ReflectionClearFlag = ReflectionClearFlagEnum.Skybox;
		//water.Settings.ReflectionClearColor = Color.black;
		//water.Settings.ReflectionClipPlaneOffset = 0.0075f;
		//water.Settings.ReflectioDepthHolesFillDistance = 5;

		water.Settings.UseAnisotropicReflections = true;
		water.Settings.AnisotropicReflectionsHighQuality = false;
		water.Settings.AnisotropicReflectionsScale = 0.55f;

		water.Settings.ReflectSun = true;
		water.Settings.ReflectedSunCloudinessStrength = 0.04f;
		water.Settings.ReflectedSunStrength = 1.0f;


		//Refraction settings
		water.Settings.RefractionProfile = WaterProfileEnum.High;
		water.Settings.RefractionMode = RefractionModeEnum.PhysicalAproximationIOR;
		water.Settings.RefractionAproximatedDepth = 2f;
		water.Settings.UseRefractionDispersion = true;
		water.Settings.RefractionDispersionStrength = 0.35f;

		//Volumetric settings
		water.Settings.VolumetricLightProfile = WaterProfileEnum.High;
		water.Settings.UseVolumetricLight = true;
		//water.Settings.ShowVolumetricLightSettings = false;
		//water.Settings.ShowExpertVolumetricLightSettings = false;
		water.Settings.VolumetricLightResolutionQuality = VolumetricLightResolutionQualityEnum.High;
		water.Settings.VolumetricLightIteration = 6;
		water.Settings.VolumetricLightBlurRadius = 1.0f;
		water.Settings.VolumetricLightFilter = VolumetricLightFilterEnum.Gaussian;


		//FlowMap settings
		//FlowmapProfile = WaterProfileEnum.High;
		water.Settings.UseFlowMap = false;
		//water.Settings.ShowFlowMap = false;
		//water.Settings.ShowExpertFlowmapSettings = false;
		//water.Settings.FlowMapInEditMode = false;
		//water.Settings.FlowMapAreaPosition = new Vector3(0, 0, 0);
		//water.Settings.FlowMapAreaSize = 200;
		//water.Settings.FlowMapTextureResolution = FlowmapTextureResolutionEnum._2048;
		//water.Settings.FlowMapBrushStrength = 0.75f;
		//water.Settings.FlowMapSpeed = 1;

		//water.Settings.UseFluidsSimulation = false;
		//water.Settings.FluidsAreaSize = 40;
		//water.Settings.FluidsSimulationIterrations = 2;
		//water.Settings.FluidsTextureSize = 1024;
		//water.Settings.FluidsSimulationFPS = 60;
		//water.Settings.FluidsSpeed = 1;
		//water.Settings.FluidsFoamStrength = 0.5f;

		//Dynamic waves settings
		water.Settings.DynamicWavesProfile = WaterProfileEnum.High;
		water.Settings.UseDynamicWaves = true;
		//water.Settings.ShowDynamicWaves = false;
		//water.Settings.ShowExpertDynamicWavesSettings = false;
		water.Settings.DynamicWavesAreaSize = 50;
		water.Settings.DynamicWavesSimulationFPS = 15;
		water.Settings.DynamicWavesResolutionPerMeter = 34;
		water.Settings.DynamicWavesPropagationSpeed = 0.5f;
		water.Settings.UseDynamicWavesRainEffect = false;
		//water.Settings.DynamicWavesRainStrength = 0.2f;


		//Shoreline settings
		//water.Settings.ShorelineProfile = WaterProfileEnum.High;
		water.Settings.UseShorelineRendering = false;
		//water.Settings.ShowShorelineMap = false;
		//water.Settings.ShowExpertShorelineSettings = false;
		//water.Settings.FoamLodQuality = QualityEnum.Medium;
		//water.Settings.FoamCastShadows = true;
		//water.Settings.FoamReceiveShadows = false;
		//water.Settings.ShorelineInEditMode = false;
		//water.Settings.ShorelineAreaPosition;
		//water.Settings.ShorelineAreaSize = 512;
		//water.Settings.ShorelineCurvedSurfacesQuality = QualityEnum.Medium;


		//Caustic settings
		water.Settings.CausticProfile = WaterProfileEnum.High;
		water.Settings.UseCausticEffect = true;
		//water.Settings.ShowCausticEffectSettings = false;
		//water.Settings.ShowExpertCausticEffectSettings = false;
		water.Settings.UseCausticBicubicInterpolation = true;
		water.Settings.UseCausticDispersion = true;
		water.Settings.CausticTextureSize = 1024;
		water.Settings.CausticMeshResolution = 320;
		water.Settings.CausticActiveLods = 4;
		water.Settings.CausticStrength = 1;
		water.Settings.UseDepthCausticScale = false;
		//water.Settings.CausticDepthScaleInEditMode = false;
		//water.Settings.CausticDepthScale = 1;
		//water.Settings.CausticOrthoDepthPosition = Vector3.positiveInfinity;
		//water.Settings.CausticOrthoDepthAreaSize = 512;
		//water.Settings.CausticOrthoDepthTextureResolution = 2048;

		//Underwater settings
		water.Settings.UseUnderwaterEffect = true;
		//water.Settings.ShowUnderwaterEffectSettings = false;
		water.Settings.UseUnderwaterBlur = false;
		//water.Settings.UnderwaterBlurRadius = 1.5f;

		water.Settings.MeshProfile = WaterProfileEnum.High;
		water.Settings.WaterMeshType = WaterMeshTypeEnum.FiniteBox;
		//water.Settings.RiverSplineNormalOffset = 1;
		//water.Settings.RiverSplineVertexCountBetweenPoints = 20;
		//water.Settings.CustomMesh = mesh;
		//water.Settings.InfiniteMeshQuality = WaterInfiniteMeshQualityEnum._100k;
		//water.Settings.FiniteMeshQuality = WaterFiniteMeshQualityEnum._50k;
		//water.Settings.MeshSize = new Vector3(scale.x, scale.y, scale.z);
		//water.Settings.UseTesselation = true;
		//water.Settings.TesselationFactor = 0.6f;
		//water.Settings.TesselationInfiniteMeshMaxDistance = 2000f;
		//water.Settings.TesselationOtherMeshMaxDistance = 200f;
		// water.Settings.InitializeOrUpdateMesh();

		//waterBox.transform.parent = meshHolder.transform.parent;
#endif
	}//

	public static void SizeWater(this ScenePrimData spd, Vector3 vec)
	{
#if USE_KWS
		spd.water.Settings.MeshSize = new Vector3(vec.x, vec.y, vec.z);
		spd.waterBox.transform.position = spd.meshHolder.transform.position + new Vector3(0f, vec.y * 0.5f, 0f);
		spd.waterBox.transform.rotation = spd.meshHolder.transform.rotation * Quaternion.Euler(0, 0, 0f);
#endif
	}

	public static void ResizeWater(this ScenePrimData spd, Vector3 vec)
	{
#if USE_KWS
		spd.water.Settings.MeshSize = new Vector3(vec.x, vec.y, vec.z);
		spd.waterBox.transform.position = spd.meshHolder.transform.position + new Vector3(0f, vec.y * 0.5f, 0f);
		spd.water.ForceUpdateWaterSettings();
#endif
	}

	public static void MakeInteractWithWater(this ScenePrimData spd)
	{
#if USE_KWS
		if (spd.interactsWithWater) return;
		spd.waterInteractor = SimManager.Instantiate(Resources.Load<GameObject>("WaterInteractor"));
		KW_InteractWithWater interactor = spd.waterInteractor.GetComponent<KW_InteractWithWater>();

		//make the size of the interactor be the average scale;
		float size = (spd.prim.Scale.X + spd.prim.Scale.Y + spd.prim.Scale.Z) * 0.3333333f;
		interactor.Size = (spd.prim.Scale.X + spd.prim.Scale.Y + spd.prim.Scale.Z) * 0.3333333f;

		spd.waterInteractor.transform.parent = spd.meshHolder.transform;
		spd.waterInteractor.transform.localPosition = Vector3.zero;
		spd.waterInteractor.transform.localScale = Vector3.one;
		interactor.Strength = Mathf.Clamp(size * 0.1f, 0f, 1f);
		spd.interactsWithWater = true;

		if (spd.prim.PrimData.PCode == PCode.Avatar)
		{
			SimManager.Destroy(spd.waterInteractor);
			for (int i = 0; i < Mathf.Round(spd.prim.Scale.Z / 0.25f); i++)
			{
				spd.waterInteractor = SimManager.Instantiate(Resources.Load<GameObject>("WaterInteractor"));
				interactor = spd.waterInteractor.GetComponent<KW_InteractWithWater>();

				//make the size of the interactor be the average scale;
				interactor.Size = (spd.prim.Scale.X + spd.prim.Scale.Y + spd.prim.Scale.Z) * 0.3333333f;

				spd.waterInteractor.transform.parent = spd.meshHolder.transform;
				spd.waterInteractor.transform.localPosition = new Vector3(0f, (spd.prim.Scale.Z * -0.5f) + (0.25f * (float)i), 0f);
				spd.waterInteractor.transform.localScale = Vector3.one;
				interactor.Strength = Mathf.Clamp(size * 0.1f, 0f, 1f);
				spd.interactsWithWater = true;
			}
		}
#endif
	}

	public static void SetProperties(this ScenePrimData spd, Primitive.ObjectProperties properties)
	{
		spd.prim.Properties = properties;

		if (properties.Description.Contains("#WATERBOX")) spd.MakeWater();
		else if (properties.Description.Contains("#SPLOOSH")) spd.MakeInteractWithWater();

		spd.SetName($"Object: {properties.Name}");
		//ClientManager.client.Objects.SelectObject
		//    SelectObjects(ClientManager.client.Network.CurrentSim, new uint[] { prim.LocalID }); break;

		//SetName(properties.Name);
	}

	public static void SetName(this ScenePrimData spd, string name)
	{
		spd.name = name;
		spd.obj.transform.parent.name = name;
		if (spd.text != null) spd.text.text = name;
	}

	public static void Render(this ScenePrimData spd)
	{
		MeshRenderer mr;
		//if (prim.IsAttachment)
		//{
		mr = spd.obj.GetComponent<MeshRenderer>();
		mr.enabled = false;
		//	return;
		//}
		if (spd.renderers != null)
		{
			foreach (Renderer r in spd.renderers)
			{
				if (r != null) r.enabled = true;//DestroyImmediate(r.gameObject);
			}
			//renderers = null;
#if USE_KWS
			if (spd.isWater)
			{
				spd.isWater = false;
				SimManager.Destroy(spd.water);
				SimManager.Destroy(spd.waterBox);
				spd.water = null;
				spd.waterBox = null;
			}
#endif
		}

		if (spd.prim.Velocity.ToVector3() == Vector3.zero && spd.prim.AngularVelocity.ToVector3() == Vector3.zero)
			spd.GetObjectProperties();

		switch (spd.prim.Type)
		{
			case PrimType.Sculpt:
				//Debug.Log("Is Sculpt");
				spd.obj.name = $"sculpt: {spd.prim.LocalID}";
				mr = spd.obj.GetComponent<MeshRenderer>();
				mr.enabled = false;
				//Request mesh from server.
				ClientManager.simManager.meshRequests.Add(new SimManager.MeshRequestData(spd.prim.LocalID, spd.prim.Sculpt.SculptTexture, spd.meshHolder));
				ClientManager.assetManager.RequestSculpt(spd.meshHolder, spd.prim);
				DebugStatsManager.AddStateUpdate(DebugStatsType.SculptDownloadRequest, spd.prim.LocalID.ToString());
				if (!spd.prim.IsAttachment && spd.prim.Velocity.Length() == 0f && spd.prim.AngularVelocity.Length() == 0f) ClientManager.client.Objects.SelectObject(ClientManager.client.Network.CurrentSim, spd.prim.LocalID);
				break;
			case PrimType.Unknown:
				break;
			case PrimType.Mesh:
				spd.obj.name = $"mesh: {spd.prim.LocalID}";
				mr = spd.obj.GetComponent<MeshRenderer>();
				mr.enabled = false;

				ClientManager.assetManager.RequestMesh2(spd.obj, spd.prim, spd.prim.Sculpt.SculptTexture, spd.meshHolder);
				DebugStatsManager.AddStateUpdate(DebugStatsType.MeshDownloadRequest, spd.prim.LocalID.ToString());

				if (!spd.prim.IsAttachment && spd.prim.Velocity.Length() == 0f && spd.prim.AngularVelocity.Length() == 0f) ClientManager.client.Objects.SelectObject(ClientManager.client.Network.CurrentSim, spd.prim.LocalID);
				break;
			default:
				spd.RenderPrim();
				break;
		}

		if (spd.prim.Light != null)
		{
			/* //Crystal Frost abandoned prototype HDRP light settings
            //Debug.Log("light");
            GameObject golight = Instantiate<GameObject>(Resources.Load<GameObject>("Point Light"));
            golight.transform.parent = go.transform;
            children.Add(golight);
            golight.transform.localPosition = Vector3.zero;
            golight.transform.localRotation = Quaternion.identity;
            Light light = golight.GetComponent<Light>();
            HDAdditionalLightData hdlight = light.GetComponent<HDAdditionalLightData>();

            //light. = prim.Light.Radius;
            hdlight.color = prim.Light.Color.ToUnity();
            //HDRP requires insane amounts of lumens to make lights show up like they do in SL
            //Not sure why, but yeah...
            hdlight.intensity = prim.Light.Intensity * 10000000f;
            hdlight.range = prim.Light.Radius;
            */

			GameObject golight = SimManager.Instantiate<GameObject>(ResourceCache.pointLight);
			golight.transform.parent = spd.obj.transform;
			//children.Add(golight);
			golight.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
			Light light = golight.GetComponent<Light>();
			light.range = spd.prim.Light.Radius;
			light.color = spd.prim.Light.Color.ToUnity();
			light.intensity = spd.prim.Light.Intensity;
		}


		if (spd.prim.ParticleSys.Pattern != Primitive.ParticleSystem.SourcePattern.None)
		{
			//return;
			UnityEngine.ParticleSystem ps = spd.obj.AddComponent<UnityEngine.ParticleSystem>();
			spd.SetupParticles();
		}
	}

	public static void SetupParticles(this ScenePrimData spd)
	{
		//return;
		//if (particles) return;
		if (spd.prim.ParticleSys.Pattern != Primitive.ParticleSystem.SourcePattern.None)
		{
			//Debug.Log($"ParticleSystem found on {prim.LocalID}");
			UnityEngine.ParticleSystem ps = spd.obj.GetComponent<UnityEngine.ParticleSystem>();
			if(ps == null) ps = spd.obj.AddComponent<UnityEngine.ParticleSystem>();
			//GameObject goParticleSystem = Resources.Load<GameObject>("Particle System");
			if (!spd.particles) ps.Stop();
			var main = ps.main;

			AnimationCurve burstSpeedCurve = new();
			burstSpeedCurve.AddKey(spd.prim.ParticleSys.BurstSpeedMin, 0f);
			burstSpeedCurve.AddKey(spd.prim.ParticleSys.BurstSpeedMax, 1f);
			main.startSpeed = new UnityEngine.ParticleSystem.MinMaxCurve(1f, burstSpeedCurve);

			main.startLifetime = spd.prim.ParticleSys.PartMaxAge;
			//main.startDelay = prim.ParticleSys.StartAge;
			if (!spd.particles)
			{
				if (spd.prim.ParticleSys.MaxAge != 0f)
				{
					main.loop = false;
					main.duration = spd.prim.ParticleSys.MaxAge;
				}
			}

			var em = ps.emission;
			em.enabled = true;
			em.rateOverTime = spd.prim.ParticleSys.BurstPartCount / spd.prim.ParticleSys.BurstRate;

			var fo = ps.forceOverLifetime;

			fo.enabled = true;
			Vector3 vec = spd.prim.ParticleSys.PartAcceleration.ToVector3();
			fo.x = vec.x / spd.obj.transform.lossyScale.x;
			fo.y = vec.y / spd.obj.transform.lossyScale.y;
			fo.z = vec.z / spd.obj.transform.lossyScale.z;

			vec = spd.prim.ParticleSys.AngularVelocity.ToVector3() * Mathf.Rad2Deg;

			var vel = ps.velocityOverLifetime;
			vel.enabled = true;

			vel.orbitalX = vec.x;
			vel.orbitalY = vec.y;
			vel.orbitalZ = vec.z;

			var sh = ps.shape;
			sh.enabled = true;
			sh.radius = (spd.prim.ParticleSys.BurstRadius / spd.obj.transform.lossyScale.magnitude) * 2f;
			sh.rotation = new Vector3(-90f, 0f, 0f);

			Material mat;

			if (!spd.particles)
			{
				if (spd.prim.ParticleSys.BlendFuncDest == 0x0)
					mat = SimManager.Instantiate(ResourceCache.additiveParticleMaterial);
				else
					mat = SimManager.Instantiate(ResourceCache.particleMaterial);
			}

			ParticleSystemRenderer r = ps.GetComponent<ParticleSystemRenderer>();
			r.material = SimManager.Instantiate(ResourceCache.particleMaterial);
			r.material.SetTexture("_BaseMap", ClientManager.assetManager.RequestTexture(spd.prim.ParticleSys.Texture));

			switch (spd.prim.ParticleSys.Pattern)
			{
				case Primitive.ParticleSystem.SourcePattern.Angle:
					sh.shapeType = ParticleSystemShapeType.Circle;
					sh.arc = (spd.prim.ParticleSys.OuterAngle - spd.prim.ParticleSys.InnerAngle) * Mathf.Rad2Deg;
					break;
				case Primitive.ParticleSystem.SourcePattern.Drop:
					sh.shapeType = ParticleSystemShapeType.Sphere;
					sh.radius = 0f;
					main.startSpeed = 0f;
					break;
				case Primitive.ParticleSystem.SourcePattern.Explode:
					sh.shapeType = ParticleSystemShapeType.Sphere;
					sh.radiusThickness = 1f;
					sh.arc = 360f;
					break;
				case Primitive.ParticleSystem.SourcePattern.AngleCone:
					sh.shapeType = ParticleSystemShapeType.Cone;
					sh.angle = (spd.prim.ParticleSys.OuterAngle - spd.prim.ParticleSys.InnerAngle) * Mathf.Rad2Deg;
					break;
				case Primitive.ParticleSystem.SourcePattern.AngleConeEmpty:
					sh.shapeType = ParticleSystemShapeType.Sphere;
					sh.radius = 0f;
					main.startSpeed = 0f;
					break;
			}

			var col = ps.colorOverLifetime;
			col.enabled = true;
			Gradient grad = new();
			grad.SetKeys(
				new GradientColorKey[]
				{
						new GradientColorKey(spd.prim.ParticleSys.PartStartColor.ToUnity(), 0f),
						new GradientColorKey(spd.prim.ParticleSys.PartEndColor.ToUnity(), 0f)
				},
				new GradientAlphaKey[]
				{
						new GradientAlphaKey(spd.prim.ParticleSys.PartStartColor.A, 0f),
						new GradientAlphaKey(spd.prim.ParticleSys.PartEndColor.A, 1f)
				});
			col.color = grad;

			var sz = ps.sizeOverLifetime;
			sz.enabled = true;
			sz.separateAxes = true;
			AnimationCurve xcurve = new();
			xcurve.AddKey(spd.prim.ParticleSys.PartStartScaleX / spd.obj.transform.lossyScale.x, 0f);
			xcurve.AddKey(spd.prim.ParticleSys.PartEndScaleX / spd.obj.transform.lossyScale.x, 1f);
			AnimationCurve ycurve = new();
			ycurve.AddKey(spd.prim.ParticleSys.PartStartScaleY / spd.obj.transform.lossyScale.y, 0f);
			ycurve.AddKey(spd.prim.ParticleSys.PartEndScaleY / spd.obj.transform.lossyScale.y, 1f);
			AnimationCurve zcurve = new();
			zcurve.AddKey(1f / spd.obj.transform.lossyScale.z, 0f);
			zcurve.AddKey(1f / spd.obj.transform.lossyScale.z, 1f);
			//UnityEngine.ParticleSystem.MinMaxCurve minMaxCurveX = new UnityEngine.ParticleSystem.MinMaxCurve(1f, xcurve);
			//UnityEngine.ParticleSystem.MinMaxCurve minMaxCurveY = new UnityEngine.ParticleSystem.MinMaxCurve(1f, ycurve);
			sz.x = new UnityEngine.ParticleSystem.MinMaxCurve(1f, xcurve);
			sz.y = new UnityEngine.ParticleSystem.MinMaxCurve(1f, ycurve);
			sz.z = new UnityEngine.ParticleSystem.MinMaxCurve(1f, zcurve);

			if (spd.prim.ParticleSys.PartDataFlags.HasFlag(Primitive.ParticleSystem.ParticleDataFlags.Bounce))
			{
				var co = ps.collision;
				co.enabled = true;
				co.quality = ParticleSystemCollisionQuality.Low;
				co.enableDynamicColliders = true;
				co.collidesWith = LayerMask.GetMask(new string[] { "Default", "Transparent", "Glow", "Terrain", "Water" });
				co.type = ParticleSystemCollisionType.World;
				co.mode = ParticleSystemCollisionMode.Collision3D;
				co.maxCollisionShapes = 10;
			}

			if (spd.prim.ParticleSys.PartDataFlags.HasFlag(Primitive.ParticleSystem.ParticleDataFlags.FollowSrc))
			{
				main.simulationSpace = ParticleSystemSimulationSpace.Local;
			}
			else
			{
				main.simulationSpace = ParticleSystemSimulationSpace.World;
			}

			if (spd.prim.ParticleSys.HasGlow())
			{
				spd.obj.layer = 7;
			}
			else
			{
				spd.obj.layer = 0;
			}


			if (!spd.particles) ps.Play();
			spd.particles = true;
		}
		else if (spd.particles)
		{
			SimManager.Destroy(spd.obj.GetComponent<UnityEngine.ParticleSystem>());
			spd.particles = false;
		}

	}

	private static readonly ITransformTexCoords _transformTexCoords = Services.GetService<ITransformTexCoords>();

	/*	public static Renderer[] GeneratePrim(this ScenePrimData spd, DetailLevel detailLevel)
		{
			if (spd.prim.PrimData.PCode == PCode.Avatar) return new Renderer[0];

			Renderer[] _renderers = new Renderer[48];
			MeshmerizerR mesher = new();
			FacetedMesh fmesh = mesher.GenerateFacetedMesh(spd.prim, detailLevel);

			int j;

			for (j = 0; j < fmesh.Faces.Count; j++)
			{
				GameObject gomesh;
				Renderer rendr;
				MeshFilter meshFilter;

				if (_renderers[j] == null)
				{
					gomesh = SimManager.Instantiate(ResourceCache.cubePrefab);
	#if UNITY_EDITOR
					gomesh.name = $"face {j}.";
	#endif
					gomesh.transform.SetPositionAndRotation(spd.obj.transform.localPosition, spd.obj.transform.localRotation);
					gomesh.transform.parent = spd.meshHolder.transform;
					gomesh.transform.localScale = Vector3.one;
					rendr = gomesh.GetComponent<Renderer>();
					_renderers[j] = rendr;
				}
				else
				{
					rendr = _renderers[j];
					gomesh = rendr.gameObject;
					rendr.enabled = true;
					rendr.GetComponent<MeshFilter>().mesh = null;
				}

				PrimInfo pi = gomesh.GetComponent<PrimInfo>();
				pi.face = j;
				if (spd.prim.LocalID == 0) Debug.Log("local ID cannot be 0");
				pi.localID = spd.prim.LocalID;
				pi.uuid = spd.uuid;

				spd.faces.Add(gomesh);

				var faceVertices = fmesh.Faces[j].Vertices;
				int vertexCount = faceVertices.Count;

				Vector3[] vertices = new Vector3[vertexCount];
				Vector3[] normals = new Vector3[vertexCount];
				Vector2[] uvs = new Vector2[vertexCount];

				meshFilter = gomesh.GetComponent<MeshFilter>();
				Primitive.TextureEntryFace textureEntryFace = spd.prim.Textures.GetFace((uint)j);

				_transformTexCoords.TransformTexCoords(faceVertices, fmesh.Faces[j].Center, textureEntryFace, spd.prim.Scale);

				for (int i = 0; i < vertexCount; i++)
				{
					vertices[i] = faceVertices[i].Position.ToUnity();
					normals[i] = faceVertices[i].Normal.ToUnity() * -1f;
					uvs[i] = faceVertices[i].TexCoord.ToUnity();
				}

				Mesh mesh = meshFilter.mesh;
				mesh.Clear();
				mesh.vertices = vertices;
				mesh.normals = normals;
				mesh.uv = uvs;
				mesh.SetIndices(fmesh.Faces[j].Indices, MeshTopology.Triangles, 0);
				mesh.MarkDynamic();
				mesh = mesh.ReverseWind().FlipNormals();
				//meshFilter.mesh = mesh;//mesh.ReverseWind().FlipNormals();
				meshFilter.sharedMesh = mesh;

				if (!AreVerticesDistinct(mesh.vertices))
				{
					MeshCollider mc = gomesh.GetComponent<MeshCollider>();
					mc.sharedMesh = null; // Clear the shared mesh before setting cooking options
					mc.cookingOptions = MeshColliderCookingOptions.None;
					mc.sharedMesh = mesh; // Assign the mesh after setting cooking options
					mc.enabled = true; // Enable the collider
				}

				Color color = textureEntryFace.RGBA.ToUnity();
				Material clonemat;
				string colorstring = "_BaseColor";

				if (color.a < 0.999f)
				{
					clonemat = new Material(textureEntryFace.Fullbright ? ResourceCache.alphaFullbrightMaterial : ResourceCache.alphaMaterial);
				}
				else
				{
					clonemat = new Material(textureEntryFace.Fullbright ? ResourceCache.opaqueFullbrightMaterial : ResourceCache.opaqueMaterial);
				}

				rendr.material = clonemat;
				rendr.material.SetColor(colorstring, color);
				pi.prim = spd.prim;
				SimManager.PreTextureFace(spd.prim, j, rendr);
			}

			return _renderers;
		}*/

	public static Renderer[] GeneratePrim(this ScenePrimData spd, DetailLevel detailLevel)
	{
		if (spd.prim.PrimData.PCode == PCode.Avatar) return new Renderer[0];

		MeshmerizerR mesher = new();
		FacetedMesh fmesh = mesher.GenerateFacetedMesh(spd.prim, detailLevel);

		GameObject gomesh;
		Renderer rendr;
		MeshFilter meshFilter;
		Renderer _renderers = new Renderer();
		// Create a single GameObject if it doesn't exist
		if (_renderers == null)
		{
			gomesh = SimManager.Instantiate(ResourceCache.cubePrefab);
#if UNITY_EDITOR
			gomesh.name = "Combined Mesh";
#endif
			//gomesh.transform.SetPositionAndRotation(spd.obj.transform.localPosition, spd.obj.transform.localRotation);
			gomesh.transform.parent = spd.meshHolder.transform;
			// Instead of setting the global position of the combined mesh directly
			// Reset the local position and rotation of 'gomesh' relative to its parent which already set at correct position
			gomesh.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
			gomesh.transform.localScale = Vector3.one;
			rendr = gomesh.GetComponent<Renderer>();
			_renderers = rendr;
		}
		else
		{
			rendr = _renderers;
			gomesh = rendr.gameObject;
			rendr.enabled = true;
			rendr.GetComponent<MeshFilter>().mesh = null;
		}
		PrimInfo pi = gomesh.GetComponent<PrimInfo>();
		if (spd.prim.LocalID == 0) Debug.Log("local ID cannot be 0");
		pi.localID = spd.prim.LocalID;
		pi.uuid = spd.uuid;

		List<Vector3> allVertices = new List<Vector3>();
		List<Vector3> allNormals = new List<Vector3>();
		List<Vector2> allUvs = new List<Vector2>();
		List<Material> materials = new List<Material>();
		List<int> subMeshIndices = new List<int>();

		int vertexOffset = 0;

		for (int j = 0; j < fmesh.Faces.Count; j++)
		{
			var faceVertices = fmesh.Faces[j].Vertices;
			int vertexCount = faceVertices.Count;

			Vector3[] vertices = new Vector3[vertexCount];
			Vector3[] normals = new Vector3[vertexCount];
			Vector2[] uvs = new Vector2[vertexCount];

			Primitive.TextureEntryFace textureEntryFace = spd.prim.Textures.GetFace((uint)j);

			_transformTexCoords.TransformTexCoords(faceVertices, fmesh.Faces[j].Center, textureEntryFace, spd.prim.Scale);

			for (int i = 0; i < vertexCount; i++)
			{
				vertices[i] = faceVertices[i].Position.ToUnity();
				normals[i] = faceVertices[i].Normal.ToUnity() * -1f;
				uvs[i] = faceVertices[i].TexCoord.ToUnity();
			}

			allVertices.AddRange(vertices);
			allNormals.AddRange(normals);
			allUvs.AddRange(uvs);

			int[] indices = fmesh.Faces[j].Indices.Select(index => index + vertexOffset).ToArray();
			subMeshIndices.AddRange(indices);

			vertexOffset += vertexCount;

			Color color = textureEntryFace.RGBA.ToUnity();
			Material clonemat;

			if (color.a < 0.999f)
			{
				clonemat = new Material(textureEntryFace.Fullbright ? ResourceCache.alphaFullbrightMaterial : ResourceCache.alphaMaterial);
			}
			else
			{
				clonemat = new Material(textureEntryFace.Fullbright ? ResourceCache.opaqueFullbrightMaterial : ResourceCache.opaqueMaterial);
			}

			clonemat.SetColor("_BaseColor", color);

			materials.Add(clonemat);
		}

		rendr.materials = materials.ToArray();

		meshFilter = gomesh.GetComponent<MeshFilter>();
		Mesh mesh = new Mesh();
		mesh.SetVertices(allVertices);
		mesh.SetNormals(allNormals);
		mesh.SetUVs(0, allUvs);

		mesh.subMeshCount = fmesh.Faces.Count;

		int offset = 0;
		for (int j = 0; j < fmesh.Faces.Count; j++)
		{

			pi.face = j;
			int count = fmesh.Faces[j].Indices.Count;
			mesh.SetTriangles(subMeshIndices.GetRange(offset, count), j);
			offset += count;
			ClientManager.simManager.TextureFace(spd.prim, j, rendr);
		}

		meshFilter.sharedMesh = mesh.ReverseWind().FlipNormals();


		return new Renderer[] { rendr };
	}

	// New Helper function to replace LINQ call
	public static bool AreVerticesDistinct(Vector3[] vertices)
	{
		// Use a HashSet to collect distinct vertices
		HashSet<Vector3> distinctVertices = new HashSet<Vector3>();

		// Iterate through the vertices and add them to the HashSet
		for (int i = 0; i < vertices.Length; i++)
		{
			distinctVertices.Add(vertices[i]);

			// If more than 2 distinct vertices are found, return false
			if (distinctVertices.Count > 2)
			{
				return false;
			}
		}

		// If there are 2 or fewer distinct vertices, return true
		return true;
	}



	public static void RenderPrim(this ScenePrimData spd)
	{
		var primData = spd.prim.PrimData;
		if (primData.PCode != PCode.Prim && primData.PCode != PCode.Avatar) return;

		spd.obj.GetComponent<Renderer>().enabled = false;
		LODGroup group = spd.meshHolder.GetComponent<LODGroup>();
		spd.obj.name = $"prim: {spd.prim.LocalID}";

		Renderer[] highest = spd.GeneratePrim(DetailLevel.Highest);
		Renderer[] medium = spd.GeneratePrim(DetailLevel.Medium);
		Renderer[] low = spd.GeneratePrim(DetailLevel.Low);
		int totalLength = highest.Length + medium.Length + low.Length;
		spd.renderers = new Renderer[totalLength];
		highest.CopyTo(spd.renderers, 0);
		medium.CopyTo(spd.renderers, highest.Length);
		low.CopyTo(spd.renderers, highest.Length + medium.Length);

		LOD[] lods = new LOD[3]
		{
		new LOD(1.0f, spd.renderers),
		new LOD(0.5f, medium),
		new LOD(0.1f, low)
		};

		group.SetLODs(lods);
		group.fadeMode = LODFadeMode.SpeedTree;
		group.animateCrossFading = true;
		group.RecalculateBounds();
		group.size = 10f;

		if (IsHUD(spd.prim)) group.enabled = false;
	}

	public static void DoAvatarStuff(this ScenePrimData spd)
	{
		var prim = spd.prim;
		var localID = prim.LocalID;

#if UNITY_EDITOR
		string _name = $"Avatar: {localID}";
		spd.obj.name = _name;
#endif
		//Debug.Log(_name);
		Avatar av = ClientManager.simManager.gameObject.GetComponent<Avatar>();

		if (localID == ClientManager.client.Self.LocalID || prim.ID == ClientManager.client.Self.AgentID)
		{
			//Debug.Log("found self");
			ClientManager.currentOutfitFolder = new CurrentOutfitFolder();
			av.myAvatar.parent = spd.meshHolder.transform.root;
			av.myAvatar.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
			av.rotation = prim.Rotation.ToUnity();
		}

		if (spd.nameTag == null)
		{
			spd.MakeInteractWithWater();
			ClientManager.client.Avatars.RequestAvatarName(prim.ID);
			Transform nameTag = SimManager.Instantiate(ResourceCache.nameplate);
			spd.nameTag = nameTag;
			nameTag.parent = spd.obj.transform.parent;
			nameTag.localPosition = new Vector3(0f, (prim.Scale.Z * 0.5f) + 0.25f, 0f);
			spd.text = nameTag.GetComponent<TMPro.TMP_Text>();
		}
	}

	public static void UpdateObject(this ScenePrimData spd, PrimEventArgs update, float t)
	{
		DebugStatsManager.AddStateUpdate(DebugStatsType.PrimUpdate, update.Prim.Type.ToString());


		var prim = update.Prim;
		bool isHUD = IsHUD(prim);

		if (!isHUD && !spd.parentIsHUD)
		{
			spd.MoveAndRotate(prim.ParentID, prim.Position, prim.Rotation);
			spd.UpdateMotion(prim);
		}

		spd.UpdateHudPosition(prim, prim.Rotation);

		if (!isHUD && spd.HasPrimChanged(prim))
		{
			spd.Render();
		}

		//spd.UpdateTextures(update.Prim.Textures, update.Prim.Textures);

		spd.SetupParticles();

		//spd.UpdateWater();
		spd.prim = prim;
	}


	public static void ObjectBlockUpdate(this ScenePrimData spd, ObjectDataBlockUpdateEventArgs update, float t)
	{
		var prim = update.Prim;
		var isHUD = IsHUD(prim);
		var position = prim.Position;
		var rotation = prim.Rotation;

		if (!isHUD && !spd.parentIsHUD)
		{
			spd.MoveAndRotate(prim.ParentID, position, rotation);
			// Consider uncommenting and optimizing the commented out code
		}

		spd.UpdateHudPosition(prim, rotation);

		if (!isHUD && spd.HasPrimChanged(prim))
		{
			spd.Render();
		}

		// Consider whether the commented-out methods are needed or not

		spd.prim = prim;
	}

	public static void UpdateMotion(this ScenePrimData spd, OMVVector3 v, OMVVector3 av, uint localID)
	{
		Vector3 newVelocity = v.ToVector3();
		Vector3 newOmega = av.ToVector3() * Mathf.Rad2Deg;

		if (spd.velocity != newVelocity || spd.omega != newOmega)
		{
			spd.velocity = newVelocity;
			spd.omega = newOmega;

			var movingObjects = ClientManager.simManager.movingObjects; // Assuming it's a HashSet

			if (newVelocity != Vector3.zero || newOmega != Vector3.zero)
			{
				movingObjects.Add(localID); // Add to HashSet
			}
			else
			{
				movingObjects.Remove(localID); // Remove from HashSet
			}
		}
	}



	public static void GetObjectProperties(this ScenePrimData spd)
	{
		if (!spd.prim.IsAttachment &&
			spd.prim.Velocity.Length() == 0f &&
			spd.prim.AngularVelocity.Length() == 0f)
			ClientManager.client.Objects.SelectObject(ClientManager.client.Network.CurrentSim, spd.prim.LocalID);
	}

	public static void MoveAndRotate(this ScenePrimData spd, uint parentID, OMVVector3 newPosition, OpenMetaverse.Quaternion newRotation)
	{
		/*if (parentID == 0)
		{
			spd.pos = newPosition.ToVector3() + new Vector3(
						ClientManager.simManager.simulators[spd.prim.RegionHandle].x - ClientManager.simManager.simulators[ClientManager.simManager.thissim].x,
						0f,
						ClientManager.simManager.simulators[spd.prim.RegionHandle].y - ClientManager.simManager.simulators[ClientManager.simManager.thissim].y);
			spd.obj.transform.root.SetPositionAndRotation(
				spd.pos,
				newRotation.ToUnity());
		}
		else
		{*/
			spd.obj.transform.parent.SetLocalPositionAndRotation(
				newPosition.ToVector3(),
				newRotation.ToUnity());
		//}
	}

	public static void UpdateWater(this ScenePrimData spd)
	{
		if (spd.isWater && spd.prim.Scale != null)
		{
			spd.ResizeWater(spd.prim.Scale.ToVector3());
			spd.GetObjectProperties();
		}
	}

	public static void UpdateMotion(this ScenePrimData spd, ObjectMovementUpdate updated)
	{
		spd.UpdateMotion(updated.Velocity, updated.AngularVelocity, updated.LocalID);
	}

	public static void UpdateMotion(this ScenePrimData spd, Primitive updated)
	{
		spd.UpdateMotion(updated.Velocity, updated.AngularVelocity, updated.LocalID);
	}

	/*	public static void UpdateMotion(this ScenePrimData spd, OMVVector3 v, OMVVector3 av, uint localID)
		{
			if (spd.velocity != v.ToVector3() ||
				spd.omega != av.ToVector3())
			{
				spd.velocity = v.ToVector3();
				spd.omega = av.ToVector3() * Mathf.Rad2Deg;
				if (spd.velocity != Vector3.zero || spd.omega != Vector3.zero)
				{
					if (!ClientManager.simManager.movingObjects.Contains(localID))
					{
						ClientManager.simManager.movingObjects.Add(localID);
					}
				}
				else if (spd.velocity == Vector3.zero && spd.omega == Vector3.zero)
				{
					if (ClientManager.simManager.movingObjects.Contains(localID))
					{
						ClientManager.simManager.movingObjects.Remove(localID);
					}
				}
			}
		}*/

	public static void TerseUpdate(this ScenePrimData spd, TerseObjectUpdateEventArgs update, float t)
	{
		var prim = update.Prim;
		var isHUD = IsHUD(prim);
		var terseUpdate = update.Update;
		var rotation = terseUpdate.Rotation;

		if (!isHUD && !spd.parentIsHUD)
		{
			spd.MoveAndRotate(prim.ParentID, terseUpdate.Position, rotation);
			spd.UpdateMotion(terseUpdate);
		}

		spd.UpdateHudPosition(prim, rotation);

		if (!isHUD && spd.HasPrimChanged(prim))
		{
			spd.Render();
		}

		spd.prim = prim;

		// Consider whether the commented-out methods are needed or not
	}


	public static void UpdateHudPosition(this ScenePrimData spd, Primitive updatedPrim, OpenMetaverse.Quaternion updatedRot)
	{
		if (!IsHUD(spd.prim)) return;
		spd.obj.transform.SetLocalPositionAndRotation(GetHUDPosition(updatedPrim), updatedRot.ToUnity());
	}

	/*  public static void UpdateTextures(this ScenePrimData spd, Primitive.TextureEntry targetTextures, Primitive.TextureEntry updatedTextures)
		{
			if (updatedTextures?.FaceTextures == null) return;
			if (spd.renderers == null) return;

			var rs = spd.meshHolder.GetComponentsInChildren<Renderer>(true);
			Color col;
			for (var j = 0; j < targetTextures.FaceTextures.Length; j++)
			{
				var tex = updatedTextures.GetFace((uint)j);
				if (tex == null || tex.TextureID == UUID.Zero) continue;
				for (int _i = 0; _i < rs.Length; _i++)
				{
					var f = int.Parse(string.Concat(rs[_i].name.Where(char.IsDigit)));
					if (f != j) continue;
					var existingTexture = rs[_i].GetComponent<PrimInfo>().textureID;
					if (tex.TextureID == existingTexture)
					{
						col = tex.RGBA.ToUnity();
						if (col != rs[_i].material.color)
						{
							rs[_i].material.color = col;
						}
						else
						{
							rs[_i].sharedMaterial = ClientManager.assetManager.materialContainer[tex.TextureID].GetMaterial(tex.RGBA.ToUnity(), tex.Glow, tex.Fullbright);
						}
					}
					else
					{
						rs[_i].sharedMaterial = SimManager.Instantiate(Resources.Load<Material>("Opaque Material"));
						ClientManager.simManager.TextureFace(tex, rs[_i]);
					}
				}
			}
		}
	*/
	public static void UpdateTextures(this ScenePrimData spd, Primitive.TextureEntry targetTextures, Primitive.TextureEntry updatedTextures)
	{
		if (updatedTextures?.FaceTextures == null || spd.renderers == null) return;

		var rs = spd.meshHolder.GetComponentsInChildren<Renderer>(true);
		var faceTexturesLength = targetTextures.FaceTextures.Length;
		var materialContainer = ClientManager.assetManager.materialContainer; // Caching the material container

		for (var j = 0; j < faceTexturesLength; j++)
		{
			var tex = updatedTextures.GetFace((uint)j);
			if (tex == null || tex.TextureID == UUID.Zero) continue;

			var rsLength = rs.Length;
			for (int _i = 0; _i < rsLength; _i++)
			{
				var f = int.Parse(string.Concat(rs[_i].name.Where(char.IsDigit)));
				if (f != j) continue;

				var existingTexture = rs[_i].GetComponent<PrimInfo>().textureID;
				if (tex.TextureID == existingTexture)
				{
					Color col = tex.RGBA.ToUnity();
					if (col != rs[_i].material.color)
					{
						rs[_i].material.color = col;
					}
					else
					{
						rs[_i].sharedMaterial = materialContainer[tex.TextureID].GetMaterial(col, tex.Glow, tex.Fullbright); // Using cached material container
					}
				}
				else
				{
					rs[_i].sharedMaterial = SimManager.Instantiate(ResourceCache.opaqueMaterial);
					//ClientManager.simManager.TextureFace(tex, rs[_i]);
				}
			}
		}
	}


	public static bool HasPrimChanged(this ScenePrimData spd, Primitive updated)
	{
		return updated.PrimData.GetHashCode() != spd.ConstructionHash;
	}

	public static void TranslateObject(this ScenePrimData spd, float t)
	{
		float _t = t * ClientManager.simManager._thissim.Stats.Dilation;
		float t5 = t * 5f;
		Vector3 o = spd.omega;
		o.y = -o.y;
		spd.pos += spd.velocity * _t + spd.prim.Acceleration.ToVector3();
		spd.obj.transform.parent.localPosition = Vector3.LerpUnclamped(spd.obj.transform.localPosition, spd.pos, t5);
		spd.obj.transform.parent.localRotation *= Quaternion.Euler(o * _t);
	}


	public static void Click(this ScenePrimData spd,
		Vector3 uvTouch, Vector3 surfaceTouch, int face,
		Vector3 position, Vector3 normal, Vector3 tangent)
	{
		uvTouch.y = 1f - uvTouch.y;
		surfaceTouch.y = 1f - surfaceTouch.y;
		ClientManager.client.Objects.ClickObject(ClientManager.simManager._thissim,
			spd.prim.LocalID, uvTouch.FromUnity(), surfaceTouch.FromUnity(), face,
			position.FromUnity(), normal.FromUnity(), tangent.FromUnity());
	}
}

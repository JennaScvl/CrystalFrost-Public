using CrystalFrost.Extensions;
using CrystalFrost.WorldState;
using Microsoft.Extensions.Logging;
using OpenMetaverse;
using System;
using UnityEngine;

namespace CrystalFrost.Assets.CFEngine.WorldState
{
	public interface IHandleObjectUpdate
	{

	}

	public class HandleObjectUpdate : IHandleObjectUpdate, IDisposable
	{
		private readonly ILogger<HandleObjectUpdate> _log;
		private readonly GridClient _client;
		private readonly IWorld _world;

		public HandleObjectUpdate(
			ILogger<HandleObjectUpdate> log,
			GridClient client,
			IWorld world)
		{
			_log = log;
			_client = client;
			_world = world;

			client.Objects.ObjectUpdate += ObjectUpdate;
		}

		private void ObjectUpdate(object sender, PrimEventArgs e)
		{
			_log.ObjectUpdate(e.Prim.LocalID);

			var worldObject = _world.AllObjects.AddOrUpdate(
				e.Prim.LocalID,
				() => NewObjectFromObjectUpdate(e),
				(existing) => UpdateObjectFromObjectUpdate(existing, e));

			// Moar stuff?
		}

		private SimObject NewObjectFromObjectUpdate(PrimEventArgs e)
		{
			var result = new SimObject()
			{
				LocalID = e.Prim.LocalID,
				IsAttachment = e.Prim.IsAttachment,
				ParentID = e.Prim.ParentID,
				UUID = e.Prim.ID,
				SimPosition = e.Prim.Position.ToVector3(),
				SimRotation = e.Prim.Rotation.ToUnity(),
				SimVelocity = e.Prim.Velocity.ToVector3(),
				SimAngularVelocity = e.Prim.AngularVelocity.ToVector3() * Mathf.Rad2Deg,
				Scale = e.Prim.Scale.ToUnity(),
				PrimType = e.Prim.Type,
				ParticleSystem = e.Prim.ParticleSys,
			};


			if (e.Prim.Light is not null)
			{
				result.IsLight = true;
				result.LightRadius = e.Prim.Light.Radius;
				result.LightColor = e.Prim.Light.Color.ToUnity();
				result.LightIntensity = e.Prim.Light.Intensity;
			}

			// Get the region, or create it if needed.	
			result.Region = _world.Regions.GetOrDefault(e.Simulator.RegionID) 
				?? _world.Regions.AddOrUpdate(e.Simulator);

			result.Region.Objects.Add(result);

			// TODO - look for orphans whom this object is the parent.

			return result;
		}

		private SimObject UpdateObjectFromObjectUpdate(SimObject existing, PrimEventArgs e)
		{
			// TODO 
			return existing;
		}

		public void Dispose()
		{
			_client.Objects.ObjectUpdate -= ObjectUpdate;
			GC.SuppressFinalize(this);
		}
	}
}

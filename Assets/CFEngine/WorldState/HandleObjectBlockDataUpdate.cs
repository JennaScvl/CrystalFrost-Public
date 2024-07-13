﻿using CrystalFrost.Extensions;
using CrystalFrost.WorldState;
using Microsoft.Extensions.Logging;
using OpenMetaverse;
using System;
using UnityEngine;

namespace CrystalFrost.Assets.CFEngine.WorldState
{
	public interface IHandleObjectBlockDataUpdate
	{

	}

	public class HandleObjectBlockDataUpdate : IHandleObjectBlockDataUpdate, IDisposable
	{
		private readonly ILogger<HandleObjectBlockDataUpdate> _log;
		private readonly GridClient _client;
		private readonly IWorld _world;

		public HandleObjectBlockDataUpdate(
			ILogger<HandleObjectBlockDataUpdate> log,
			GridClient client,
			IWorld world
			)
		{
			_log = log;
			_client = client;
			_world = world;

			_client.Objects.ObjectDataBlockUpdate += ObjectDataBlockUpdate;
		}

		public void Dispose()
		{
			_client.Objects.ObjectDataBlockUpdate -= ObjectDataBlockUpdate;
			GC.SuppressFinalize(this);
		}

		private void ObjectDataBlockUpdate(object sender, ObjectDataBlockUpdateEventArgs e)
		{
			_log.ObjectBlockDataUpdate(e.Prim.LocalID);

			var worldObject = _world.AllObjects.AddOrUpdate(
				e.Prim.LocalID,
				() => NewObjectFromObjectDataBlockUpdate(e),
				(existing) => UpdateObjectFromObjectDataBlockUpdate(existing, e));

			// Moar stuff?
		}

		private SimObject NewObjectFromObjectDataBlockUpdate(ObjectDataBlockUpdateEventArgs e)
		{
			var result = new SimObject()
			{
				LocalID = e.Update.LocalID,
				IsAttachment = e.Prim.IsAttachment,
				ParentID = e.Prim.ParentID,
				UUID = e.Prim.ID,
				SimPosition = e.Update.Position.ToVector3(),
				SimRotation = e.Update.Rotation.ToUnity(),
				SimVelocity = e.Update.Velocity.ToVector3(),
				SimAngularVelocity = e.Update.AngularVelocity.ToVector3() * Mathf.Rad2Deg,
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

			return result;
		}

		private SimObject UpdateObjectFromObjectDataBlockUpdate(SimObject existing, ObjectDataBlockUpdateEventArgs e)
		{
			// todo
			return existing;
		}
	}
}

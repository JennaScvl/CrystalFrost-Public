using CrystalFrost.Extensions;
using Microsoft.Extensions.Logging;
using OpenMetaverse;
using System;
using UnityEngine;

namespace CrystalFrost.WorldState
{
	/// <summary>
	/// Defines a piece of code that runs when a TerseObjectObject
	/// is recieved from the simulator.
	/// </summary>
	public interface IHandleTerseUpdate
	{
	
	}

	public class HandleTerseUpdate : IHandleTerseUpdate, IDisposable
	{
		private readonly ILogger<HandleTerseUpdate> _log;
		private readonly GridClient _client;
		private readonly IWorld _world;

		/// <summary>
		/// Initializes a new instance of the <see cref="HandleTerseUpdate"/> class.
		/// </summary>
		/// <param name="log">The logger for recording messages.</param>
		/// <param name="client">The grid client.</param>
		/// <param name="world">The world state.</param>
		public HandleTerseUpdate(
			ILogger<HandleTerseUpdate> log,
			GridClient client,
			IWorld world)
		{
			_log = log;
			_client = client;
			_world = world;

			_client.Objects.TerseObjectUpdate += TerseObjectUpdate;
		}

		private void TerseObjectUpdate(object sender, TerseObjectUpdateEventArgs e)
		{
			_log.TerseObjectUpdate(e.Update.LocalID);

			var worldObject = _world.AllObjects.AddOrUpdate(
				e.Prim.LocalID,
				() => NewObjectFromTerseObjectUpdate(e),
				(existing) => UpdateObjectFromTerseObjectUpdate(existing, e));

			// Moar stuff?
		}

		private SimObject NewObjectFromTerseObjectUpdate(TerseObjectUpdateEventArgs e)
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

		private SimObject UpdateObjectFromTerseObjectUpdate(SimObject existing, TerseObjectUpdateEventArgs e)
		{
			// todo, figure out what changes we can detect and what we care about.
			return existing;
		}

		public void Dispose()
		{
			_client.Objects.TerseObjectUpdate -= TerseObjectUpdate;
			GC.SuppressFinalize(this);
		}
	}
}

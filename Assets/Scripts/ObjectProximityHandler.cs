
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;

using OpenMetaverse;
using OMV = OpenMetaverse;

using CrystalFrost;
using CrystalFrost.Config;

using Microsoft.Extensions.Options;
using UnityEngine;
using CrystalFrost.Extensions;
using System;
using Unity.VisualScripting;
using System.Linq;

public interface IObjectProximityHandler
{
	// Method to check if an object is in the camera view
	public bool IsInCameraView(PrimEventArgs primEvent);

	// Method to stop scanning objects
	public void StopScanningObject();

	// Method to start scanning objects
	public void StartScanningObject();

	// Method to add an object to the list of objects to be scanned
	public void AddObject(PrimEventArgs primEvent);

	// Updates the camera Properties
	public void UpdateCameraProperties(UnityEngine.Camera camera);
}

public class ObjectProximityHandler : IObjectProximityHandler
{
	private ConcurrentDictionary<string, List<PrimEventArgs>> allPrimObjects;
	private ConcurrentQueue<PrimEventArgs> objectsToProcessQueue;

	private Thread proximityThread;
	private GridClient client;
	private bool isRunning;
	private ViewConfig _viewConfig;
	private FrustumManager frustumManager;

	private ConcurrentDictionary<uint, Tuple<uint, OMV.Vector3>> orphannedPrims;
	private ConcurrentDictionary<uint, OMV.Vector3> primPositions;

	class Plane
	{
		public OMV.Vector3 normal;
		public OMV.Vector3 pointOnPlane;

		public Plane(OMV.Vector3 normal, OMV.Vector3 pointOnPlane)
		{
			normal.Normalize();
			this.normal = normal;
			this.pointOnPlane = pointOnPlane;
		}

		public Plane(OMV.Vector3 a, OMV.Vector3 b, OMV.Vector3 c)
		{
			normal = OMV.Vector3.Cross(b - a, c - a);
			normal.Normalize();
			pointOnPlane = a;
		}

		public float GetDistanceToPoint(OMV.Vector3 point)
		{
			return OMV.Vector3.Dot(normal, point - pointOnPlane);
		}

		public string ToString()
		{
			return string.Format("Plane: normal={0}, distance={1}", normal, pointOnPlane);
		}
	}

	class FrustumManager
	{
		private Plane[] planes = new Plane[6];
		private OMV.Vector3 cameraPosition;
		private OMV.Vector3 cameraUp;
		private OMV.Vector3 cameraForward;
		private OMV.Vector3 cameraLeft;
		private float cameraFOV;
		private float cameraAspect;
		private float cameraNearClip;
		private float cameraFarClip;

		public Plane[] Planes { get { return planes; } }



		public void Update(UnityEngine.Camera camera)
		{
			this.cameraFOV = camera.fieldOfView * Mathf.Deg2Rad;
			this.cameraAspect = camera.aspect;

			this.cameraNearClip = camera.nearClipPlane;
			this.cameraFarClip = camera.farClipPlane; // this looks better
													  // this.cameraFarClip = ClientManager.viewDistance;

			this.cameraPosition = new OMV.Vector3(camera.transform.position.x, camera.transform.position.z, camera.transform.position.y);
			this.cameraForward = new OMV.Vector3(camera.transform.forward.x, camera.transform.forward.z, camera.transform.forward.y);
			this.cameraUp = new OMV.Vector3(camera.transform.up.x, camera.transform.up.z, camera.transform.up.y);
			this.cameraLeft = OMV.Vector3.Cross(this.cameraForward, this.cameraUp);

			this.cameraForward.Normalize();
			this.cameraUp.Normalize();
			this.cameraLeft.Normalize();
			this.cameraUp.Normalize();

			this.RecalculatePlanes();
		}

		public void RecalculatePlanes()
		{
			// Calculate frustum parameters
			var tanHalfFov = Mathf.Tan(this.cameraFOV / 2.0f);
			var nearHeight = 2 * tanHalfFov * this.cameraNearClip;
			var nearWidth = nearHeight * this.cameraAspect;
			var farHeight = 2 * tanHalfFov * this.cameraFarClip;
			var farWidth = farHeight * this.cameraAspect;

			// Calculate frustum points
			var nearCenter = this.cameraPosition + this.cameraForward * this.cameraNearClip;
			var farCenter = this.cameraPosition + this.cameraForward * this.cameraFarClip;
			var right = -cameraLeft;
			var up = this.cameraUp;
			var nearTopLeft = nearCenter - (right * nearWidth / 2) + (up * nearHeight / 2);
			var nearTopRight = nearCenter + (right * nearWidth / 2) + (up * nearHeight / 2);
			var nearBottomLeft = nearCenter - (right * nearWidth / 2) - (up * nearHeight / 2);
			var nearBottomRight = nearCenter + (right * nearWidth / 2) - (up * nearHeight / 2);
			var farTopLeft = farCenter - (right * farWidth / 2) + (up * farHeight / 2);
			var farTopRight = farCenter + (right * farWidth / 2) + (up * farHeight / 2);
			var farBottomLeft = farCenter - (right * farWidth / 2) - (up * farHeight / 2);
			var farBottomRight = farCenter + (right * farWidth / 2) - (up * farHeight / 2);


			// Calculate frustum planes
			var nearPlane = new Plane(nearTopRight, nearTopLeft, nearBottomLeft);
			var farPlane = new Plane(farTopLeft, farTopRight, farBottomRight);
			var leftPlane = new Plane(nearBottomLeft, nearTopLeft, farTopLeft);
			var rightPlane = new Plane(farBottomRight, farTopRight, nearTopRight);
			var topPlane = new Plane(nearTopLeft, nearTopRight, farTopRight);
			var bottomPlane = new Plane(farBottomLeft, farBottomRight, nearBottomRight);

			this.planes = new Plane[] { nearPlane, farPlane, leftPlane, rightPlane, topPlane, bottomPlane };
		}

		public bool IntersectsSphere(OMV.Vector3 center, float radius)
		{
			var planes = this.planes;

			// Check if the sphere intersects any of the planes
			for (int i = 0; i < 6; i++)
			{
				var distance = planes[i].GetDistanceToPoint(center);

				if (distance < -radius)
				{
					// The sphere is outside the frustum
					return false;
				}
			}

			// The sphere is inside the frustum
			return true;
		}

		public void Draw()
		{
			// Calculate frustum parameters
			var tanHalfFov = Mathf.Tan(this.cameraFOV / 2.0f);
			var nearHeight = 2 * tanHalfFov * this.cameraNearClip;
			var nearWidth = nearHeight * this.cameraAspect;
			var farHeight = 2 * tanHalfFov * this.cameraFarClip;
			var farWidth = farHeight * this.cameraAspect;

			// Calculate frustum points
			var nearCenter = this.cameraPosition + this.cameraForward * this.cameraNearClip;
			var farCenter = this.cameraPosition + this.cameraForward * this.cameraFarClip;
			var right = -cameraLeft;
			var up = this.cameraUp;
			var nearTopLeft = nearCenter - (right * nearWidth / 2) + (up * nearHeight / 2);
			var nearTopRight = nearCenter + (right * nearWidth / 2) + (up * nearHeight / 2);
			var nearBottomLeft = nearCenter - (right * nearWidth / 2) - (up * nearHeight / 2);
			var nearBottomRight = nearCenter + (right * nearWidth / 2) - (up * nearHeight / 2);
			var farTopLeft = farCenter - (right * farWidth / 2) + (up * farHeight / 2);
			var farTopRight = farCenter + (right * farWidth / 2) + (up * farHeight / 2);
			var farBottomLeft = farCenter - (right * farWidth / 2) - (up * farHeight / 2);
			var farBottomRight = farCenter + (right * farWidth / 2) - (up * farHeight / 2);

			Debug.DrawLine(nearTopLeft.ToUnity(), nearTopRight.ToUnity(), Color.red);
			Debug.DrawLine(nearTopRight.ToUnity(), nearBottomRight.ToUnity(), Color.red);
			Debug.DrawLine(nearBottomRight.ToUnity(), nearBottomLeft.ToUnity(), Color.red);
			Debug.DrawLine(nearBottomLeft.ToUnity(), nearTopLeft.ToUnity(), Color.red);

			Debug.DrawLine(farTopLeft.ToUnity(), farTopRight.ToUnity(), Color.red);
			Debug.DrawLine(farTopRight.ToUnity(), farBottomRight.ToUnity(), Color.red);
			Debug.DrawLine(farBottomRight.ToUnity(), farBottomLeft.ToUnity(), Color.red);
			Debug.DrawLine(farBottomLeft.ToUnity(), farTopLeft.ToUnity(), Color.red);

			Debug.DrawLine(nearTopLeft.ToUnity(), farTopLeft.ToUnity(), Color.red);
			Debug.DrawLine(nearTopRight.ToUnity(), farTopRight.ToUnity(), Color.red);
			Debug.DrawLine(nearBottomRight.ToUnity(), farBottomRight.ToUnity(), Color.red);
			Debug.DrawLine(nearBottomLeft.ToUnity(), farBottomLeft.ToUnity(), Color.red);

			Debug.DrawLine(farTopLeft.ToUnity(), farTopRight.ToUnity(), Color.red);
			Debug.DrawLine(farTopRight.ToUnity(), farBottomRight.ToUnity(), Color.red);
			Debug.DrawLine(farBottomRight.ToUnity(), farBottomLeft.ToUnity(), Color.red);
			Debug.DrawLine(farBottomLeft.ToUnity(), farTopLeft.ToUnity(), Color.red);


		}
	}


	public ObjectProximityHandler(ConcurrentQueue<PrimEventArgs> objectInCurrentView, GridClient client)
	{
		this.client = client;
		this.isRunning = false;
		this.allPrimObjects = new();
		this.frustumManager = new();
		this.primPositions = new();
		this.orphannedPrims = new();
		this.objectsToProcessQueue = objectInCurrentView;
		this._viewConfig = Services.GetService<IOptions<ViewConfig>>().Value;
		this.proximityThread = new(ObjectProximityChecker);
		this.StartScanningObject();
	}

	public void UpdateCameraProperties(UnityEngine.Camera camera)
	{
		this.frustumManager.Update(camera);
	}

	// Thread function for checking object proximit
	private void ObjectProximityChecker()
	{
		while (isRunning)
		{
			// update all orphans
			var orphannedKeys = this.orphannedPrims.Keys;
			foreach (var key in orphannedKeys)
			{
				if (this.orphannedPrims.TryGetValue(key, out var prim))
				{
					if(this.primPositions.TryGetValue(prim.Item1, out var position))
					{
						var parentPosition = position;
						var primPosition = prim.Item2;
						var actualPosition = parentPosition + primPosition;
						if (this.primPositions.TryAdd(key, actualPosition))
						{
							this.orphannedPrims.TryRemove(key, out _);
						}
					}
				}
			}		


			// Debug.LogError($"{this.orphannedPrims.Count} orphans of {this.primPositions.Count} prims of which {this.allPrimObjects.Count} are left");


			var toAddToQueue = new List<PrimEventArgs>();
			var keys = this.allPrimObjects.Keys.ToArray().Clone() as string[];

			foreach (var key in keys)
			{
				List<PrimEventArgs> primEvent;

				if (!this.allPrimObjects.TryGetValue(key, out primEvent))
				{
					// WARN: Handle this issue
				}

				if (primEvent.Any(IsInCameraView))
				{
					toAddToQueue.AddRange(primEvent);

					if (!this.allPrimObjects.TryRemove(key, out primEvent))
					{
						Debug.LogError("Failed to remove object from dictionary");
					}
				}

			}


			foreach (var primEvent in toAddToQueue)
			{
				this.objectsToProcessQueue.Enqueue(primEvent);
			}

			Thread.Sleep(_viewConfig.NewObjectPollMS);
		}
	}

	public void AddPrimPositionID(Primitive prim)
	{
		this.AddPrimPositionID(prim.LocalID, prim.ParentID, prim.Position);
	}

	public void AddPrimPositionID(uint LocalID, uint ParentID, OMV.Vector3 Position)
	{
		// calculate prim position or put it in orphannedPrims
		if (ParentID == 0)
		{
			this.primPositions.TryAdd(LocalID, Position);
		}
		else
		{
			if (this.primPositions.ContainsKey(ParentID))
			{
				var parentPosition = this.primPositions[ParentID];
				this.primPositions.TryAdd(LocalID, parentPosition + Position);
			}
			else
			{
				this.orphannedPrims.TryAdd(LocalID, new(ParentID, Position));
			}
		}

	}

	/// <summary>
	/// Adds a PrimEventArgs object to the list of all prim objects.
	/// </summary>
	/// <param name="primEvent">The PrimEventArgs object to add.</param>
	public void AddObject(PrimEventArgs primEvent)
	{
		// check if the object is already in the list
		if (!this.allPrimObjects.ContainsKey(primEvent.Prim.ID.ToString()))
		{
			this.allPrimObjects.TryAdd(primEvent.Prim.ID.ToString(), new());
		}

		// Update the object in the list
		this.allPrimObjects[primEvent.Prim.ID.ToString()].Add(primEvent);

		this.AddPrimPositionID(primEvent.Prim);

		DebugStatsManager.AddStateUpdate(DebugStatsType.PrimEvent, primEvent.Prim.Type.ToString());
	}

	/// <summary>
	/// Determines if a given prim is within the camera's view frustum.
	/// </summary>
	/// <param name="primEvent">The event containing the prim to check.</param>
	/// <returns>True if the prim is within the camera's view frustum, false otherwise.</returns>
	public bool IsInCameraView(PrimEventArgs primEvent)
	{
		// return true;

		if ((primEvent.Prim.Type != OMV.PrimType.Mesh && primEvent.Prim.Type != OMV.PrimType.Sculpt)
			|| primEvent.IsAttachment)
		{
			return true; // No point in checking for non-mesh or non-sculpt prims as there is nothing to download there
		}


		if(!this.primPositions.TryGetValue(primEvent.Prim.LocalID, out var primPosition))
		{
			// position not yet ready
			return false;
		}


		// This is a simple approximation that should be good enough for now
		var primRadius = primEvent.Prim.Scale.Length() * 0.5f;


		if (this.frustumManager.IntersectsSphere(primPosition, primRadius))
		{
			return true;
		}

		return false;
	}

	public void StopScanningObject()
	{
		// If the thread is not running, don't stop it
		if (!isRunning)
		{
			return;
		}

		// Stop the thread
		isRunning = false;
		proximityThread.Join();
	}

	/// <summary>
	/// Starts scanning for objects.
	/// </summary>
	public void StartScanningObject()
	{
		// If the thread is already running, don't start it again
		if (isRunning)
		{
			return;
		}

		// Start the thread
		isRunning = true;
		proximityThread.Start();
	}

	public void DrawFrustum()
	{
		this.frustumManager.Draw();
	}


}

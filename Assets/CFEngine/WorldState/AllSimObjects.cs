using CrystalFrost.UnityRendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CrystalFrost.WorldState
{
	public interface IAllSimObject
	{
		/// <summary>
		/// Returns the object or null if not found.
		/// </summary>
		/// <param name="localID"></param>
		/// <returns></returns>
		SimObject GetOrDefault(uint localID);

		/// <summary>
		/// Adds or updates the object.
		/// </summary>
		/// <param name="localID"></param>
		/// <param name="buildNew"></param>
		/// <param name="update"></param>
		/// <returns></returns>
		SimObject AddOrUpdate(uint localID, Func<SimObject> buildNew, Func<SimObject, SimObject> update);
	}

	public class AllSimObjects : IAllSimObject
	{
		private readonly Dictionary<uint, List<SimObject>> _orphans = new();

		private readonly ConcurrentDictionary<uint, SimObject> _objects = new();

		private readonly ILogger<AllSimObjects> _log;
		private readonly INewSimObjectQueue _newSimObjectQueue;

		/// <summary>
		/// Initializes a new instance of the <see cref="AllSimObjects"/> class.
		/// </summary>
		/// <param name="log">The logger for recording messages.</param>
		/// <param name="newSimObjectQueue">The queue for new simulation objects.</param>
		public AllSimObjects(ILogger<AllSimObjects> log,
			INewSimObjectQueue newSimObjectQueue)
		{
			_log = log;
			_newSimObjectQueue = newSimObjectQueue;
		}

		public SimObject GetOrDefault(uint localID)
		{
			if (_objects.TryGetValue(localID, out var result)) { return result; }
			return default;
		}

		public SimObject AddOrUpdate(uint localID, Func<SimObject> buildNew, Func<SimObject, SimObject> update)
		{
			var result = _objects.AddOrUpdate(
				localID,
				(id) => buildNew(),
				(id, existing) => update(existing));

			// try to setup the parent.
			if (result.IsOrphan())
			{
				result.Parent = GetOrDefault(result.ParentID);
			}

			if(result.IsOrphan())
			{
				// create a list for the parent if needed.
				if (!_orphans.ContainsKey(result.ParentID))
				{
					_orphans.Add(result.ParentID, new());
				}

				// check if this object is already in the list.
				var orphanChildren = _orphans[result.ParentID];
				if (!orphanChildren.Any(o => o.LocalID == result.LocalID))
				{
					// this object is not in its parent list yet, add it.
					_log.OrphanDetected(result.LocalID, result.ParentID);
					orphanChildren.Add(result);
				}
			}

			if (!result.IsOrphan())
			{
				_newSimObjectQueue.Enqueue(result);
			}

			// check to see if this object has any orphan children.
			if (_orphans.ContainsKey(result.LocalID))
			{
				// it does, re-unite the parent and children
				var children = _orphans[result.LocalID];
				_orphans.Remove(result.LocalID);
				foreach (var child in children)
				{
					child.Parent = result;
					result.Children.Add(child);
					_log.OrphanReuinited(child.LocalID, child.ParentID);
					_newSimObjectQueue.Enqueue(child);
				}
			}

			return result;
		}
	}
}

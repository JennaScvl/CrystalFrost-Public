using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

//using UnityEngine.Utility;
	
public static class GameObjectExtensions {
	
	public static void SetLayerRecursively(this GameObject inst, int layer) {
		inst.layer = layer;
		foreach(Transform child in inst.transform)
			child.gameObject.SetLayerRecursively(layer);
	}

	
	/// <summary>
	/// Gets a component from a game object (supports interfaces)
	/// </summary>
	/// <returns>
	/// The component found in the game object
	/// </returns>
	/// <param name='inst'>
	/// Instance of game object to add the component to
	/// </param>
	/// <typeparam name='T'>
	/// The type of component, or interface, to find
	/// </typeparam>
	/// 
	public static T GetComponent<T>(this GameObject inst)
		where T : class {
		return inst.GetComponent(typeof(T)) as T;
	} 
	
	public static GameObject FindTypeAboveObject<T>(this GameObject inst) 
		where T : class { 
		if(inst == null) {
			return null;
		}
		
		return FindTypeAboveObjectRecursive<T>(inst);
	}
	
	public static GameObject FindTypeAboveObjectRecursive<T>(this GameObject inst) 
		where T : class {
		if(inst == null) {
			return null;
		}
		
		if(inst != null) {
			if(inst.GetComponent<T>() != null) {
				return inst;
			}
			
			if(inst.transform.parent != null) {
				return FindTypeAboveObjectRecursive<T>(inst.transform.parent.gameObject);
			}
		}
		 
		return null;
	}
	
	public static Transform FindBelow(this GameObject inst, string name) {
		if(inst == null) {
			return null;
		}
		
		if (inst.transform.childCount == 0) {
			return null;
		}
		var child = inst.transform.Find(name);
		if (child != null) {
			return child;
		}
		foreach (GameObject t in inst.transform) {
			child = FindBelow(t, name);
			if (child != null) {
				return child;
			}
		}
		return null;
	}
    
    public static bool IsReady(this UnityEngine.Object inst) {
        return inst != null ? true : false;
    }
	
	public static void DestroyChildren(this GameObject inst) {
		if(inst == null)
			return;
		
		List<Transform> transforms = new List<Transform>();// inst.transform.childCount;
		int b = 0;
		foreach(Transform t in inst.transform) {
			transforms.Add(t);// = t;
			b++;
		}
		
		foreach(Transform t in transforms) {
			t.parent = null;
			UnityEngine.Object.Destroy(t.gameObject);
		}
		
		transforms.Clear();
		transforms = null;
	}
	
	public static void ChangeLayersRecursively(this GameObject inst, string name) {
		if(inst == null)
			return;
		
	    foreach (Transform child in inst.transform) {
	        child.gameObject.layer = LayerMask.NameToLayer(name);
	        ChangeLayersRecursively(child.gameObject, name);
	    }
	}
	
}

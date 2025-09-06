using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace CrystalFrost.ObjectPooling
{
    /// <summary>
    /// A generic object pool for managing and reusing GameObjects to reduce instantiation overhead.
    /// </summary>
    public partial class ObjectPool
    {
        private ConcurrentDictionary<string, (GameObject, PoolObjectManager)> objectsInUse = new(); // Items in use 
        private ConcurrentQueue<(GameObject, PoolObjectManager)> objectsInQueue = new();
        // private ConcurrentQueue<GameObject> objectsToDelete = new(); // List to Delete Object who reached there limit
        private GameObject poolParentObject = null; // The parent object for all pooled objects
        private Type[] requiredComponents = null; // Types of components that objects in the pool must have

        private float maxAge = 600f;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPool"/> class.
        /// </summary>
        /// <param name="requiredComponents">An array of component types that all pooled objects must have.</param>
        /// <param name="initialSize">The initial number of objects to create in the pool.</param>
        /// <param name="poolParentObject">The parent GameObject under which all pooled objects will be organized.</param>
        public ObjectPool(Type[] requiredComponents, int initialSize, GameObject poolParentObject)
        {
            this.requiredComponents = requiredComponents;
            this.poolParentObject = poolParentObject;

            // Create and add initial objects to the pool
            for (int i = 0; i < initialSize; i++)
            {
                objectsInQueue.Enqueue(this.CreateObject(System.Guid.NewGuid().ToString()));
            }
        }

        /// <summary>
        /// Acquires an object from the pool, creating a new one if necessary.
        /// </summary>
        /// <param name="deallocationLogic">The logic that determines when the object should be returned to the pool.</param>
        /// <returns>An active GameObject from the pool.</returns>
        public GameObject AcquireObject(IPoolObjectDeallocationLogic deallocationLogic)
        {
            (GameObject obj, PoolObjectManager manager) = (null, null);

            // Check if there are any objects in the queue
            if (objectsInQueue.Count > 0)
            {
                (GameObject, PoolObjectManager) pair = (null, null);
                // Dequeue an object from the queue
                if (!objectsInQueue.TryDequeue(out pair))
                {
                    throw new Exception("Failed to dequeue an object from the queue.");
                }
                (obj, manager) = pair;
            }
            else
            {
                // Create a new object if the queue is empty
                (obj, manager) = this.CreateObject($"{poolParentObject.name}Object{System.Guid.NewGuid().ToString()}");
            }

            // Add the object to the in-use list
            objectsInUse[manager.UID] = (obj, manager);
            
            // Activate the object and provide deallocation logic if available
            manager.AllocateSelf(deallocationLogic);

            return obj;
        }

        // Create a new object and set up its components
        private (GameObject, PoolObjectManager) CreateObject(string uid)
        {
            var obj = new GameObject();
            obj.name = $"{poolParentObject.name}Object{uid}";
            obj.transform.SetParent(poolParentObject.transform);

            // Add required components to the object
            foreach (Type cmpnt in requiredComponents)
            {
                // Check if the component type is valid
                if (!cmpnt.IsSubclassOf(typeof(Component)))
                {
                    throw new Exception($"Type '{cmpnt.Name}' is not a component.");
                }

                // Skip if the component already exists
                if (obj.GetComponent(cmpnt) != null)
                {
                    continue;
                }

                // Add the component to the object
                var cmpnt2 = obj.AddComponent(cmpnt) as Component;

                // Disable the component if it's a Behaviour
                if (cmpnt.IsAssignableFrom(typeof(Behaviour)))
                {
                    (cmpnt2 as Behaviour).enabled = false;
                }
            }

            // Add a PoolObjectManager to the object and set its properties
            var manager = obj.AddComponent<PoolObjectManager>() as PoolObjectManager;
            manager.Pool = this;
            manager.RequiredComponents = requiredComponents;
            manager.UID = uid;

            // Deactivate the object initially
            obj.SetActive(false);

            return (obj, manager);
        }

        /// <summary>
        /// Deallocates an object and returns it to the pool using its unique identifier.
        /// </summary>
        /// <param name="uid">The unique identifier of the object to deallocate.</param>
        public void DeallocateObject(string uid)
        {            
            (GameObject obj, PoolObjectManager manager) = objectsInUse[uid];

            manager.DeallocateSelf();

            if (!objectsInUse.TryRemove(uid, out _))
            {
                throw new Exception($"Failed to remove object '{obj.name}' from the in-use list.");
            }

            objectsInQueue.Enqueue((obj, manager));
        }

        /// <summary>
        /// Deallocates an object and returns it to the pool using its GameObject reference.
        /// </summary>
        /// <param name="obj">The GameObject to deallocate.</param>
        public void DeallocateObject(GameObject obj)
        {
            var manager = obj.GetComponent<PoolObjectManager>();

            if (manager == null)
            {
                throw new Exception($"Object '{obj.name}' does not have a PoolObjectManager component.");
            }

            this.DeallocateObject(manager.UID);
        }

        public void UpdatePoolObjectsOnMainThread()
        {
            // go through all objects in deletion queue and Destroy them
            // while (objectsToDelete.Count > 0)
            // {
            //     if (objectsToDelete.TryDequeue(out var obj))
            //     {
            //         UnityEngine.Object.Destroy(obj);
            //     }
            //     else 
            //     {
            //         Debug.LogWarning($"Failed to remove object from the deletion list.");
            //         break;
            //     }
            // }   
        }

        /// <summary>
        /// Updates the state of all active objects in the pool.
        /// </summary>
        public void UpdatePoolObjects()
        {
            foreach (var pair in objectsInUse)
            {
                var (obj, manager) = pair.Value;

                if (manager.IsActivated)
                {
                    manager.UpdateObject();
                }
                else
                {
                    if (!objectsInUse.TryRemove(manager.UID, out _))
                    {
                        // throw new Exception($"Failed to remove object '{obj.name}' from the in-use list.");
                        Debug.LogWarning($"Failed to remove object '{obj.name}' from the in-use list.");
                    }
                    objectsInQueue.Enqueue((obj, manager));
                }
            }
        }

        /// <summary>
        /// Updates the pool size by removing objects that have exceeded their maximum age.
        /// </summary>
        public void UpdatePoolSize()
        {
            while (objectsInQueue.Count > 0)
            {
                if (objectsInQueue.TryPeek(out var pair))
                {
                    var (obj, manager) = pair;

                    // Check if the object's age exceeds the maximum age limit
                    if (manager.Age > maxAge)
                    {
                        if (!objectsInQueue.TryDequeue(out _))
                        {
                            // throw new Exception($"Failed to remove object '{obj.name}' from the in-use list.");
                            Debug.LogWarning($"Failed to remove object '{obj.name}' from the in-use list.");
                            break;
                        }

                        // Add Object to List for Delete from main thread
                        // objectsToDelete.Enqueue(obj);
                    }
                    else
                    {
                        // Stop removing objects when the age limit is not exceeded
                        break;
                    }
                }
            }
        }

    }
}

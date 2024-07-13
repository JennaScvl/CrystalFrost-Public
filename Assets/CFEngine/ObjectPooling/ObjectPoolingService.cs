using UnityEngine;
using System.Threading;
using System.Collections.Concurrent;
using System;

namespace CrystalFrost.ObjectPooling
{
    // Enum for object pool names 
    public enum ObjectPoolName
    {
        Rigidbody
        // add more object pool names here
    }

    // Define an interface for object pooling service
    public interface IObjectPoolingService
    {
        public void Initialize();
        public GameObject AcquireObject(ObjectPoolName poolName, IPoolObjectDeallocationLogic deallocationLogic);
    }

    // Implementation of the object pooling service
    public class ObjectPoolingService : IObjectPoolingService
    {
        // Create a dictionary to hold object pools for different object types.
        private ConcurrentDictionary<ObjectPoolName, ObjectPool> objectPools = new();

        // GameObject that serves as a parent for all object pools
        private GameObject globalObjectPool = null;

        // Worker thread for managing object pools
        private Thread poolObjectManagerThread = null;

        // Flag to control the worker thread
        private bool isWorkerThreadRunning = false;

        // Interval (in seconds) for updating object pools in the worker thread
        private float poolObjectManagerThreadInterval = 5.0f;

        // Initialize default object pools
        public void Initialize()
        {
            this.globalObjectPool = new GameObject();
            this.globalObjectPool.name = "GlobalObjectPool";

            // Start the worker thread for managing object pools
            this.isWorkerThreadRunning = true;
            this.poolObjectManagerThread = new Thread(PoolObjectManagerThreadWorker);
            this.poolObjectManagerThread.Start();

            // You can add more object types and pools as needed.
            this.CreateObjectPool(ObjectPoolName.Rigidbody, new Type[] { typeof(Rigidbody), typeof(BoxCollider) }, 10);
        }

        // Destructor: Cleanup when the object is destroyed
        ~ObjectPoolingService()
        {
            // Stop the worker thread and wait for it to finish
            this.isWorkerThreadRunning = false;
            this.poolObjectManagerThread.Join();
        }

        // Create an object pool for a specific object type
        private void CreateObjectPool(ObjectPoolName poolName, Type[] requiredComponents, int initialSize)
        {
            if (!this.objectPools.ContainsKey(poolName))
            {
                // Create a GameObject to serve as the parent for the object pool
                var poolParentObject = new GameObject();
                poolParentObject.name = $"{poolName}Pool";
                poolParentObject.transform.SetParent(globalObjectPool.transform);

                // Create and initialize the ObjectPool instance
                this.objectPools[poolName] = new ObjectPool(requiredComponents, initialSize, poolParentObject);
            }
            else
            {
                throw new Exception($"Object pool '{poolName}' already exists.");
            }
        }

        // Acquire an object from a specific object pool with deallocation logic
        public GameObject AcquireObject(ObjectPoolName poolName, IPoolObjectDeallocationLogic deallocationLogic)
        {
            if (objectPools.ContainsKey(poolName))
            {
                return objectPools[poolName].AcquireObject(deallocationLogic);
            }
            else
            {
                throw new Exception($"Object pool '{poolName}' not found.");
            }
        }

        // Deallocate an object by its pool name and GameObject reference
        public void DeallocateObject(ObjectPoolName poolName, GameObject gameObject)
        {
            if (objectPools.ContainsKey(poolName))
            {
                objectPools[poolName].DeallocateObject(gameObject);
            }
            else
            {
                throw new Exception($"Object pool '{poolName}' not found.");
            }
        }
        
        

        // Worker thread method for managing object pools
        private void PoolObjectManagerThreadWorker()
        {
            while (isWorkerThreadRunning)
            {
                foreach (var pool in objectPools)
                {
                    // Update objects within each object pool
                    pool.Value.UpdatePoolObjects();
                    pool.Value.UpdatePoolSize();
                }
                // Sleep for a specified interval (in milliseconds)
                Thread.Sleep((int)(poolObjectManagerThreadInterval * 1000));
            }
        }
    }
}

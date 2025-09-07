using UnityEngine;
using System;


namespace CrystalFrost.ObjectPooling
{

    public partial class ObjectPool 
    {
        /// <summary>
        /// Manages the lifecycle of a pooled object, including its allocation, deallocation, and updates.
        /// </summary>
        public class PoolObjectManager : MonoBehaviour
        {
            private Type[] requiredComponents = null;
            private ObjectPool pool = null;
            private IPoolObjectDeallocationLogic deallocationLogic = null;
            private bool isActivated = false;
            private float updationTime = 0f;
            private string uid = null;
            public float CreationTime => updationTime;
            public bool IsActivated => isActivated;
            public float Age => Time.time - updationTime; // returns age of object alive when its alive or age of object dead when its dead

            public string UID { get => uid; set => uid = value;}
            public Type[] RequiredComponents { set => requiredComponents = value; }
            public ObjectPool Pool { set => pool = value; }
            private bool requiresDeallocationCall = false;

            /// <summary>
            /// Activates the pooled object and assigns its deallocation logic.
            /// </summary>
            /// <param name="deallocationLogic">The logic to determine when the object should be deallocated.</param>
            public void AllocateSelf(IPoolObjectDeallocationLogic deallocationLogic)
            {
                // set the object to active
                gameObject.SetActive(true);

                updationTime = Time.time;

                isActivated = true;
                this.deallocationLogic = deallocationLogic;
            }

            /// <summary>
            /// Updates the object's state and checks if it requires deallocation.
            /// </summary>
            public void UpdateObject()
            {
                if (isActivated && deallocationLogic != null && deallocationLogic.RequiresDeallocation())
                {
                    requiresDeallocationCall = true;
                }
            }

            private void LateUpdate()
            {
                if (requiresDeallocationCall)
                {
                    this.DeallocateSelf();
                    requiresDeallocationCall = false;
                }
            }

            /// <summary>
            /// Sets the deallocation logic for the pooled object.
            /// </summary>
            /// <param name="deallocationLogic">The deallocation logic to assign.</param>
            public void SetDeallocationLogic(IPoolObjectDeallocationLogic deallocationLogic)
            {
                this.deallocationLogic = deallocationLogic;
            }

            /// <summary>
            /// Deactivates the object, returns it to the pool, and performs necessary cleanup.
            /// </summary>
            public void DeallocateSelf()
            {
                isActivated = false;

                // destroy any component that is not in the requiredComponents list and is not a Transform
                // May be for later
                // foreach (Component cmpnt in gameObject.GetComponents<Component>())
                // {
                //     if (!requiredComponents.Contains(cmpnt.GetType()) && !(cmpnt is Transform))
                //     {
                //         Destroy(cmpnt);
                //     }
                // }

                // reset the transform
                transform.SetParent(pool.poolParentObject.transform);

                gameObject.SetActive(false);

                updationTime = Time.time;
            }
            
        }
    }

}

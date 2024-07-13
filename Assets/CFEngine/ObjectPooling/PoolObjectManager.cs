using UnityEngine;
using System;


namespace CrystalFrost.ObjectPooling
{

    public partial class ObjectPool 
    {
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

            // Activate the object and assign deallocation logic
            public void AllocateSelf(IPoolObjectDeallocationLogic deallocationLogic)
            {
                // set the object to active
                gameObject.SetActive(true);

                updationTime = Time.time;

                isActivated = true;
                this.deallocationLogic = deallocationLogic;
            }

            // Update the object, checking if it requires deallocation
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

            // Set the deallocation logic for the object
            public void SetDeallocationLogic(IPoolObjectDeallocationLogic deallocationLogic)
            {
                this.deallocationLogic = deallocationLogic;
            }

            // Deactivate the object and perform cleanup
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

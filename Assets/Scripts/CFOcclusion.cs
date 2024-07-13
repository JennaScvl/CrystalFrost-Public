using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CFOcclusion : MonoBehaviour
{
    // Start is called before the first frame update
    public Dictionary<GameObject, double> agedMeshHolders= new();
    public int rayCount = 500;
    public float sphereCastRadius = 0.2f;
    public float lastSeenTimeOut = 0.25f;
    public float shadowCasterRadius = 32f;
    public float rayFrequency = 0.01f;
    Camera camera;
    void Start()
    {
        camera = gameObject.GetComponent<Camera>();
        StartCoroutine(CastRays());
    }

    IEnumerator CastRays()
    {
		Ray ray;
		int i;
        double t;
        Renderer[] renderers;
        Queue<GameObject> deleteList;
        LayerMask layerMask = LayerMask.GetMask(new string[] { "Default" });
		while (true)
        {
            t = Time.realtimeSinceStartupAsDouble;
            for(i=0;i< rayCount;i++)
            {
                ray = camera.ScreenPointToRay(new Vector3(Random.Range(0, Screen.width - 1), Random.Range(0, Screen.height - 1), 0f));

                //if (Physics.SphereCast(ray, sphereCastRadius, out hit, 256f, 0))
                if (Physics.SphereCast(ray, sphereCastRadius, out RaycastHit hit, 256f, layerMask))
				{
					GameObject objectHit = hit.transform.gameObject;
					Debug.Log($"Got Hit {objectHit.transform.parent.name}");
					if (agedMeshHolders.ContainsKey(objectHit.transform.parent.gameObject))
                    {
						Debug.Log("Hit By Spherecast Refreshing age");
						agedMeshHolders[objectHit.transform.parent.gameObject] = t;
                    }
					// Do something with the object that was hit by the raycast.
				}
			}
			Collider[] hitColliders = Physics.OverlapSphere(transform.position, shadowCasterRadius, layerMask);
			foreach (var hitCollider in hitColliders)
			{
				if(agedMeshHolders.ContainsKey(hitCollider.transform.parent.gameObject))
                {
                    Debug.Log("Overlap Sphere Refreshing age");
                    agedMeshHolders[hitCollider.transform.parent.gameObject] = t;
                }
			}

            deleteList = new Queue<GameObject>();
			foreach (KeyValuePair<GameObject,double> kvp in agedMeshHolders)
            {
                if(kvp.Key == null)
                {
					deleteList.Enqueue(kvp.Key);
					continue;
                }
				renderers = kvp.Key.GetComponentsInChildren<Renderer>(true);
				if (t - kvp.Value < lastSeenTimeOut)
                {
                    Debug.Log("under age, enabling renderer");
                    kvp.Key.GetComponent<LODGroup>().enabled = true;
                    foreach (Renderer tf in renderers)

                    {
                        tf.enabled = true;
                    }
                }
                else
                {
					Debug.Log("over age, disabling renderer");
					kvp.Key.GetComponent<LODGroup>().enabled = false;
					foreach (Renderer tf in renderers)
					{
						tf.enabled = false;
					}
				}
			}
            while(deleteList.Count > 0)
            {
                agedMeshHolders.Remove(deleteList.Dequeue());
            }
			yield return new WaitForSecondsRealtime(rayFrequency);
        }
    }


}

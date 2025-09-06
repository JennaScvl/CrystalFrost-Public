using UnityEngine;

public class ClickToInteract : MonoBehaviour
{
    void Update()
    {
        // Check for left mouse button down
        if (Input.GetMouseButtonDown(0))
        {
            // Create a ray from the camera at the mouse position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Perform the raycast
            if (Physics.Raycast(ray, out hit))
            {
                // Check if the hit object has a PrimInfo component
                PrimInfo primInfo = hit.collider.GetComponent<PrimInfo>();
                if (primInfo != null)
                {
                    // Get the ScenePrimData from the SimManager
                    if (ClientManager.simManager.scenePrims.TryGetValue(primInfo.localID, out ScenePrimData spd))
                    {
                        // We have the prim, now call the Click method
                        Debug.Log($"Clicked on prim: {spd.prim.ID} (Local: {spd.prim.LocalID}), Face: {primInfo.face}");

                        // Calculate tangent (a simple perpendicular vector to the normal)
                        Vector3 tangent = Vector3.Cross(hit.normal, Vector3.up).normalized;
                        if (tangent.sqrMagnitude == 0)
                        {
                            tangent = Vector3.Cross(hit.normal, Vector3.right).normalized;
                        }

                        // Call the existing Click extension method
                        spd.Click(
                            hit.textureCoord,  // uvTouch
                            hit.textureCoord,  // surfaceTouch (using textureCoord as per original Click method)
                            primInfo.face,
                            hit.point,         // position
                            hit.normal,
                            tangent
                        );
                    }
                }
            }
        }
    }
}

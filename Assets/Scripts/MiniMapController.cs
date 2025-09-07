using UnityEngine;

public class MiniMapController : MonoBehaviour
{
    private const float MAP_HEIGHT_OFFSET = 512f;
    private const float MAP_ORTHO_SIZE = 64f;

    void Start()
    {
        CreateRenderingSystem();
    }

    void LateUpdate()
    {
        // Make the camera follow the player's avatar
        if (ClientManager.miniMapCamera != null && ClientManager.avatar != null && ClientManager.avatar.myAvatar != null)
        {
            Vector3 playerPos = ClientManager.avatar.myAvatar.position;
            ClientManager.miniMapCamera.transform.position = new Vector3(playerPos.x, playerPos.y + MAP_HEIGHT_OFFSET, playerPos.z);
        }
    }

    private void CreateRenderingSystem()
	{
		// 1. Create RenderTexture
		ClientManager.miniMapTexture = new RenderTexture(512, 512, 16);

		// 2. Create Camera GameObject
		GameObject camGO = new GameObject("MiniMapCamera");
		camGO.transform.rotation = Quaternion.Euler(90, 0, 0); // Look straight down

		// 3. Configure Camera component
		Camera cam = camGO.AddComponent<Camera>();
		cam.orthographic = true;
		cam.orthographicSize = MAP_ORTHO_SIZE;
		cam.targetTexture = ClientManager.miniMapTexture;
		cam.clearFlags = CameraClearFlags.SolidColor;
		cam.backgroundColor = Color.black;

		// Render Default and Water layers. Terrain is on the Default layer.
		cam.cullingMask = LayerMask.GetMask("Default", "Water");

		ClientManager.miniMapCamera = cam;
	}
}

using OpenMetaverse;
using UnityEngine;
using UnityEngine.UI;

public class PreviewManager : MonoBehaviour
{
    public RawImage previewImage; // Assign this in the editor
    public Vector2 renderTextureSize = new Vector2(256, 256);

    private Camera _previewCamera;
    private GameObject _previewSceneRoot;
    private GameObject _previewObject;
    private RenderTexture _renderTexture;

    void Awake()
    {
        SetupPreviewScene();
    }

    private void SetupPreviewScene()
    {
        // Create a root object for the preview scene to keep things tidy
        _previewSceneRoot = new GameObject("PreviewScene");
        _previewSceneRoot.transform.position = new Vector3(5000, 5000, 5000); // Place it far away

        // Create the preview camera
        GameObject camGo = new GameObject("PreviewCamera");
        camGo.transform.SetParent(_previewSceneRoot.transform);
        _previewCamera = camGo.AddComponent<Camera>();
        _previewCamera.cullingMask = LayerMask.GetMask("Preview"); // Use a dedicated layer
        _previewCamera.clearFlags = CameraClearFlags.SolidColor;
        _previewCamera.backgroundColor = Color.gray;

        // Create the RenderTexture
        _renderTexture = new RenderTexture((int)renderTextureSize.x, (int)renderTextureSize.y, 16, RenderTextureFormat.Default);
        _previewCamera.targetTexture = _renderTexture;

        // Assign the RenderTexture to the UI Image
        if (previewImage != null)
        {
            previewImage.texture = _renderTexture;
            previewImage.gameObject.SetActive(true);
        }

        // Add a light to the preview scene
        GameObject lightGo = new GameObject("PreviewLight");
        lightGo.transform.SetParent(_previewSceneRoot.transform);
        Light light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.0f;
        lightGo.transform.rotation = Quaternion.Euler(50, -30, 0);
    }

    public void ShowPreview(InventoryItem item)
    {
        if (item == null) return;

        // Clear previous object
        if (_previewObject != null)
        {
            Destroy(_previewObject);
        }

        // Request the mesh for the item
        // This is a simplified example. A real implementation would use the asset manager
        // and handle the asynchronous nature of mesh loading.
        Debug.Log("Requesting preview for item: " + item.Name);

        // For now, we'll just create a placeholder cube.
        _previewObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _previewObject.transform.SetParent(_previewSceneRoot.transform);
        _previewObject.transform.localPosition = Vector3.zero;
        _previewObject.layer = LayerMask.NameToLayer("Preview"); // Set to the preview layer

        // Center camera on the object
        _previewCamera.transform.position = _previewSceneRoot.transform.position + new Vector3(0, 0, -2);
        _previewCamera.transform.LookAt(_previewObject.transform);
    }

    void OnDestroy()
    {
        if (_renderTexture != null)
        {
            _renderTexture.Release();
        }
        if (_previewSceneRoot != null)
        {
            Destroy(_previewSceneRoot);
        }
    }
}

using UnityEngine;
using TMPro;

public class StatusUIManager : MonoBehaviour
{
    private TextMeshProUGUI flyingStatusText;
    private GameObject flyingStatusObject;

    void Start()
    {
        // Find the main canvas.
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("StatusUIManager: Could not find a Canvas in the scene.");
            // As a fallback, create a canvas.
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        Transform canvasTransform = canvas.transform;

        // Create the GameObject for the status text
        flyingStatusObject = new GameObject("FlyingStatus");
        flyingStatusObject.transform.SetParent(canvasTransform, false);

        // Add the TextMeshPro component
        flyingStatusText = flyingStatusObject.AddComponent<TextMeshProUGUI>();

        // Set text properties
        flyingStatusText.text = "Flying";
        flyingStatusText.fontSize = 24;
        flyingStatusText.color = Color.white;
        flyingStatusText.alignment = TextAlignmentOptions.TopCenter;

        // Set RectTransform properties to anchor it to the top center
        RectTransform rectTransform = flyingStatusText.rectTransform;
        rectTransform.anchorMin = new Vector2(0.5f, 1);
        rectTransform.anchorMax = new Vector2(0.5f, 1);
        rectTransform.pivot = new Vector2(0.5f, 1);
        rectTransform.anchoredPosition = new Vector2(0, -50); // 50 pixels from the top
        rectTransform.sizeDelta = new Vector2(300, 50);

        // Initially hide the status
        flyingStatusObject.SetActive(false);
    }

    public void SetFlyingStatus(bool isFlying)
    {
        if (flyingStatusObject != null)
        {
            flyingStatusObject.SetActive(isFlying);
        }
    }
}

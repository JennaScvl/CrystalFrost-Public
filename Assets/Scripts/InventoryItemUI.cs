using OpenMetaverse;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour
{
    public InventoryItem Item { get; private set; }

    [Header("Buttons (assigned in prefab)")]
    public Button wearButton;
    public Button previewButton;

    private PreviewManager _previewManager;

    public void Initialize(InventoryItem item, PreviewManager previewManager)
    {
        this.Item = item;
        this._previewManager = previewManager;

        // Configure Wear Button
        if (wearButton != null)
        {
            bool canBeWorn = item is InventoryWearable || item is InventoryAttachment;
            wearButton.gameObject.SetActive(canBeWorn);
            if(canBeWorn)
            {
                wearButton.onClick.AddListener(OnWear);
            }
        }

        // Configure Preview Button
        if (previewButton != null)
        {
            bool canBePreviewed = item is InventoryObject; // Only preview objects for now
            previewButton.gameObject.SetActive(canBePreviewed);
            if(canBePreviewed)
            {
                previewButton.onClick.AddListener(OnPreview);
            }
        }
    }

    private void OnWear()
    {
        Debug.Log($"Wearing item: {Item.Name}");
        // Use the existing appearance manager to wear the item.
        ClientManager.client.Appearance.Attach(Item, AttachmentPoint.Default, true);
    }

    private void OnPreview()
    {
        if (_previewManager != null)
        {
            Debug.Log($"Requesting preview for: {Item.Name}");
            _previewManager.ShowPreview(Item);
        }
    }
}

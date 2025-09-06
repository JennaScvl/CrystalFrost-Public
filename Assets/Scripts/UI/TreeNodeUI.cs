using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OpenMetaverse;

using UnityEngine.EventSystems;

public class TreeNodeUI : MonoBehaviour, IPointerClickHandler
{
    public TMP_Text itemNameText;
    public Image itemIcon;
    public Button expandButton; // The button to expand/collapse folders
    public LayoutElement indentElement; // Used to create the visual indentation for the tree

    private InventoryBase itemData;
    private InventoryWindowUI inventoryWindow;
    private int depth;
    private bool isExpanded = false;

    public void SetData(InventoryBase data, int nodeDepth, InventoryWindowUI window)
    {
        itemData = data;
        depth = nodeDepth;
        inventoryWindow = window;

        itemNameText.text = data.Name;
        indentElement.flexibleWidth = depth * 20; // Use flexible width for layout group

        if (data is InventoryFolder)
        {
            expandButton.gameObject.SetActive(true);
            // TODO: Set folder icon
            expandButton.onClick.AddListener(ToggleExpand);
        }
        else
        {
            expandButton.gameObject.SetActive(false);
            // TODO: Set item icon based on type
        }
    }

    private void ToggleExpand()
    {
        isExpanded = !isExpanded;
        // Update visual cue for expansion (e.g., rotate arrow)
        expandButton.transform.localRotation = isExpanded ? Quaternion.Euler(0, 0, 90) : Quaternion.identity;

        inventoryWindow.ToggleFolder(itemData as InventoryFolder, this, depth);
    }

    public bool IsExpanded() => isExpanded;
    public UUID GetItemUUID() => itemData.UUID;
    public InventoryBase GetItemData() => itemData;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            inventoryWindow.ShowContextMenu(itemData, eventData.position);
        }
    }
}

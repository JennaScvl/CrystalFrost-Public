using UnityEngine;
using OpenMetaverse;
using System.Collections.Generic;

public class InventoryWindowUI : MonoBehaviour
{
    public GameObject treeNodePrefab;
    public Transform contentRoot;
    public ContextMenuUI contextMenu;

    // A dictionary to keep track of the UI nodes to manage expand/collapse state
    private Dictionary<UUID, TreeNodeUI> uiNodes = new Dictionary<UUID, TreeNodeUI>();
    private Dictionary<UUID, List<GameObject>> childNodes = new Dictionary<UUID, List<GameObject>>();

    private void Start()
    {
        // Subscribe to the folder update event to know when the inventory is ready
        ClientManager.client.Inventory.FolderUpdated += Inventory_FolderUpdated;
    }

    private void OnDestroy()
    {
        // Always unsubscribe from events
        ClientManager.client.Inventory.FolderUpdated -= Inventory_FolderUpdated;
    }

    private void Inventory_FolderUpdated(object sender, FolderUpdatedEventArgs e)
    {
        // Check if the root folder has been updated, which signals the inventory is loaded
        if (e.FolderID == ClientManager.client.Inventory.Store.RootFolder.UUID)
        {
            // Unsubscribe so we don't re-populate on every subsequent folder update
            ClientManager.client.Inventory.FolderUpdated -= Inventory_FolderUpdated;

            // Now it's safe to populate the tree
            PopulateTree(ClientManager.client.Inventory.Store.RootFolder, contentRoot, 0);
        }
    }

    public void PopulateTree(InventoryFolder parentFolder, Transform parentTransform, int depth)
    {
        List<InventoryBase> contents = ClientManager.client.Inventory.Store.GetContents(parentFolder.UUID);

        childNodes[parentFolder.UUID] = new List<GameObject>();

        foreach (var item in contents)
        {
            GameObject nodeGO = Instantiate(treeNodePrefab, parentTransform);
            TreeNodeUI nodeUI = nodeGO.GetComponent<TreeNodeUI>();

            nodeUI.SetData(item, depth, this);
            uiNodes[item.UUID] = nodeUI;
            childNodes[parentFolder.UUID].Add(nodeGO);

            if (item is InventoryFolder)
            {
                nodeGO.name = $"Folder: {item.Name}";
            }
            else
            {
                nodeGO.name = $"Item: {item.Name}";
            }
        }
    }

    public void ToggleFolder(InventoryFolder folder, TreeNodeUI nodeUI, int depth)
    {
        bool isExpanded = nodeUI.IsExpanded();

        if (isExpanded)
        {
            // If it's already expanded, we need to populate its children
            if (!childNodes.ContainsKey(folder.UUID))
            {
                PopulateTree(folder, nodeUI.transform, depth + 1);
            }
            // Show children
            if (childNodes.TryGetValue(folder.UUID, out var children))
            {
                foreach(var child in children)
                {
                    child.SetActive(true);
                }
            }
        }
        else
        {
            // If it's collapsed, hide all descendants
            HideChildren(folder.UUID);
        }
    }

    private void HideChildren(UUID folderId)
    {
        if (childNodes.TryGetValue(folderId, out var children))
        {
            foreach (var child in children)
            {
                child.SetActive(false);
                // If this child is a folder, recursively hide its children too
                InventoryBase itemData = uiNodes[child.GetComponent<TreeNodeUI>().GetItemUUID()].GetItemData();
                if (itemData is InventoryFolder)
                {
                    HideChildren(itemData.UUID);
                }
            }
        }
    }

    public void ShowContextMenu(InventoryBase item, Vector2 position)
    {
        contextMenu.ClearButtons();

        // Add actions based on item type
        if (item is InventoryWearable || item is InventoryAttachment)
        {
            contextMenu.AddButton("Wear", () => {
                ClientManager.client.Appearance.AddToOutfit(new List<InventoryItem> { (InventoryItem)item }, true);
            });
            contextMenu.AddButton("Take Off", () => {
                ClientManager.client.Appearance.RemoveFromOutfit(new List<InventoryItem> { (InventoryItem)item });
            });
        }

        if (item is InventoryAttachment)
        {
             // The "Attach To..." functionality requires a sub-menu for attachment points,
             // which is out of scope for this initial implementation.
             // contextMenu.AddButton("Attach To", () => { /* TODO */ });
             contextMenu.AddButton("Detach", () => {
                ClientManager.client.Appearance.Detach((InventoryItem)item);
             });
        }

        // Add more general actions
        contextMenu.AddButton("Delete", () => {
            ClientManager.client.Inventory.Remove(item.UUID, null);
        });

        contextMenu.Show(position);
    }
}

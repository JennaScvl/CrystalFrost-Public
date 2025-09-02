using OpenMetaverse;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("UI Components")]
    public PreviewManager previewManager;

    [Header("UI Prefabs")]
    public GameObject FolderPrefab;
    public GameObject ItemPrefab;

    [Header("UI Roots")]
    public Transform TreeRoot; // The parent for the folder hierarchy UI
    public Transform ContentRoot; // The parent for the content of the selected folder

    [Header("UI Settings")]
    public float IndentSize = 20f;

    private GridClient _client;
    private InventoryFolder _currentFolder;

    private Dictionary<UUID, GameObject> _folderUIItems = new Dictionary<UUID, GameObject>();
    private Dictionary<UUID, GameObject> _itemUIItems = new Dictionary<UUID, GameObject>();

    void Start()
    {
        _client = ClientManager.client;

        if (FolderPrefab != null) FolderPrefab.SetActive(false);
        if (ItemPrefab != null) ItemPrefab.SetActive(false);

        // Ensure we have a preview manager
        if (previewManager == null)
        {
            previewManager = FindObjectOfType<PreviewManager>();
            if (previewManager == null)
            {
                gameObject.AddComponent<PreviewManager>();
                previewManager = GetComponent<PreviewManager>();
            }
        }

        if (_client.Inventory.Store.RootFolder != null)
        {
            InitializeInventoryUI();
        }
        else
        {
            _client.Inventory.InventoryObjectAdded += OnInventoryObjectAdded;
        }
    }

    void OnDestroy()
    {
        if (_client != null)
        {
            _client.Inventory.InventoryObjectAdded -= OnInventoryObjectAdded;
        }
    }

    private void OnInventoryObjectAdded(object sender, InventoryObjectAddedEventArgs e)
    {
        if (e.Obj is InventoryFolder folder && folder.ParentUUID == UUID.Zero)
        {
            InitializeInventoryUI();
            _client.Inventory.InventoryObjectAdded -= OnInventoryObjectAdded;
        }
    }

    private void InitializeInventoryUI()
    {
        if (_client.Inventory.Store.RootFolder == null) return;

        foreach (Transform child in TreeRoot)
        {
            if(child.gameObject.activeSelf) Destroy(child.gameObject);
        }
        _folderUIItems.Clear();

        CreateFolderNode(_client.Inventory.Store.RootFolder, TreeRoot, 0);
        DisplayFolderContents(_client.Inventory.Store.RootFolder);
    }

    private void CreateFolderNode(InventoryFolder folder, Transform parent, int depth)
    {
        GameObject folderGo = Instantiate(FolderPrefab, parent);
        folderGo.name = $"Folder: {folder.Name}";
        folderGo.SetActive(true);
        _folderUIItems[folder.UUID] = folderGo;

        var text = folderGo.GetComponentInChildren<TMP_Text>();
        if (text != null) text.text = folder.Name;

        var spacer = folderGo.transform.Find("Spacer");
        if (spacer != null)
        {
            var layoutElement = spacer.GetComponent<LayoutElement>();
            if (layoutElement != null) layoutElement.preferredWidth = depth * IndentSize;
        }

        var button = folderGo.GetComponent<Button>();
        if (button != null) button.onClick.AddListener(() => OnFolderClicked(folder));

        List<InventoryBase> contents = _client.Inventory.Store.GetContents(folder.UUID);
        contents.Sort((a, b) => a.Name.CompareTo(b.Name));

        foreach (var content in contents)
        {
            if (content is InventoryFolder subFolder)
            {
                CreateFolderNode(subFolder, parent, depth + 1);
            }
        }
    }

    private void OnFolderClicked(InventoryFolder folder)
    {
        DisplayFolderContents(folder);
    }

    private void DisplayFolderContents(InventoryFolder folder)
    {
        _currentFolder = folder;

        foreach (Transform child in ContentRoot)
        {
            Destroy(child.gameObject);
        }
        _itemUIItems.Clear();

        List<InventoryBase> contents = _client.Inventory.Store.GetContents(folder.UUID);
        contents.Sort((a, b) => {
            if (a is InventoryFolder && !(b is InventoryFolder)) return -1;
            if (!(a is InventoryFolder) && b is InventoryFolder) return 1;
            return a.Name.CompareTo(b.Name);
        });

        foreach (var content in contents)
        {
            GameObject uiGo;
            if (content is InventoryFolder subFolder)
            {
                uiGo = Instantiate(FolderPrefab, ContentRoot);
                uiGo.GetComponent<Button>().onClick.AddListener(() => OnFolderClicked(subFolder));
            }
            else if (content is InventoryItem item)
            {
                uiGo = Instantiate(ItemPrefab, ContentRoot);
                var itemUI = uiGo.AddComponent<InventoryItemUI>();
                itemUI.Initialize(item, previewManager); // Pass the preview manager
            }
            else
            {
                continue;
            }

            uiGo.name = content.Name;
            uiGo.SetActive(true);
            uiGo.GetComponentInChildren<TMP_Text>().text = content.Name;
            _itemUIItems[content.UUID] = uiGo;
        }
    }
}

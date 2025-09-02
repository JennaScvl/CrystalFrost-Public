using OpenMetaverse;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ContactsList : MonoBehaviour
{
    [Header("UI Prefabs")]
    public GameObject baseContactButton; // The prefab for a contact entry in the list

    [Header("UI Settings")]
    public Color onlineColor = new Color(0.5f, 1.0f, 0.5f, 1.0f); // A pleasant green
    public Color offlineColor = Color.gray;

    // A class to hold the data and UI element for a single contact
    public class ContactEntry
    {
        public string name;
        public UUID uuid;
        public GameObject button;
        public bool isOnline;
        private TMP_Text nameTag;

        public ContactEntry(string name, UUID uuid, GameObject button, bool isOnline, ContactsList listManager)
        {
            this.name = name;
            this.uuid = uuid;
            this.button = button;
            this.nameTag = button.GetComponentInChildren<TMP_Text>();

            UI_IMButton b = button.GetComponent<UI_IMButton>();
            b.name = name;
            b.uuid = uuid;
            b.isContactButton = true;

            UpdateEntry(name, isOnline, listManager);
        }

        public void UpdateEntry(string newName, bool newIsOnline, ContactsList listManager)
        {
            this.name = newName;
            this.isOnline = newIsOnline;

            if (nameTag != null)
            {
                nameTag.text = this.name;
                nameTag.color = this.isOnline ? listManager.onlineColor : listManager.offlineColor;
            }

            if (button != null)
            {
                button.name = $"Contact: {this.name} ({ (this.isOnline ? "Online" : "Offline") })";
            }
        }
    }

    public List<ContactEntry> contactEntries = new List<ContactEntry>();

    void Awake()
    {
        // The base button is a template; it should be inactive in the scene.
        if (baseContactButton != null)
        {
            baseContactButton.SetActive(false);
        }
    }

    public void UpdateContactStatus(UUID contactId, bool isOnline)
    {
        var entry = contactEntries.Find(e => e.uuid == contactId);
        if (entry != null)
        {
            entry.UpdateEntry(entry.name, isOnline, this);
        }
    }

    public void UpdateContactName(UUID contactId, string newName)
    {
        var entry = contactEntries.Find(e => e.uuid == contactId);
        if (entry != null)
        {
            entry.UpdateEntry(newName, entry.isOnline, this);
        }
    }

    public void AddContact(string name, UUID uuid, bool isOnline)
    {
        if (name == null) { name = "Loading..."; }

        // Instantiate the button from the prefab and set its parent.
        // A VerticalLayoutGroup on the parent will handle positioning automatically.
        GameObject newButton = Instantiate(baseContactButton, baseContactButton.transform.parent, false);
        newButton.SetActive(true);

        ContactEntry newEntry = new ContactEntry(name, uuid, newButton, isOnline, this);
        contactEntries.Add(newEntry);
    }

    public void ClearContacts()
    {
        foreach (var entry in contactEntries)
        {
            if (entry.button != null)
            {
                Destroy(entry.button);
            }
        }
        contactEntries.Clear();
    }
}

using OpenMetaverse;
using System.Collections.Generic;
using UnityEngine;
using OMVVector2 = OpenMetaverse.Vector2;
using Vector2 = UnityEngine.Vector2;

public class ContactsList : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject baseContactButton;
	public GameObject console;
	
    public class ContactEntry
    {
        public string name;
        public UUID uuid;
        public GameObject button;
        TMPro.TMP_Text nameTag;
        public ContactEntry(string name, UUID uuid, GameObject button, int index)
        {

			this.name = name;
			this.uuid = uuid;
			this.button = button;
			nameTag = button.GetComponentInChildren<TMPro.TMP_Text>();
			nameTag.text = name;
            RectTransform rect = button.GetComponent<RectTransform>();
            Vector2 anchoredPos = rect.anchoredPosition;
			anchoredPos.y -= 30f * index;
			rect.anchoredPosition = anchoredPos;
			UI_IMButton b = button.GetComponent<UI_IMButton>();
			b.name = name;
			b.uuid = uuid;

		}

		public void UpdateEntry(string name)
		{
			//Debug.Log($"Updating name to {name}");
			this.name = name;
			nameTag.text = name;
			button.name = name;
			UI_IMButton b = button.GetComponent<UI_IMButton>();
			b.name = name;
			b.uuid = uuid;
			b.isContactButton = true;
			//Debug.Log($"Name is now {nameTag.text}");
		}
	}

    public List<ContactEntry> contactEntries = new List<ContactEntry>();
    void Awake()
    {
        //Debug.Log(baseContactButton.name);
        contactEntries.Add(new ContactEntry("Loading...", UUID.Zero, baseContactButton, 0));
		console.SetActive(false);
    }

	public void UpdateContact(string name, UUID uuid)
	{
		//Debug.Log($"{name}, {uuid}");s
		foreach(ContactEntry entry in contactEntries)
		{
			if (entry.uuid == uuid)
			{
				entry.UpdateEntry(name);
			}
		}
	}
	public ContactEntry AddContact(string name, UUID uuid)
    {
        if(name == null) { name = "Loading..."; }
		contactEntries.Add(new ContactEntry($"{name}", uuid, Instantiate(baseContactButton, baseContactButton.transform.parent, true), contactEntries.Count - 1));
		return contactEntries[contactEntries.Count - 1];
	}
}

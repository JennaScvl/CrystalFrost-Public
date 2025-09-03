using OpenMetaverse;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatWindowUI : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject chatConsole;
    public GameObject contactsConsole;
    public ContactsList contactsList;
    public RectTransform contactsRectTransform;
    void Start()
    {
        ClientManager.chatWindow = this;
        contactsList = gameObject.GetComponent<ContactsList>();
    }

    public TMPro.TMP_Text contactButtonText;
    bool contactsMode = false;
    public void ContactsButton()
    {
        contactsMode = !contactsMode;
        if (contactsMode) contactButtonText.text = "Chat";
        else contactButtonText.text = "Contacts";

        chatConsole.SetActive(!contactsMode);
        contactsConsole.SetActive(contactsMode);
		ClientManager.soundManager.PlayUISound(new UUID("4c8c3c77-de8d-bde2-b9b8-32635e0fd4a6"));

	}

	public void SwitchToIM(UUID uuid)
	{
		contactsMode = !contactsMode;
		if (contactsMode) contactButtonText.text = "Chat";
		else contactButtonText.text = "Contacts";

		chatConsole.SetActive(!contactsMode);
		contactsConsole.SetActive(contactsMode);
		ClientManager.chat.SwitchTab(uuid);
	}

	public void PopulateContacts()
    {
        ContactsList.ContactEntry contactEntry;
        int counter = 0;
        List<UUID> avatarNames = new List<UUID>();
		ClientManager.client.Friends.FriendList.ForEach(delegate (FriendInfo friend)
		{
            // append the name of the friend to our output
            //Debug.Log($"Contact: {friend.Name} {friend.UUID}");
            contactEntry = contactsList.AddContact(friend.Name, friend.UUID);

            contactEntry.button.SetActive(true);
            //contactsRectTransform.rect.height += 30f;
            Rect rect = contactsRectTransform.rect;
            Rect parentRect = contactsRectTransform.transform.parent.GetComponent<RectTransform>().rect;
            avatarNames.Add(friend.UUID);
			//contactsRectTransform.

			/*contactsRectTransform.localScale = new Vector3(1f, 1f, 1f);
			contactsRectTransform.anchorMax = new Vector2(1f, 1f);
			contactsRectTransform.anchorMin = new Vector2(0f, 0f);
			contactsRectTransform.sizeDelta = new Vector2(0f, (parentRect.height + 30f));
			contactsRectTransform.offsetMin = new Vector3(0f, 1f);
		    contactsRectTransform.offsetMax = new Vector3(0f, -0f);*/

			//contactsRectTransform.rect = rect;
		});
		ClientManager.client.Avatars.RequestAvatarNames(avatarNames);


	}

}

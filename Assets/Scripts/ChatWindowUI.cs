using OpenMetaverse;
using System.Collections.Generic;
using UnityEngine;

public class ChatWindowUI : MonoBehaviour
{
    public GameObject chatConsole;
    public GameObject contactsConsole;
    public ContactsList contactsList;

    public TMPro.TMP_Text contactButtonText;
    private bool _contactsMode = false;

    void Start()
    {
        ClientManager.chatWindow = this;
        if (contactsList == null)
        {
            contactsList = GetComponentInChildren<ContactsList>(true);
        }

        // Subscribe to friend status events
        ClientManager.client.Friends.FriendOnline += Friends_OnFriendOnline;
        ClientManager.client.Friends.FriendOffline += Friends_OnFriendOffline;
        ClientManager.client.Friends.FriendRightsUpdate += Friends_OnFriendRightsUpdate;
    }

    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (ClientManager.client != null && ClientManager.client.Friends != null)
        {
            ClientManager.client.Friends.FriendOnline -= Friends_OnFriendOnline;
            ClientManager.client.Friends.FriendOffline -= Friends_OnFriendOffline;
            ClientManager.client.Friends.FriendRightsUpdate -= Friends_OnFriendRightsUpdate;
        }
    }

    private void Friends_OnFriendRightsUpdate(object sender, FriendInfoEventArgs e)
    {
        if (contactsList != null)
        {
            contactsList.UpdateContactStatus(e.Friend.UUID, e.Friend.IsOnline);
        }
    }

    private void Friends_OnFriendOffline(object sender, FriendInfoEventArgs e)
    {
        if (contactsList != null)
        {
            contactsList.UpdateContactStatus(e.Friend.UUID, false);
        }
    }

    private void Friends_OnFriendOnline(object sender, FriendInfoEventArgs e)
    {
        if (contactsList != null)
        {
            contactsList.UpdateContactStatus(e.Friend.UUID, true);
        }
    }

    public void ContactsButton()
    {
        _contactsMode = !_contactsMode;
        if (_contactsMode)
        {
            contactButtonText.text = "Chat";
            PopulateContacts(); // Refresh contacts when switching to the view
        }
        else
        {
            contactButtonText.text = "Contacts";
        }

        chatConsole.SetActive(!_contactsMode);
        contactsConsole.SetActive(_contactsMode);
		ClientManager.soundManager.PlayUISound(new UUID("4c8c3c77-de8d-bde2-b9b8-32635e0fd4a6"));
	}

	public void SwitchToIM(UUID uuid)
	{
		_contactsMode = false; // Switch back to chat mode
		contactButtonText.text = "Contacts";

		chatConsole.SetActive(true);
		contactsConsole.SetActive(false);
		ClientManager.chat.SwitchTab(uuid);
	}

	public void PopulateContacts()
    {
        if (contactsList == null) return;

        contactsList.ClearContacts();

        List<UUID> avatarNamesToRequest = new List<UUID>();
		ClientManager.client.Friends.FriendList.ForEach(delegate (FriendInfo friend)
		{
            contactsList.AddContact(friend.Name, friend.UUID, friend.IsOnline);
            if (string.IsNullOrEmpty(friend.Name))
            {
                avatarNamesToRequest.Add(friend.UUID);
            }
		});

        if (avatarNamesToRequest.Count > 0)
        {
            // This will trigger name updates, which should be handled by the ContactsList
            ClientManager.client.Avatars.RequestAvatarNames(avatarNamesToRequest);
        }
	}
}

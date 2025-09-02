using OpenMetaverse;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Chat;

public class Chat : MonoBehaviour
{
	public UnityEngine.UI.Button dummyButton;
    public TMP_Text log;
    public GameObject chatTabButtonPrefab;
	public GameObject nearbyButton;
    public Transform chatTabRoot; // A VerticalLayoutGroup should be attached to this object's GameObject.
	public TMP_InputField input;
	public TMP_Text inputText;
	public Color unreadColor = Color.yellow;
	private Color _defaultTabColor;
	UUID selectedChat = UUID.Zero;
	private bool _isGroupChatSelected = false;


	public Dictionary<UUID, string> avatarNames = new();
	private void Awake()
	{
        ClientManager.chat = this;
	}
	public class ChatTab
    {
        public string name;
        public UUID uuid; // For IMs, this is the agent ID. For Group Chat, it's the session ID.
        public string log;
        public GameObject tabButton;
		public bool hasUnreadMessages = false;
		public bool isGroupChat = false;
    }

    public ConcurrentDictionary<UUID, ChatTab> tabs = new();
    public ConcurrentQueue<InstantMessageEventArgs> imEvents = new();
	private readonly ConcurrentQueue<string> chatStrings = new();

    void Start()
    {
		_defaultTabColor = nearbyButton.GetComponentInChildren<TMP_Text>().color;

		// Event subscriptions
		ClientManager.client.Self.IM += new EventHandler<InstantMessageEventArgs>(IncomingIM);
		ClientManager.client.Self.ChatFromSimulator += new EventHandler<ChatEventArgs>(ChatFromSimulator);
		ClientManager.client.Groups.GroupNameReply += new EventHandler<GroupNameReplyEventArgs>(Groups_GroupNameReply);

		// Setup the default "Local Chat" tab
		tabs.TryAdd(UUID.Zero, new ChatTab() { log = string.Empty, name = "Local Chat", tabButton = nearbyButton, uuid = UUID.Zero});
	}

    void OnDestroy()
    {
        if (ClientManager.client == null) return;
		ClientManager.client.Self.IM -= new EventHandler<InstantMessageEventArgs>(IncomingIM);
		ClientManager.client.Self.ChatFromSimulator -= new EventHandler<ChatEventArgs>(ChatFromSimulator);
		ClientManager.client.Groups.GroupNameReply -= new EventHandler<GroupNameReplyEventArgs>(Groups_GroupNameReply);
	}

    void IncomingIM(object sender, InstantMessageEventArgs e)
    {
        imEvents.Enqueue(e);
    }

	public void SetKeyToName(UUID uuid, string name)
	{
		avatarNames.TryAdd(uuid, name);
	}

	void ChatFromSimulator(object sender, ChatEventArgs e)
	{
		// Only handle Normal, Whisper, and Shout for local chat display
		if (e.Type == ChatType.Normal || e.Type == ChatType.Whisper || e.Type == ChatType.Shout)
		{
			string chat;
			try
			{
				string senderName = "Unknown";
				if (ClientManager.simManager.scenePrims.TryGetValue(e.SourceID, out var scenePrim))
                {
					senderName = scenePrim.name;
                }

				chat = $"[{DateTime.UtcNow:HH:mm:ss}] {senderName}: {e.Message}".Replace("<", "<\u200B"); ;

				if (e.Type == ChatType.Whisper)
				{
					chat = $"[{DateTime.UtcNow:HH:mm:ss}] {senderName}: <i><size=80%>{e.Message}</size></i>";
				}
				else if (e.Type == ChatType.Shout)
				{
					chat = $"[{DateTime.UtcNow:HH:mm:ss}] {senderName}: <b><size=120%>{e.Message}</size></b>";
				}
				chatStrings.Enqueue(chat);

				if (selectedChat != UUID.Zero)
                {
					NotifyUnread(UUID.Zero);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError($"Error processing incoming chat message: {ex.Message}");
			}
		}
	}

	void ParseIMEvents()
    {
		while (imEvents.TryDequeue(out var e))
		{
			if (e.IM.Message == string.Empty || e.IM.Dialog == InstantMessageDialog.StartTyping || e.IM.Dialog == InstantMessageDialog.StopTyping) continue;

			bool isGroup = e.IM.Dialog == InstantMessageDialog.SessionSend || e.IM.Dialog == InstantMessageDialog.SessionOffline;
			UUID tabId = isGroup ? e.IM.IMSessionID : e.IM.FromAgentID;

			string chat = $"[{DateTime.UtcNow:HH:mm:ss}] {e.IM.FromAgentName}: {e.IM.Message}".Replace("<", "<\u200B");

			if (!tabs.TryGetValue(tabId, out var tab))
			{
				// New session
				string tabName = isGroup ? "Group Chat..." : e.IM.FromAgentName;
				tab = CreateNewTab(tabId, tabName, isGroup);
				if(isGroup)
                {
					ClientManager.client.Groups.RequestGroupName(tabId);
                }
			}

			tab.log += "\n" + chat;

			if (selectedChat != tabId)
			{
				NotifyUnread(tabId);
			}

			if (selectedChat == tabId)
			{
				log.text = tab.log;
			}
		}
	}

	private ChatTab CreateNewTab(UUID id, string name, bool isGroup)
    {
		GameObject b = Instantiate(nearbyButton, chatTabRoot, false);
		b.SetActive(true);
		var newTab = new ChatTab
		{
			name = name,
			uuid = id,
			log = string.Empty,
			tabButton = b,
			isGroupChat = isGroup
		};

		UI_IMButton button = newTab.tabButton.GetComponent<UI_IMButton>();
		button.buttonText.text = newTab.name;
		button.uuid = newTab.uuid;

		tabs.TryAdd(id, newTab);
		return newTab;
	}

	private void Groups_GroupNameReply(object sender, GroupNameReplyEventArgs e)
    {
		if (tabs.TryGetValue(e.GroupID, out var tab))
        {
			tab.name = e.GroupName;
			tab.tabButton.GetComponent<UI_IMButton>().buttonText.text = e.GroupName;
        }
    }

	public void StartIM(UUID agentID)
	{
		if (tabs.ContainsKey(agentID))
        {
			SwitchTab(agentID);
			return;
        }

		string name = "Loading...";
		if(avatarNames.ContainsKey(agentID)) name = avatarNames[agentID];

		CreateNewTab(agentID, name, false);
		SwitchTab(agentID);
	}

	private void NotifyUnread(UUID tabId)
    {
		if (tabs.TryGetValue(tabId, out var tab) && !tab.hasUnreadMessages)
        {
			tab.hasUnreadMessages = true;
			tab.tabButton.GetComponentInChildren<TMP_Text>().color = unreadColor;
			if(tabId != UUID.Zero) // Don't play sound for local chat notifications
			{
				ClientManager.soundManager.PlayUISound(new UUID("67cc2844-00f3-2b3c-b991-6418d01e1bb7"));
			}
		}
	}

	public void ParseChatEvents()
	{
		while (chatStrings.TryDequeue(out var chat))
		{
			tabs[UUID.Zero].log += "\n" + chat;
			if (selectedChat == UUID.Zero)
			{
				log.text = tabs[UUID.Zero].log;
			}
		}
	}

	public void SwitchTab(UUID uuid)
    {
        if (tabs.TryGetValue(uuid, out var tab))
        {
			selectedChat = uuid;
			_isGroupChatSelected = tab.isGroupChat;
            log.text = tab.log;

			if (tab.hasUnreadMessages)
            {
				tab.hasUnreadMessages = false;
				tab.tabButton.GetComponentInChildren<TMP_Text>().color = _defaultTabColor;
			}
        }
		else
		{
			StartIM(uuid);
		}
	}

	void Update()
    {
        ParseIMEvents();
		ParseChatEvents();

		if(!input.isFocused && Input.GetKeyDown(KeyCode.Return))
		{
			input.Select();
			input.ActivateInputField();
		}

		if(ClientManager.avatar != null)
			ClientManager.avatar.canMove = !input.isFocused;
	}

	public void SendChat()
	{
		if (string.IsNullOrEmpty(input.text)) return;

		string message = input.text;
		input.text = string.Empty;

		if (selectedChat == UUID.Zero)
		{
			// Local Chat
			ClientManager.client.Self.Chat(message, 0, ChatType.Normal);
		}
		else
		{
			// IM or Group Chat
			if (_isGroupChatSelected)
			{
				ClientManager.client.Self.InstantMessage(ClientManager.client.Self.Name, selectedChat, message, selectedChat, InstantMessageDialog.SessionSend, InstantMessageOnline.Online, this.transform.position, UUID.Zero, new byte[0]);
			}
			else
            {
				ClientManager.client.Self.InstantMessage(selectedChat, message);
			}

			// Add our own message to the log
			tabs[selectedChat].log += $"\n[{DateTime.UtcNow:HH:mm:ss}] {ClientManager.client.Self.Name}: {message}".Replace("<", "<\u200B");
			log.text = tabs[selectedChat].log;
		}

		dummyButton.Select();
		input.DeactivateInputField();
	}
}

using OpenMetaverse;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using TMPro;
using static Chat;
using System.IO;
using OggVorbisEncoder;
#if UNITY_EDITOR
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEditor.VersionControl;
#endif
using OMVVector2 = OpenMetaverse.Vector2;
using Vector2 = UnityEngine.Vector2;

public class Chat : MonoBehaviour
{
	public UnityEngine.UI.Button dummyButton;
    // Start is called before the first frame update
    public TMP_Text log;
    //public GameObjec
    public GameObject chatTabButtonPrefab;
	public GameObject nearbyButton;
    public Transform chatTabRoot; // A VerticalLayoutGroup should be attached to this object's GameObject.
	public TMP_InputField input;
	public TMP_Text inputText;
	UUID selectedChat = UUID.Zero;

	public Dictionary<UUID, string> avatarNames = new();
	private void Awake()
	{
        ClientManager.chat = this;
	}
	public class ChatTab
    {
        public string name;
        public UUID uuid;
        public string log;
        public GameObject tabButton;
    }

	
    public class ChatEvent
    {
        public UUID uuid;
        public string newchat;
    }

    public ConcurrentDictionary<UUID, ChatTab> tabs = new();
    public ConcurrentQueue<InstantMessageEventArgs> imEvents = new();

    void Start()
    {
		ClientManager.client.Self.IM += new EventHandler<InstantMessageEventArgs>(IncomingIM);
		ClientManager.client.Self.ChatFromSimulator += new EventHandler<ChatEventArgs>(ChatFromSimulator);

		tabs.TryAdd(UUID.Zero, new ChatTab() { log = string.Empty, name = "Local Chat", tabButton = nearbyButton, uuid = UUID.Zero});
	}

	void IncomingIM(object sender, InstantMessageEventArgs e)
    {
        imEvents.Enqueue(e);
    }

	public void SetKeyToName(UUID uuid, string name)
	{
		avatarNames.TryAdd(uuid, name);
	}

	private readonly ConcurrentQueue<string> chatStrings = new();
	void ChatFromSimulator(object sender, ChatEventArgs e)
	{
		if ((int)e.Type <= 3)
		{
			string chat;
			try
			{
				chat = ($"[{System.DateTime.UtcNow.ToShortTimeString()}] {ClientManager.simManager.scenePrims[ClientManager.simManager.scenePrimIndexUUID[e.SourceID]].name}: {e.Message}").Replace("<", "<\u200B"); ;

				if (e.Type == ChatType.Whisper)
				{
					chat = $"[{System.DateTime.UtcNow.ToShortTimeString()}] {ClientManager.simManager.scenePrims[ClientManager.simManager.scenePrimIndexUUID[e.SourceID]].name}: <i><size=80%>{e.Message}</size></i>";
				}
				else if (e.Type == ChatType.Shout)
				{
					chat = $"[{System.DateTime.UtcNow.ToShortTimeString()}] {ClientManager.simManager.scenePrims[ClientManager.simManager.scenePrimIndexUUID[e.SourceID]].name}: <b><size=120%>{e.Message}</size></b>";
				}
				chatStrings.Enqueue(chat);
			}
			catch (Exception ex)
			{
				Debug.LogError($"Error processing incoming chat message: {ex.Message}");
			}
		}
	}

	void ParseIMEvents()
    {
		string chat;
		while (imEvents.Count > 0)
		{
			if (imEvents.TryDequeue(out var e))
			{
				if (e.IM.Message == string.Empty || e.IM.Dialog == InstantMessageDialog.StartTyping || e.IM.Dialog == InstantMessageDialog.StopTyping) continue;
                chat = ($"[{System.DateTime.UtcNow.ToShortTimeString()}] {e.IM.FromAgentName}: {e.IM.Message}").Replace("<", "<\u200B");
				if (tabs.ContainsKey(e.IM.FromAgentID))
				{
                    tabs[e.IM.FromAgentID].log += "\n" + chat;
				}
                else
                {
					// A VerticalLayoutGroup on the parent will handle positioning.
					GameObject b = Instantiate(nearbyButton, nearbyButton.transform.parent, true);
					ChatTab chatTab = new()
					{
						name = e.IM.FromAgentName,
						uuid = e.IM.FromAgentID,
						log = chat,
						tabButton = b
					};

					ClientManager.soundManager.PlayUISound(new UUID("67cc2844-00f3-2b3c-b991-6418d01e1bb7"));

					UI_IMButton button = chatTab.tabButton.GetComponent<UI_IMButton>();
                    button.buttonText.text = chatTab.name;
					button.uuid = chatTab.uuid;

					tabs.TryAdd(e.IM.FromAgentID, chatTab);
				}
			}
			if (selectedChat == e.IM.FromAgentID)
			{
				log.text = tabs[e.IM.FromAgentID].log;
			}
		}
	}

	public void StartIM(UUID agentID)
	{
		// A VerticalLayoutGroup on the parent will handle positioning.
		GameObject b = Instantiate(nearbyButton, nearbyButton.transform.parent, true);
		string name = "Loading...";
		if(avatarNames.ContainsKey(agentID))name = avatarNames[agentID];
		ChatTab chatTab = new()
		{
			name = name,
			uuid = agentID,
			log = string.Empty,
			tabButton = b
		};

		UI_IMButton button = chatTab.tabButton.GetComponent<UI_IMButton>();
		button.buttonText.text = chatTab.name;
		button.uuid = chatTab.uuid;

		tabs.TryAdd(agentID, chatTab);
	}

	public void ParseChatEvents()
	{
		while (chatStrings.Count > 0)
		{
			if (chatStrings.TryDequeue(out var chat))
			{
				tabs[UUID.Zero].log += "\n" + chat;
				if (selectedChat == UUID.Zero)
				{
					log.text = tabs[UUID.Zero].log;
				}
			}
		}
	}

	public void SwitchTab(UUID uuid)
    {
        if (tabs.ContainsKey(uuid))
        {
			selectedChat = uuid;
            log.text = tabs[uuid].log;
        }
		else
		{
			Debug.Log("Starting new IM");
			StartIM(uuid);
		}
	}

	// Update is called once per frame
	float lastEnterUp = 0f;
	void Update()
    {
        ParseIMEvents();
		ParseChatEvents();

		lastEnterUp += Time.deltaTime;

		if (Input.GetKeyUp(KeyCode.Return))
			lastEnterUp = 0f;
		if(!input.isFocused && Input.GetKeyDown(KeyCode.Return) && lastEnterUp >= 0.25f)
		{
			input.Select();
			input.ActivateInputField();
		}

		if(ClientManager.avatar != null)
			ClientManager.avatar.canMove = !input.isFocused;
	}


	public void SendChat()
	{
		if (input.text == string.Empty) return;
		if (selectedChat == UUID.Zero)
		{
			if (Input.GetKeyDown(KeyCode.Return))// && ClientManager.active)
			{
				if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
				{
					ClientManager.client.Self.Chat(inputText.text, 0, ChatType.Shout);
				}
				else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
				{
					ClientManager.client.Self.Chat(inputText.text, 0, ChatType.Whisper);
				}
				else
				{
					ClientManager.client.Self.Chat(inputText.text, 0, ChatType.Normal);
				}
				input.text = string.Empty;
			}
			else
			{
			}
		}
		else
		{
			ClientManager.client.Self.InstantMessage(selectedChat, inputText.text);
			tabs[selectedChat].log += "\n" + ($"[{System.DateTime.UtcNow.ToShortTimeString()}] {ClientManager.client.Self.Name}: {inputText.text}").Replace("<", "<\u200B");
			log.text = tabs[selectedChat].log;
			input.text = string.Empty;
		}

		dummyButton.Select();
		input.DeactivateInputField();
	}
}

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
    public TMP_Text log; // This will now be the text within the active content panel
    public GameObject chatTabButtonPrefab;
	public GameObject chatContentPanelPrefab; // Prefab for the chat history panel
	public GameObject nearbyButton;
    public Transform chatTabRoot;
	public Transform chatContentRoot; // Parent for all content panels
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
		public GameObject contentPanel; // To hold the chat history for this tab
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

		//just storing this here for future use, no reason for why here, just here
		//AudioClip clip;
		//clip.SetData()
		/*using (var vorbis = new NVorbis.VorbisReader(new MemoryStream(sample_data, false)))
		{
			Debug.Log($"Found ogg ch={vorbis.Channels} freq={vorbis.SampleRate} samp={vorbis.TotalSamples}");
			float[] _audioBuffer = new float[vorbis.TotalSamples]; // Just dump everything
			int read = vorbis.ReadSamples(_audioBuffer, 0, (int)vorbis.TotalSamples);
			AudioClip audioClip = AudioClip.Create(samplename, (int)(vorbis.TotalSamples / vorbis.Channels), vorbis.Channels, vorbis.SampleRate, false);
			audioClip.SetData(_audioBuffer, 0);
			samples.Add(audioClip);
		}*/

	}

	void IncomingIM(object sender, InstantMessageEventArgs e)
    {
		//Debug.Log($"Incoming IM from: {e.IM.FromAgentName}: {e.IM.Message}");
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
			string chat = ($"[{System.DateTime.UtcNow.ToShortTimeString()}] {ClientManager.simManager.scenePrims[ClientManager.simManager.scenePrimIndexUUID[e.SourceID]].name}: {e.Message}").Replace("<", "<\u200B"); ;
			//chat = Regex.Replace(chat, "<.*?>", String.Empty);

			try
			{
				if (e.Type == ChatType.Whisper)
				{
					chat = $"[{System.DateTime.UtcNow.ToShortTimeString()}] {ClientManager.simManager.scenePrims[ClientManager.simManager.scenePrimIndexUUID[e.SourceID]].name}: <i><size=80%>{e.Message}</size></i>";
				}
				else if (e.Type == ChatType.Shout)
				{
					chat = $"[{System.DateTime.UtcNow.ToShortTimeString()}] {ClientManager.simManager.scenePrims[ClientManager.simManager.scenePrimIndexUUID[e.SourceID]].name}: <b><size=120%>{e.Message}</size></b>";
				}
				//Debug.Log(chat);
				chatStrings.Enqueue(chat);

			}
			catch
			{
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

				if (!tabs.ContainsKey(e.IM.FromAgentID))
                {
					// This is a new IM session, create the tab and content area
					StartIM(e.IM.FromAgentID, e.IM.FromAgentName);
					ClientManager.soundManager.PlayUISound(new UUID("67cc2844-00f3-2b3c-b991-6418d01e1bb7"));
				}

				// Append log and update UI if it's the selected tab
				tabs[e.IM.FromAgentID].log += "\n" + chat;
				if (selectedChat == e.IM.FromAgentID)
				{
					log.text = tabs[e.IM.FromAgentID].log;
				}
			}
		}
	}

	public void StartIM(UUID agentID, string agentName = "Loading...")
	{
		if (tabs.ContainsKey(agentID))
		{
			SwitchTab(agentID);
			return;
		}

		// Instantiate UI elements from prefabs
		GameObject tabButton = Instantiate(chatTabButtonPrefab, chatTabRoot);
		GameObject contentPanel = Instantiate(chatContentPanelPrefab, chatContentRoot);

		if(avatarNames.ContainsKey(agentID))
		{
			agentName = avatarNames[agentID];
		}

		ChatTab chatTab = new()
		{
			name = agentName,
			uuid = agentID,
			log = string.Empty,
			tabButton = tabButton,
			contentPanel = contentPanel
		};

		// Configure the new tab button
		UI_IMButton button = tabButton.GetComponent<UI_IMButton>();
		button.buttonText.text = chatTab.name;
		button.uuid = chatTab.uuid;

		tabs.TryAdd(agentID, chatTab);
		SwitchTab(agentID); // Switch to the newly created tab
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
					// Find the content panel for local chat and update its text
					if(tabs[UUID.Zero].contentPanel != null)
					{
						tabs[UUID.Zero].contentPanel.GetComponentInChildren<TMP_Text>().text = tabs[UUID.Zero].log;
					}
				}
			}
		}
	}

	public void SwitchTab(UUID uuid)
    {
        if (!tabs.ContainsKey(uuid))
        {
			StartIM(uuid);
			return;
        }

		selectedChat = uuid;

		// Iterate through all tabs to set the correct content panel active
		foreach(var tab in tabs.Values)
		{
			bool isActive = tab.uuid == uuid;
			if(tab.contentPanel != null)
			{
				tab.contentPanel.SetActive(isActive);
				if(isActive)
				{
					// Update the main log text reference to point to the active panel's text
					log = tab.contentPanel.GetComponentInChildren<TMP_Text>();
					log.text = tab.log;
				}
			}
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
					//Debug.Log("Shout");
				}
				else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
				{
					ClientManager.client.Self.Chat(inputText.text, 0, ChatType.Whisper);
					//Debug.Log("Whisper");
				}
				else
				{
					ClientManager.client.Self.Chat(inputText.text, 0, ChatType.Normal);
					//Debug.Log("Normal");
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
		//dummyButton.



	}


}

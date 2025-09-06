using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using LibreMetaverse;
//using LibreMetaverse.Voice;

using OpenMetaverse;
using OpenMetaverse.Packets;
//using OpenMetaverse.TestClient_;

using CrystalFrost;
using CrystalFrost.Client.Credentials;
using CrystalFrost.Config;
using CrystalFrost.Extensions;
using CrystalFrost.Scripts;
using CrystalFrost.Timing;

using Microsoft.Extensions.Logging;
using Temp;
using static System.Net.WebRequestMethods;
//using UnityEditor.PackageManager;
//using OpenMetaverse.ImportExport.Collada14;


#if USE_KWS
using KWS;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif
//using UnityEngine.WSA;
//UnityEditor.EditorApplication.isPlaying = false;


public class Login : MonoBehaviour
{
	// Start is called before the first frame update
	public class LoginDetails
	{
		public string FirstName;
		public string LastName;
		public string Password;
		public string StartLocation;
		public bool GroupCommands;
		public string MasterName;
		public UUID MasterKey;
		public string URI;
	}

	[SerializeField]
	GameObject loggedInUI;
	[SerializeField]
	TMPro.TMP_InputField firstName;
	[SerializeField]
	TMPro.TMP_InputField lastName;
	[SerializeField]
	TMPro.TMP_InputField password;
	[SerializeField]
	TMPro.TMP_Text console;
	[SerializeField]
	TMPro.TMP_InputField gridURL;
	[SerializeField]
	GameObject loginUI;
	[SerializeField]
	GameObject consoleUI;

	public Terrain terrainPrefab;
	readonly LoginDetails loginDetails;

	public UUID GroupID = UUID.Zero;
	public Dictionary<UUID, GroupMember> GroupMembers;
	public Dictionary<UUID, AvatarAppearancePacket> Appearances = new();
	//public Dictionary<string, Command> Commands = new Dictionary<string, Command>();
	public bool Running = true;
	public bool GroupCommands = false;
	public string MasterName = string.Empty;
	public UUID MasterKey = UUID.Zero;
	public bool AllowObjectMaster = false;
	//public ClientManager ClientManager;
	//public VoiceManager VoiceManager;
	// Shell-like inventory commands need to be aware of the 'current' inventory folder.
	public InventoryFolder CurrentDirectory = null;

	private readonly System.Timers.Timer updateTimer;
	private UUID GroupMembersRequestID;
	public Dictionary<UUID, Group> GroupsCache = null;
	//UnityEngine.Vector3 vector3;
	//OpenMetaverse.Vector3 vector3omv;

	const float DEG_TO_RAD = 0.017453292519943295769236907684886f;

	private ILogger<Login> _log;
	private ICredentialsStore _credentials;
	private ILoginUriProvider _loginUriProvider;

	private CrystalFrost.Client.Credentials.LoginCredential _currentCredential;

	public ulong GetNorth(ulong handle) => (handle & 0xFFFFFFFF00000000) | ((uint)handle + 256);
	public ulong GetSouth(ulong handle) => (handle & 0xFFFFFFFF00000000) | ((uint)handle - 256);
	public ulong GetEast(ulong handle) => (((ulong)((uint)(handle >> 32) + 256)) << 32) | (uint)handle;
	public ulong GetWest(ulong handle) => (((ulong)((uint)(handle >> 32) - 256)) << 32) | (uint)handle;

	void Awake()
	{
		Application.targetFrameRate = 1000;
		QualitySettings.vSyncCount = 0;

		_log = Services.GetService<ILogger<Login>>();
		_loginUriProvider = Services.GetService<ILoginUriProvider>();

		_credentials = Services.GetService<ICredentialsStore>();
		_credentials.Load();
		if (!_credentials.Any()) _credentials.Add(new());
		_currentCredential = _credentials.First();
		firstName.text = _currentCredential.FirstName;
		lastName.text = _currentCredential.LastName;
		password.text = _currentCredential.Password;

		loggedInUI.SetActive(false);
		ClientManager.mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

		Bunny.Console.textOutput = console;
		ClientManager.client = Services.GetService<GridClient>();

		ClientManager.client.Self.Movement.Camera.Far = 32;
		ClientManager.client.Self.Movement.SetFOVVerticalAngle(Camera.main.fieldOfView * DEG_TO_RAD);

		ClientManager.client.Settings.USE_ASSET_CACHE = false;
		//client.Objects.KillObject += new EventHandler<KillObjectEventArgs>(Objects_KillObject);
		//ClientManager.client.Self.Movement.
		//ClientManager.client.Self.Movement.Camera.
		ClientManager.client.Settings.AVATAR_TRACKING = true;
		//loginUI.SetActive(true);
		//ClientManager.texturePipeline = new TexturePipeline(ClientManager.client);
		ClientManager.assetManager = new CrystalFrost.CFAssetManager();

		//ClientManager.client.Settings
		ClientManager.client.Network.SimConnected += new EventHandler<SimConnectedEventArgs>(SimConnectedEventHandler);
		ClientManager.client.Network.SimConnecting += new EventHandler<SimConnectingEventArgs>(SimConnectingEventHandler);
		ClientManager.client.Network.SimDisconnected += new EventHandler<SimDisconnectedEventArgs>(SimDisconnectedEventHandler);
		ClientManager.client.Network.SimChanged += new EventHandler<SimChangedEventArgs>(SimChangedEventHandler);
		ClientManager.client.Self.RegionCrossed += new EventHandler<RegionCrossedEventArgs>(RegionCrossedEventHandler);

		//ClientManager.client.Self.AgentID
		//ClientManager.client.Network.
		//ClientManager.client.Grid.GridRegion += new EventHandler<GridRegionEventArgs>(GridRegionEventHandler);
		//ClientManager.client.Grid.CoarseLocationUpdate += new EventHandler<CoarseLocationUpdateEventArgs>(GridCourseLocationUpdateEventHandler);
		//ClientManager.client.Grid.GridItems += new EventHandler<GridItemsEventArgs>(GridItemsEventHandler);
		//ClientManager.client.Grid.
		//ClientManager.client.Objects.
		if (_loginUriProvider.GetLoginUri() != "https://login.agni.lindenlab.com/cgi-bin/login.cgi")
			gridURL.text = _loginUriProvider.GetLoginUri();

	}

	private void OnDestroy()
	{
		ClientManager.client.Network.SimConnected -= new EventHandler<SimConnectedEventArgs>(SimConnectedEventHandler);
		ClientManager.client.Network.SimConnecting -= new EventHandler<SimConnectingEventArgs>(SimConnectingEventHandler);
		ClientManager.client.Network.SimDisconnected -= new EventHandler<SimDisconnectedEventArgs>(SimDisconnectedEventHandler);
		ClientManager.client.Network.SimChanged -= new EventHandler<SimChangedEventArgs>(SimChangedEventHandler);
		ClientManager.client.Self.RegionCrossed -= new EventHandler<RegionCrossedEventArgs>(RegionCrossedEventHandler);
	}

	//RegionCrossedEventArgs>(RegionCrossedEventHandler
	public void RegionCrossedEventHandler(object sender, RegionCrossedEventArgs e)
	{
		if (ClientManager.IsMainThread)
		{
			_log.LogInformation($"RegionCrossed: From {e.OldSimulator.Name} ({e.OldSimulator.Handle}) to {e.NewSimulator.Name} ({e.NewSimulator.Handle})");

			// Clean up assets from the old region
			if (ClientManager.simManager != null)
			{
				ClientManager.simManager.ClearRegion(e.OldSimulator.Handle);
			}
		}
		else
		{
			UnityMainThreadDispatcher.Instance().Enqueue(() => RegionCrossedEventHandler(sender, e));
		}
	}
	public void SimConnectedEventHandler(object sender, SimConnectedEventArgs e)
	{
		if (ClientManager.IsMainThread)
		{

			//Debug.Log($"GRIDTEST: Connected to sim:  {e.Simulator.Name} / {e.Simulator.Handle}");
			//display coordinates relative to current sim
			uint x, y, _x, _y, relx, rely;
			// _x/_y are the world coordinates of the current sim (in "region" units assuming 256m regions)
			// relx/rely are the relative coordinates of the sim we just connected to
			Utils.LongToUInts(ClientManager.client.Network.CurrentSim.Handle, out _x, out _y);
			Utils.LongToUInts(e.Simulator.Handle, out x, out y);
			relx = x - _x;
			rely = y - _y;
			Debug.Log($"GRIDEVENT: Connected to sim:  {e.Simulator.Name} / {e.Simulator.Handle} / <{relx},{rely}> / {e.Simulator.SizeX}x{e.Simulator.SizeY}");

			ClientManager.simManager.SimConnected(e.Simulator, x, y, relx, rely);
			// ClientManager.simManager.simulators.TryAdd(e.Simulator.Handle, new SimManager.SimulatorContainer(e.Simulator, x, y));
		}
		else
		{
			UnityMainThreadDispatcher.Instance().Enqueue(() => SimConnectedEventHandler(sender, e));
		}
	}

	public void SimConnectingEventHandler(object sender, SimConnectingEventArgs e)
	{
		if (ClientManager.IsMainThread)
		{
			Debug.Log($"GRIDEVENT: Connecting to sim: {e.Simulator.Name} / {e.Simulator.Handle}");

			//set terrain neighbors for stitching
			/*List<ulong> neighborHandles = new List<ulong>();
			neighborHandles.Add(((ulong)(x+256) << 32) | y);
			neighborHandles.Add(((ulong)(x - 256) << 32) | y);
			neighborHandles.Add(((ulong)(x) << 32) | y+256);
			neighborHandles.Add(((ulong)(x) << 32) | y-256);*/
		}
		else
		{
			UnityMainThreadDispatcher.Instance().Enqueue(() => SimConnectingEventHandler(sender, e));
		}
	}

	public void SimChangedEventHandler(object sender, SimChangedEventArgs e)
	{
		if (ClientManager.IsMainThread)
		{
			Debug.Log($"GRIDEVENT: Sim changed to {ClientManager.client.Network.CurrentSim.Name}");
		}
		else
		{
			UnityMainThreadDispatcher.Instance().Enqueue(() => SimChangedEventHandler(sender, e));
		}
	}

	public void SimDisconnectedEventHandler(object sender, SimDisconnectedEventArgs e)
	{
		if (ClientManager.IsMainThread)
		{
			//ClientManager.simManager.simulators.Remove(e.Simulator.Handle);
			Debug.Log($"GRIDEVENT: Disconnected from sim: {e.Simulator.Name} / {e.Simulator.Handle} / {e.Reason}");
			Destroy(ClientManager.simManager.simulators[e.Simulator.Handle].transform.gameObject);
			ClientManager.simManager.simulators.TryRemove(e.Simulator.Handle, out var simCont);
		}
		else
		{
			UnityMainThreadDispatcher.Instance().Enqueue(() => SimDisconnectedEventHandler(sender, e));
		}
	}

	public void GridRegionEventHandler(object sender, GridRegionEventArgs e)
	{
		if (ClientManager.IsMainThread)
		{
			Debug.Log($"GRIDEVENT: GridRegionEvent {e.Region.Name}, {e.Region.RegionHandle}, <{e.Region.X},{e.Region.Y}>");
		}
		else
		{
			UnityMainThreadDispatcher.Instance().Enqueue(() => GridRegionEventHandler(sender, e));
		}
	}

	public void GridItemsEventHandler(object sender, GridItemsEventArgs e)
	{
		if (ClientManager.IsMainThread)
		{
			//e.Items[0].
			Debug.Log($"GRIDEVENT: GridItemsEvent");
		}
		else
		{
			UnityMainThreadDispatcher.Instance().Enqueue(() => GridItemsEventHandler(sender, e));
		}
	}

	public void GridCourseLocationUpdateEventHandler(object sender, CoarseLocationUpdateEventArgs e)
	{
		if (ClientManager.IsMainThread)
		{
			//e.Items[0].
			Debug.Log($"GRIDEVENT: CourseLocationUpdate new entries: {e.NewEntries.Count}, removed entries: {e.RemovedEntries.Count}");
		}
		else
		{
			UnityMainThreadDispatcher.Instance().Enqueue(() => GridCourseLocationUpdateEventHandler(sender, e));
		}
	}

	public void CreateSimulatorTerrainTiles(string name, uint handle, uint sizeX, uint sizeY)
	{
		GameObject terrainRoot = new GameObject(name);
		Terrain[,] terrains = new Terrain[sizeX, sizeY];

		for (uint x = 0; x < sizeX; x++)
		{
			for (uint y = 0; y < sizeY; y++)
			{
				GameObject terrainObject = new GameObject($"Terrain_{x}_{y}");
				terrainObject.transform.parent = terrainRoot.transform;

				Terrain terrain = terrainObject.AddComponent<Terrain>();
				TerrainData terrainData = new TerrainData();
				terrainData.size = new UnityEngine.Vector3(256, 256, 256);
				terrain.terrainData = terrainData;

				terrains[x, y] = terrain;
			}
		}

		// Set neighbors
		for (uint x = 0; x < sizeX; x++)
		{
			for (uint y = 0; y < sizeY; y++)
			{
				Terrain left = x > 0 ? terrains[x - 1, y] : null;
				Terrain right = x < sizeX - 1 ? terrains[x + 1, y] : null;
				Terrain top = y < sizeY - 1 ? terrains[x, y + 1] : null;
				Terrain bottom = y > 0 ? terrains[x, y - 1] : null;

				terrains[x, y].SetNeighbors(left, top, right, bottom);
			}
		}

		//terrainDictionary[handle] = terrains;
	}

	EventSystem system;

	private void Start()
	{
		system = EventSystem.current;
		cameraControls.enabled = true;
		//waterSystem.UseCausticEffect = true;

		// Add the ClickToInteract component to this GameObject to enable world interaction
		if (gameObject.GetComponent<ClickToInteract>() == null)
		{
			gameObject.AddComponent<ClickToInteract>();
		}
	}

	public Transform sun;
#if USE_KWD
	public WaterSystem waterSystem;
#endif
	private void Update()
	{
		if (!ClientManager.active)
		{
			//sun.Rotate(Time.deltaTime, 0f, 0f);
		}
		else
		{
		}
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			Selectable next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();

			if (next != null)
			{

				InputField inputfield = next.GetComponent<InputField>();
				if (inputfield != null) inputfield.OnPointerClick(new PointerEventData(system));  //if it's an input field, also set the text caret

				system.SetSelectedGameObject(next.gameObject, new BaseEventData(system));
			}
			//else Debug.Log("next nagivation element not found");

		}
	}

	public void TryLogin()
	{
		//Debug.Log($"Login\nFirst Name: {firstName.text}\nLast Name: {lastName.text}\nPassword: {password.text}");
		//loginDetails.FirstName = firstName.text;
		//loginDetails.LastName = lastName.text;
		//loginDetails.Password = password.text;
		//loginURI = Settings.AGNI_LOGIN_SERVER;

		StartCoroutine(_TryLogin());

		//StartCoroutine(LogOut(30));
		//NetworkManager.    

	}

	public GameObject chatUI;
	public CameraControls cameraControls;
	public GameObject contactsUI;
	public Transform capsule;
	IEnumerator _TryLogin()
	{
		_log.LogInformation($"Logging in as {firstName.text} {lastName.text}");

		_currentCredential.FirstName = firstName.text;
		_currentCredential.LastName = lastName.text;
		_currentCredential.Password = password.text;

		//Console.WriteLine($"{System.DateTime.UtcNow.ToShortTimeString()}: Password: {password.text}");
		loginUI.SetActive(false);
		loggedInUI.SetActive(true);

		yield return null;


		var loginUri = _loginUriProvider.GetLoginUri();
		string uri = loginUri;
		//loginUri = "http://grid.wolfterritories.org:8002";
		if (gridURL.text != string.Empty)
		{
			loginUri = gridURL.text;
		}
		else
		{
			loginUri = Settings.AGNI_LOGIN_SERVER;
			Debug.Log(loginUri);
		}

		LoginParams loginParams = new(
			ClientManager.client,
			firstName.text,
			lastName.text, password.text,
			"CrystalFrost",
			"0.2",
			loginUri);

		if (loginParams.URI != uri) ClientManager.isOpenSim = true;

		_log.LoggingIn(firstName.text, lastName.text, loginUri);

		if (ClientManager.client.Network.Login(loginParams))
		{
			Bunny.Console.WriteLine(System.DateTime.UtcNow.ToShortTimeString() + ": " + ClientManager.client.Network.LoginMessage);
			Bunny.Console.WriteLine("Logging in. The viewer might appear to lock up for a short while, while the sim floods it with new objects.");

			ClientManager.client.Network.CurrentSim.Caps.CapabilitiesReceived += ((sender, e) => {
				ClientManager.active = true;
			});
			ClientManager.client.Estate.RequestInfo();
			Simulator sim = ClientManager.client.Network.CurrentSim;

			//ClientManager.simManager.thissim = sim.Handle;
			ClientManager.simManager._thissim = sim;

			ClientManager.simManager.water.position = new UnityEngine.Vector3(127f, sim.WaterHeight, 127f);

			Avatar av = gameObject.GetComponent<Avatar>();
			av.id = ClientManager.client.Self.LocalID;
			capsule.SetPositionAndRotation(
				ClientManager.client.Self.SimPosition.ToVector3(),
				ClientManager.client.Self.SimRotation.ToUnity());
			Camera.main.transform.SetPositionAndRotation(
				ClientManager.client.Self.SimPosition.ToVector3(),
				ClientManager.client.Self.SimRotation.ToUnity());


			chatUI.SetActive(true);

			cameraControls.enabled = true;

			//sun.GetComponent<Light>().intensity = 1f;

			_currentCredential.LastUsed = System.DateTime.UtcNow;
			_credentials.Save();
			gameObject.GetComponent<ChatWindowUI>().PopulateContacts();

		}
		else
		{
			Bunny.Console.WriteLine(System.DateTime.UtcNow.ToShortTimeString() + ": " + ClientManager.client.Network.LoginMessage);
			loginUI.SetActive(true);
			ClientManager.active = false;
			loggedInUI.SetActive(false);


		}
	}


	public void LogOut()
	{
		StartCoroutine(_LogOut());
		ClientManager.currentOutfitFolder.Dispose();
		//Destroy(ClientManager.client.);
	}

	IEnumerator _LogOut()
	{
		_log.LoggingOut();
		loggedInUI.SetActive(false);

		yield return null;

		// Gracefully disconnect from the network
		if (ClientManager.client != null && ClientManager.client.Network.Connected)
		{
			ClientManager.client.Network.Logout();
		}

		ClientManager.active = false;

		// Dispose managers to clean up state
		if (ClientManager.assetManager != null)
		{
			ClientManager.assetManager.Dispose();
			ClientManager.assetManager = new CrystalFrost.CFAssetManager(); // Re-initialize for next session
		}

		if (ClientManager.simManager != null)
		{
			ClientManager.simManager.Dispose();
		}

		// Add other manager disposals here if needed
		// e.g., ClientManager.soundManager.Dispose();

		// Clean up any remaining assets
		Resources.UnloadUnusedAssets();

		// Dump performance data if it was enabled
		if (!Perf.Disabled)
		{
			var perfFile = System.IO.Path.Combine(
				UnityEngine.Application.persistentDataPath,
				System.DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + ".perf.csv");
			Perf.DumpStats(perfFile);
		}

		// Return to the login screen
		loginUI.SetActive(true);
		consoleUI.SetActive(true); // Or manage console visibility as needed
	}


	//private readonly ManualResetEvent GroupsEvent = new ManualResetEvent(false);

	// Update is called once per frame

}

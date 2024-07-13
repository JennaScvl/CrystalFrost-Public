using System.Collections;
using UnityEngine;
using OpenMetaverse;
using UnityEngine.Extensions;
using OMVVector3 = OpenMetaverse.Vector3;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;
using CrystalFrost.Extensions;
using CrystalFrost;

public class Avatar : MonoBehaviour
{
	public DynamicJoystick joystick;

    public Transform myAvatar;

    public Vector3 simPos;
    public string firstName;
    public string lastName;

    public string _uuid;
    public UUID uuid;

    public bool fly = false;

    public bool canMove = true;

    public Vector3 offset = Vector3.zero;
    //public string displayName;

    GridClient client;
    AgentManager self;

    public uint id;

	//    Transform avatar

	// Start is called before the first frame update
	private void Awake()
	{
	}
	void Start()
    {
        client = ClientManager.client;
        self = client.Self;
        //hud = ClientManager.simManager.hudCamera.GetComponent<Camera>();

       // client.Self.

        StartCoroutine(TimerRoutine());
        //Camera.main
    }

    Transform lastMyAvatar;

    public Quaternion rotation = Quaternion.identity;

    public float rotationSpeed = 5f;

	private void Update()
    {
        id = ClientManager.client.Self.LocalID;
        bool update = false;
        float t = Time.deltaTime;
        /*id = ClientManager.client.Self.localID;
		if (ClientManager.simManager.scenePrims.ContainsKey(id))
		{
            //myAvatar.position = simPos;
			//Debug.Log("Parenting avatar");
			myAvatar.transform.parent = ClientManager.simManager.scenePrims[id].meshHolder.transform.parent;
			myAvatar.transform.localPosition = Vector3.zero;
			myAvatar.transform.localRotation = Quaternion.identity;
		}*/
        //Camera.main.transform.parent = myAvatar;


		if (ClientManager.simManager is null) return;
        ClientManager.simManager.cameraRoot.localPosition = offset;
        //Camera.main.transform.localrotation = Quaternion.Euler(0f, myAvatar.eulerAngles.y + 90, 0f);

        if (canMove)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKey(KeyCode.W) || joystick.Vertical > 0.5f)
            {
                client.Self.Movement.AtPos = true;
                update = true;
            }
            else// if(Input.GetKeyUp(KeyCode.W))
            {
                client.Self.Movement.AtPos = false;
                update = true;
            }

            if (Input.GetKeyDown(KeyCode.S) || Input.GetKey(KeyCode.S) || joystick.Vertical < -0.5f)
            {
                client.Self.Movement.AtNeg = true;
                update = true;
            }
            else// if (Input.GetKeyUp(KeyCode.S))
            {
                client.Self.Movement.AtNeg = false;
                update = true;
            }


			if (Input.GetKeyDown(KeyCode.Space) || Input.GetKey(KeyCode.Space))
			{
				client.Self.Movement.UpPos = true;
				update = true;
			}
			else// if (Input.GetKeyUp(KeyCode.S))
			{
				client.Self.Movement.UpPos = false;
				update = true;
			}

			if (Input.GetKeyDown(KeyCode.C) || Input.GetKey(KeyCode.C))
			{
				client.Self.Movement.UpNeg = true;
				update = true;
			}
			else// if (Input.GetKeyUp(KeyCode.S))
			{
				client.Self.Movement.UpNeg = false;
				update = true;
			}

#if false //turn by yaw, doesn't seem to work
            if (Input.GetKey(KeyCode.D))
            {
                client.Self.Movement.YawPos = true;
                update = true;
            }
            else if (Input.GetKeyUp(KeyCode.D))
            {
                client.Self.Movement.YawPos = false;
                update = true;
            }

			if (Input.GetKey(KeyCode.A))
			{
				client.Self.Movement.YawNeg = true;
				update = true;
			}
			else if (Input.GetKeyUp(KeyCode.A))
			{
				client.Self.Movement.YawNeg = false;
				update = true;
			}
#endif
#if true //turn by turn untested
			if (Input.GetKey(KeyCode.D) || joystick.Horizontal > 0.5f)
            {
                client.Self.Movement.TurnRight = true;
                rotation *= Quaternion.EulerAngles(0f, rotationSpeed * t, 0f);
                client.Self.Movement.BodyRotation = rotation.ToLMV();
                update = true;
            }
            else if (Input.GetKeyUp(KeyCode.D) || joystick.Horizontal < 0.5f)
            {
                client.Self.Movement.TurnRight = false;
                update = true;
            }

            if (Input.GetKey(KeyCode.A) || joystick.Horizontal < -0.5f)
            {
                client.Self.Movement.TurnLeft = true;
                rotation *= Quaternion.EulerAngles(0f, -rotationSpeed * t, 0f);
                client.Self.Movement.BodyRotation = rotation.ToLMV();
                update = true;
            }
            else if (Input.GetKeyUp(KeyCode.A) || joystick.Horizontal > -0.5f)
            {
                client.Self.Movement.TurnLeft = false;
                update = true;
            }
#endif

            if (Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftAlt))
            {
                UnityEngine.Ray ray = hudCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

				if (Physics.Raycast(ray, out hit, 100))
                {
                    if (hit.transform.name != "Terrain" && hit.transform.root.name != "Water" && hit.transform.GetComponent<PrimInfo>() != null)
                    {
                        PrimInfo pi = hit.transform.GetComponent<PrimInfo>();
                        //Debug.Log($"clicked on face {pi.face} on object {pi.localID}");
                        uint[] ids = { pi.localID };
                        //ClientManager.client.Objects.SelectObjects(ClientManager.client.Network.CurrentSim, ids);

                        Vector3 normal = hit.normal;
                        Vector3 tangent;
                        Vector3 t1 = Vector3.Cross(normal, Vector3.forward);
                        Vector3 t2 = Vector3.Cross(normal, Vector3.up);
                        if (t1.magnitude > t2.magnitude)
                        {
                            tangent = t1;
                        }
                        else
                        {
                            tangent = t2;
                        }

                        ClientManager.simManager.scenePrims[pi.localID].Click(hit.textureCoord, hit.textureCoord, pi.face, hit.point, hit.normal, tangent);
                    }
                }
                else
                {
                    ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out hit, 100))
                    {
                        if (hit.transform.name != "Terrain" && hit.transform.root.name != "Water" && hit.transform.GetComponent<PrimInfo>() != null)
                        {
                            PrimInfo pi = hit.transform.GetComponent<PrimInfo>();
                            //Debug.Log($"clicked on face {pi.face} on object {pi.localID}");
                            uint[] ids = { pi.localID };
                            //ClientManager.client.Objects.SelectObjects(ClientManager.client.Network.CurrentSim, ids);

                            Vector3 normal = hit.normal;
                            Vector3 tangent;
                            Vector3 t1 = Vector3.Cross(normal, Vector3.forward);
                            Vector3 t2 = Vector3.Cross(normal, Vector3.up);
                            if (t1.magnitude > t2.magnitude)
                            {
                                tangent = t1;
                            }
                            else
                            {
                                tangent = t2;
                            }

                            ClientManager.simManager.scenePrims[pi.localID].Click(hit.textureCoord, hit.textureCoord, pi.face, hit.point, hit.normal, tangent);
                        }
                    }
                }


            }

            if (update)
            {
                client.Self.Movement.SendUpdate();
            }


        }

		/*
        /// <summary>HUD Center position 2</summary>
        [EnumInfo(Text = "HUD Center 2")]
        HUDCenter2,
        /// <summary>HUD Top-right</summary>
        [EnumInfo(Text = "HUD Top Right")]
        HUDTopRight,
        /// <summary>HUD Top</summary>
        [EnumInfo(Text = "HUD Top Center")]
        HUDTop,
        /// <summary>HUD Top-left</summary>
        [EnumInfo(Text = "HUD Top Left")]
        HUDTopLeft,
        /// <summary>HUD Center</summary>
        [EnumInfo(Text = "HUD Center 1")]
        HUDCenter,
        /// <summary>HUD Bottom-left</summary>
        [EnumInfo(Text = "HUD Bottom Left")]
        HUDBottomLeft,
        /// <summary>HUD Bottom</summary>
        [EnumInfo(Text = "HUD Bottom")]
        HUDBottom,
        /// <summary>HUD Bottom-right</summary>
        [EnumInfo(Text = "HUD Bottom Right")]
        HUDBottomRight,
        */
		ClientManager.simManager.hudAnchors[0].position = hudCamera.ScreenToWorldPoint(new Vector3(hudCamera.pixelWidth* 0.5f, hudCamera.pixelHeight * 0.5f,2f));
		ClientManager.simManager.hudAnchors[1].position = hudCamera.ScreenToWorldPoint(new Vector3(hudCamera.pixelWidth, hudCamera.pixelHeight, 2f));
		ClientManager.simManager.hudAnchors[2].position = hudCamera.ScreenToWorldPoint(new Vector3(hudCamera.pixelWidth* 0.5f, hudCamera.pixelHeight, 2f));
		ClientManager.simManager.hudAnchors[3].position = hudCamera.ScreenToWorldPoint(new Vector3(0f, hudCamera.pixelHeight, 2f));
		ClientManager.simManager.hudAnchors[4].position = hudCamera.ScreenToWorldPoint(new Vector3(hudCamera.pixelWidth * 0.5f, hudCamera.pixelHeight * 0.5f, 2f));
		ClientManager.simManager.hudAnchors[5].position = hudCamera.ScreenToWorldPoint(new Vector3(0f, 0f, 2f));
		ClientManager.simManager.hudAnchors[6].position = hudCamera.ScreenToWorldPoint(new Vector3(hudCamera.pixelWidth * 0.5f, 0f, 2f));
		ClientManager.simManager.hudAnchors[7].position = hudCamera.ScreenToWorldPoint(new Vector3(hudCamera.pixelWidth, 0f, 2f));

        for(int i = 0; i < 8; i++)
        {
            ClientManager.simManager.hudAnchors[i].position = new Vector3(ClientManager.simManager.hudAnchors[i].position.x, ClientManager.simManager.hudAnchors[i].position.y, 0f);
		}
	}
	public Camera hudCamera;


	void SetFlyMode(bool b)
    {
        if (client.Settings.SEND_AGENT_APPEARANCE)
        {
            client.Self.Movement.Fly = b;
            //client.Self.Movement.SendUpdate(true);
        }
    }

    void SetAlwaysRunMode(bool b)
    {
        if (client.Settings.SEND_AGENT_APPEARANCE)
        {
            client.Self.Movement.AlwaysRun = b;
            //client.Self.Movement.SendUpdate(true);
        }
    }

    void SetSit(bool b)
    {
        client.Self.Movement.SitOnGround = b;
        client.Self.Movement.StandUp = !b;
        //client.Self.Movement.SendUpdate(true);
    }

    void Jump(bool b)
    {
        client.Self.Movement.UpPos = b;
        client.Self.Movement.FastUp = b;
    }

    void Crouch(bool b)
    {
        client.Self.Movement.UpNeg = true;
    }

    void MoveToTarget(OMVVector3 v)
    {
        var _v = new OMVVector3(v);
        client.Self.Movement.TurnToward(_v);
        client.Self.AutoPilotCancel();
        client.Self.AutoPilot(_v.X, _v.Y, _v.Z);
    }

    void TeleportHome()
    {
        client.Self.Teleport(UUID.Zero);
    }

    void Teleport(string sim, OMVVector3 pos, OMVVector3 localLookAt)
    {
        client.Self.Teleport(sim, new OMVVector3(pos), new OMVVector3(localLookAt));
    }

    // Update is called once per frame
    IEnumerator TimerRoutine()
    {
        while (true)
        {
            if (client.Settings.SEND_AGENT_UPDATES && ClientManager.active)
            {
                //OpenMetaverse.Vector3;
                simPos = self.SimPosition.ToUnity();
                firstName = self.FirstName;
                lastName = self.LastName;
                //displayName = "Not Implemented";
            }

			if (ClientManager.isOpenSim && myAvatar == null)
			{
				if(ClientManager.simManager.scenePrims.ContainsKey(ClientManager.client.Self.LocalID)) 
				{
					ClientManager.avatar = this;
					Debug.Log("found self");
					ClientManager.currentOutfitFolder = new CurrentOutfitFolder();
					ScenePrimData spd = ClientManager.simManager.scenePrims[ClientManager.client.Self.LocalID];
					myAvatar.parent = spd.meshHolder.transform.root;
					myAvatar.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
					rotation = spd.prim.Rotation.ToUnity();
				}
			}


			yield return new WaitForSeconds(5f);
        }
    }
}

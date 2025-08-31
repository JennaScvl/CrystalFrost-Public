using CrystalFrost;
using OpenMetaverse;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // --- Public Fields ---
    [Header("Camera Controls")]
    public Transform target; // The object to orbit around (the avatar)
    public float distance = 5.0f;
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;
    public float distanceMin = .5f;
    public float distanceMax = 15f;
    public float zoomSpeed = 5f;

    // --- Private Fields ---
    private GridClient _client;
    private float _x = 0.0f;
    private float _y = 0.0f;

    void Start()
    {
        // Attempt to get the GridClient from the services.
        _client = Services.GetService<GridClient>();

        // Set the target for the camera to this object's transform
        if (target == null)
        {
            target = this.transform;
        }

        // Initialize camera angles
        Vector3 angles = transform.eulerAngles;
        _x = angles.y;
        _y = angles.x;
    }

    void Update()
    {
        HandleMovement();
    }

    void LateUpdate()
    {
        HandleCamera();
    }

    private void HandleMovement()
    {
        // If the client is not available, try to get it.
        if (_client == null || !_client.Network.Connected)
        {
            _client = Services.GetService<GridClient>();
            if (_client == null || !_client.Network.Connected)
            {
                return; // Still not connected, do nothing.
            }
        }

        AgentManager.ControlFlags controlFlags = AgentManager.ControlFlags.NONE;

        // Forward/Backward movement
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            controlFlags |= AgentManager.ControlFlags.AGENT_CONTROL_AT_POS;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            controlFlags |= AgentManager.ControlFlags.AGENT_CONTROL_AT_NEG;

        // Strafing Left/Right
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            controlFlags |= AgentManager.ControlFlags.AGENT_CONTROL_LEFT_POS;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            controlFlags |= AgentManager.ControlFlags.AGENT_CONTROL_LEFT_NEG;

        // Fly Up (Jump)
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))
            controlFlags |= AgentManager.ControlFlags.AGENT_CONTROL_UP_POS;

        // Fly Down (Crouch)
        if (Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.PageDown))
            controlFlags |= AgentManager.ControlFlags.AGENT_CONTROL_UP_NEG;

        // Send the update.
        _client.Self.Movement.SendUpdate(controlFlags, Camera.main.transform.rotation);
    }

    private void HandleCamera()
    {
        if (target)
        {
            // Orbit camera on right mouse button drag
            if (Input.GetMouseButton(1))
            {
                _x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
                _y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
                _y = ClampAngle(_y, yMinLimit, yMaxLimit);
            }

            // Zoom camera with scroll wheel
            distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * zoomSpeed, distanceMin, distanceMax);

            // Calculate camera rotation and position
            Quaternion rotation = Quaternion.Euler(_y, _x, 0);
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * negDistance + target.position;

            // Apply rotation and position to the main camera
            Camera.main.transform.rotation = rotation;
            Camera.main.transform.position = position;
        }
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}

using CrystalFrost;
using OpenMetaverse;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    private GridClient _client;

    void Start()
    {
        // Attempt to get the GridClient from the services.
        _client = Services.GetService<GridClient>();
    }

    void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        // Ensure we have a connected client before trying to send updates.
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

        // Send the movement update with the current camera rotation.
        // The existing CameraControls script will manage the main camera's transform.
        if (Camera.main != null)
        {
            _client.Self.Movement.SendUpdate(controlFlags, Camera.main.transform.rotation);
        }
    }
}

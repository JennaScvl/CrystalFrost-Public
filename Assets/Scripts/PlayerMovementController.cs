using UnityEngine;
using OpenMetaverse;

public class PlayerMovementController : MonoBehaviour
{
    private GridClient client;
    private AgentManager.ControlFlags controlFlags = 0;
    private bool isFlying = false; // Add a flag to track flying state

    void Start()
    {
        client = ClientManager.client;
    }

    void Update()
    {
        // Toggle flying state on 'F' key press
        if (Input.GetKeyDown(KeyCode.F))
        {
            isFlying = !isFlying;
            client.Self.Fly(isFlying);
        }

        var newFlags = GetControlFlagsFromInput();

        if (newFlags != this.controlFlags)
        {
            this.controlFlags = newFlags;
            client.Self.Movement.AgentControls = this.controlFlags;
            client.Self.Movement.SendUpdate();
        }
    }

    private AgentManager.ControlFlags GetControlFlagsFromInput()
    {
        AgentManager.ControlFlags flags = 0;

        // Forward/Backward
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            flags |= AgentManager.ControlFlags.AGENT_CONTROL_AT_POS;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            flags |= AgentManager.ControlFlags.AGENT_CONTROL_AT_NEG;
        }

        // Strafe Left/Right
        if (Input.GetKey(KeyCode.A))
        {
            flags |= AgentManager.ControlFlags.AGENT_CONTROL_LEFT_POS;
        }
        if (Input.GetKey(KeyCode.D))
        {
            flags |= AgentManager.ControlFlags.AGENT_CONTROL_RIGHT_POS;
        }

        // Turn Left/Right
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            flags |= AgentManager.ControlFlags.AGENT_CONTROL_TURN_LEFT;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            flags |= AgentManager.ControlFlags.AGENT_CONTROL_TURN_RIGHT;
        }

        // Jump / Fly Up
        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.E))
        {
            flags |= AgentManager.ControlFlags.AGENT_CONTROL_UP_POS;
        }

        // Crouch / Fly Down
        if (Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.PageDown))
        {
            flags |= AgentManager.ControlFlags.AGENT_CONTROL_UP_NEG;
        }

        // Always set the flying flag if active
        if (isFlying)
        {
            flags |= AgentManager.ControlFlags.AGENT_CONTROL_FLY;
        }

        return flags;
    }
}

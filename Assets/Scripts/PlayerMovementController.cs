using UnityEngine;
using OpenMetaverse;
using static OpenMetaverse.Animations;

public class PlayerMovementController : MonoBehaviour
{
    public bool MouselookEnabled { get; set; } = false;

    private GridClient client;
    private AgentManager.ControlFlags controlFlags = 0;
    private bool isFlying = false;

    // Animation state tracking
    private bool wasWalking = false;
    private bool wasFlying = false;
    private bool wasJumping = false;
    private bool wasStanding = false;

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

        UpdateAnimations(newFlags);
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

        // Turn Left/Right (Keyboard)
        if (!MouselookEnabled)
        {
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                flags |= AgentManager.ControlFlags.AGENT_CONTROL_TURN_LEFT;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                flags |= AgentManager.ControlFlags.AGENT_CONTROL_TURN_RIGHT;
            }
        }
        else // Turn Left/Right (Mouse)
        {
            float mouseX = Input.GetAxis("Mouse X");
            if (mouseX < -0.1f)
            {
                flags |= AgentManager.ControlFlags.AGENT_CONTROL_TURN_LEFT;
            }
            else if (mouseX > 0.1f)
            {
                flags |= AgentManager.ControlFlags.AGENT_CONTROL_TURN_RIGHT;
            }
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

    private void UpdateAnimations(AgentManager.ControlFlags currentFlags)
    {
        bool isWalking = (currentFlags & AgentManager.ControlFlags.AGENT_CONTROL_AT_POS) != 0 ||
                         (currentFlags & AgentManager.ControlFlags.AGENT_CONTROL_AT_NEG) != 0 ||
                         (currentFlags & AgentManager.ControlFlags.AGENT_CONTROL_LEFT_POS) != 0 ||
                         (currentFlags & AgentManager.ControlFlags.AGENT_CONTROL_RIGHT_POS) != 0;

        bool isJumping = (currentFlags & AgentManager.ControlFlags.AGENT_CONTROL_UP_POS) != 0 && !isFlying;

        // Walking
        if (isWalking && !wasWalking) { client.Self.AnimationStart(Animations.WALK, true); }
        else if (!isWalking && wasWalking) { client.Self.AnimationStop(Animations.WALK, true); }
        wasWalking = isWalking;

        // Flying
        if (isFlying && !wasFlying) { client.Self.AnimationStart(Animations.FLY, true); }
        else if (!isFlying && wasFlying) { client.Self.AnimationStop(Animations.FLY, true); }
        wasFlying = isFlying;

        // Jumping
        if (isJumping && !wasJumping) { client.Self.AnimationStart(Animations.JUMP, false); }
        wasJumping = isJumping;

        // Standing
        bool isStanding = !isWalking && !isFlying && !isJumping;
        if (isStanding && !wasStanding)
        {
            client.Self.AnimationStart(Animations.STAND, true);
        }
        wasStanding = isStanding;
    }
}

using UnityEngine;

/// <summary>
/// This is a utility script for testing purposes. It finds the player avatar
/// in the scene and attaches the PlayerController to it.
/// In a production environment, the PlayerController would typically be part of a player prefab.
/// </summary>
public class SetupPlayerController : MonoBehaviour
{
    void Start()
    {
        // We need a main camera for the controller to work.
        if (Camera.main == null)
        {
            Debug.LogError("No main camera found in the scene. The PlayerController requires a camera tagged 'MainCamera'.");
            return;
        }

        // Find the GameObject representing the player avatar.
        // We'll assume it's tagged "Player". This is a common Unity convention.
        GameObject playerAvatar = GameObject.FindWithTag("Player");

        if (playerAvatar != null)
        {
            // Check if the PlayerController is already attached to avoid duplicates.
            if (playerAvatar.GetComponent<PlayerController>() == null)
            {
                // Add the PlayerController component to the avatar.
                var controller = playerAvatar.AddComponent<PlayerController>();

                // The PlayerController needs a target to orbit around. By default, it uses its own transform,
                // but we explicitly set it here for clarity.
                controller.target = playerAvatar.transform;

                Debug.Log("PlayerController script was successfully added to the Player avatar.");
            }
            else
            {
                Debug.Log("PlayerController script is already attached to the Player avatar.");
            }
        }
        else
        {
            Debug.LogError("Could not find a GameObject with the 'Player' tag. Please tag your avatar GameObject with 'Player' for the PlayerController to be attached.");
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using OpenMetaverse;

public class UI_IMButton : MonoBehaviour
{
    // Start is called before the first frame update

    public TMP_Text buttonText;
    public UUID uuid = UUID.Zero;
    public bool isContactButton = false;
    public void Click()
    {
        if(isContactButton)
        {
            ClientManager.simManager.gameObject.GetComponent<ChatWindowUI>().SwitchToIM(uuid);
		}
		ClientManager.chat.SwitchTab(uuid);
		ClientManager.soundManager.PlayUISound(new UUID("4c8c3c77-de8d-bde2-b9b8-32635e0fd4a6"));

	}
}

using UnityEngine;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
{
    public NetworkManager networkManager;
    public Text numberDisplay;
    public RawImage imageDisplay; // Assign this in the Unity Editor
    public string messageType;

    public void OnButtonClick()
    {
        if (messageType == "One"){
			numberDisplay.text = networkManager.RequestRandomNumber().ToString();
		}
/*
		else if (messageType == "Two"){
			imageDisplay = networkManager.RequestImage();
		}
*/
        
    }
}
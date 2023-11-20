using UnityEngine;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
{
    public NetworkManager networkManager;
    public Text numberDisplay;
    public string messageType;

    public void OnButtonClick()
    {
        networkManager.RequestRandomNumber();
        numberDisplay.text = "Waiting for random number...";
    }
}
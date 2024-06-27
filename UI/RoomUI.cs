using Mirror;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class RoomUI : MonoBehaviour
{

    public TMP_InputField roomNameInputField; // For creating rooms
    public TMP_InputField joinRoomInputField; // For joining rooms by address

    public Button createRoomButton;
    public Button joinRoomButton;

    void Start()
    {
        createRoomButton.onClick.AddListener(() => NetworkManager.singleton.StartHost());
        joinRoomButton.onClick.AddListener(() => NetworkManager.singleton.StartClient());
    }


}
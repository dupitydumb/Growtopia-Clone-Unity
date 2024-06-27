using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
public class Loby : MonoBehaviour
{

    public TMP_InputField usernameInput;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetUsername()
    {
        string username = usernameInput.text;
        //Set playerPrefs username
        PlayerPrefs.SetString("playerName", username);
    }

    public void JoinLoby()
    {
        NetworkManager.singleton.StartClient();
    }

    public void HostLoby()
    {
        NetworkManager.singleton.StartHost();
    }

    public void HostAndJoinLoby()
    {
        NetworkManager.singleton.StartHost();
        NetworkManager.singleton.StartClient();
    }
}

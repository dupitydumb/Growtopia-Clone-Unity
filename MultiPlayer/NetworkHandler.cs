using Mirror;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;
public class NetworkHandler : MonoBehaviour
{

    public TMP_InputField ipAddressInput;
    //Add log for network messages
    
    NetworkManager manager;

    public GameObject world;
    private void Awake()
    {
        manager = GetComponent<NetworkManager>();
    }

    void Start()
    {
        LogUI.instance.AddLog("NetworkHandler Start");
        LogUI.instance.AddLog("Network Address: " + manager.networkAddress);
        LogUI.instance.AddLog("Internet status: " + Application.internetReachability);

        //Log The Player IP Address
        LogUI.instance.AddLog("Player IP Address: " + manager.networkAddress);
    }

    // Call this method when the player submits their IP address
    public void ConnectToPlayerIP(string ipAddress)
    {
        if (!string.IsNullOrEmpty(ipAddress))
        {
            manager.networkAddress = ipAddress;
            LogUI.instance.AddLog($"Attempting to connect to: {ipAddress}");
            manager.StartClient(); // Start the client with the new IP address
        }
        else
        {
            LogUI.instance.AddLog("IP Address is empty.");
        }
    }

    public void HostWithIPAddress(string ipAddress)
    {
        if (!string.IsNullOrEmpty(ipAddress))
        {
            manager.networkAddress = ipAddress;
            LogUI.instance.AddLog($"Hosting at: {ipAddress}");
            manager.StartHost(); // Start hosting with the specified IP address
        }
        else
        {
            LogUI.instance.AddLog("IP Address is empty. Cannot host game.");
        }
    }

    public void StartServer()
    {
        StartCoroutine(TryToJoinOrHost());
    }
    public IEnumerator TryToJoinOrHost()
    {
        string ipAddress = ipAddressInput.text;
        bool attemptToConnect = true;
        manager.networkAddress = ipAddress;

        // Attempt to start a client to connect to the host
        manager.StartClient();

        // Give it some time to attempt a connection
        yield return new WaitForSeconds(3);

        // Check if the client is connected
        if (manager.isNetworkActive)
        {
            // Successfully connected to a game
            LogUI.instance.AddLog($"Joined game at: {ipAddress}");
            attemptToConnect = false;
        }
        else
        {
            // Failed to connect, stop the client
            manager.StopClient();
        }

        // If connection attempt failed, start hosting a game
        if (attemptToConnect)
        {
            LogUI.instance.AddLog($"No game found at: {ipAddress}. Hosting a new game.");
            manager.StartHost();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class ServerRequest : NetworkBehaviour
{

    public GameObject messagePrefab;
    public GameObject messageParent;
    // Example command where a player requests something from the server
    [Command]
    public void CmdSendRequestToServer(string messageString)
    {
        // Here, 'connectionToClient' is used to identify which player sent the request
        Debug.Log($"Request received from player with connection: {connectionToClient}");

        // Assuming you have a method to find the player object or ID based on the connection
        var player = FindPlayerObject(connectionToClient);
        if (player != null)
        {
            Debug.Log($"Request made by player: {player.playerName} or ID: {player.playerId}");
        }

        // Process the request here...

        RpcSendRequestToClient(messageString);

    }

    [ClientRpc]
    public void RpcSendRequestToClient(string messageString)
    {
        GameObject message = Instantiate(messagePrefab, messageParent.transform);
        message.GetComponent<TMP_Text>().text = messageString;
    }

    // Dummy method to represent finding a player object based on a network connection
    // You would implement this method based on how your player objects are managed
    private Player FindPlayerObject(NetworkConnection conn)
    {
        // Your logic here to find and return the player object associated with 'conn'
        return null; // Placeholder return
    }
}

// Dummy Player class for demonstration purposes
public class Player
{
    public string playerName;
    public string playerId;
}
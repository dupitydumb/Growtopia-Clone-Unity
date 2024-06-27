using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror; // Make sure to include this if you're using Mirror for networking

public class CameraFollow : MonoBehaviour
{
    public GameObject player;

    public float cameraSpeed = 0.1f;
    public float smoothSpeed = 0.125f;
    void Start()
    {
        InvokeRepeating("SearchLocalPlayer", 0.5f, 1f); // Repeatedly search for the local player every second, starting after 0.5 seconds
    }

    void Update()
    {
        if (player != null)
        {
            FollowPlayer();
        }
        
    }

    void SearchLocalPlayer()
    {
        foreach (var netPlayer in FindObjectsOfType<NetworkIdentity>()) // Find all NetworkIdentity objects in the scene
        {
            if (netPlayer.isLocalPlayer) // Check if it's the local player
            {
                player = netPlayer.gameObject; // Set the local player object
                break; // Exit the loop once the local player is found
            }
        }
    }

    void FollowPlayer()
    {
        if (player != null)
        {
            Vector3 desiredPosition = new Vector3(player.transform.position.x, player.transform.position.y, transform.position.z);
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
    }
}
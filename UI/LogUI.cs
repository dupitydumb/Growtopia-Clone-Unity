using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using Mirror;

public class LogUI : NetworkBehaviour
{
    // Start is called before the first frame update
    public Color SystemLogColor;
    public Color PlayerLogColor;
    public static LogUI instance;
    public GameObject textPrefab;

    public TMP_InputField messageInput;
    public GameObject parent;

    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (messageInput.text != "")
            {
                SendMessage();
            }
            else
            {
                messageInput.ActivateInputField();
            }
        }
    }

    public void AddLog(string log)
    {
        GameObject text = Instantiate(textPrefab, parent.transform);
        text.transform.localPosition = Vector3.zero; // Set local position relative to parent
        text.transform.localScale = Vector3.one; // Ensure the text scales correctly
        text.GetComponent<TextMeshProUGUI>().text = $"<color=#{ColorUtility.ToHtmlStringRGBA(SystemLogColor)}>[System] </color>: {log}";
    }

    public void SendMessage()
    {
        string playerId = PlayerPrefs.GetString("playerId", "Player");
        string message = messageInput.text;


        if (!string.IsNullOrEmpty(messageInput.text))
        {
            // Find the player object
            GameObject player = GameObject.FindWithTag("LocalPlayer");
            Playbox playerScript = player.GetComponent<Playbox>();
            
            if (player != null)
            {
                // Construct the message with player ID and text
                string formattedMessage = $"<color=#{ColorUtility.ToHtmlStringRGBA(PlayerLogColor)}>[{playerId}]</color>: {messageInput.text}";
                playerScript.CmdSendMessage(formattedMessage);
            }
            else
            {
                Debug.LogError("Player object not found.");
            }

            // Clear the input field, etc.
            messageInput.text = "";
            messageInput.ActivateInputField();
        }
    }



    [ClientRpc]
    public void RpcDisplayMessage(string message)
    {
        GameObject text = Instantiate(textPrefab, parent.transform);
        text.transform.localPosition = Vector3.zero;
        text.transform.localScale = Vector3.one;
        text.GetComponent<TextMeshProUGUI>().text = message;

        messageInput.transform.SetAsLastSibling();
    }
}

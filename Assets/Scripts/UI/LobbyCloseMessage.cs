using Mirror;
using TMPro;
using UnityEngine;

public class LobbyCloseMessage : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI message;

    private void Awake()
    {
        message = gameObject.GetComponent<TextMeshProUGUI>();
    }
 
    private void OnEnable()
    {
        if(NetworkServer.active)
        {
            message.text = "A session is currently running.\r\n\r\nClose the session? ";
        }
        else
        {
            message.text = "You are currently in a session.\r\n\r\nLeave the session?";
        }
        
    }   
  
    
}

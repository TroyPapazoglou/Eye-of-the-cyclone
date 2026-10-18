using UnityEngine;

public class DesktopShortcut : MonoBehaviour
{
    [SerializeField] private PCApplicationData applicationData;
    [SerializeField] private bool shouldStartLobby = true;

    public void OpenApplication()
    {
        if (!PCBehaviour.Instance.IsApplicationRunning())
        {
            PCBehaviour.Instance.OpenApplication(applicationData);

            if(shouldStartLobby) 
                SteamLobbyManager.Instance.HostLobby();

        }
    }
}

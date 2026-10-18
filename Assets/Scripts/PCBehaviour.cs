using Mirror;
using System;
using UnityEngine;

public enum PCApplication
{
    EyeOfCyclone,
    Chess,
    PlayerMovement,
    None
}

[System.Serializable]
public class PCApplicationData
{
    public PCApplication application;
    public Sprite icon;
    public GameObject window;

    [Header("Lobby UI")]
    public UserUI userEntryPrefab;

}

public class PCBehaviour : MonoBehaviour
{
    [SerializeField] private PCApplicationData[] applications;

    public static PCBehaviour Instance { get; private set; }

    public static event Action<bool> OnApplicationGameStateChanged;

    private PCApplication applicationOpen = PCApplication.None;

    private IPCApplication currentApplication;
    private PCApplicationData currentApplicationData;

    public PCApplication ApplicationOpen => applicationOpen;
    public PCApplicationData CurrentApplicationData => currentApplicationData;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        PCNetworkManager.OnDisconnected += CloseApplication;
        PCNetworkManager.OnServerStopped += UnregisterApplication;
    }

    private void OnDestroy()
    {
        PCNetworkManager.OnDisconnected -= CloseApplication;
        PCNetworkManager.OnServerStopped -= UnregisterApplication;

        UnregisterApplication();
    } 

    public void RegisterApplication(IPCApplication application)
    {
        UnregisterApplication();

        currentApplication = application;

        currentApplication.OnGameStateChanged += HandleApplicationGameStateChanged;
    }

    public void UnregisterApplication()
    {
        if (currentApplication != null)
        {
            currentApplication.OnGameStateChanged -= HandleApplicationGameStateChanged;

            currentApplication = null;
        }
    }

    private void HandleApplicationGameStateChanged(bool isActive)
    {
        OnApplicationGameStateChanged?.Invoke(isActive);
    }

    public void StartCurrentApplication()
    {
        if (currentApplication == null)
        {
            Debug.LogWarning("Current application has not been registered yet.");
            return;
        }

        currentApplication.StartGame();
    }

    private void CloseApplication()
    {
        if (CurrentApplicationData != null)
        {
            TaskbarManager.Instance.RemoveApplication(
                CurrentApplicationData.application
            );

            CurrentApplicationData.window.SetActive(false);

            currentApplicationData = null;
        }

        applicationOpen = PCApplication.None;
    }

    public bool IsApplicationRunning()
    {
        return applicationOpen != PCApplication.None;
    }

    public void OpenApplication(PCApplicationData data)
    {
        CurrentApplicationData?.window.SetActive(false);

        currentApplicationData = data;
        applicationOpen = data.application;

        data.window.SetActive(true);

        TaskbarManager.Instance.AddApplication(data);
    }

    public void ToggleApplication(PCApplicationData data)
    {
        if (CurrentApplicationData == data)
        {         
            bool isOpen = data.window.activeSelf;

            data.window.SetActive(!isOpen);
            return;
        }

        OpenApplication(data);
    }

    public void ToggleCurrentApplication()
    {
        if (CurrentApplicationData == null)
            return;

        bool isOpen = CurrentApplicationData.window.activeSelf;

        CurrentApplicationData.window.SetActive(!isOpen);
    }

    public void PlayerReady(NetworkConnectionToClient connection)
    {
        if (currentApplication == null)
        {
            Debug.LogWarning("No application is currently registered.");
            return;
        }

        currentApplication.PlayerReady(connection);
    }

    public void PlayerJoined(PlayerNetworkObject player, NetworkConnectionToClient connection)
    {
        if (currentApplication == null)
        {
            Debug.LogWarning("No application is currently active.");
            return;
        }

        currentApplication.PlayerJoined(player, connection);
    }

    public void PlayerLeft(PlayerNetworkObject player, NetworkConnectionToClient connection)
    {
        if (currentApplication == null)
        {
            Debug.LogWarning("No application is currently active.");
            return;
        }

        currentApplication.PlayerLeft(player, connection);
    }

    public PCApplicationData GetApplicationData(PCApplication application)
    {
        foreach(PCApplicationData data in  applications)
        {
            if(data.application == application)
                return data;
        }
        Debug.LogWarning($"No application data found for {application}");
        return null;    
    }
}

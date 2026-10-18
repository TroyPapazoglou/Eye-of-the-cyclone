using Mirror;
using Mirror.FizzySteam;
using Steamworks;
using System;
using UnityEngine;

public class SteamLobbyManager : MonoBehaviour
{
    private Callback<LobbyCreated_t> lobbyCreated;
    private Callback<GameLobbyJoinRequested_t> lobbyJoinRequested;
    private Callback<LobbyEnter_t> lobbyEntered;

    private CSteamID currentLobby;

    public event Action OnLobbyClosed;

    public static SteamLobbyManager Instance;

    public event Action OnFailToHostLobby;

    public event Action OnLeftLobby;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        lobbyCreated =
            Callback<LobbyCreated_t>.Create(OnLobbyCreated);

        lobbyJoinRequested =
            Callback<GameLobbyJoinRequested_t>.Create(OnLobbyJoinRequested);

        lobbyEntered =
            Callback<LobbyEnter_t>.Create(OnLobbyEntered);

        //Debug.Log("Lobby callbacks registered.");
    }

    public void HostLobby()
    {
        if (!SteamManager.Instance.CheckIfSteamRunning())
        {
            OnFailToHostLobby?.Invoke();
            return;
        }        

        Debug.Log("Creating lobby...");

        SteamMatchmaking.CreateLobby(
            ELobbyType.k_ELobbyTypeFriendsOnly,
            2);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {       
        Debug.Log($"Lobby created: {callback.m_ulSteamIDLobby}");

        CSteamID lobbyID = new CSteamID(callback.m_ulSteamIDLobby);

        currentLobby = lobbyID;

        string mySteamID = SteamUser.GetSteamID().ToString();

        SteamMatchmaking.SetLobbyData(
            lobbyID,
            "HostAddress",
            mySteamID);

        Debug.Log($"Stored host SteamID: {mySteamID}");

        NetworkManager.singleton.StartHost();
    }

    private void OnLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        Debug.Log("Join requested from Steam!");

        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        currentLobby = new CSteamID(callback.m_ulSteamIDLobby);

        Debug.Log($"Entered lobby {callback.m_ulSteamIDLobby}");

        // If we're the host, don't connect to ourselves.
        if (NetworkServer.active)
            return;

        CSteamID lobbyID = new CSteamID(callback.m_ulSteamIDLobby);

        string hostAddress =
            SteamMatchmaking.GetLobbyData(
                lobbyID,
                "HostAddress");

        Debug.Log($"Host SteamID: {hostAddress}");

        NetworkManager.singleton.networkAddress = hostAddress;

        Debug.Log($"Network address set to: {NetworkManager.singleton.networkAddress}");

        NetworkManager.singleton.StartClient();
    }

    public void HandleLobbyState()
    {
        if(NetworkServer.active)
        {
            CloseLobby();
        }
        else
        {
            LeaveLobby();
        }
    }

    public void CloseLobby()
    {
        if (!NetworkServer.active)
            return;

        OnLobbyClosed?.Invoke();

        SteamMatchmaking.SetLobbyJoinable(currentLobby, false);
              
        NetworkManager.singleton.StopHost();
               
        SteamMatchmaking.LeaveLobby(currentLobby);

        currentLobby = CSteamID.Nil;       
    }

    public void LeaveLobby()
    {      
        OnLeftLobby?.Invoke();

        if (NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopClient();
        }

        SteamMatchmaking.LeaveLobby(currentLobby);

        currentLobby = CSteamID.Nil;       
    }
}
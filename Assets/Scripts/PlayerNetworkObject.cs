using Mirror;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerNetworkObject : NetworkBehaviour
{
    [SyncVar]
    public PCApplication application;

    [SyncVar]
    public int playerIndex;

    [SyncVar]
    public ulong steamId;

    [SyncVar]
    public string playerName;

#pragma warning disable CS0414
    [SyncVar(hook = nameof(OnRegistrationCompleteChanged))]
    private bool registrationComplete;
#pragma warning restore CS0414

    [SyncVar(hook = nameof(OnHostChanged))]
    public bool isHost;

    [SyncVar]
    public bool isOut = false;

    public bool IsActive => !isOut;

    public bool IsSpectator = false;

    public static event Action<PlayerNetworkObject> OnPlayerReady;

    public static event Action<PlayerNetworkObject> OnPlayerLeft;

    public event Action<bool> OnHostChangedEvent;  
    public static PlayerNetworkObject LocalPlayer { get; private set; }

    [SerializeField] private CyclonePlayerNetwork cyclonePlayerNetwork;
    [SerializeField] private ChessPlayerNetwork chessPlayerNetwork;
    [SerializeField] private SandboxPlayerNetwork sandboxPlayerNetwork;

    public CyclonePlayerNetwork Cyclone => cyclonePlayerNetwork;
    public ChessPlayerNetwork Chess => chessPlayerNetwork;
    public SandboxPlayerNetwork Sandbox => sandboxPlayerNetwork;


    private void OnRegistrationCompleteChanged(bool oldValue, bool newValue)
    {
        if (!newValue)
            return;

        OnPlayerReady?.Invoke(this);
    }

    private void OnHostChanged(bool oldValue, bool newValue)
    {
        OnHostChangedEvent?.Invoke(newValue);
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        LocalPlayer = this;

        Debug.Log($"Local player spawned. netId={netId}");      

        CmdRegisterPlayer(SteamUser.GetSteamID().m_SteamID, SteamFriends.GetPersonaName());

    }

    public override void OnStopClient()
    {
      
        OnPlayerLeft?.Invoke(this);

        base.OnStopClient();
    }

    [Command]
    private void CmdRegisterPlayer(ulong steamId, string playerName)
    {
        Debug.Log($"Server received: {playerName}");

        this.steamId = steamId;
        this.playerName = playerName;

        application = PCBehaviour.Instance.ApplicationOpen;

        Debug.Log("application is: " + application);

        registrationComplete = true;

        PCBehaviour.Instance.PlayerReady(connectionToClient);
    }  

    [TargetRpc]
    public void SwitchPlayersInputMaps(NetworkConnection target, PlayerNetworkObject player, PlayerControlMap controlMap)
    {
        PlayerBehaviour.Instance.SwitchActionMap(controlMap);
    }

    public void SwitchInputMap(PlayerControlMap controlMap)
    {
        PlayerBehaviour.Instance.SwitchActionMap(controlMap);
    }

    public void Eliminate()
    {
        isOut = true;
    }

}

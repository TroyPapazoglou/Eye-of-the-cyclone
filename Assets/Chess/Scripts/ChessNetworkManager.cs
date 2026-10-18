using Mirror;
using System;
using UnityEngine;

public class ChessNetworkManager : NetworkManager
{ 
    [SerializeField]
    private ChessGameManager chessGameManager;

    public static event Action OnDisconnected;

    public override void OnStartServer()
    {
        base.OnStartServer();

        Debug.Log("SERVER STARTED");
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        Debug.Log("SERVER STOPPED");
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        NetworkClient.AddPlayer();
        Debug.Log("CLIENT CONNECTED TO SERVER");
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        OnDisconnected?.Invoke();
        Debug.Log("CLIENT DISCONNECTED FROM SERVER");

    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);

        Debug.Log($"Player joined! Connection ID: {conn.connectionId}");

    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        Debug.Log($"Player left! Connection ID: {conn.connectionId}");

        if (conn.identity != null)
        {
            PlayerNetworkObject player = conn.identity.GetComponent<PlayerNetworkObject>();


            if (player != null)
            {
                chessGameManager.players.Remove(player);
                chessGameManager.PlayerUnready(conn);
            }
        }

        base.OnServerDisconnect(conn);
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        PlayerNetworkObject player =
            conn.identity.GetComponent<PlayerNetworkObject>();

        player.isHost = numPlayers == 1;

        chessGameManager.players.Add(player);

        if (ChatNetworkBehaviour.Instance != null)
        {
            ChatNetworkBehaviour.Instance.SendChatHistory(conn);
        }

        Debug.Log($"Server added player: {conn.connectionId}");
    }

}

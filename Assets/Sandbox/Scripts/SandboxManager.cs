using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SandboxManager : NetworkBehaviour, IPCApplication
{
    public static SandboxManager Instance { get; private set; }

    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;

    [Header("Player Spawns")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Barrel")]
    [SerializeField] private GameObject barrelPrefab;
    [SerializeField] private float barrelInteractionRange = 20f;
    [SerializeField] private LayerMask barrelLayer;

    [Header("Barrel Spawns")]
    [SerializeField] private Transform[] barrelSpawnPoints;

    

  

    private HashSet<NetworkConnectionToClient> playersFinishedSandboxAnimation = new();
    private HashSet<int> readyPlayers = new();
    private readonly List<PlayerNetworkObject> players = new();

    private bool gameStarted;

    public event Action<bool> OnGameStateChanged;

    public static event Action OnGameStart;

    private void Start()
    {
        SteamLobbyManager.Instance.OnLobbyClosed += ServerResetGame;
        SteamLobbyManager.Instance.OnLeftLobby += ClientResetGame;
        CycloneNetworkManager.OnDisconnected += ClientResetGame;
    }

    private void OnDestroy()
    {
        SteamLobbyManager.Instance.OnLobbyClosed -= ServerResetGame;
        SteamLobbyManager.Instance.OnLeftLobby -= ClientResetGame;
        CycloneNetworkManager.OnDisconnected -= ClientResetGame;
    }

    private void ServerResetGame()
    {
        PlayerBehaviour.Instance.SetCanInteract(false);
        CameraManager.Instance.SetActiveCamera(CameraType.Main);
        InteractionHUD.Instance.FadeHUDOut();
        playersFinishedSandboxAnimation.Clear();
        readyPlayers.Clear();
        players.Clear();
        gameStarted = false;
    }

    private void ClientResetGame()
    {
        PlayerBehaviour.Instance.SetCanInteract(false);
        CameraManager.Instance.SetActiveCamera(CameraType.Main);
        InteractionHUD.Instance.FadeHUDOut();
    }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        if (PCBehaviour.Instance.ApplicationOpen == PCApplication.PlayerMovement)
        {
            PCBehaviour.Instance.RegisterApplication(this);
        }
    }

    public override void OnStopServer()
    {
        if (PCBehaviour.Instance != null)
        {
            PCBehaviour.Instance.UnregisterApplication();
        }

        base.OnStopServer();
    }

    [Server]
    public void StartGame()
    {      
        if (!NetworkServer.active)
        {
            Debug.LogWarning("PLAYER MOVEMENT: Not the server!");
            return;
        }


        if (!NetworkServer.active)
            return;

        if (gameStarted)
            return;

        if (players.Count == 0)
        {
            Debug.LogWarning(
                "Cannot start Player Movement: no players are ready."
            );

            return;
        }

        OnGameStart?.Invoke();

        gameStarted = true;

        RpcAnimateSandbox();       

        OnGameStateChanged?.Invoke(true);
    }

    public void PlayerReady(NetworkConnectionToClient connection)
    {
        if (!NetworkServer.active)
            return;

        if (connection.identity == null)
        {
            Debug.LogWarning(
                "PlayerReady received but connection has no identity."
            );

            return;
        }

        
        if (!connection.identity.TryGetComponent<PlayerNetworkObject>(out var player))
        {
            Debug.LogWarning(
                "PlayerReady connection does not have a PlayerNetworkObject."
            );

            return;
        }

        if (players.Contains(player))
            return;

        players.Add(player);

        Debug.Log(
            $"Player Movement: {player.playerName} is ready. " +
            $"Players ready: {players.Count}"
        );
    }

    public void PlayerJoined(
        PlayerNetworkObject player,
        NetworkConnectionToClient connection)
    {
        if (!NetworkServer.active)
            return;

        Debug.Log(
            $"Player Movement: {player.playerName} joined."
        );
    }

    public void PlayerLeft(
        PlayerNetworkObject player,
        NetworkConnectionToClient connection)
    {
        if (players.Contains(player))
        {
            players.Remove(player);
        }

        Debug.Log(
            $"Player Movement: {player.playerName} left."
        );
    }

    [Server]
    private void SpawnPlayers()
    {
        if (playerPrefab == null)
        {
            Debug.LogError(
                "Player Movement Manager has no player prefab assigned."
            );

            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError(
                "Player Movement Manager has no spawn points assigned."
            );

            return;
        }

        for (int i = 0; i < players.Count; i++)
        {
            PlayerNetworkObject player = players[i];

            if (player == null)
                continue;

            Transform spawnPoint =
                spawnPoints[i % spawnPoints.Length];

            GameObject playerObject = Instantiate(
                playerPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

            GameplayPlayer gameplayPlayer = playerObject.GetComponent<GameplayPlayer>();          

            if (gameplayPlayer == null)
            {
                Debug.LogError(
                    "Player prefab does not contain a GameplayPlayer component."
                );

                Destroy(playerObject);
                continue;
            }

            gameplayPlayer.SetPlayer(player);

            NetworkServer.Spawn(
                playerObject,
                player.connectionToClient
            );

            Debug.Log(
                $"Spawned gameplay player for {player.playerName}"
            );
        }
    }

    [Server]
    public void PlayerFinishedSandboxAnimation(NetworkConnectionToClient connectionId)
    {
        playersFinishedSandboxAnimation.Add(connectionId);

        if (playersFinishedSandboxAnimation.Count == players.Count)
        {
            playersFinishedSandboxAnimation.Clear();

            //RpcSwitchPlayerActionMap();

            //RpcPlayerCanInteract(true);

            SpawnPlayers();

            //SpawnBarrels();

            RpcMoveCameraToPlayer();
        }
    }

    [Server]
    private void RpcSwitchPlayerActionMap()
    {
        foreach(PlayerNetworkObject player in players)
        {
            player.SwitchPlayersInputMaps(player.connectionToClient, player, PlayerControlMap.PlayerMovement);
        }
    }
     
    [Server]
    public void Interact(PlayerNetworkObject player)
    {
        if (player == null)
            return;

        GameplayPlayer gameplayPlayer = player.Sandbox.GameplayPlayer;

        if (gameplayPlayer == null)
            return;

        Vector3 origin = gameplayPlayer.InteractionOrigin.position;

        Vector3 direction = gameplayPlayer.InteractionOrigin.forward;

        if (!Physics.Raycast(
                origin,
                direction,
                out RaycastHit hit,
                gameplayPlayer.InteractionRange,
                gameplayPlayer.InteractionLayer,
                QueryTriggerInteraction.Ignore))
        {          
            return;
        }


        NetworkInteractable interactable = hit.collider.GetComponentInParent<NetworkInteractable>();

        if (interactable == null)
            return;

        if (interactable.CanBePickedUp())
        {
            TryInteractWithPickup(player, interactable);
            player.Sandbox.GameplayPlayer.HandController.SetHoldingItem(true);
            return;
        }

        interactable.Interact(player);
      
    }

    [Server]
    public void Punch(PlayerNetworkObject player)
    {
        if (player == null)
            return;

        GameplayPlayer gameplayPlayer = player.Sandbox.GameplayPlayer;

        if (gameplayPlayer == null)
            return;

        Vector3 origin = gameplayPlayer.InteractionOrigin.position;

        Vector3 direction = gameplayPlayer.InteractionOrigin.forward;

        if (!Physics.Raycast(
                origin,
                direction,
                out RaycastHit hit,
                gameplayPlayer.InteractionRange,
                gameplayPlayer.InteractionLayer,
                QueryTriggerInteraction.Ignore))
        {
            return;
        }       

        NetworkInteractable interactable = hit.collider.GetComponentInParent<NetworkInteractable>();

        if (interactable != null)
        {
            interactable.Punch(player);
            return;
        }
    }

    [Server]
    private void TryInteractWithPickup(PlayerNetworkObject player, NetworkInteractable interactable)
    {
        if (!interactable.CanBePickedUp())
            return;

        GameplayPlayer gameplayPlayer = player.Sandbox.GameplayPlayer;

        if (gameplayPlayer == null)
            return;

        if (gameplayPlayer.interactable != null)
            return;

        interactable.PickUp(player);

        gameplayPlayer.interactable = interactable;
    }

    [Server]
    public void PickUpBarrel(PlayerNetworkObject player)
    {
        if (player == null)
            return;

        GameplayPlayer gameplayPlayer = player.Sandbox.GameplayPlayer;

        if (gameplayPlayer == null)
            return;

        if (!TryFindInteractableBarrel(gameplayPlayer, out Barrel barrel))
        {
            return;
        }

        TryInteractWithPickup(player, barrel);
    }

    [Server]
    public void ThrowBarrel(PlayerNetworkObject player)
    {
        if (player == null)
            return;

        GameplayPlayer gameplayPlayer = player.Sandbox.GameplayPlayer;

        if (gameplayPlayer == null)
            return;

        if (gameplayPlayer.interactable == null)
            return;

        gameplayPlayer.interactable.Throw(gameplayPlayer.InteractionOrigin.forward);

        gameplayPlayer.interactable = null;
    }

    [Server]
    public bool TryFindInteractableBarrel(GameplayPlayer player, out Barrel barrel)
    {
        barrel = null;

        Vector3 origin = player.InteractionOrigin.position;
        Vector3 direction = player.InteractionOrigin.forward;

        if (!Physics.Raycast(
            origin,
            direction,
            out RaycastHit hit,
            barrelInteractionRange,
            barrelLayer))
        {
            return false;
        }

        barrel = hit.collider.GetComponentInParent<Barrel>();

        return barrel != null;
    }

    [Server]
    private void SpawnBarrels()
    {
        foreach (Transform spawnPoint in barrelSpawnPoints)
        {
            GameObject barrelObject = Instantiate(
                barrelPrefab,
                spawnPoint.position,
                Quaternion.identity
            );

            Barrel barrel = barrelObject.GetComponent<Barrel>();

            barrel.SetSpawnedState();

            NetworkServer.Spawn(barrelObject);
        }
    }

    [ClientRpc]
    private void RpcAnimateSandbox()
    {
        SandboxAnimator.Instance.PlayIntroAnimation();

    }

    [ClientRpc]
    private void RpcMoveCameraToPlayer()
    {
        CameraManager.Instance.MoveToLocalPlayer();
    }

    [ClientRpc]
    private void RpcPlayerCanInteract(bool canInteract)
    {
        PlayerBehaviour.Instance.canInteract = canInteract;      
    }

}

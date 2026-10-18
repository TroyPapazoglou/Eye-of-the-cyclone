using Mirror;
using System;
using UnityEngine;

public class GameplayPlayer : NetworkBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private LayerMask interactionLayer;

    public float InteractionRange => interactionRange;
    public LayerMask InteractionLayer => interactionLayer;
    
    [SerializeField] private UsernameDisplay usernameDisplay;
    [SerializeField] private PlayerAppearance playerAppearance;
    [SerializeField] private HandController handController;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerLook playerLook;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private Transform cameraLookAtTarget;
    [SerializeField] private Transform barrelHoldPoint;
    [SerializeField] private Transform interactionOrigin;
    [SerializeField] private Transform root;
   
    public UsernameDisplay UsernameDisplay => usernameDisplay;
    public PlayerAppearance PlayerAppearance => playerAppearance;
    public HandController HandController => handController;
    public PlayerMovement PlayerMovement => playerMovement;
    public PlayerLook PlayerLook => playerLook;
    public Transform CameraTarget => cameraTarget;
    public Transform CameraLookAtTarget => cameraLookAtTarget;
    public Transform BarrelHoldPoint => barrelHoldPoint;
    public Transform InteractionOrigin => interactionOrigin;
    public Transform Root => root;
    public PlayerCustomisationMachine.CustomisationPart CurrentCustomisationPart => currentCustomisationPart;

    [SyncVar(hook = nameof(OnPlayerNetIdChanged))]
    private uint playerNetId;

    [SyncVar(hook = nameof(OnCustomisationPartChanged))]
    private PlayerCustomisationMachine.CustomisationPart currentCustomisationPart = PlayerCustomisationMachine.CustomisationPart.Body;

    public PlayerNetworkObject Player { get; private set; }

    public static GameplayPlayer LocalPlayer { get; private set; }

    public NetworkInteractable interactable;

    public event Action<PlayerCustomisationMachine.CustomisationPart> OnCustomisationPartChangedEvent;

    public static event Action<GameplayPlayer> OnLocalPlayerReady;



    [Server]
    public void SetPlayer(PlayerNetworkObject player)
    {
        playerNetId = player.netId;

        Player = player;
        player.Sandbox.SetGameplayPlayer(this);
        usernameDisplay.SetUsername(player.playerName);
    }


    [Server]
    public void SetCustomisationPart(PlayerCustomisationMachine.CustomisationPart part)
    {    
        currentCustomisationPart = part;
    }

    private void OnCustomisationPartChanged(PlayerCustomisationMachine.CustomisationPart oldPart, PlayerCustomisationMachine.CustomisationPart newPart)
    {
        OnCustomisationPartChangedEvent?.Invoke(newPart);
    }


    public override void OnStartClient()
    {
        base.OnStartClient();

        PlayerNetworkObject.OnPlayerReady += OnPlayerReady;

        TryFindPlayer();
    }

    public override void OnStopClient()
    {
        PlayerNetworkObject.OnPlayerReady -= OnPlayerReady;

        base.OnStopClient();
    }

    private void OnPlayerReady(PlayerNetworkObject player)
    {
        if (player.netId != playerNetId)
            return;

        Player = player;
        Player.Sandbox.SetGameplayPlayer(this);
    }

    private void OnPlayerNetIdChanged(uint oldNetId, uint newNetId)
    {
        if (newNetId == 0)
            return;

        if (!NetworkClient.spawned.TryGetValue(newNetId, out NetworkIdentity identity))
            return;

        Player = identity.GetComponent<PlayerNetworkObject>();

        if (Player == null)
            return;

        Player.Sandbox.SetGameplayPlayer(this);
    }

    private void TryFindPlayer()
    {
        if (playerNetId == 0)
            return;

        if (!NetworkClient.spawned.TryGetValue(
            playerNetId,
            out NetworkIdentity identity))
        {
            return;
        }

        Player = identity.GetComponent<PlayerNetworkObject>();

        if (Player == null)
            return;

        Player.Sandbox.SetGameplayPlayer(this);
    }



    public override void OnStartAuthority()
    {
        base.OnStartAuthority();

        LocalPlayer = this;

        OnLocalPlayerReady?.Invoke(this);
    }

    public override void OnStopAuthority()
    {
        if (LocalPlayer == this)
            LocalPlayer = null;

        base.OnStopAuthority();
    }
}

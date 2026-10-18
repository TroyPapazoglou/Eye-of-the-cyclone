using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerControlMap
{
    Idle,
    EyeOfTheCyclone,
    Chess,
    PlayerMovement
}

public class PlayerBehaviour : MonoBehaviour
{       

    public static Player Controls;

    public bool canInteract = false; 

    public static PlayerBehaviour Instance { get; private set; }


    private void Awake()
    {      

        Controls = new Player();

        if(Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
           
    }

    private void Start()
    {
        ChessGameManager.OnGameStart += OnChessGameStart;
        CycloneGameManager.OnGameStart += OnEyeOfTheCycloneStart;
        SandboxManager.OnGameStart += OnPlayerMovementStart;
        SteamLobbyManager.Instance.OnLobbyClosed += OnLobbyClosed;
        CycloneNetworkManager.OnDisconnected += OnDisconnected;
    }

    private void OnDestroy()
    {
        ChessGameManager.OnGameStart -= OnChessGameStart;
        CycloneGameManager.OnGameStart -= OnEyeOfTheCycloneStart;
        SandboxManager.OnGameStart -= OnPlayerMovementStart;
        SteamLobbyManager.Instance.OnLobbyClosed -= OnLobbyClosed;
        CycloneNetworkManager.OnDisconnected -= OnDisconnected;
    }

    private void OnEnable()
    {
        Controls.Idle.Zoom.performed += OnZoom;

        Controls.EyeOfTheCylone.Click.performed += OnClick;
        Controls.EyeOfTheCylone.Zoom.performed += OnZoom;

        Controls.Chess.ChessHold.started += OnChessHoldStarted;
        Controls.Chess.ChessHold.canceled += OnChessHoldCanceled;
        Controls.Chess.MousePosition.performed += OnMousePosition;
        Controls.Chess.Zoom.performed += OnZoom;
        Controls.Chess.ZoomChessboard.performed += OnZoomChessboard;

        Controls.PlayerMovement.Move.performed += OnMove;
        Controls.PlayerMovement.Move.canceled += OnMove;
        Controls.PlayerMovement.Jump.started += OnJumpStarted;
        Controls.PlayerMovement.Jump.canceled += OnJumpCanceled;
        Controls.PlayerMovement.PickUp.performed += OnInteract;
        Controls.PlayerMovement.Throw.performed += OnThrow;
        Controls.PlayerMovement.Look.performed += OnLook;
        Controls.PlayerMovement.EyesLook.started += OnEyesLook;
        Controls.PlayerMovement.EyesLook.canceled += OnEyesLook;
        Controls.PlayerMovement.Punch.performed += OnPunch;
      

        SwitchActionMap(PlayerControlMap.Idle);
    }

    private void OnDisable()
    {
        Controls.Idle.Zoom.performed -= OnZoom;

        Controls.EyeOfTheCylone.Click.performed -= OnClick;
        Controls.EyeOfTheCylone.Zoom.performed -= OnZoom;

        Controls.Chess.ChessHold.started -= OnChessHoldStarted;
        Controls.Chess.ChessHold.canceled -= OnChessHoldCanceled;
        Controls.Chess.MousePosition.performed -= OnMousePosition;
        Controls.Chess.Zoom.performed -= OnZoom;
        Controls.Chess.ZoomChessboard.performed -= OnZoomChessboard;

        Controls.PlayerMovement.Move.performed -= OnMove;
        Controls.PlayerMovement.Move.canceled -= OnMove;
        Controls.PlayerMovement.Jump.started -= OnJumpStarted;
        Controls.PlayerMovement.Jump.canceled -= OnJumpCanceled;
        Controls.PlayerMovement.PickUp.performed -= OnInteract;
        Controls.PlayerMovement.Throw.performed -= OnThrow;
        Controls.PlayerMovement.Look.performed -= OnLook;
        Controls.PlayerMovement.EyesLook.started -= OnEyesLook;
        Controls.PlayerMovement.EyesLook.canceled -= OnEyesLook;
        Controls.PlayerMovement.Punch.performed -= OnPunch;
      

        Controls.Idle.Disable();
        Controls.EyeOfTheCylone.Disable();
        Controls.Chess.Disable();
        Controls.PlayerMovement.Disable();
    }

    public void OnChessTurn()
    {
        SwitchActionMap(PlayerControlMap.Chess);
    }

    private void OnChessGameStart()
    {
        SwitchActionMap(PlayerControlMap.Chess);
    }

    private void OnEyeOfTheCycloneStart()
    {
        SwitchActionMap(PlayerControlMap.EyeOfTheCyclone);
    }

    private void OnPlayerMovementStart()
    {
        SwitchActionMap(PlayerControlMap.PlayerMovement);
    }

    private void OnLobbyClosed()
    {
        SwitchActionMap(PlayerControlMap.Idle);
        canInteract = false;
    }

    private void OnDisconnected()
    {
        SwitchActionMap(PlayerControlMap.Idle);
        canInteract = false;
    }

    public void SwitchActionMap(PlayerControlMap controlMap)
    {
        Controls.Idle.Disable();
        Controls.EyeOfTheCylone.Disable();
        Controls.Chess.Disable();
        Controls.PlayerMovement.Disable();

        switch(controlMap)
        {
            case PlayerControlMap.Idle:
                Controls.Idle.Enable();
                break;

            case PlayerControlMap.EyeOfTheCyclone:
                Controls.EyeOfTheCylone.Enable();
                break;

            case PlayerControlMap.Chess:
                Controls.Chess.Enable();
                break;

            case PlayerControlMap.PlayerMovement:
                Controls.PlayerMovement.Enable();
                break;
        }       
    }


    private void OnClick(InputAction.CallbackContext context)
    {       
        if (!canInteract) return;       

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.TryGetComponent(out CardData cardData))
            {
                canInteract = false;

                hit.collider.enabled = false;

                CardInfo played = cardData.card;

                PlayerNetworkObject.LocalPlayer.Cyclone.CmdPlayCard(played);
            }
        }
    }

    private void OnZoom(InputAction.CallbackContext context)
    {      
        CameraManager.Instance.TogglePCCamera();
    }

    private void OnZoomChessboard(InputAction.CallbackContext context)
    {
        CameraManager.Instance.ToggleChessboardCamera();
    }

    private void OnChessHoldStarted(InputAction.CallbackContext context)
    {        
        if(!canInteract) return;
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        ChessInputController.Instance.BeginHold(mousePosition);
    }

    private void OnChessHoldCanceled(InputAction.CallbackContext context)
    {
        if (!canInteract) return;
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        ChessInputController.Instance.EndHold(mousePosition);
    }

    private void OnMousePosition(InputAction.CallbackContext context)
    {
        if (!canInteract)
            return;

        ChessInputController.Instance.UpdateMousePosition(context.ReadValue<Vector2>());
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        if (!canInteract)
            return;

        PlayerNetworkObject localPlayer = PlayerNetworkObject.LocalPlayer;

        if (localPlayer == null)
            return;

        SandboxPlayerNetwork sandbox = localPlayer.Sandbox;

        if (sandbox == null)
            return;

        GameplayPlayer gameplayPlayer = sandbox.GameplayPlayer;

        if (gameplayPlayer == null)
            return;

        gameplayPlayer.PlayerMovement.SetMoveInput(context.ReadValue<Vector2>());
    }

    private void OnJumpStarted(InputAction.CallbackContext context)
    {
        if (!canInteract)
            return;

        PlayerNetworkObject localPlayer = PlayerNetworkObject.LocalPlayer;

        if (localPlayer == null)
            return;

        GameplayPlayer gameplayPlayer = localPlayer.Sandbox.GameplayPlayer;

        if (gameplayPlayer == null)
            return;

        gameplayPlayer.PlayerMovement.SetJumpHeld(true);
    }

    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        if (PlayerNetworkObject.LocalPlayer == null)
            return;

        GameplayPlayer gameplayPlayer = PlayerNetworkObject.LocalPlayer.Sandbox.GameplayPlayer;

        if (gameplayPlayer == null)
            return;

        gameplayPlayer.PlayerMovement.SetJumpHeld(false);
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if(!canInteract) 
            return;

        PlayerNetworkObject.LocalPlayer.Sandbox.TryPlayLocalInteractionAnimation();
        PlayerNetworkObject.LocalPlayer.Sandbox.CmdInteract();
    }

    private void OnThrow(InputAction.CallbackContext context)
    {
        if (!canInteract)
            return;

        PlayerNetworkObject.LocalPlayer.Sandbox.CmdThrowBarrel();
    }
    
    private void OnLook(InputAction.CallbackContext context)
    {
        if (!canInteract)
            return;

        if (PlayerNetworkObject.LocalPlayer == null)
        {
            Debug.LogError("LOOK: LocalPlayer is NULL");
            return;
        }

        if (PlayerNetworkObject.LocalPlayer.Sandbox == null)
        {
            Debug.LogError("LOOK: Sandbox is NULL");
            return;
        }

        if (PlayerNetworkObject.LocalPlayer.Sandbox.GameplayPlayer == null)
        {
            Debug.LogError("LOOK: GameplayPlayer is NULL");
            return;
        }

        if (PlayerNetworkObject.LocalPlayer.Sandbox.GameplayPlayer.PlayerLook == null)
        {
            Debug.LogError("LOOK: PlayerLook is NULL");
            return;
        }

        PlayerNetworkObject.LocalPlayer.Sandbox.GameplayPlayer.PlayerLook.HandleLook(context.ReadValue<Vector2>());
    }

    private void OnEyesLook(InputAction.CallbackContext context)
    {
        if (!canInteract)
            return;

        if (context.started)
        {
            PlayerNetworkObject.LocalPlayer.Sandbox.GameplayPlayer.PlayerLook.BeginEyeLook();
        }
        else if (context.canceled)
        {
            PlayerNetworkObject.LocalPlayer.Sandbox.GameplayPlayer.PlayerLook.EndEyesLook();
        }
    }

    private void OnPunch(InputAction.CallbackContext context)
    {
        if (!canInteract)
            return;

        PlayerNetworkObject player = PlayerNetworkObject.LocalPlayer;

        if (player == null)
            return;

        if (!player.Sandbox.CanPunch())
            return;

        player.Sandbox.TryPlayLocalPunchAnimation();
        player.Sandbox.CmdPunch();
    }

    public void SetCanInteract(bool value)
    {
        canInteract = value;
    }

}

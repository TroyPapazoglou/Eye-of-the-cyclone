using PrimeTween;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public enum CameraType
{
    Main,
    PC,
    Chessboard,
    Player,
    FrontStand,
    BackStand,
}

[RequireComponent(typeof(Camera))]
public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    private Camera mainCam;

    public Vector3 StartPos { get; private set; } = new Vector3(0, 0.15f, -0.1f);

    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private CinemachineCamera MainCamera;
    [SerializeField] private CinemachineCamera PcCamera;
    [SerializeField] private CinemachineCamera ChessboardCam;
    [SerializeField] private CinemachineCamera PlayerCam;

    [SerializeField] private CinemachineCamera FrontStandCamera;

    [SerializeField] private CinemachineCamera BackStandCamera;

    [SerializeField] private CinemachineSplineDolly splineDolly;

    public CinemachineCamera BackgroundCam => MainCamera;

    bool wasBlending;


    private bool canToggle = false;

    private CameraType currentCamera;

    private void Awake()
    {
        Instance = this;
        mainCam = GetComponent<Camera>();
    }

    private void Start()
    {
        CinemachineCore.BlendFinishedEvent.AddListener(OnBlendEnd);
    }

    private void OnEnable()
    {       

        if (AnimationManager.Instance != null)
            AnimationManager.Instance.OnIntroStart += PlayIntroAnimation;
    }

    private void OnDisable()
    {
        AnimationManager.Instance.OnIntroStart -= PlayIntroAnimation;
        CinemachineCore.BlendFinishedEvent.RemoveListener(OnBlendEnd);

    }       

    public void OnBlendEnd(ICinemachineMixer mixer, ICinemachineCamera cam) 
    {
        //Debug.Log("HUH?");
        GameplayPlayer player = GameplayPlayer.LocalPlayer;

        if (player == null)
            return;

        PlayerBehaviour.Instance.SetCanInteract(true);

        player.Player.SwitchInputMap(PlayerControlMap.PlayerMovement);
    }

    private void PlayIntroAnimation()
    {
        SetActiveCamera(CameraType.Main);

        splineDolly.CameraPosition = 0f;

        Tween.Custom(
            0f,
            1f,
            3f,
            value =>
            {
                splineDolly.CameraPosition = value;
            },
            ease: Ease.Linear,
            startDelay: 2.1f
            
        ).OnComplete(() => { canToggle = true; });
    
    }

    public Vector3 GetScreenCenterWorldPosition(float distanceFromCamera)
    {
        return mainCam.ViewportToWorldPoint(
            new Vector3(
                0.5f,
                0.5f,
                distanceFromCamera));
    }

    public void TogglePCCamera()
    {
        if (!canToggle)
            return;

        if (currentCamera == CameraType.Main)
            SetActiveCamera(CameraType.PC);
        else
            SetActiveCamera(CameraType.Main);
    }

    public void ToggleChessboardCamera()
    {
        if (!canToggle)
            return;

        if (currentCamera == CameraType.Main)
            SetActiveCamera(CameraType.Chessboard);
        else
            SetActiveCamera(CameraType.Main);
    }

    public void TogglePlayerCamera()
    {
        if (!canToggle)
            return;

        if (currentCamera == CameraType.Main)
        {
            SetActiveCamera(CameraType.Player);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }        
        else
            SetActiveCamera(CameraType.Main);
    }

    public void SetActiveCamera(CameraType cameraType)
    {           

        MainCamera.Priority = 0;
        PcCamera.Priority = 0;
        ChessboardCam.Priority = 0;
        PlayerCam.Priority = 0;
        FrontStandCamera.Priority = 0;
        BackStandCamera.Priority = 0;

        switch (cameraType)
        {
            case CameraType.Main:
                MainCamera.Priority = 10;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;

            case CameraType.PC:
                PcCamera.Priority = 10;
                break;

            case CameraType.Chessboard:
                ChessboardCam.Priority = 10;
                break;

            case CameraType.Player:
                PlayerCam.Priority = 10;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;

            case CameraType.FrontStand:
                FrontStandCamera.Priority = 10;
                break;

            case CameraType.BackStand:
                BackStandCamera.Priority = 10;
                break;
        }

        currentCamera = cameraType;

        //Debug.Log("current camera is now: " + currentCamera);
    }

    public void MoveToLocalPlayer()
    {
        GameplayPlayer player = GameplayPlayer.LocalPlayer;

        if (player == null)
            return;

        PlayerCam.Follow = player.CameraTarget;
        PlayerCam.LookAt = player.CameraLookAtTarget;

        SetActiveCamera(CameraType.Player);

        if (InteractionHUD.Instance != null) 
            InteractionHUD.Instance.FadeHUDIn();
    }   
}

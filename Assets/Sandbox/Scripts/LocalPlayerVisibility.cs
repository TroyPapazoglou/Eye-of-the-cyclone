using UnityEngine;

[RequireComponent (typeof(GameplayPlayer))]
public class LocalPlayerBodyVisibility : MonoBehaviour
{
       

    [Header("Layers")]
    [SerializeField] private string outlineHideLayer = "OutlineHide";

    private Camera playerCamera;

    private GameplayPlayer localPlayer;

    private void OnEnable()
    {
        GameplayPlayer.OnLocalPlayerReady += HandleLocalPlayerReady;
    }    
   
    private void Start()
    {
        if(GameplayPlayer.LocalPlayer != null)
        {
            HandleLocalPlayerReady(GameplayPlayer.LocalPlayer);
        }
    }

    private void OnDisable()
    {
        GameplayPlayer.OnLocalPlayerReady -= HandleLocalPlayerReady;
    }

    private void HandleLocalPlayerReady(GameplayPlayer player)
    {
        localPlayer = player;

        if(playerCamera == null)
            playerCamera = Camera.main;

        SetLocalPlayerBodyLayer();
        HideLocalPlayerFromCamera();
    }

    private void SetLocalPlayerBodyLayer()
    {
        int layer = LayerMask.NameToLayer(outlineHideLayer);

        if (layer == -1)
        {
            Debug.LogError($"Layer '{outlineHideLayer}' does not exist.");
            return;
        }

        localPlayer.PlayerAppearance.Body.layer = layer;
        localPlayer.PlayerAppearance.LeftEye.layer = layer;
        localPlayer.PlayerAppearance.RightEye.layer = layer;
        localPlayer.PlayerAppearance.LeftEyePupil.layer = layer;
        localPlayer.PlayerAppearance.RightEyePupil.layer = layer;      
    }

    private void HideLocalPlayerFromCamera()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("LocalPlayerBodyVisibility could not find the player camera.");
            return;
        }

        int layer = LayerMask.NameToLayer(outlineHideLayer);

        if (layer == -1)
            return;

        playerCamera.cullingMask &= ~(1 << layer);
    }
}

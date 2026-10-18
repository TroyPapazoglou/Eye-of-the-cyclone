using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class PlanarMirror : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Camera reflectionCamera;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private Material mirrorMaterial;

    [Header("Mirror Settings")]
    [SerializeField] private int textureResolution = 1024;

    private GameplayPlayer localPlayer;

    private void OnEnable()
    {
        GameplayPlayer.OnLocalPlayerReady += HandleLocalPlayerReady;
    }

    private void Start()
    {
        if (GameplayPlayer.LocalPlayer != null)
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

        if (playerCamera == null)
            playerCamera = Camera.main;
    }


    private void LateUpdate()
    {
        if (localPlayer == null || playerCamera == null || reflectionCamera == null)
            return;

        UpdateReflectionCamera();
    }

    private void UpdateReflectionCamera()
    {
        Transform mirror = transform;

        Vector3 localPosition =
            mirror.InverseTransformPoint(localPlayer.transform.position);

        localPosition.z = -localPosition.z;

        reflectionCamera.transform.position =
            mirror.TransformPoint(localPosition);

        reflectionCamera.transform.rotation =
            mirror.rotation * Quaternion.Euler(0f, 180f, 0f);

        reflectionCamera.Render();
    }
}
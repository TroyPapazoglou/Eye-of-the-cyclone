using UnityEngine;
using UnityEngine.InputSystem;
[RequireComponent(typeof(PlayerMovement))]

[RequireComponent(typeof(GameplayPlayer))]
public class PlayerLook : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraOrbit;
    [SerializeField] private Transform cameraRoot;
    [SerializeField] private Transform eyeballs;
    [SerializeField] private Transform leftEyePivot;
    [SerializeField] private Transform rightEyePivot;

    [Header("Look")]
    [SerializeField] private float lookSensitivity = 0.1f;
    [SerializeField] private float minLookAngle = -80f;
    [SerializeField] private float maxLookAngle = 80f;

    [Header("Eye Look")]
    [SerializeField] private float eyeYawLimit = 100f;
    [SerializeField] private float eyePitchLimit = 80f;
    [SerializeField] private float eyeReturnSpeed = 8f;

    [Header("Body Rotation")]
    [SerializeField] private float bodyTurnSpeed = 12f;

    private GameplayPlayer gameplayPlayer;
    private PlayerMovement playerMovement;

    private float cameraPitch;

    private float eyeYaw;
    private float eyePitch;

    private float cameraLookYaw;
    private float cameraLookPitch;

    private bool wasEyeLooking;
    private bool returningEyesToNeutral;

    private Quaternion bodyRotationTarget;

    private Quaternion eyeLookStartRotation;

    private Quaternion eyeLookStartRootRotation;

    private Quaternion cameraWorldRotationTarget;

    private Quaternion eyeReturnStartRotation;

    public bool CanLook { get; set; } = true;

    private void Awake()
    {
        gameplayPlayer = GetComponent<GameplayPlayer>();
        playerMovement = GetComponent<PlayerMovement>();        
    }

    private void Update()
    {
        if (gameplayPlayer == null || !gameplayPlayer.isOwned)
        {
            return;
        }

        if (returningEyesToNeutral)
        {
            ReturnEyesAndTurnBody();
        }
    }

    public void HandleLook(Vector2 lookInput)
    {
        if (!CanLook)
            return;

        if (gameplayPlayer == null || !gameplayPlayer.isOwned)
            return;

        if (returningEyesToNeutral)
            return;

        if (wasEyeLooking)
        {
            LookWithEyes(lookInput);
        }
        else
        {
            HandleBodyLook(lookInput);
        }
    }

    public void HandleEyesLook(Vector2 lookInput)
    {
        if (!wasEyeLooking)
            BeginEyeLook();

        returningEyesToNeutral = false;

        LookWithEyes(lookInput);

        wasEyeLooking = true;
    }

    public void BeginEyesLook()
    {
        BeginEyeLook();
    }

    public void EndEyesLook()
    {
        if (!wasEyeLooking)
            return;

        UpdateEyeRotation();

        BeginBodyTurnFromEyes();
        wasEyeLooking = false;
    }

    public void BeginEyeLook()
    {
        if (!gameplayPlayer.isOwned)
            return;

        if (playerMovement.IsMoving)
            return;

        wasEyeLooking = true;

        eyeLookStartRotation = cameraOrbit.localRotation;

        eyeLookStartRootRotation = cameraRoot.localRotation;

        cameraLookYaw = 0f;
        cameraLookPitch = 0f;

        Vector3 cameraEuler = cameraRoot.localEulerAngles;

        float currentPitch = NormalizeAngle(cameraEuler.x);
        float currentYaw = NormalizeAngle(cameraOrbit.localEulerAngles.y);

        // Eyes start by matching the camera's current direction,
        // but are restricted to their own limits.
        eyeYaw = Mathf.Clamp(
            currentYaw,
            -eyeYawLimit,
            eyeYawLimit
        );

        eyePitch = Mathf.Clamp(
            currentPitch,
            -eyePitchLimit,
            eyePitchLimit
        );

        ApplyEyeRotation(
            eyePitch,
            eyeYaw
        );
    }

    private void UpdateEyeRotation()
    {
        float normalizedYaw =
            Mathf.DeltaAngle(0f, cameraLookYaw);

        eyeYaw = Mathf.Clamp(
            normalizedYaw,
            -eyeYawLimit,
            eyeYawLimit
        );

        float startingPitch =
            NormalizeAngle(
                eyeLookStartRootRotation.eulerAngles.x
            );

        float currentPitch =
            startingPitch + cameraLookPitch;

        eyePitch = Mathf.Clamp(
            currentPitch,
            -eyePitchLimit,
            eyePitchLimit
        );

        ApplyEyeRotation(
            eyePitch,
            eyeYaw
        );
    }

    private float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;

        return angle;
    }

    private void LookWithEyes(Vector2 lookInput)
    {
        float mouseX = lookInput.x * lookSensitivity;

        float mouseY = lookInput.y * lookSensitivity;

        cameraLookYaw += mouseX;
        cameraLookPitch -= mouseY;

        float startingPitch = NormalizeAngle(eyeLookStartRootRotation.eulerAngles.x);

        cameraLookPitch = Mathf.Clamp(
            cameraLookPitch,
            minLookAngle - startingPitch,
            maxLookAngle - startingPitch
        );

        cameraOrbit.localRotation = eyeLookStartRotation * Quaternion.Euler(0f, cameraLookYaw, 0f);

        cameraRoot.localRotation = eyeLookStartRootRotation * Quaternion.Euler(cameraLookPitch, 0f, 0f);

        UpdateEyeRotation();
    }

    private void HandleBodyLook(Vector2 lookInput)
    {
        float mouseX =
         lookInput.x * lookSensitivity;

        float mouseY =
            lookInput.y * lookSensitivity;

        transform.Rotate(
            Vector3.up * mouseX
        );

        cameraPitch -= mouseY;

        cameraPitch = Mathf.Clamp(
            cameraPitch,
            minLookAngle,
            maxLookAngle
        );

        cameraRoot.localRotation =
            Quaternion.Euler(
                cameraPitch,
                0f,
                0f
            );

        // Make the eyes follow the camera's vertical look.
        float eyePitch =
            Mathf.Clamp(
                cameraPitch,
                -eyePitchLimit,
                eyePitchLimit
            );

        ApplyEyeRotation(
            eyePitch,
            0f
        );
    }

    private void BeginBodyTurnFromEyes()
    {
        cameraWorldRotationTarget = cameraRoot.rotation;

        eyeReturnStartRotation = leftEyePivot.localRotation;

        Vector3 cameraForward =
            cameraRoot.forward;

        cameraForward.y = 0f;

        if (cameraForward.sqrMagnitude < 0.001f)
            return;

        cameraForward.Normalize();

        bodyRotationTarget =
            Quaternion.LookRotation(
                cameraForward,
                Vector3.up
            );

        returningEyesToNeutral = true;
    }

    private void ReturnEyesAndTurnBody()
    {
        float turnAmount =
         bodyTurnSpeed * 100f * Time.deltaTime;

        transform.rotation =
            Quaternion.RotateTowards(
                transform.rotation,
                bodyRotationTarget,
                turnAmount
            );

        // Keep camera pointing at the same world direction.
        cameraOrbit.rotation =
            Quaternion.Euler(
                0f,
                cameraWorldRotationTarget.eulerAngles.y,
                0f
            );

        // -----------------------------------------
        // Calculate where the camera is relative
        // to the body's current rotation.
        // -----------------------------------------

        Vector3 cameraForward =
            cameraWorldRotationTarget * Vector3.forward;

        // Yaw relative to the body
        Vector3 flatForward =
            cameraForward;

        flatForward.y = 0f;

        if (flatForward.sqrMagnitude > 0.001f)
        {
            flatForward.Normalize();

            Vector3 bodyForward =
                transform.forward;

            float targetEyeYaw =
                Vector3.SignedAngle(
                    bodyForward,
                    flatForward,
                    Vector3.up
                );

            eyeYaw = Mathf.Clamp(
                targetEyeYaw,
                -eyeYawLimit,
                eyeYawLimit
            );
        }

        // Pitch directly from the camera's forward vector
        float horizontalMagnitude =
            new Vector2(
                cameraForward.x,
                cameraForward.z
            ).magnitude;

        float targetEyePitch =
            -Mathf.Atan2(
                cameraForward.y,
                horizontalMagnitude
            ) * Mathf.Rad2Deg;

        eyePitch = Mathf.Clamp(
            targetEyePitch,
            -eyePitchLimit,
            eyePitchLimit
        );

        ApplyEyeRotation(
            eyePitch,
            eyeYaw
        );

        bool bodyFinished =
            Quaternion.Angle(
                transform.rotation,
                bodyRotationTarget
            ) < 0.1f;

        if (bodyFinished)
        {
            transform.rotation =
                bodyRotationTarget;

            cameraRoot.localRotation =
                Quaternion.Inverse(transform.rotation) *
                cameraWorldRotationTarget;

            cameraOrbit.localRotation =
                Quaternion.Inverse(transform.rotation) *
                Quaternion.Euler(
                    0f,
                    cameraWorldRotationTarget.eulerAngles.y,
                    0f
                );

            returningEyesToNeutral = false;

            // Restore the normal camera pitch state.
            cameraPitch =
                NormalizeAngle(
                    cameraRoot.localEulerAngles.x
                );

            cameraLookYaw = 0f;
            cameraLookPitch = 0f;

            // Make the eyes immediately match the camera's
            // current pitch when returning to normal look.
            eyePitch = Mathf.Clamp(
                cameraPitch,
                -eyePitchLimit,
                eyePitchLimit
            );

            eyeYaw = 0f;

            ApplyEyeRotation(
                eyePitch,
                eyeYaw
            );
        }
    }

    private void ApplyEyeRotation(float pitch, float yaw)
    {
        Quaternion rotation =
            Quaternion.Euler(
                pitch,
                yaw,
                0f
            );

        leftEyePivot.localRotation =
            rotation;

        rightEyePivot.localRotation =
            rotation;
    }
}

using UnityEngine;

public class ProceduralHandMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform handRig;

    [SerializeField] private Vector3 positionOffset = new Vector3(0.3f, -0.3f, 0.5f);

    [Header("Position Lag")]
    [SerializeField] private float positionSmooth = 12f;

    [Header("Rotation")]
    [SerializeField] private float rotationSmooth = 12f;

    [Header("Movement Bounce")]
    [SerializeField] private float movementLag = 0.08f;
    [SerializeField] private float bounceAmount = 0.05f;

    [Header("Spring Bounce")]
    [SerializeField] private float springStrength = 80f;
    [SerializeField] private float damping = 8f;

    [Header("Jitter")]
    [SerializeField] private float jitterAmount = 0.02f;

    private Vector3 previousCameraPosition;
    private Vector3 velocity;

    private void Start()
    {
        previousCameraPosition = cameraTransform.position;

        transform.position = cameraTransform.position;

        velocity = Vector3.zero;
    }

    private void LateUpdate()
    {
        float deltaTime = Time.deltaTime;

        if (deltaTime <= 0f)
            return;

        // -------------------------
        // 1. Calculate camera velocity
        // -------------------------

        Vector3 cameraVelocity =
            (cameraTransform.position - previousCameraPosition)
            / deltaTime;

        previousCameraPosition = cameraTransform.position;


        // -------------------------
        // 2. Calculate movement lag
        // -------------------------

        Vector3 lagOffset =
            -cameraVelocity * movementLag;


        // -------------------------
        // 3. Calculate procedural jitter
        // -------------------------

        float noiseX =
            Mathf.PerlinNoise(Time.time * 8f, 0f) - 0.5f;

        float noiseY =
            Mathf.PerlinNoise(Time.time * 8f, 1f) - 0.5f;

        float noiseZ =
            Mathf.PerlinNoise(Time.time * 8f, 2f) - 0.5f;

        Vector3 jitter =
            new Vector3(noiseX, noiseY, noiseZ)
            * jitterAmount;


        // -------------------------
        // 4. Calculate target position
        // -------------------------

        Quaternion yawRotation =
    Quaternion.Euler(
        0f,
        cameraTransform.eulerAngles.y,
        0f
    );

        Vector3 targetPosition =
            cameraTransform.position
            + yawRotation * positionOffset
            + lagOffset
            + jitter;


        // -------------------------
        // 5. Spring physics
        // -------------------------

        Vector3 displacement =
            targetPosition - transform.position;

        velocity +=
            displacement
            * springStrength
            * deltaTime;

        velocity *=
            Mathf.Exp(-damping * deltaTime);

        transform.position +=
            velocity
            * deltaTime;


        // -------------------------
        // 6. Rotation lag
        // -------------------------

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                cameraTransform.rotation,
                rotationSmooth * deltaTime
            );
    }
}

using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class SplineFollower : MonoBehaviour
{
    [Header("Spline")]
    public SplineContainer spline;

    [Header("Movement")]
    public float speed = 3f;
    public bool loop = true;

    [Header("Rotation")]
    public bool rotateToPath = true;

    [Range(0f, 1f)]
    public float t;

    private float splineLength;

    void Start()
    {
        splineLength = spline.CalculateLength();
    }

    void Update()
    {
        if (spline == null)
            return;

        // Convert speed into normalized movement.
        t += (speed / splineLength) * Time.deltaTime;

        if (loop)
            t %= 1f;
        else
            t = Mathf.Clamp01(t);

        spline.Evaluate(t, out float3 position, out float3 tangent, out float3 up);

        transform.position = position;

        if (rotateToPath)
            transform.rotation = Quaternion.LookRotation(tangent, up);
    }
}

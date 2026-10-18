using UnityEngine;

public class Dartboard : MonoBehaviour
{
    public static Dartboard Instance { get; private set; }

    [Header("Board References")]
    [SerializeField] private Transform scoringCentre;

    [Header("Board Dimensions")]
    [Tooltip("Radius from the bullseye to the outer edge of the double ring, in local units.")]
    [SerializeField, Min(0.001f)]
    private float boardRadius = 0.5f;

    private void Awake()
    {
        Instance = this;
    }

    // Starting at the top and moving clockwise.
    private static readonly int[] Numbers =
    {
        20, 1, 18, 4, 13,
        6, 10, 15, 2, 17,
        3, 19, 7, 16, 8,
        11, 14, 9, 12, 5
    };

    /// <summary>
    /// Calculates the dartboard score at a world-space hit point.
    /// </summary>
    public int CalculateScore(Vector3 worldHitPoint)
    {
        Transform centre = scoringCentre != null
            ? scoringCentre
            : transform;

        // Convert hit position into the scoring centre's local space.
        Vector3 local = centre.InverseTransformPoint(worldHitPoint);

        float x = local.x;
        float y = local.y;

        // Distance from the bullseye.
        float radius = Mathf.Sqrt(x * x + y * y);
        float r = radius / boardRadius;

        // Outside the scoring face.
        if (r > 1f)
            return 0;

        // Bullseye: 50 points.
        if (r <= 0.03735f)
            return 50;

        // Outer bull: 25 points.
        if (r <= 0.09353f)
            return 25;

        // Calculate the numbered wedge.
        float angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
        float angleFromTop = Mathf.Repeat(90f - angle, 360f);

        int segmentIndex = Mathf.FloorToInt(
            Mathf.Repeat(angleFromTop + 9f, 360f) / 18f
        );

        int number = Numbers[segmentIndex];

        // Double ring.
        if (r >= 0.95294f)
            return number * 2;

        // Triple ring.
        if (r >= 0.58235f && r <= 0.62941f)
            return number * 3;

        // Inner and outer single regions.
        return number;
    }

    /// <summary>
    /// Call this after a raycast hits the dartboard.
    /// Prints the score to the Console.
    /// </summary>
    public void RegisterHit(Vector3 worldHitPoint)
    {
        Transform centre = scoringCentre != null
            ? scoringCentre
            : transform;

        Vector3 local = centre.InverseTransformPoint(worldHitPoint);

        float radius = new Vector2(local.x, local.y).magnitude;
        float normalisedRadius = radius / boardRadius;

        int score = CalculateScore(worldHitPoint);

        Debug.Log(
            $"Dart hit! Score: {score} | " +
            $"Radius: {normalisedRadius:F3} | " +
            $"Local hit: ({local.x:F3}, {local.y:F3})",
            this
        );
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirHockeyNavigation : MonoBehaviour
{
    public static AirHockeyNavigation Instance;

    public enum TableSide
    {
        Front,
        Back
    }

    [Header("Corner Waypoints")]
    [SerializeField] private Transform[] corners = new Transform[4];

    [Header("Stand Positions")]
    [SerializeField] private Transform frontStandPoint;
    [SerializeField] private Transform backStandPoint;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private float arrivalDistance = 0.05f;
    [SerializeField] private float waypointTimeout = 10f;
    [SerializeField] private float blockedMovementTimeout = 1f;

    private CharacterController activeController;
    private Behaviour activeMovement;   
    private Coroutine movementRoutine;
    private float verticalVelocity;
    private bool lastMoveSucceeded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void MovePlayerToSide(
        Transform playerRoot,
        CharacterController controller,
        Behaviour movementScript,
        TableSide targetSide)
    {
        if (playerRoot == null ||
            controller == null ||
            movementScript == null ||
            corners == null ||
            corners.Length != 4)
            return;

        foreach (Transform corner in corners)
        {
            if (corner == null)
                return;
        }

        Transform destination = targetSide == TableSide.Front
            ? frontStandPoint
            : backStandPoint;

        if (destination == null)
            return;

        if (movementRoutine != null)
            StopCoroutine(movementRoutine);

        activeController = controller;
        activeMovement = movementScript;
        verticalVelocity = 0f;

        int startIndex = GetClosestCornerIndex(playerRoot.position);
        List<Transform> bestRoute = null;
        float bestDistance = float.PositiveInfinity;

        for (int end = 0; end < corners.Length; end++)
        {
            for (int direction = -1; direction <= 1; direction += 2)
            {
                List<Transform> route = BuildCornerRoute(
                    startIndex,
                    end,
                    direction
                );

                float distance = CalculateRouteDistance(
                    playerRoot.position,
                    corners[startIndex].position,
                    route,
                    destination.position
                );

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestRoute = route;
                }
            }
        }

        if (bestRoute == null)
            return;

        activeMovement.enabled = false;

        movementRoutine = StartCoroutine(
            FollowPath(
                playerRoot,
                corners[startIndex],
                bestRoute,
                destination
            )
        );
    }

    private IEnumerator FollowPath(
        Transform playerRoot,
        Transform startCorner,
        List<Transform> route,
        Transform destination)
    {
        yield return MoveToPosition(playerRoot, startCorner.position);

        if (!lastMoveSucceeded)
        {
            FinishMovement();
            yield break;
        }

        foreach (Transform corner in route)
        {
            yield return MoveToPosition(playerRoot, corner.position);

            if (!lastMoveSucceeded)
            {
                FinishMovement();
                yield break;
            }
        }

        yield return MoveToPosition(playerRoot, destination.position);

        if (!lastMoveSucceeded)
        {
            FinishMovement();
            yield break;
        }

        float rotationTimeout = 5f;
        float elapsed = 0f;

        while (Quaternion.Angle(playerRoot.rotation, destination.rotation) > 0.5f
               && elapsed < rotationTimeout)
        {
            playerRoot.rotation = Quaternion.RotateTowards(
                playerRoot.rotation,
                destination.rotation,
                rotationSpeed * Time.deltaTime
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        FinishMovement();
    }

    private IEnumerator MoveToPosition(
        Transform playerRoot,
        Vector3 targetPosition)
    {
        if (activeController == null || !activeController.enabled)
        {
            lastMoveSucceeded = false;
            yield break;
        }

        float elapsed = 0f;
        float blockedTime = 0f;

        while (elapsed < waypointTimeout)
        {
            Vector3 position = playerRoot.position;

            if (HorizontalDistance(position, targetPosition)
                <= arrivalDistance + 0.001f)
            {
                lastMoveSucceeded = true;
                yield break;
            }

            Vector3 direction = targetPosition - position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.000001f)
            {
                direction.Normalize();

                Quaternion targetRotation = Quaternion.LookRotation(direction);

                playerRoot.rotation = Quaternion.RotateTowards(
                    playerRoot.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }

            if (activeController.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -2f;

            verticalVelocity += gravity * Time.deltaTime;

            Vector3 movement = direction * moveSpeed;
            movement.y = verticalVelocity;

            Vector3 beforeMove = playerRoot.position;

            activeController.Move(movement * Time.deltaTime);

            Vector3 actualMovement = playerRoot.position - beforeMove;
            actualMovement.y = 0f;

            if (direction.sqrMagnitude > 0f &&
                actualMovement.magnitude < 0.001f)
            {
                blockedTime += Time.deltaTime;

                if (blockedTime >= blockedMovementTimeout)
                {
                    lastMoveSucceeded = false;
                    yield break;
                }
            }
            else
            {
                blockedTime = 0f;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        lastMoveSucceeded = false;
    }

    private List<Transform> BuildCornerRoute(
        int startIndex,
        int destinationIndex,
        int direction)
    {
        List<Transform> route = new List<Transform>();
        int current = startIndex;

        while (current != destinationIndex)
        {
            current = (current + direction + corners.Length) % corners.Length;
            route.Add(corners[current]);
        }

        return route;
    }

    private float CalculateRouteDistance(
        Vector3 playerPosition,
        Vector3 startCorner,
        List<Transform> route,
        Vector3 destination)
    {
        float distance = HorizontalDistance(playerPosition, startCorner);
        Vector3 previous = startCorner;

        foreach (Transform corner in route)
        {
            distance += HorizontalDistance(previous, corner.position);
            previous = corner.position;
        }

        return distance + HorizontalDistance(previous, destination);
    }

    private int GetClosestCornerIndex(Vector3 position)
    {
        int closestIndex = 0;
        float closestDistance = float.PositiveInfinity;

        for (int i = 0; i < corners.Length; i++)
        {
            float distance = HorizontalDistance(position, corners[i].position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    private float HorizontalDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    private void FinishMovement()
    {
        movementRoutine = null;
    }

    public void ReleasePlayer()
    {
        if (movementRoutine != null)
        {
            StopCoroutine(movementRoutine);
            movementRoutine = null;
        }

        if (activeMovement != null)
            activeMovement.enabled = true;

        activeController = null;
        activeMovement = null;
        verticalVelocity = 0f;
    }
}

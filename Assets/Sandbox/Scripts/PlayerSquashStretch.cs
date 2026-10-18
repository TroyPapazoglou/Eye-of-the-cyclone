using PrimeTween;
using UnityEngine;

public class PlayerSquashStretch : MonoBehaviour
{
    private enum MovementState
    {
        Grounded,
        Jumping,
        Apex,
        Falling
    }

    [Header("References")]
    [SerializeField] private Transform squashRoot;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Stretch")]
    [SerializeField] private Vector3 jumpStretch = new Vector3(0.9f, 1.15f, 0.9f);
    [SerializeField] private Vector3 fallStretch = new Vector3(0.95f, 1.08f, 0.95f);

    [Header("Squash")]
    [SerializeField] private Vector3 landingSquash = new Vector3(1.15f, 0.8f, 1.15f);

    [Header("Timing")]
    [SerializeField] private float stretchDuration = 0.12f;
    [SerializeField] private float squashDuration = 0.08f;
    [SerializeField] private float returnDuration = 0.14f;

    [Header("Apex")]
    [SerializeField] private float apexVelocityThreshold = 0.5f;

    [Header("Tween")]
    [SerializeField] private Ease stretchEase = Ease.OutQuad;
    [SerializeField] private Ease squashEase = Ease.OutQuad;
    [SerializeField] private Ease returnEase = Ease.OutBack;

    private MovementState currentState;
    private bool hasBeenAirborne;
    private Sequence visualScaleSequence;
    private Sequence squashScaleSequence;

    private Vector3 originalVisualScale;
    private Vector3 originalSquashScale;

    private void Awake()
    {
        if (visualRoot == null)
            visualRoot = transform;

        originalVisualScale = visualRoot.localScale;
        originalSquashScale = squashRoot.localScale;
    }

    private void Start()
    {       
        currentState = MovementState.Grounded;
    }

    private void Update()
    {
        bool grounded = IsGrounded();
        float verticalVelocity = GetVerticalVelocity();

        if (!grounded)
        {
            if (!hasBeenAirborne)
            {
                hasBeenAirborne = true;
                ResetSquash();
            }

            HandleAirborne(verticalVelocity);
        }
        else if (hasBeenAirborne)
        {
            hasBeenAirborne = false;
            HandleLanding();
        }
    }

    private void HandleAirborne(float verticalVelocity)
    {
        if (verticalVelocity > apexVelocityThreshold)
        {
            if (currentState != MovementState.Jumping)
            {
                currentState = MovementState.Jumping;

                TweenScale(
                    jumpStretch,
                    stretchDuration,
                    stretchEase
                );
            }

            return;
        }

        if (verticalVelocity >= -apexVelocityThreshold)
        {
            if (currentState != MovementState.Apex)
            {
                currentState = MovementState.Apex;

                TweenScale(
                    originalVisualScale,
                    returnDuration,
                    returnEase
                );
            }

            return;
        }

        if (currentState != MovementState.Falling)
        {
            currentState = MovementState.Falling;

            TweenScale(
                fallStretch,
                stretchDuration,
                stretchEase
            );
        }
    }

    private void HandleLanding()
    {
        currentState = MovementState.Grounded;

        StopSquashTween();

        TweenScale(
            originalVisualScale,
            returnDuration,
            returnEase
        );

        squashScaleSequence = Sequence
            .Create(
                Tween.Scale(
                    squashRoot,
                    landingSquash,
                    squashDuration,
                    squashEase
                )
            )
            .Chain(
                Tween.Scale(
                    squashRoot,
                    originalSquashScale,
                    returnDuration,
                    returnEase
                )
            )
            .OnComplete(() =>
            {
                squashScaleSequence = default;
            });
    }

    private void TweenScale(Vector3 targetScale, float duration, Ease ease)
    {
        if (visualRoot == null)
            return;

        if (Vector3.Distance(visualRoot.localScale, targetScale) <= 0.001f)
        {
            return;
        }

        StopVisualScaleTween();

        visualScaleSequence = Sequence
       .Create(
           Tween.Scale(
               visualRoot,
               targetScale,
               duration,
               ease
           )
       );
    }

    private void StopVisualScaleTween()
    {
        visualScaleSequence.Stop();
        visualScaleSequence = default;
    }

    private void StopSquashTween()
    {
        squashScaleSequence.Stop();
        squashScaleSequence = default;
    }

    private void ResetSquash()
    {
        StopSquashTween();
        squashRoot.localScale = originalSquashScale;
    }

    private bool IsGrounded()
    {
        if (playerMovement == null)
            return false;

        return playerMovement.Grounded;
    }

    private float GetVerticalVelocity()
    {
        if (playerMovement == null)
            return 0f;

        return playerMovement.VerticalVelocity;
    }
}

using PrimeTween;
using System.Collections;
using UnityEngine;

public class HandController : MonoBehaviour
{
    public enum HandPose
    {
        Idle,
        Running,
        Jumping,
        Interacting,
        Punching,
        HoldingItem,
    }

    [Header("Hand References")]
    [SerializeField] private Transform leftHand;
    [SerializeField] private Transform rightHand;

    [Header("Animation")]
    [SerializeField] private Animator leftHandAnimator;
    [SerializeField] private Animator rightHandAnimator;

    [Header("Pose Targets")]
    [SerializeField] private HandPoseTarget idle;
    [SerializeField] private HandPoseTarget running;
    [SerializeField] private HandPoseTarget jumping;
    [SerializeField] private HandPoseTarget holdingItem;

    [Header("Movement")]
    [SerializeField] private float transitionDuration = 0.15f;
    [SerializeField] private Ease transitionEase = Ease.OutQuad;

    [Header("Movement Lag")]
    [SerializeField] private float movementLagDistance = 0.025f;
    [SerializeField] private float movementLagKickDuration = 0.04f;
    [SerializeField] private float movementLagReturnDuration = 0.12f;
    [SerializeField] private Ease movementLagKickEase = Ease.OutQuad;
    [SerializeField] private Ease movementLagReturnEase = Ease.OutQuad;

    [Header("Running Bob")]
    [SerializeField] private bool enableRunningBob = true;
    [SerializeField] private float runningBobAmount = 0.015f;
    [SerializeField] private float runningBobDuration = 0.18f;
    [SerializeField] private Ease runningBobEase = Ease.InOutSine;

    [Header("Idle Rotation Lag")]
    [SerializeField] private bool enableRotationLag = true;
    [SerializeField] private float rotationLagAmount = 0.08f;
    [SerializeField] private float rotationLagDuration = 0.12f;

    [Header("Jitter")]
    [SerializeField] private bool enableJitter = true;
    [SerializeField] private float jitterAmount = 0.01f;
    [SerializeField] private float jitterSpeed = 8f;

    private const float PositionEpsilon = 0.001f;
    private const float RotationEpsilon = 0.1f;

    private bool isHoldingItem;
    private bool isInteracting;
    private HandPose currentPose;
    private HandPose previousPose;
    private HandPose movementPose;

    private Sequence leftPositionSequence;
    private Tween leftRotationTween;

    private Sequence rightPositionSequence;
    private Tween rightRotationTween;

    private Tween leftInteractionPositionTween;
    private Tween rightInteractionPositionTween;

    private Tween leftInteractionRotationTween;
    private Tween rightInteractionRotationTween;

    private Coroutine runningBobCoroutine;

    private Tween leftBobTween;
    private Tween rightBobTween;

    private Quaternion previousPlayerRotation;

    private Coroutine interactionCoroutine;

    private static readonly int IdleHash = Animator.StringToHash("Idle");
    private static readonly int PokeHash = Animator.StringToHash("Poke");
    private static readonly int FistHash = Animator.StringToHash("Fist");

    private void Awake()
    {
        previousPlayerRotation = transform.rotation;
    }

    private void Start()
    {
        SetIdleImmediate();
    }

    private void LateUpdate()
    {
        if (!enableRotationLag)
            return;

        ApplyRotationLag();
    }

    // --------------------------------
    // POSE SWITCHING
    // --------------------------------

    public void SetIdle()
    {
        movementPose = HandPose.Idle;
        SetPose(HandPose.Idle);
    }

    public void SetRunning(Vector3 movementDirection)
    {
        movementPose = HandPose.Running;
        SetPose(HandPose.Running, movementDirection);
    }

    public void SetJumping()
    {
        movementPose = HandPose.Jumping;
        SetPose(HandPose.Jumping);
    }

    public void SetPose(HandPose newPose, Vector3 movementDirection = default)
    {
        if (isInteracting)
            return;

        if (isHoldingItem && newPose != HandPose.HoldingItem)
            return;

        if (currentPose == newPose)
            return;
             
        StopRunningBob();

        HandPoseTarget target = GetTarget(newPose);

        if (target == null)
            return;

        previousPose = currentPose;

        currentPose = newPose;

        bool enteringRunning = previousPose == HandPose.Idle && newPose == HandPose.Running && movementDirection.sqrMagnitude > 0.001f;

        MoveHandsToTarget(
            target,
            transitionDuration,
            enteringRunning,
            movementDirection
        );

        SetHandAnimation();
              
        if (newPose == HandPose.Running && enableRunningBob)
        {
            float movementDuration = enteringRunning ? movementLagKickDuration + movementLagReturnDuration : transitionDuration;

            runningBobCoroutine = StartCoroutine(StartRunningBobAfterDelay(movementDuration));
        }
    }

    private void SetHandAnimation()
    {
        switch (currentPose)
        {
            case HandPose.Idle:            
            case HandPose.Jumping:
            case HandPose.HoldingItem:
                leftHandAnimator.Play(IdleHash);
                rightHandAnimator.Play(IdleHash);
                break;
            case HandPose.Running:
                leftHandAnimator.Play(FistHash);
                rightHandAnimator.Play(FistHash);
                break;       

        }
    }

    public void SetHoldingItem(bool holding)
    {
        isHoldingItem = holding;

        if (holding)
        {
            SetPose(HandPose.HoldingItem);
            return;
        }

        SetPose(movementPose);
    }

    private int GetHashByHandPose( HandPose handPose )
    {
        switch (handPose)
        {
            case HandPose.Idle:
                return IdleHash;
            case HandPose.Jumping:
                return FistHash;
            case HandPose.Running:
                return FistHash;
            case HandPose.Interacting:
                return PokeHash;
            case HandPose.Punching:
                return FistHash;
            default:
                break;
        }
        return 0;
    }

    private void SetHandAnimationByHash(int hash, InteractionHand hand)
    {
        if (hand == InteractionHand.Left)
            leftHandAnimator.Play(hash);
        else
            rightHandAnimator.Play(hash);
    }

    private HandPoseTarget GetTarget(HandPose pose)
    {
        return pose switch
        {
            HandPose.Idle => idle,
            HandPose.Running => running,
            HandPose.Jumping => jumping,
            HandPose.HoldingItem => holdingItem,
            _ => null
        };
    }

    // --------------------------------
    // HAND MOVEMENT
    // --------------------------------

    private void MoveHandsToTarget(HandPoseTarget target, float duration, bool useMovementLag = false, Vector3 movementDirection = default)
    {
        if (target == null)
            return;

        MoveHandToTarget(
            leftHand,
            target.leftHandPoint,
            true,
            duration,
            useMovementLag,
            movementDirection
        );

        MoveHandToTarget(
            rightHand,
            target.rightHandPoint,
            false,
            duration,
            useMovementLag,
            movementDirection
        );
    }

    private void MoveHandToTarget(Transform hand,Transform target, bool isLeftHand, float duration, bool useMovementLag, Vector3 movementDirection)
    {
        if (hand == null || target == null)
            return;

        Vector3 targetPosition = GetHandTargetPosition(hand, target);

        Quaternion targetRotation = GetHandTargetRotation(hand, target);

        bool alreadyAtPosition = Vector3.Distance(hand.localPosition, targetPosition) <= PositionEpsilon;

        bool alreadyAtRotation = Quaternion.Angle(hand.localRotation, targetRotation) <= RotationEpsilon;

        StopHandTweens(isLeftHand ? InteractionHand.Left : InteractionHand.Right);

        // --------------------------------
        // POSITION
        // --------------------------------

        if (!alreadyAtPosition)
        {
            if (useMovementLag)
            {
                Vector3 localMovement = hand.parent.InverseTransformDirection(transform.TransformDirection(movementDirection));

                localMovement.y = 0f;

                if (localMovement.sqrMagnitude > 0.001f)
                    localMovement.Normalize();

                Vector3 lagPosition = targetPosition - localMovement * movementLagDistance;

                Sequence sequence = Sequence
                    .Create(
                        Tween.LocalPosition(
                            hand,
                            lagPosition,
                            movementLagKickDuration,
                            movementLagKickEase
                        )
                    )
                    .Chain(
                        Tween.LocalPosition(
                            hand,
                            targetPosition,
                            movementLagReturnDuration,
                            movementLagReturnEase
                        )
                    );

                if (isLeftHand)
                    leftPositionSequence = sequence;
                else
                    rightPositionSequence = sequence;
            }
            else
            {
                Sequence sequence = Sequence.Create(
                    Tween.LocalPosition(
                        hand,
                        targetPosition,
                        duration,
                        transitionEase
                    )
                );

                if (isLeftHand)
                    leftPositionSequence = sequence;
                else
                    rightPositionSequence = sequence;
            }
        }
        
        if (!alreadyAtRotation)
        {
            Tween rotationTween = Tween.LocalRotation(
                hand,
                targetRotation,
                duration,
                transitionEase
            );

            if (isLeftHand)
                leftRotationTween = rotationTween;
            else
                rightRotationTween = rotationTween;
        }
    }

    // --------------------------------
    // TARGET CONVERSION
    // --------------------------------

    private Vector3 GetHandTargetPosition(Transform hand, Transform target)
    {
        Transform handParent = hand.parent;

        /*
         * Convert the target from world space into the hand
         * parent's coordinate space without allowing the
         * squash/stretch scale to alter the target itself.
         */

        Vector3 position = Quaternion.Inverse(handParent.rotation) * (target.position - handParent.position);

        Vector3 scale = handParent.lossyScale;

        position.x /= scale.x;
        position.y /= scale.y;
        position.z /= scale.z;

        return position;
    }

    private Quaternion GetHandTargetRotation(Transform hand, Transform target)
    {
        return Quaternion.Inverse(hand.parent.rotation) * target.rotation;
    }

    // --------------------------------
    // INTERACTION
    // --------------------------------

    private void MoveHandToTarget(InteractionHand hand, HandPoseTarget target, float duration)
    {
        if (target == null)
            return;

        if (hand == InteractionHand.Left)
        {
            MoveHandToTarget(
                leftHand,
                target.leftHandPoint,
                true,
                duration,
                false,
                Vector3.zero
            );
        }
        else
        {
            MoveHandToTarget(
                rightHand,
                target.rightHandPoint,
                false,
                duration,
                false,
                Vector3.zero
            );
        }
    }

    private void SetIdleImmediate()
    {
        currentPose = HandPose.Idle;

        if (idle.leftHandPoint != null)
        {
            
            leftHand.SetLocalPositionAndRotation(GetHandTargetPosition(leftHand, idle.leftHandPoint), 
                GetHandTargetRotation(leftHand, idle.leftHandPoint
            ));
        }

        if (idle.rightHandPoint != null)
        {
            
            rightHand.SetLocalPositionAndRotation(GetHandTargetPosition(rightHand, idle.rightHandPoint),
                GetHandTargetRotation(rightHand, idle.rightHandPoint
            ));
        }
    }

    // --------------------------------
    // ROTATION LAG
    // --------------------------------

    private void ApplyRotationLag()
    {
        Quaternion currentRotation =
            transform.rotation;

        Quaternion rotationDelta =
            currentRotation *
            Quaternion.Inverse(previousPlayerRotation);

        previousPlayerRotation =
            currentRotation;

        if (rotationDelta == Quaternion.identity)
            return;

        Vector3 rotationEuler =
            rotationDelta.eulerAngles;

        if (rotationEuler.x > 180f)
            rotationEuler.x -= 360f;

        if (rotationEuler.y > 180f)
            rotationEuler.y -= 360f;

        if (rotationEuler.z > 180f)
            rotationEuler.z -= 360f;

        Vector3 lagOffset =
            new Vector3(
                0f,
                0f,
                -rotationEuler.y
            ) * rotationLagAmount;

        if (enableJitter)
        {
            float time =
                Time.time * jitterSpeed;

            lagOffset += new Vector3(
                Mathf.PerlinNoise(time, 0f) - 0.5f,
                Mathf.PerlinNoise(time, 1f) - 0.5f,
                Mathf.PerlinNoise(time, 2f) - 0.5f
            ) * jitterAmount;
        }

        leftHand.position =
            leftHand.position + lagOffset;

        rightHand.position =
            rightHand.position + lagOffset;
    }

    // --------------------------------
    // TWEEN STOPPING
    // --------------------------------

    private void StopHandTweens(
        InteractionHand hand)
    {
        if (hand == InteractionHand.Left)
        {
            leftPositionSequence.Stop();
            leftRotationTween.Stop();
        }
        else
        {
            rightPositionSequence.Stop();
            rightRotationTween.Stop();
        }
    }

    // --------------------------------
    // INTERACTION
    // --------------------------------

    public void PlayInteraction(NetworkInteractable interactable, InteractionHand hand, HandPose handPose)
    {
        if (interactable == null)
            return;

        if (isInteracting)
            return;

        isInteracting = true;

        if (interactionCoroutine != null)
            StopCoroutine(interactionCoroutine);

        StopHandTweens(hand);

        interactionCoroutine = StartCoroutine(InteractionRoutine(interactable, hand, handPose));
    }

    private IEnumerator InteractionRoutine(NetworkInteractable interactable, InteractionHand hand, HandPose handPose)
    {
        Transform selectedHand = hand == InteractionHand.Left ? leftHand : rightHand;

        StopHandTweens(hand);

        Vector3 interactionPosition = interactable.GetInteractionPosition(transform);

        Quaternion interactionRotation = interactable.GetInteractionRotation(transform);

        Tween positionTween =
            Tween.Position(
                selectedHand,
                interactionPosition,
                transitionDuration,
                transitionEase
            );

        Tween rotationTween =
            Tween.Rotation(
                selectedHand,
                interactionRotation,
                transitionDuration,
                transitionEase
            );

        if (hand == InteractionHand.Left)
        {
            leftInteractionPositionTween =
                positionTween;

            leftInteractionRotationTween =
                rotationTween;
        }
        else
        {
            rightInteractionPositionTween =
                positionTween;

            rightInteractionRotationTween =
                rotationTween;
        }

        yield return new WaitForSeconds(
            transitionDuration
        );

       
        SetHandAnimationByHash(GetHashByHandPose(handPose), hand);

        float pokeDuration = 0.15f;
        float elapsed = 0f;

        while (elapsed < pokeDuration)
        {
            selectedHand.SetPositionAndRotation(
                interactionPosition,
                interactionRotation
            );

            elapsed += Time.deltaTime;

            yield return null;
        }

        selectedHand.SetPositionAndRotation(
            interactionPosition,
            interactionRotation
        );

        SetHandAnimation();

        MoveHandToTarget(
            hand,
            GetCurrentPoseTarget(),
            transitionDuration
        );

        yield return new WaitForSeconds(
            transitionDuration
        );

        isInteracting = false;
        interactionCoroutine = null;
    }

    private IEnumerator StartRunningBobAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Make sure we are still running.
        if (currentPose != HandPose.Running || isInteracting)
            yield break;

        Vector3 leftBasePosition = leftHand.localPosition;
        Vector3 rightBasePosition = rightHand.localPosition;

        while (currentPose == HandPose.Running && !isInteracting)
        {
            // -------------------------
            // UP
            // -------------------------

            Vector3 leftUp =
                leftBasePosition +
                Vector3.up * runningBobAmount;

            Vector3 rightUp =
                rightBasePosition +
                Vector3.up * runningBobAmount;

            leftBobTween = Tween.LocalPosition(
                leftHand,
                leftUp,
                runningBobDuration,
                runningBobEase
            );

            rightBobTween = Tween.LocalPosition(
                rightHand,
                rightUp,
                runningBobDuration,
                runningBobEase
            );

            yield return new WaitForSeconds(runningBobDuration);

            if (currentPose != HandPose.Running || isInteracting)
                yield break;

            // -------------------------
            // DOWN
            // -------------------------

            Vector3 leftDown =
                leftBasePosition -
                Vector3.up * runningBobAmount;

            Vector3 rightDown =
                rightBasePosition -
                Vector3.up * runningBobAmount;

            leftBobTween = Tween.LocalPosition(
                leftHand,
                leftDown,
                runningBobDuration,
                runningBobEase
            );

            rightBobTween = Tween.LocalPosition(
                rightHand,
                rightDown,
                runningBobDuration,
                runningBobEase
            );

            yield return new WaitForSeconds(runningBobDuration);

            if (currentPose != HandPose.Running || isInteracting)
                yield break;

            // -------------------------
            // RETURN TO BASE
            // -------------------------

            leftBobTween = Tween.LocalPosition(
                leftHand,
                leftBasePosition,
                runningBobDuration,
                runningBobEase
            );

            rightBobTween = Tween.LocalPosition(
                rightHand,
                rightBasePosition,
                runningBobDuration,
                runningBobEase
            );

            yield return new WaitForSeconds(runningBobDuration);
        }
    }

    private void StopRunningBob()
    {
        if (runningBobCoroutine != null)
        {
            StopCoroutine(runningBobCoroutine);
            runningBobCoroutine = null;
        }

        leftBobTween.Stop();
        rightBobTween.Stop();

        leftBobTween = default;
        rightBobTween = default;
    }

    private Quaternion GetInteractionRotation(Transform handTarget)
    {
        return handTarget.rotation;
    }

    private Vector3 GetInteractionPosition(Transform handTarget)
    {
        return handTarget.position;
    }

    private HandPoseTarget GetCurrentPoseTarget()
    {
        return currentPose switch
        {
            HandPose.Idle => idle,
            HandPose.Running => running,
            HandPose.Jumping => jumping,
            _ => idle
        };
    }
}

using PrimeTween;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerDodgeType
{
    Side,
    Jump
}
public enum DodgeDirection
{
    Left,
    Right
}

public enum DodgeDirectionRestriction
{
    None,
    ForbidLeft,
    ForbidRight
}

public class CharacterAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform dodgeRoot;
    [SerializeField] private Transform characterModel;

    [Header("Idle Animations")]
    [SerializeField] private List<string> idleAnimations = new();

    [Header("Dodge Animations")]
    [SerializeField] private List<string> sideDodgeAnimations = new();
    [SerializeField] private List<string> jumpDodgeAnimations = new();
    [SerializeField] private List<string> duckDodgeAnimations = new();

    [Header("Dodge Movement")]
    [SerializeField] private float dodgeDistance = 0.15f;
    [SerializeField] private float dodgeMoveDuration = 0.35f;
    [SerializeField] private float dodgeReturnDuration = 0.35f;
    [SerializeField] private float dodgeHoldDuration = 0.15f;  

    [Header("Dodge Jitter")]
    [SerializeField] private float jitterAmount = 0.025f;
    [SerializeField] private float jitterDuration = 0.04f;

    private Sequence dodgeSequence;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    public static CharacterAnimationController Instance;

    private void Awake()
    {
        Instance = this;
        originalPosition = dodgeRoot.localPosition;
        originalRotation = dodgeRoot.localRotation;
    }

    public void DodgeLeft()
    {
        TryDodge(
            PlayerDodgeType.Side,
            DodgeDirection.Left
        );
    }

    public void DodgeRight()
    {
        TryDodge(
            PlayerDodgeType.Side,
            DodgeDirection.Right
        );
    }

    private void TryDodge(PlayerDodgeType dodgeType, DodgeDirection direction)
    {
        if (CombatManager.Instance == null)
            return;

        // Tell combat manager what the player attempted.
        CombatManager.Instance.RegisterPlayerReaction(dodgeType, direction);
    }

    public void PlayDodge(PlayerDodgeType dodgeType, DodgeDirection direction, System.Action onReturnHalfway = null)
    {
        switch (dodgeType)
        {
            case PlayerDodgeType.Side:
                PlaySideDodge(direction, onReturnHalfway);
                break;

            case PlayerDodgeType.Jump:
                PlayJumpDodge(onReturnHalfway);
                break;
        }
    }

    private void PlaySideDodge(DodgeDirection dodgeDirection, Action onReturnHalfway)
    {
       if (sideDodgeAnimations.Count == 0)
            return;

        dodgeSequence.Stop();

        originalPosition = dodgeRoot.localPosition;

        float rotationY = dodgeDirection == DodgeDirection.Right ? -90f : 90f;

        dodgeRoot.localRotation =
            Quaternion.Euler(
                originalRotation.eulerAngles.x,
                rotationY,
                originalRotation.eulerAngles.z
            );

        string dodgeAnimation = sideDodgeAnimations[UnityEngine.Random.Range(0, sideDodgeAnimations.Count)];

        animator.Play(dodgeAnimation);

        float directionMultiplier = dodgeDirection == DodgeDirection.Right ? 1f : -1f;

        Vector3 dodgePosition =
            originalPosition +
            new Vector3(
                dodgeDistance * directionMultiplier,
                0f,
                0f
            );

        dodgeSequence = Sequence.Create();

        dodgeSequence.Group(
            Tween.LocalPosition(
                dodgeRoot,
                dodgePosition,
                dodgeMoveDuration,
                ease: Ease.OutQuad
            )
        );

        AddJitter(dodgeSequence);

        dodgeSequence.ChainDelay(
            dodgeHoldDuration
        );

        dodgeSequence.Chain(
            Tween.LocalPosition(
                dodgeRoot,
                originalPosition,
                dodgeReturnDuration,
                ease: Ease.InOutQuad
            )
        );       

        dodgeSequence.Group(
            Tween.Delay(dodgeReturnDuration * 0.5f, () =>
            {
                onReturnHalfway?.Invoke();
            })
        );

        dodgeSequence.OnComplete(() =>
        {
            PlayRandomIdle();
        });
    }

    private void PlayJumpDodge(Action onReturnHalfway)
    {
        if (jumpDodgeAnimations.Count == 0)
            return;

        dodgeSequence.Stop();

        originalPosition = dodgeRoot.localPosition;     
           
        string dodgeAnimation = jumpDodgeAnimations[UnityEngine.Random.Range(0, jumpDodgeAnimations.Count)];

        animator.Play(dodgeAnimation);

        float directionMultiplier = 1f;

        Vector3 dodgePosition =
            originalPosition +
            new Vector3(
                0f,
                dodgeDistance * directionMultiplier,
                0f
            );

        dodgeSequence = Sequence.Create();

        dodgeSequence.Group(
            Tween.LocalPosition(
                dodgeRoot,
                dodgePosition,
                dodgeMoveDuration,
                ease: Ease.OutQuad
            )
        );

        AddJitter(dodgeSequence);

        dodgeSequence.ChainDelay(
            dodgeHoldDuration
        );

        dodgeSequence.Chain(
            Tween.LocalPosition(
                dodgeRoot,
                originalPosition,
                dodgeReturnDuration,
                ease: Ease.InOutQuad
            )
        );

        dodgeSequence.Group(
            Tween.Delay(dodgeReturnDuration * 0.5f, () =>
            {
                onReturnHalfway?.Invoke();
            })
        );

        dodgeSequence.OnComplete(() =>
        {
            PlayRandomIdle();
        });
    }

  
    private void AddJitter(Sequence sequence)
    {
        sequence.Group(
            Tween.LocalPosition(
                characterModel,
                originalPosition +
                new Vector3(-jitterAmount, 0f, 0f),
                jitterDuration,
                ease: Ease.OutQuad
            )
        );

        sequence.Chain(
            Tween.LocalPosition(
                characterModel,
                originalPosition +
                new Vector3(jitterAmount, 0f, 0f),
                jitterDuration * 2f,
                ease: Ease.InOutQuad
            )
        );

        sequence.Chain(
            Tween.LocalPosition(
                characterModel,
                originalPosition +
                new Vector3(-jitterAmount, 0f, 0f),
                jitterDuration * 2f,
                ease: Ease.InOutQuad
            )
        );

        sequence.Chain(
            Tween.LocalPosition(
                characterModel,
                originalPosition,
                jitterDuration,
                ease: Ease.OutQuad
            )
        );
    }

    public void PlayRandomIdle()
    {
        if (idleAnimations.Count == 0)
            return;

        dodgeRoot.rotation = originalRotation;

        string idleAnimation =
            idleAnimations[
                UnityEngine.Random.Range(0, idleAnimations.Count)
            ];

        animator.Play(idleAnimation);
    }

    private void OnDestroy()
    {
        dodgeSequence.Stop();
    }
}

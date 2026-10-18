using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyAttackType
{
    Punch,
    Sweep
}

[System.Serializable]
public class EnemyAttack
{
    public EnemyAttackType attackType;

    [Tooltip("Animation state name in the Animator.")]
    public string animationName;

    [Tooltip("What the player needs to do to dodge this attack.")]
    public PlayerDodgeType requiredDodge;

    [Tooltip("Optional direction restriction for the dodge.")]
    public DodgeDirectionRestriction directionRestriction;    

    [Tooltip("How long the attack pauses while the player reacts.")]
    public float reactionPauseDuration = 0.6f;
}

public class EnemyAnimationController : MonoBehaviour
{
  

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterAnimationController playerAnimator;

    [Header("Idle Animations")]
    [SerializeField] private List<string> idleAnimations = new();

    [Header("Attacks")]
    [SerializeField] private List<EnemyAttack> attacks = new();

    [Header("Testing")]
    [SerializeField] private float delayBeforeAttack = 2f;
    [SerializeField] private float delayAfterAttack = 2f;

    private Coroutine testRoutine;
    private Coroutine reactionPauseRoutine;

    private EnemyAttack currentAttack;

    private bool playerHasReacted;
    private bool attackResolved;

    private PlayerDodgeType registeredDodgeType;
    private DodgeDirection registeredDodgeDirection;

    public EnemyAttack CurrentAttack => currentAttack;


    private void Start()
    {
        PlayRandomIdle();

        testRoutine = StartCoroutine(TestAttackRoutine());
    }

    private IEnumerator TestAttackRoutine()
    {
        yield return new WaitForSeconds(1f);

        while (true)
        {
            if (attacks.Count == 0)
                yield break;

            currentAttack = attacks[Random.Range(0, attacks.Count)];

            yield return new WaitForSeconds(delayBeforeAttack);

            StartAttack(currentAttack);           

            yield return new WaitUntil(
                () => attackResolved
            );

            yield return new WaitForSeconds(
                delayAfterAttack
            );
        }
    }

    private void StartAttack(EnemyAttack attack)
    {
        if (attack == null)
            return;

        playerHasReacted = false;
        attackResolved = false;

        registeredDodgeType = default;
        registeredDodgeDirection = default;

        Debug.Log($"Enemy attacks with {attack.attackType}");

        if (!string.IsNullOrEmpty(attack.animationName))
        {
            animator.Play(attack.animationName);
        }
    }

    // Call this from the animation when the reaction window begins.
    public void BeginReactionPause()
    {
        if (currentAttack == null)
            return;

        if (reactionPauseRoutine != null)
            StopCoroutine(reactionPauseRoutine);

        reactionPauseRoutine =
            StartCoroutine(ReactionPauseRoutine());
    }

    private IEnumerator ReactionPauseRoutine()
    {
        // Freeze the enemy animation.
        animator.speed = 0f;

        Debug.Log($"Reaction window started for " + $"{currentAttack.reactionPauseDuration}s");

        CombatManager.Instance.BeginReactionWindow(currentAttack);

        // Wait in real time.
        yield return new WaitForSecondsRealtime(currentAttack.reactionPauseDuration);

        // Automatically end reaction window.
        CombatManager.Instance.EndReactionWindow();

        // Resume enemy animation.
        animator.speed = 1f;

        Debug.Log("Reaction window ended.");

        reactionPauseRoutine = null;
    }

    // Call this from the animation at the exact hit frame.
    public void ResolveAttack()
    {
        if (attackResolved)
            return;

        attackResolved = true;

        CombatManager.Instance.EndReactionWindow();

        if (!playerHasReacted)
        {
            Debug.Log("Player failed to react.");

            CombatManager.Instance.PlayerHit(currentAttack);

            return;
        }

        Debug.Log(
            $"Player successfully reacted with " +
            $"{registeredDodgeType} " +
            $"{registeredDodgeDirection}"
        );

        // IMPORTANT:
        // The dodge animation happens HERE,
        // at the actual attack/hit point.
        if (playerAnimator != null)
        {
            playerAnimator.PlayDodge(registeredDodgeType, registeredDodgeDirection, PlayRandomIdle);
        }

        CombatManager.Instance.ResolvePlayerReaction();
    }

    public void RegisterPlayerReaction(PlayerDodgeType dodgeType, DodgeDirection direction)
    {
        if (!CombatManager.Instance.IsReactionWindowActive)
            return;

        registeredDodgeType = dodgeType;
        registeredDodgeDirection = direction;

        playerHasReacted = true;

        // Player reacted early, so immediately end the pause.
        if (reactionPauseRoutine != null)
        {
            StopCoroutine(reactionPauseRoutine);
            reactionPauseRoutine = null;
        }

        // Resume the enemy animation immediately.
        animator.speed = 1f;

        // Reaction window is no longer active.
        CombatManager.Instance.EndReactionWindow();

        Debug.Log("Player reaction registered - enemy animation resumed.");
    }

    public void PlayRandomIdle()
    {
        if (idleAnimations.Count == 0)
            return;

        string idleAnimation =
            idleAnimations[
                Random.Range(0, idleAnimations.Count)
            ];

        animator.Play(idleAnimation);
    }

    public void PerformAttack(EnemyAttack attack)
    {
        if (attack == null)
            return;

        currentAttack = attack;

        StartAttack(currentAttack);
    }


    private void OnDestroy()
    {
        if (testRoutine != null)
            StopCoroutine(testRoutine);

        if (reactionPauseRoutine != null)
            StopCoroutine(reactionPauseRoutine);

        if (animator != null)
            animator.speed = 1f;
    }
}

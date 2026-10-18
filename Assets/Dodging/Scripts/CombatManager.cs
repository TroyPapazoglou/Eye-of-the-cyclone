using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private EnemyAnimationController enemy;

    public bool IsReactionWindowActive { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void BeginReactionWindow(EnemyAttack attack)
    {
        IsReactionWindowActive = true;

        Debug.Log(
            $"Reaction window active. " +
            $"Required dodge: {attack.requiredDodge}"
        );
    }

    public void EndReactionWindow()
    {
        IsReactionWindowActive = false;

        Debug.Log("Reaction window inactive.");
    }

    // =========================================================
    // PLAYER REACTION
    // =========================================================

    public void RegisterPlayerReaction(PlayerDodgeType dodgeType, DodgeDirection direction)
    {
        if (!IsReactionWindowActive)
            return;

        if (enemy == null)
            return;

        EnemyAttack attack = enemy.CurrentAttack;

        if (attack == null)
            return;

        // -----------------------------------------------------
        // Check dodge type
        // -----------------------------------------------------

        if (dodgeType != attack.requiredDodge)
        {
            Debug.Log(
                $"Wrong dodge! " +
                $"Required: {attack.requiredDodge}, " +
                $"Used: {dodgeType}"
            );

            return;
        }

        // -----------------------------------------------------
        // Check direction
        // -----------------------------------------------------

        if (!IsDirectionValid(attack.directionRestriction, direction))
        {
            Debug.Log(
                $"Wrong dodge direction: {direction}"
            );

            return;
        }

        // -----------------------------------------------------
        // Valid reaction
        // -----------------------------------------------------

        enemy.RegisterPlayerReaction(dodgeType, direction);

        Debug.Log(
            $"Successful reaction: " +
            $"{dodgeType} {direction}"
        );
    }

    private bool IsDirectionValid(DodgeDirectionRestriction restriction, DodgeDirection direction)
    {
        switch (restriction)
        {
            case DodgeDirectionRestriction.ForbidLeft:
                return direction != DodgeDirection.Left;

            case DodgeDirectionRestriction.ForbidRight:
                return direction != DodgeDirection.Right;

            case DodgeDirectionRestriction.None:
            default:
                return true;
        }
    }

    // =========================================================
    // COMBAT RESULT
    // =========================================================

    public void PlayerHit(EnemyAttack attack)
    {
        Debug.Log(
            $"PLAYER HIT by {attack.attackType}"
        );

        // Later:
        // player health
        // damage
        // hit animation
        // stun
        // etc.
    }

    public void ResolvePlayerReaction()
    {
        Debug.Log("Player successfully avoided attack.");

        // Later:
        // successful dodge effects
        // stamina
        // score
        // combo
        // etc.
    }
}

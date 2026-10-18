using UnityEngine;

public class CycloneUserUI : MonoBehaviour
{
    [SerializeField] private CycloneUIAnimator cycloneUIAnimator;

    public UserUI UserUI { get; private set; }

    private void Awake()
    {
        UserUI = GetComponent<UserUI>();
    }

    public void Initialise(PlayerNetworkObject player)
    {
        UserUI.Initialise(player);
    }

    public void PlayerDrewCards(CardInfo[] cards)
    {
        cycloneUIAnimator.PlayTurnAnimation(cards);
    }

    public void PlayerPlayedCard(CardInfo playedCard)
    {
        cycloneUIAnimator.PlayPlacedCard(playedCard);
    }

    public void PlayerDiscardCard(CardInfo discardedCard)
    {
        cycloneUIAnimator.DiscardCardAnimation(discardedCard);
    }

    public void PlayerGainedBackup(CardInfo backupGained)
    {
        cycloneUIAnimator.MoveCardToBackup(backupGained);
    }

    public void PlayerPlayedBackup(CardInfo backupPlayed)
    {
        cycloneUIAnimator.PlayBackup(backupPlayed);
    }

    public void PlayerDiscardedBackup(CardInfo discardedBackup)
    {
        cycloneUIAnimator.DiscardBackup(discardedBackup);
    }
}

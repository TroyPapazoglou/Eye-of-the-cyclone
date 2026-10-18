using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CyclonePlayerNetwork : NetworkBehaviour
{
    private PlayerNetworkObject player; 

    public List<CardInfo> handCards = new List<CardInfo>(); 

    public CardInfo? backupCard;

    private void Awake() 
    { 
        player = GetComponent<PlayerNetworkObject>(); 
    }

    public override void OnStartLocalPlayer()
    {
        CardManager.OnFirstCardPlaced += CmdFinishedFirstCardAnimation;
        CardManager.OnPlayerCardPlaced += CmdPlayerCardPlaced;
        CardManager.OnPlayerCardDiscarded += CmdPlayerCardDiscarded;
        CardManager.OnPlayerBackupPlaced += CmdPlayerBackupPlaced;
        CardManager.OnPlayerBackupDiscarded += CmdPlayerBackupDiscarded;
        CardManager.OnPlayerFinishedTurn += CmdPlayerFinishedTurn;
        CardManager.OnPlayerDrawingCards += CmdPlayerDrewCards;
        CardManager.OnPlayerCardsFinishedReturning += CmdFinishedShuffleAnimation;
    }

    public override void OnStopClient()
    {
        CardManager.OnFirstCardPlaced -= CmdFinishedFirstCardAnimation;
        CardManager.OnPlayerCardPlaced -= CmdPlayerCardPlaced;
        CardManager.OnPlayerCardDiscarded -= CmdPlayerCardDiscarded;
        CardManager.OnPlayerBackupPlaced -= CmdPlayerBackupPlaced;
        CardManager.OnPlayerBackupDiscarded -= CmdPlayerBackupDiscarded;
        CardManager.OnPlayerFinishedTurn -= CmdPlayerFinishedTurn;
        CardManager.OnPlayerDrawingCards -= CmdPlayerDrewCards;
        CardManager.OnPlayerCardsFinishedReturning -= CmdFinishedShuffleAnimation;
    }

    // ========================================================= 
    // TARGET RPCs //
    // =========================================================

    [TargetRpc]
    public void TargetStartTurn(NetworkConnection target, PlayerNetworkObject player, CardInfo[] cards)
    {
        handCards = cards.ToList();
        CardManager.Instance.DrawCards(cards);
        CycloneScreenUIManager.Instance.PlayerDrewCards(player.netId, cards);
    }

    [TargetRpc]
    public void TargetOpponentDrewCards(NetworkConnection target, int playerId, CardInfo[] drawnCards)
    {
        Debug.Log($"Player {playerId} drew cards.");
        StartCoroutine(CardManager.Instance.OpponentDrawCards(drawnCards));
        CycloneScreenUIManager.Instance.PlayerDrewCards(netId, drawnCards);
    }

    [TargetRpc]
    public void TargetOpponentPlacedCard(NetworkConnection target, int playerId, CardInfo cardPlaced, CardInfo activeCard)
    {
        Debug.Log($"Player {playerId} placed a card.");

        CardManager.Instance.OpponentPlacedCard(cardPlaced, activeCard);
        CycloneScreenUIManager.Instance.PlayerPlacedCard(netId, cardPlaced);
    }

    [TargetRpc]
    public void TargetOpponentDiscardedCard(NetworkConnection target, int playerId, CardInfo cardDiscarded)
    {
        Debug.Log($"Player {playerId} discarded a card.");

        CardManager.Instance.OpponentDiscardedCard(cardDiscarded);
        CycloneScreenUIManager.Instance.PlayerDiscardCard(netId, cardDiscarded);
    }

    [TargetRpc]
    public void TargetOpponentPlacedBackup(NetworkConnection target, int playerId, CardInfo backupPlaced, CardInfo previousActiveCard)
    {
        Debug.Log($"Player {playerId} placed a backup.");

        CardManager.Instance.OpponentPlacedBackup(backupPlaced, previousActiveCard);
        CycloneScreenUIManager.Instance.PlayerPlacedBackup(netId, backupPlaced);
    }

    [TargetRpc]
    public void TargetOpponentDiscardedBackup(NetworkConnection target, int playerId, CardInfo backupDiscarded)
    {
        Debug.Log($"Player {playerId} discarded a backup.");

        CardManager.Instance.OpponentDiscardedBackup(backupDiscarded);
        CycloneScreenUIManager.Instance.PlayerDiscardedBackup(netId, backupDiscarded);
    }

    // ========================================================= 
    // TEST RPCs //
    // =========================================================

    [TargetRpc]

    public void TESTTargetOpponentDrewCards(CardInfo[] drawnCards, CardInfo activeCard)
    {
        StartCoroutine(CardManager.Instance.TESTOpponentDrawCards(drawnCards, () => { TESTTargetOpponentPlacedCard(drawnCards[0], drawnCards[1], activeCard); }));
    }

    public void TESTTargetOpponentPlacedCard(CardInfo cardPlaced, CardInfo cardDiscarded, CardInfo activeCard)
    {
        CardManager.Instance.OpponentDiscardedCard(cardDiscarded);
        CardManager.Instance.OpponentPlacedCard(cardPlaced, activeCard);
    }

    public void TESTTargetOpponentPlacedBackup(CardInfo cardPlaced, CardInfo activeCard)
    {
        CardManager.Instance.OpponentPlacedBackup(cardPlaced, activeCard);
    }

    // ========================================================= 
    // COMMANDS //
    // =========================================================

    [Command]
    public void CmdPlayerFinishedTurn()
    {
        CycloneGameManager.Instance.NotifyServerOfPlayerTurn(connectionToClient);
    }

    [Command]
    public void CmdPlayerBackupDiscarded(CardInfo backupDiscarded)
    {
        CycloneGameManager.Instance.NotifyOtherPlayersOfBackupDiscarded(connectionToClient, backupDiscarded);
    }

    [Command]
    public void CmdPlayerBackupPlaced(CardInfo backupPlaced, CardInfo previousActiveCard)
    {
        CycloneGameManager.Instance.NotifyOtherPlayersOfBackupPlaced(connectionToClient, backupPlaced, previousActiveCard);
    }

    [Command]
    public void CmdPlayerCardDiscarded(CardInfo cardDiscarded)
    {
        CycloneGameManager.Instance.NotifyOtherPlayersOfCardDiscarded(connectionToClient, cardDiscarded);
    }

    [Command]
    public void CmdPlayerCardPlaced(CardInfo cardPlaced, CardInfo previousActiveCard)
    {
        CycloneGameManager.Instance.NotifyOtherPlayersOfCardPlaced(connectionToClient, cardPlaced, previousActiveCard);
    }

    [Command]
    public void CmdPlayerDrewCards(CardInfo[] drawnCards)
    {
        CycloneGameManager.Instance.NotifyOtherPlayersOfDraw(connectionToClient, drawnCards);
    }

    [Command]
    public void CmdFinishedFirstCardAnimation()
    {
        Debug.Log("Command sent");
        CycloneGameManager.Instance.PlayerFinishedFirstCardAnimation(connectionToClient);
    }

    [Command]
    public void CmdFinishedShuffleAnimation()
    {
        Debug.Log("Command sent");
        CycloneGameManager.Instance.PlayerFinishedShuffleAnimation(connectionToClient);
    }

    [Command]
    public void CmdPlayCard(CardInfo playedCard)
    {
        CycloneGameManager.Instance.ServerPlayCard(player, playedCard);
    }

    // ========================================================= 
    // CARD STATE //
    // =========================================================

    [Server]
    public void ResetPlayerForNewGame()
    {        

        handCards.Clear();

        backupCard = null;
    }

    public void ClearBackupCard()
    {
        backupCard = null;
    }

    public void SetBackupCard(CardInfo newBackup)
    {
        backupCard = newBackup;
    }

    public CardInfo? GetOtherHandCard(CardInfo played)
    {
        if (handCards == null || handCards.Count == 0)
            return null;

        foreach (CardInfo card in handCards)
        {
            if (!card.Equals(played))
                return card;
        }

        return null;
    }

    public CardInfo? TakeOtherHandCard(CardInfo played)
    {
        if (handCards == null || handCards.Count == 0)
            return null;

        for (int i = 0; i < handCards.Count; i++)
        {
            if (!handCards[i].Equals(played))
            {
                CardInfo card = handCards[i];

                handCards.RemoveAt(i);

                return card;
            }
        }

        return null;
    }

    public void RemoveCard(CardInfo card)
    {
        handCards.Remove(card);
    }

    public void DiscardBackupCard()
    {
        backupCard = null;
    }

    public void DiscardToPile(CardInfo card)
    {
        handCards.Remove(card);
    }

    public bool HasBackupCard()
    {
        return backupCard.HasValue;
    }

    public CardInfo? GetBackupCard()
    {
        return backupCard;
    }

    public bool TryGetBackupCard(out CardInfo card)
    {
        if (backupCard.HasValue)
        {
            card = backupCard.Value;
            return true;
        }

        card = default;
        return false;
    }

    

}

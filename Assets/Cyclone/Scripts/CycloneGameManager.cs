using Mirror;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class CycloneGameManager : NetworkBehaviour, IPCApplication
{
    [SerializeField] private CardManager cardManager;

    private List<CardInfo> deck = new();
    private readonly List<CardInfo> discardPile = new();
    private readonly List<CardInfo> cyclonePile = new();
    
    private int currentPlayerIndex;

    public List<PlayerNetworkObject> players = new();

    private HashSet<NetworkConnectionToClient> playersFinishedFirstCard = new();
    private HashSet<NetworkConnectionToClient> playersFinishedShuffling = new();

    public static CycloneGameManager Instance { get; private set; }

    public static event Action<int> OnCycloneCountUpdated;
    public static event Action<int> OnCyclonePileCountUpdated;
    public static event Action<int> OnDiscardPileCountUpdated;
    public static event Action<PlayerNetworkObject> OnGameEnd;
    public static event Action OnGameStart;
   

    [SyncVar(hook = nameof(OnCycloneCountChanged))]
    private int cycloneCount;
    public int CycloneCount => cycloneCount;

    [SyncVar(hook = nameof(OnCyclonePileCountChanged))]
    private int cyclonePileCount = 0;
    public int CyclonePileCount => cyclonePileCount;

    [SyncVar(hook = nameof(OnDiscardPileCountChanged))]
    private int discardPileCount = 0;

    public int DiscardPileCount => discardPileCount;

    private CardInfo? activeCard;

    public CardInfo? ActiveCard => activeCard;

    private HashSet<int> readyPlayers = new();  

    private bool hasGameStarted;

#pragma warning disable CS0414
    [SyncVar(hook = nameof(OnGameStateChangedSync))]
    private bool isGameActive;
#pragma warning restore CS0414

    public event Action<bool> OnGameStateChanged;

    private int cycloneLimit = 75;



    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        OnGameEnd += EndGame;          
    }

    private void OnDisable()
    {
        OnGameEnd -= EndGame;   
    }

    public override void OnStartServer()
    {
        base.OnStartClient();

        if (PCBehaviour.Instance.ApplicationOpen == PCApplication.EyeOfCyclone)
        {
            PCBehaviour.Instance.RegisterApplication(this);
        }
    }


    private void Start()
    {
        SteamLobbyManager.Instance.OnLobbyClosed += ServerResetGame;
        SteamLobbyManager.Instance.OnLeftLobby += ClientResetVisuals;
        PCNetworkManager.OnDisconnected += ClientResetVisuals;
        PCScreenUIManager.Instance.OnWinnerPresented += (() => isGameActive = false);
    }

    private void OnDestroy()
    {
        SteamLobbyManager.Instance.OnLobbyClosed -= ServerResetGame;
        SteamLobbyManager.Instance.OnLeftLobby -= ClientResetVisuals;
        PCNetworkManager.OnDisconnected -= ClientResetVisuals;
        PCScreenUIManager.Instance.OnWinnerPresented -= (() => isGameActive = false);
    }

    private void OnDiscardPileCountChanged(int oldValue, int newValue)
    {
        OnDiscardPileCountUpdated?.Invoke(newValue);
    }

    private void OnCyclonePileCountChanged(int oldValue, int newValue)
    {
        OnCyclonePileCountUpdated?.Invoke(newValue);
    }

    private void OnCycloneCountChanged(int oldValue, int newValue)
    {
        OnCycloneCountUpdated?.Invoke(newValue);
    }

    private void OnGameStateChangedSync(bool oldValue, bool newValue)
    {
        OnGameStateChanged?.Invoke(newValue);
    }

    [Server]
    public void SetActiveCard(CardInfo card)
    {
        Debug.Log(card.ToString());
        activeCard = card;
    }

    [Server]
    public void ResolvePlayedCard(PlayerNetworkObject player, CardInfo playedCard)
    {
        if (activeCard == null)
        {
            cycloneCount = (int)playedCard.rank;
        }
        else
        {
            bool sameRank =
                playedCard.rank == activeCard.Value.rank;

            if (sameRank)
            {
                int amount = (int)playedCard.rank;

                cycloneCount = Mathf.Max(cycloneCount - amount, 0);
                              
                PlayerStatsNetwork.Instance.TargetRecordCycloneDecremented(player.connectionToClient, amount);
            }
            else
            {
                int amount = (int)playedCard.rank;

                cycloneCount += amount;

                PlayerStatsNetwork.Instance.TargetRecordCycloneIncremented(player.connectionToClient, amount);
            }
        }

        activeCard = playedCard;

    }

    [Server]
    public void IncrementDiscardPileCount(CardInfo discardedCard)
    {
        discardPileCount++;
        discardPile.Add(discardedCard);
    }

    [Server]
    public void IncrementCyclonePileCount(CardInfo playedCard)
    {
        cyclonePileCount++;
        cyclonePile.Add(playedCard);
    }

    [Server]
    public void SetCycloneCount(int newValue)
    {
        cycloneCount = newValue;
    }

    [Server]
    public void ResetCycloneCount()
    {
        cycloneCount = 0;
    }

    [Server]
    public void ResetDiscardPileCount()
    {
        discardPileCount = 0;
    }

    [Server]
    public void ResetCyclonePileCount()
    {
        cyclonePileCount = 0;
    }

    [Server]
    public void ServerGenerateDeck()
    {
        deck = cardManager.GenerateDeck();

        CardInfo startingCard = deck.Last();

        SetCycloneCount((int)startingCard.rank);
        SetActiveCard(startingCard);

        RpcSpawnDeck(deck.ToArray(), startingCard);
               
        deck.RemoveAt(deck.Count - 1);
        cyclonePile.Add(startingCard);
    }

    private void ReshuffleDeck()
    {
        List<CardInfo> oldDeck = new();
        List<CardInfo> newDeck = new();

        oldDeck.AddRange(discardPile);
        oldDeck.AddRange(cyclonePile);        

        newDeck.AddRange(discardPile);
        newDeck.AddRange(cyclonePile);

        if (deck.Count > 0)
        {
            oldDeck.AddRange(deck);
            newDeck.AddRange(deck);
        }

        CardManager.Instance.Shuffle(newDeck);
        deck = newDeck;

        RpcAnimateReshuffle(oldDeck.ToArray(), newDeck.ToArray());         

        discardPile.Clear();
        cyclonePile.Clear();

        ResetCycloneCount();
        ResetCyclonePileCount();
        ResetDiscardPileCount();
    }

    public List<CardInfo> Draw(int count)
    {
        if (deck.Count < count && deck.Count < 1)
        {
            //Debug.Log("shuffling a new deck");
            ReshuffleDeck();            
        }

        //Debug.Log("deck count is: " + deck.Count);

        List<CardInfo> drawn = new();

        for (int i = 0; i < count && deck.Count > 0; i++)
        {
            CardInfo card = deck[^1];

            deck.RemoveAt(deck.Count - 1);

            drawn.Add(card);
        }

        return drawn;
    }

    [Server]
    public void StartTurn()
    {
        currentPlayerIndex = UnityEngine.Random.Range(0, players.Count);

        PlayerNetworkObject player = players[currentPlayerIndex];

        List<CardInfo> drawn = Draw(2);

        player.Cyclone.handCards.Clear();
        player.Cyclone.handCards.AddRange(drawn);

        player.Cyclone.TargetStartTurn(player.connectionToClient, player, drawn.ToArray());
    }

    [Server]
    private void HandleFirstCard()
    {
        CardInfo startingCard = deck.Last();

        SetCycloneCount((int)startingCard.rank);
        IncrementCyclonePileCount(startingCard);
        SetActiveCard(startingCard);

        deck.RemoveAt(deck.Count - 1);
        cyclonePile.Add(startingCard);

    }

    [Server]
    public void PlayerFinishedFirstCardAnimation(NetworkConnectionToClient connectionId)
    {
        playersFinishedFirstCard.Add(connectionId);     

        if(playersFinishedFirstCard.Count == players.Count)
        {
            playersFinishedFirstCard.Clear();       

            StartTurn();
        }
    }

    [Server]
    public void PlayerFinishedShuffleAnimation(NetworkConnectionToClient connectionId)
    {
        playersFinishedShuffling.Add(connectionId);

        //Debug.Log($"Player {connectionId} shuffled.");

        if(playersFinishedShuffling.Count == players.Count)
        {
            playersFinishedShuffling.Clear();

            //Debug.Log("Everyone shuffled!");          

            NotifyPlayersToPlaceFirstCard(deck.Last());

            HandleFirstCard();       
            

        }
    }


    [Server]
    public void NotifyServerOfPlayerTurn(NetworkConnectionToClient sender)
    {
        PlayerNetworkObject currentPlayer = players[currentPlayerIndex];

        if (currentPlayer.connectionToClient != sender)
        {
            Debug.LogWarning("Not your turn!");
            return;
        }

        FinishTurn();
    }

    [Server]
    private void FinishTurn()
    {       
        if (CheckGameEnd())
            return;
               
        if (deck.Count > 0)
        {
            AdvanceTurn();
        }
        else
        {
            ReshuffleDeck();
        }
    }

    [Server]
    public void AdvanceTurn()
    {                     

        do
        {
            currentPlayerIndex++;

            if (currentPlayerIndex >= players.Count)
                currentPlayerIndex = 0;

        } while (players[currentPlayerIndex].isOut);

        PlayerNetworkObject nextPlayer = players[currentPlayerIndex];

        List<CardInfo> cards = Draw(2);

        nextPlayer.Cyclone.handCards = cards;

        nextPlayer.Cyclone.TargetStartTurn(nextPlayer.connectionToClient, nextPlayer, cards.ToArray());

        //Debug.Log($"Player {currentPlayerIndex}'s turn");
    }

    [Server]
    public void NotifyOtherPlayersOfBackupDiscarded(NetworkConnectionToClient sender, CardInfo backupDiscarded)
    {
        foreach (PlayerNetworkObject player in players)
        {
            if (player.connectionToClient == sender)
                continue;

            player.Cyclone.TargetOpponentDiscardedBackup(
                player.connectionToClient,
                sender.connectionId,
                backupDiscarded);


        }
    }

    [Server]
    public void NotifyOtherPlayersOfBackupPlaced(NetworkConnectionToClient sender, CardInfo backupPlaced, CardInfo previousActiveCard)
    {
        foreach (PlayerNetworkObject player in players)
        {
            if (player.connectionToClient == sender)
                continue;

            player.Cyclone.TargetOpponentPlacedBackup(
                player.connectionToClient,
                sender.connectionId,
                backupPlaced,
                previousActiveCard);


        }
    }

    [Server]
    public void NotifyOtherPlayersOfCardDiscarded(NetworkConnectionToClient sender, CardInfo cardDiscarded)
    {
        foreach (PlayerNetworkObject player in players)
        {
            if (player.connectionToClient == sender)
                continue;

            player.Cyclone.TargetOpponentDiscardedCard(
                player.connectionToClient,
                sender.connectionId,
                cardDiscarded);

            
        }
    }
    
    [Server]
    public void NotifyOtherPlayersOfCardPlaced(NetworkConnectionToClient sender, CardInfo cardPlaced, CardInfo previousActiveCard)
    {        
        foreach (PlayerNetworkObject player in players)
        {
            if (player.connectionToClient == sender)
                continue;            

            player.Cyclone.TargetOpponentPlacedCard(
                player.connectionToClient,
                sender.connectionId,
                cardPlaced,
                previousActiveCard);

            
        }

        
    }
   
    [Server]
    public void NotifyOtherPlayersOfDraw(NetworkConnectionToClient sender, CardInfo[] drawnCards)
    {

        foreach (PlayerNetworkObject player in players)
        {           
            if (player.connectionToClient == sender)
                continue;

            player.Cyclone.TargetOpponentDrewCards(
                player.connectionToClient,
                sender.connectionId,
                drawnCards);

           
        }
    }

    [Server]
    public void TESTNotifyOtherPlayersOfDraw()
    {
        List<CardInfo> cards = Draw(2);

        foreach (PlayerNetworkObject player in players)
        {           

            player.Cyclone.TESTTargetOpponentDrewCards(cards.ToArray(), (CardInfo)activeCard);


        }
    }

    [Server]
    public void ServerPlayCard(PlayerNetworkObject player, CardInfo playedCard)
    {
        bool isBackup =
            player.Cyclone.TryGetBackupCard(out var backup) &&
            backup.Equals(playedCard);

        if (isBackup)
            HandleBackupCard(player, playedCard);
        else
            HandleHandCard(player, playedCard);

       
    }

    [Server]
    private void HandleBackupCard(PlayerNetworkObject player, CardInfo playedCard)
    {
        CardData backup = CardManager.Instance.GetCard((CardInfo)player.Cyclone.backupCard);

        if (backup == null)
            return;

        CardInfo previousActiveCard = (CardInfo)activeCard;

        ResolvePlayedCard(player, playedCard);

        player.Cyclone.ClearBackupCard();

        if(activeCard != null)
            TargetDiscardActiveCard(player.connectionToClient, previousActiveCard);

        TargetPlayBackupCard(player.connectionToClient, player, playedCard, previousActiveCard);       

        IncrementCyclonePileCount(playedCard);

        if (cycloneCount >= cycloneLimit)
        {
            player.Eliminate();           
        }

        PlayerStatsNetwork.Instance.TargetRecordBackupPlayed(player.connectionToClient);
    }

    [Server]
    private void HandleHandCard(PlayerNetworkObject player, CardInfo playedCard)
    {
        Debug.Log($"SERVER hand: {string.Join(", ", player.Cyclone.handCards)}");
        Debug.Log($"SERVER played: {playedCard}");

        CardInfo? otherCard = player.Cyclone.TakeOtherHandCard(playedCard);
        CardInfo previousActiveCard = (CardInfo)activeCard;

        Debug.Log($"SERVER other: {(otherCard.HasValue ? otherCard.Value.ToString() : "NONE")}");

        if (otherCard is CardInfo other)
        {
            bool sameSuit = activeCard != null && playedCard.suit == activeCard.Value.suit;
           

            TargetDiscardActiveCard(player.connectionToClient, previousActiveCard);
            ResolvePlayedCard(player,playedCard);
            player.Cyclone.RemoveCard(playedCard);

            if (sameSuit)
            {
                if (player.Cyclone.backupCard is CardInfo backup)
                {
                    TargetPlayDiscardBackupCard(player.connectionToClient, player, backup);

                    player.Cyclone.DiscardBackupCard();

                    IncrementDiscardPileCount(backup);
                }

                player.Cyclone.SetBackupCard(other);
            }
            else
            {
                player.Cyclone.DiscardToPile(other);
                IncrementDiscardPileCount(other);
            }

            IncrementCyclonePileCount(playedCard);          

            if (cycloneCount >= cycloneLimit)
            {
                player.Eliminate();               
            }           

            TargetPlayHandCard(player.connectionToClient, player, playedCard, other, previousActiveCard, sameSuit, true);
           
        }
        else
        {          
            ResolvePlayedCard(player,playedCard);
            player.Cyclone.RemoveCard(playedCard);

            IncrementCyclonePileCount(playedCard);

            if (cycloneCount >= cycloneLimit)
            {
                player.Eliminate();               
            }          

            TargetPlayHandCard(player.connectionToClient, player, playedCard, default, previousActiveCard, false, false);
        }

        PlayerStatsNetwork.Instance.TargetRecordCardPlayed(player.connectionToClient);
    }   

    [Server]
    private bool CheckGameEnd()
    {
        var activePlayers = players.Where(p => !p.isOut).ToList();

        if (activePlayers.Count > 1)
            return false;

        if (activePlayers.Count == 1)
        {                        
            OnGameEnd?.Invoke(activePlayers[0]);
            PlayerStatsNetwork.Instance.TargetRecordCycloneWin(activePlayers[0].connectionToClient);
            return true;
        }

        //if (activePlayers.Count == 1)
        //{
        //    return false;
        //}

        Debug.LogError("No winner found!");
        return true;
    }

    [Server]
    public void PlayerReady(NetworkConnectionToClient conn)
    {
        readyPlayers.Add(conn.connectionId);

        Debug.Log($"Player {conn.connectionId} ready.");

    }

    [Server]
    public void PlayerUnready(NetworkConnectionToClient conn)
    {
        readyPlayers.Remove(conn.connectionId);

        Debug.Log($"Player {conn.connectionId} unready.");

    }

    [Server]
    public void RemovePlayer(PlayerNetworkObject player)
    {
        players.Remove(player);

        player.Cyclone.handCards.Clear();
        player.Cyclone.backupCard = null;
    }

    [Server]
    public void StartGame()
    {
        if (!hasGameStarted)
        {
            if (PlayersAreReady())
            {
                RpcGameStarted();

                isGameActive = true;
                hasGameStarted = true;

                //Debug.Log("starting game!");

                ServerGenerateDeck();
            }
            else
            {
                GameEvents.InvokeNotEnoughPlayersNeeded();
            }
           
        }
        else
        {
            if (PlayersAreReady())
            {
                RpcGameStarted();

                isGameActive = true;

                //Debug.Log("starting game!");

                CollectAllCards();
            }
            else
            {
                GameEvents.InvokeNotEnoughPlayersNeeded();
            }           
        }
      
    }

    [Server]
    private bool PlayersAreReady()
    {
        return readyPlayers.Count == players.Count && players.Count > 1;
    }
        
    [Server]
    public void CollectAllCards()
    {     

        foreach (PlayerNetworkObject player in players)
        {         

            if (player.Cyclone.backupCard is CardInfo backup)
            {               
                discardPile.Add(backup);
                player.Cyclone.DiscardBackupCard();
            }

            foreach (CardInfo card in player.Cyclone.handCards)
            {
                discardPile.Add(card);
            }

            player.Cyclone.handCards.Clear();
        }

        ResetForNewGame();

        ReshuffleDeck();
    }

    [Server]
    public void ResetGame()
    {
        //add stuff in here buh
        hasGameStarted = false;
        deck.Clear();
        discardPile.Clear();
        cyclonePile.Clear();
        playersFinishedShuffling.Clear();
        readyPlayers.Clear();
        playersFinishedFirstCard.Clear();

        ResetCycloneCount();
        ResetCyclonePileCount();
        ResetDiscardPileCount();
        cardManager.ClearCards();      
    }

    [Server] 
    private void ResetForNewGame()
    {
        playersFinishedShuffling.Clear();
        playersFinishedFirstCard.Clear();

        foreach (PlayerNetworkObject player in players)
        {
            player.Cyclone.ResetPlayerForNewGame();
        }
    }

    [Server]
    public void EndGame(PlayerNetworkObject winner)
    {
        Debug.Log($"EndGame called. Winner: {winner.playerName}");               
        RpcEndGame(winner);
        Debug.Log("Game over");
    }
  

    [Server]
    private void ServerResetGame()
    {
        ResetGame();
    }

    private void ClientResetVisuals()
    {
        cardManager.ClearCards();
    }

    [ClientRpc]
    private void RpcSpawnDeck(CardInfo[] deckData, CardInfo startingCard)
    {
        Debug.Log("RPC RECEIVED ON CLIENT");
        cardManager.SpawnDeck(deckData.ToList(), startingCard);        
    }

    [ClientRpc]
    private void RpcAnimateReshuffle(CardInfo[] oldDeck, CardInfo[] newDeck)
    {
        cardManager.ShuffleDeck(oldDeck, newDeck);
    }


    [ClientRpc]
    private void NotifyPlayersToPlaceFirstCard(CardInfo startingCard)
    {       
        cardManager.PlaceFirstCard(startingCard);
    }

    [ClientRpc]
    private void RpcEndGame(PlayerNetworkObject player)
    {
        Debug.Log($"RPC ShowWinner - Frame {Time.frameCount}");
        PlayerHandVisuals.Instance.UnregisterAllCards();
        GameEvents.InvokeOnAnyGameEnd(player);
    }

    [ClientRpc]
    private void RpcGameStarted()
    {
        OnGameStart?.Invoke();
    }


    [TargetRpc]
    private void TargetPlayHandCard(NetworkConnection target, PlayerNetworkObject player, CardInfo playedCard, CardInfo otherCard, CardInfo previousActiveCard, bool sameSuit, bool hasOtherCard)
    {
        Debug.Log($"RPC played: {playedCard}");
        Debug.Log($"RPC other: {otherCard}");
        Debug.Log($"RPC hasOther: {hasOtherCard}");

        if (hasOtherCard)
        {
            CardManager.Instance.PlayHandAnimation(playedCard, otherCard, previousActiveCard, sameSuit);
            TurnLogManager.Instance.AddEntry(player.playerName, new CSteamID(player.steamId), TurnLogAction.PlayedCard, playedCard);
            TurnLogManager.Instance.AddEntry(player.playerName, new CSteamID(player.steamId), TurnLogAction.DiscardedCard, otherCard);
            CycloneScreenUIManager.Instance.PlayerPlacedCard(player.netId, playedCard);

            if (sameSuit)
            {
                CycloneScreenUIManager.Instance.PlayerGainedBackup(player.netId, otherCard);
            }
            else
            {
                CycloneScreenUIManager.Instance.PlayerDiscardCard(player.netId, otherCard);
            }
                
           
        }
        else
        {
            CardManager.Instance.PlayHandAnimation(playedCard, null, previousActiveCard, sameSuit);
            TurnLogManager.Instance.AddEntry(player.playerName, new CSteamID(player.steamId), TurnLogAction.PlayedCard, playedCard);
            CycloneScreenUIManager.Instance.PlayerPlacedCard(player.netId, playedCard);
        }
      
    }

    [TargetRpc]
    private void TargetPlayBackupCard(NetworkConnection target, PlayerNetworkObject player, CardInfo card, CardInfo previousActiveCard)
    {
        CardManager.Instance.PlayBackupAnimation(card, previousActiveCard);
        CycloneScreenUIManager.Instance.PlayerPlacedBackup(player.netId, card);
    }

    [TargetRpc]
    private void TargetPlayDiscardBackupCard(NetworkConnection target, PlayerNetworkObject player, CardInfo card)
    {
        CardManager.Instance.PlayDiscardBackupAnimation(card);
        CycloneScreenUIManager.Instance.PlayerDiscardedBackup(player.netId, card);       
    }

    [TargetRpc]
    private void TargetDiscardActiveCard(NetworkConnection target, CardInfo card)
    {
        CardManager.Instance.PlayDiscardActiveCardAnimation(card);
    }

    public void PlayerJoined(PlayerNetworkObject player, NetworkConnectionToClient connection)
    {
        players.Add(player);
    }

    public void PlayerLeft(PlayerNetworkObject player,  NetworkConnectionToClient connection)
    {
        players.Remove(player);
        PlayerUnready(connection);
    }

}
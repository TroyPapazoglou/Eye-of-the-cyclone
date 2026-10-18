using Mirror;
using PrimeTween;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;


public class CardManager : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject cardPrefab;

    [Header("Stack Settings")]
    [SerializeField] private float yOffsetPerCard = 0.0005f;

    [SerializeField] private Transform CardFanTransform;

    private Transform StartingCardTransform;

   

    public static Action OnFirstCardPlaced;
    public static Action<CardInfo, CardInfo> OnPlayerCardPlaced;
    public static Action<CardInfo> OnPlayerCardDiscarded;
    public static Action<CardInfo> OnPlayerBackupGained;
    public static Action<CardInfo, CardInfo> OnPlayerBackupPlaced;
    public static Action<CardInfo> OnPlayerBackupDiscarded;
    public static Action OnPlayerFinishedTurn;
    public static Action<CardInfo[]> OnPlayerDrawingCards;
    public static Action<CardInfo[]> OnPlayerFinishedDrawingCards;
    public static Action<CardInfo[]> OnPlayerCardsFinishedFanning;
    public static Action OnPlayerCardsFinishedReturning; 

    public static Action<CardInfo> OnDeckSpawned;  

    //private GameObject activeCard;   

    [Header("Animation Settings")]
    [SerializeField] private FirstCardAnimationSettings firstCardAnimation;
    [SerializeField] private CardAnimationSettings playCardAnimation;
    [SerializeField] private CardAnimationSettings discardAnimation;
    [SerializeField] private CardAnimationSettings discardBackupAnimation;
    [SerializeField] private CardAnimationSettings backupMoveAnimation;

    public List<CardData> cards;

    private Dictionary<CardInfo, CardData> cardLookup = new(); 
     
    public static event Action OnCardManagerReady;

    private int activeCardAnimations = 0;
    private bool waitingForTurnCompletion = false;
    public static CardManager Instance { get; private set; }  

    private void OnEnable()
    {
        OnDeckSpawned += PlaceFirstCard;
        OnPlayerCardsFinishedFanning += AnimateCardsBackToDeck;
    }

    private void OnDisable()
    {
        OnDeckSpawned -= PlaceFirstCard;
        OnPlayerCardsFinishedFanning -= AnimateCardsBackToDeck;
    }
    
    public override void OnStartClient()
    {
        base.OnStartClient();

        Instance = this;
        OnCardManagerReady?.Invoke();
    }

    public List<CardInfo> GenerateDeck()
    {
        List<CardInfo> deck = new();

        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
        {
            foreach (Rank rank in Enum.GetValues(typeof(Rank)))
            {
                deck.Add(new CardInfo(suit, rank));
            }
        }

        Shuffle(deck);

        return deck;
    }

    public void SpawnDeck(List<CardInfo> deck, CardInfo startingCard)
    {       

        for (int i = 0; i < deck.Count; i++)
        {
            GameObject cardObject = Instantiate(
                cardPrefab,
                Vector3.zero,
                Quaternion.identity,
                transform);

            CardData card = cardObject.GetComponent<CardData>();

            card.card = deck[i];

            cards.Add(card);
            cardLookup.Add(deck[i], card);

            card.GetComponent<Collider>().enabled = false;
            card.GetComponentInChildren<Renderer>().material = LoadMaterial(deck[i]);

            AnimateCardIntoDeck(card, i, startingCard);
        }

        cardLookup[startingCard].gameObject.GetComponent<Collider>().enabled = false;
       
    }

    private Material LoadMaterial(CardInfo info)
    {
        string suitName = info.GetSuitName(info.suit);
        string rankName = info.GetRankName(info.rank);

        string path =
            $"Materials/{suitName}/{rankName}of{suitName + "s"}";

        Material mat = Resources.Load<Material>(path);

        if (mat == null)
        {
            Debug.LogError($"Couldn't load material: {path}");
        }

        return mat;
    }

    public void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);

            (list[i], list[j]) = (list[j], list[i]);
        }
    }  

    public GameObject GetTopCard()
    {
        if (cards.Count == 0)
            return null;

        int lastIndex = cards.Count - 1;
        GameObject topCard = cards[lastIndex].gameObject;

        cards.RemoveAt(lastIndex);

        return topCard;
    }

    private void BeginCardAnimation()
    {
        activeCardAnimations++;
    }

    private void CompleteCardAnimation()
    {
        activeCardAnimations--;

        if (activeCardAnimations < 0)
            activeCardAnimations = 0;

        TryFinishTurn();
    }

    private void TryFinishTurn()
    {
        if (!waitingForTurnCompletion)
            return;

        if (activeCardAnimations > 0)
            return;

        waitingForTurnCompletion = false;

        OnPlayerFinishedTurn?.Invoke();
    }

    private void ResetCardAnimations()
    {
        activeCardAnimations = 0;
    }

    private void AnimateCardIntoDeck(CardData card, int index, CardInfo startingCard)
    {
        Vector3 endPos = transform.position + Vector3.up * (index * yOffsetPerCard);
        Quaternion endRotation = Quaternion.Euler(0f, -90f, 90f);

        card.transform.position = endPos + Vector3.up * 1.2f;
        card.transform.rotation = endRotation;
        card.transform.localScale = Vector3.one * 0.8f;

        Renderer renderer = card.GetComponentInChildren<Renderer>();
        Material material = renderer.material;

        Color colour = material.color;
        colour.a = 0f;
        material.color = colour;

        const float duration = 0.75f;
               
        Tween.Scale(
            card.transform,
            Vector3.one,
            duration,
            ease: Ease.OutBack,
            startDelay: index * 0.025f);
               
        Tween.Position(
            card.transform,
            endPos,
            duration,
            ease: Ease.OutCubic,
            startDelay: index * 0.025f)
        .OnComplete(() =>
        {
            card.transform.position = endPos;
            card.transform.rotation = endRotation;
            card.transform.localScale = Vector3.one;

            if (index == cards.Count - 1)
                OnDeckSpawned?.Invoke(startingCard);
        });


    }
    



    public void DrawCards(CardInfo[] cardsInfo)
    {
        OnPlayerDrawingCards?.Invoke(cardsInfo);

        Sequence sequence = Sequence.Create();
              
        Vector3[] targetPositions =
        {
            new Vector3(0.015f,-0.02F, -0.11f) + transform.position, // left card
            new Vector3(0.085f, -0.02f, -0.11f) + transform.position  // right card
        };
               
        if (cardsInfo.Length == 1)
        {
            targetPositions[0] = new Vector3(0.05f, -0.02f, -0.11f) + transform.position;
        }

        List<CardData> drawnCards = new();

        for (int i = 0; i < cardsInfo.Length; i++)
        {
            CardData card = cardLookup[cardsInfo[i]];
            drawnCards.Add(card);

            sequence
                .Chain(
                    DrawCard(
                        card.transform,
                        Quaternion.Euler(0, -90f, -45f),
                        targetPositions[i]
                    )
                );

            if (i < cardsInfo.Length - 1)
                sequence.ChainDelay(0.1f);
        }

        sequence.ChainCallback(() =>
        {
            foreach (CardData card in drawnCards)
            {
                card.GetComponent<Collider>().enabled = true;

                card.SetState(CardVisualState.Hand);                               
            }

            PlayerHandVisuals.Instance.EnableHoverForAllCards();

            PlayerBehaviour.Instance.canInteract = true;

            OnPlayerFinishedDrawingCards?.Invoke(cardsInfo);
        }); 
    }

    private Sequence DrawCard(Transform card, Quaternion targetRot, Vector3 targetPos)
    {
        return Tween.Position(
            card,
            targetPos,
            0.7f,
            ease: Ease.OutQuint
        )
        .Group(
            Tween.Rotation(
                card,
                targetRot,
                0.8f,
                ease: Ease.OutQuint
            )
        );
    }

    public void ShuffleDeck(CardInfo[] oldDeck, CardInfo[] newDeck)
    {
        List<CardData> cardsToFan = new();
        List<CardInfo> cardsBackToDeck = new();

        foreach (CardInfo info in oldDeck)
        {
            cardsToFan.Add(cardLookup[info]);
        }

        foreach (CardInfo info in newDeck)
        {
            cardsBackToDeck.Add(cardLookup[info].card);
        }

        AnimateCardsIntoFan(GetTopToBottom(cardsToFan), cardsBackToDeck.ToArray());      
    }


    private void DiscardPlayerCard(PlayerBehaviour player, CardData playedCard, CardData previousActiveCard, bool canInteractAfter, bool finishTurnAfterAnimation)
    {
        playedCard.GetComponent<Collider>().enabled = false;

        if (finishTurnAfterAnimation)
            waitingForTurnCompletion = true;

        BeginCardAnimation();

        CardSplineAnimator.Instance.PlaySpline(playedCard.transform, CardSplineType.PlayerToPlayPile, customStart: playedCard.transform.position, ease: Ease.OutCubic, onComplete: () =>
        {
            TogglePlayerInteractability(player, canInteractAfter);
               
            CompleteCardAnimation();
        });

        OnPlayerCardPlaced?.Invoke(
            playedCard.card,
            previousActiveCard.card
        );

        playedCard.SetState(CardVisualState.Played);
    }

    private void MoveCardToBackup(CardData newCard, Vector3 targetPos)
    {
        OnPlayerBackupGained?.Invoke(newCard.card);

        BeginCardAnimation();

        newCard.SetState(CardVisualState.Animating);

        Tween.Position(
            newCard.gameObject.transform,
            targetPos + transform.position,
            backupMoveAnimation.moveDuration,
            ease: backupMoveAnimation.moveEase
        )
        .OnComplete(() =>
        {
            newCard.SetState(CardVisualState.Backup);

            newCard.GetComponent<HoverableCard>().SetHoverEnabled(false);

            CompleteCardAnimation();
        });
    }




    private void MoveCardToDiscardPile(CardData newCard, Vector3 targetPos, Quaternion targetRot)
    {
        OnPlayerCardDiscarded?.Invoke(newCard.card);

        newCard.GetComponent<Collider>().enabled = false;

        CardSplineAnimator.Instance.PlaySpline(newCard.transform, CardSplineType.PlayerToDiscard, customStart: newCard.gameObject.transform.position, customEnd: targetPos, customRotation: targetRot );              

        newCard.SetState(CardVisualState.Discard);
    }
    
    public void PlaceFirstCard(CardInfo startingCard)
    {
        CardData cardData = GetCard(startingCard);

        StartingCardTransform = cardData.transform;

        Vector3 start = StartingCardTransform.localEulerAngles;
        Vector3 target = new Vector3(start.x, start.y, -90f);

        CardSplineAnimator.Instance.PlaySpline(StartingCardTransform, CardSplineType.DeckToPlayPile, customStart: StartingCardTransform.position, ease: Ease.OutCubic,  onComplete: (() => { OnFirstCardPlaced?.Invoke(); }));
            

        cards.Remove(cardLookup[startingCard]);    
    }

    public IEnumerator OpponentDrawCards(CardInfo[] cardInfo)
    {
        for (int i = 0; i < cardInfo.Length; i++)
        {
            CardData card = cardLookup[cardInfo[i]];
            cards.Remove(card);

            bool finished = false;

            Vector3 direction = CameraManager.Instance.transform.position - card.transform.position;
            Quaternion endRot = Quaternion.LookRotation(direction, Vector3.up) * Quaternion.Euler(0f, -90f, 0f);

            CardSplineAnimator.Instance.PlaySpline(
                card.transform,
                CardSplineType.DeckToPc,
                customRotation: endRot,
                onComplete: () => finished = true);

            yield return new WaitUntil(() => finished);

            if (i < cardInfo.Length - 1)
                yield return new WaitForSeconds(0.1f);
        }
    }

    public IEnumerator TESTOpponentDrawCards(CardInfo[] cardInfo, Action onComplete)
    {
        if (cardInfo.Length < 2)
            yield break;

        CardData firstCard = cardLookup[cardInfo[0]];
        CardData secondCard = cardLookup[cardInfo[1]];

        cards.Remove(firstCard);
        cards.Remove(secondCard);

        bool firstFinished = false;
        bool secondFinished = false;

        Vector3 direction1 = CameraManager.Instance.transform.position - firstCard.transform.position;
        Quaternion endRot1 = Quaternion.LookRotation(direction1, Vector3.up) * Quaternion.Euler(0f, -90f, 0f);

        Vector3 direction2 = CameraManager.Instance.transform.position - secondCard.transform.position;
        Quaternion endRot2 = Quaternion.LookRotation(direction2, Vector3.up) * Quaternion.Euler(0f, -90f, 0f);

        CardSplineAnimator.Instance.PlaySpline(
            firstCard.transform,
            CardSplineType.DeckToPc,
            customRotation: endRot1,
            onComplete: () => firstFinished = true);

        yield return new WaitForSeconds(0.1f);

        CardSplineAnimator.Instance.PlaySpline(
            secondCard.transform,
            CardSplineType.DeckToPc,
            customRotation: endRot2,
            onComplete: () => secondFinished = true);

        yield return new WaitUntil(() => firstFinished && secondFinished);

        onComplete?.Invoke();
    }

    public void OpponentPlacedCard(CardInfo cardPlaced, CardInfo previousActiveCard)
    {

        int pileCount =
        CycloneGameManager.Instance.CyclonePileCount;
                
        int previousIndex = pileCount - 2;

        Vector3 previousPosition =
            GetCyclonePilePosition(previousIndex);

        CardSplineAnimator.Instance.PlaySpline(
            cardLookup[previousActiveCard].transform,
            CardSplineType.PlayPileToDiscard,
            customEnd: previousPosition,
            customRotation:
                Quaternion.Euler(
                    0,
                    Random.Range(-120, -60),
                    90));
                   

        CardSplineAnimator.Instance.PlaySpline(
            cardLookup[cardPlaced].transform,
            CardSplineType.PcToPlayPile           
        );      
    }

    public void OpponentPlacedBackup(CardInfo backupPlaced, CardInfo previousActiveCard)
    {
        float targetY = CycloneGameManager.Instance.CyclonePileCount * yOffsetPerCard;

        CardSplineAnimator.Instance.PlaySpline(cardLookup[previousActiveCard].gameObject.transform, CardSplineType.PlayPileToDiscard, customEnd: new Vector3(.2f, targetY, .25f), customRotation: Quaternion.Euler(0, Random.Range(-120, -60), 90));
        CardSplineAnimator.Instance.PlaySpline(cardLookup[backupPlaced].gameObject.transform, CardSplineType.PcToPlayPile);      
    }

    public void OpponentDiscardedCard(CardInfo cardDiscarded)
    {
        float targetY = CycloneGameManager.Instance.DiscardPileCount * yOffsetPerCard;        
        CardSplineAnimator.Instance.PlaySpline(cardLookup[cardDiscarded].gameObject.transform, CardSplineType.PcToDiscardPile, customEnd: new Vector3(-.2f, targetY, .25f), customRotation: Quaternion.Euler(0, Random.Range(-120, -60), 90));
              
    }  

    public void OpponentDiscardedBackup(CardInfo backupDiscarded)
    {
        float targetY = CycloneGameManager.Instance.DiscardPileCount * yOffsetPerCard;
        CardSplineAnimator.Instance.PlaySpline(cardLookup[backupDiscarded].gameObject.transform, CardSplineType.PcToDiscardPile, customEnd: new Vector3(-.2f, targetY, .25f), customRotation: Quaternion.Euler(0, Random.Range(-120, -60), 90));

       
    }

    private void TogglePlayerInteractability(PlayerBehaviour player, bool state)
    {
        player.canInteract = state;
    }  

    public CardData GetCard(CardInfo info)
    {
        return cardLookup[info];
    }

    public List<CardData> GetCards(IEnumerable<CardInfo> infos)
    {
        return infos.Select(info => cardLookup[info]).ToList();
    }

    public void PlayHandAnimation(CardInfo playedCard, CardInfo? otherCard, CardInfo previousActiveCard, bool sameSuit)
    {
        Debug.Log("card: " + playedCard.ToString() + " was played");

        Debug.Log(otherCard.HasValue ? $"card {otherCard.Value} was discarded" : "no second card");
        
        DiscardPlayerCard(PlayerBehaviour.Instance, GetCard(playedCard), GetCard(previousActiveCard), false, true);

        if (otherCard is CardInfo other)
        {
            CardData otherCardData = GetCard(other);

            if (sameSuit)
            {
                MoveCardToBackup(otherCardData, new Vector3(-0.065f, -0.02f, -0.09f));
            }
            else
            {
                MoveCardToDiscardPile(otherCardData, new Vector3(-0.2f, CycloneGameManager.Instance.DiscardPileCount * yOffsetPerCard, 0.25f), Quaternion.Euler(0, -90, 90));
            }
        }



    }

    public void PlayBackupAnimation(CardInfo card, CardInfo previousActiveCard)
    {
        float targetY = CycloneGameManager.Instance.CyclonePileCount * yOffsetPerCard;

        DiscardPlayerCard(PlayerBehaviour.Instance, GetCard(card), GetCard(previousActiveCard), true, false);

        OnPlayerBackupPlaced?.Invoke(card, previousActiveCard);

        GetCard(card).SetState(CardVisualState.Played);
    }

    public void PlayDiscardBackupAnimation(CardInfo card)
    {
        float targetY = CycloneGameManager.Instance.DiscardPileCount * yOffsetPerCard;
        
        MoveCardToDiscardPile(GetCard(card), new Vector3(-0.2f, targetY, 0.25f), Quaternion.Euler(0, -90, 90));
        
        OnPlayerBackupDiscarded?.Invoke(card);

        GetCard(card).SetState(CardVisualState.Discard);
    }

    public void PlayDiscardActiveCardAnimation(CardInfo card)
    {     
        float targetY = CycloneGameManager.Instance.CyclonePileCount * yOffsetPerCard;

        CardSplineAnimator.Instance.PlaySpline(GetCard(card).transform, CardSplineType.PlayPileToDiscard, customEnd: new Vector3(0.2f, targetY, 0.25f), customRotation: Quaternion.Euler(0, Random.Range(-120, -60), 90));
    }

    private (Vector3 position, Quaternion rotation) GetFanTransform(int index, int totalCards, Vector3 fanPivot)
    {
        if (totalCards == 1)
        {
            return (
                fanPivot + new Vector3(0.05f, 0, -0.4f) + transform.position,
                Quaternion.Euler(0, 90f, 45f)
            );
        }

        float fanAngle = Mathf.Min(totalCards * 1.25f, 80f);
        float radius = 0.4f;

        float t = (float)index / (totalCards - 1);

        float angle = Mathf.Lerp(-fanAngle * 0.5f, fanAngle * 0.5f, t);

        float radians = angle * Mathf.Deg2Rad;
               
        Vector3 worldPos = fanPivot + new Vector3(
            Mathf.Sin(radians) * radius,
            0f,
            Mathf.Cos(radians) * radius
        ) + transform.position;
        
        //float layerOffset = 0.001f;
        //worldPos.y += index * layerOffset;
               
        Quaternion rot =
            Quaternion.Euler(
                0,
                90f,
                45f
            );

        return (worldPos, rot);
    }

    private void AnimateCardsIntoFan(List<CardData> cardsToShuffle, CardInfo[] newDeck)
    {
        float cardDelay = 0.05f;
        int count = cardsToShuffle.Count;

        for (int i = 0; i < count; i++)
        {
            CardData card = cardsToShuffle[i];
                       
            int fanIndex = count - 1 - i;

            var (position, rotation) =
                GetFanTransform(fanIndex, count, new Vector3(0.05f, 0, -0.2f) + transform.position);

            float delay = i * cardDelay;

            Tween.Position(card.transform, position, 0.5f, startDelay: delay);
            Tween.Rotation(card.transform, rotation, 0.5f, startDelay: delay);
        }

        float totalTime =
            (count - 1) * cardDelay + 0.5f;

        Tween.Delay(totalTime)
            .OnComplete(() =>
            {
                OnPlayerCardsFinishedFanning?.Invoke(newDeck);
            });
    }

    private void AnimateCardsBackToDeck(CardInfo[] newDeck)
    {       

        for (int i = 0; i < newDeck.Length; i++)
        {
            CardData card = cardLookup[newDeck[i]];

            Vector3 targetPos =
                Vector3.zero + Vector3.up * (i * yOffsetPerCard);

            float delay = i * 0.05f;

            Tween.Position(
                card.transform,
                targetPos + transform.position,
                0.5f,
                startDelay: delay);

            Tween.Rotation(
                card.transform,
                Quaternion.Euler(0, -90f, 90f),
                0.5f,
                startDelay: delay);
                        
            if (i == newDeck.Length - 1)
            {
                Tween.Delay(delay + 0.5f)
                    .OnComplete(() =>
                    {
                        OnPlayerCardsFinishedReturning?.Invoke();
                    });
            }
        }


    }

    private Vector3 GetCyclonePilePosition(int pileIndex)
    {
        return new Vector3(
            0.2f,
            pileIndex * yOffsetPerCard,
            0.25f
        );
    }

    private List<CardData> GetTopToBottom(List<CardData> pile)
    {
        List<CardData> result = new List<CardData>(pile.Count);

        for (int i = pile.Count - 1; i >= 0; i--)
        {
            result.Add(pile[i]);
        }

        return result;
    }

    public void ClearCards()
    {
        RemoveCards();
        cards.Clear();
        cardLookup.Clear();
        ResetCardAnimations();
    }

    public void RemoveCards()
    {
        foreach(CardData card in cardLookup.Values)
        {
            Destroy(card.gameObject);
        }
    }

}

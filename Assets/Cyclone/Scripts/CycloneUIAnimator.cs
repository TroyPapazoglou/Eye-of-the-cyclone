using PrimeTween;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Splines;

public class CycloneUIAnimator : MonoBehaviour
{
    [SerializeField] private Transform behindParent;
    [SerializeField] private Transform infrontParent;

    [Header("Cards")]
    [SerializeField] private BannerCardVisual[] cardVisuals;

    [Header("Splines")]
    [SerializeField] private SplineContainer drawingCardsSpline;
    [SerializeField] private SplineContainer playingCardSpline;

    [Header("Drawing Animation")]
    [SerializeField] private float moveDuration = 0.6f;
    [SerializeField] private float cardDelay = 0.2f;

    [Header("Card End Positions")]
    [SerializeField] private Vector2 rightCardEndPosition = new(0.005f, 0.025f);
    [SerializeField] private Vector2 leftCardEndPosition = new(-0.005f, 0.024f);
    [SerializeField] private Vector2 backupEndPosition = new(-0.0275f, 0.0017f);

    [Header("Playing Card Animation")]
    [SerializeField] private float playingCardDuration = 0.6f;
    [SerializeField] private float playingCardRotationAtPoint2 = -9f;
    [SerializeField] private float playingCardEndRotation = 0f;

    [Header("Discard Animation")]
    [SerializeField] private float discardDuration = 0.3f;
    [SerializeField] private float discardDownAmount = 0.01f;
    [SerializeField] private Ease discardEase = Ease.InQuad;

    private Sequence cardSequence;

    private BannerCardVisual leftHandCard;
    private BannerCardVisual rightHandCard;
    private BannerCardVisual backupCard;   

  
    private void OnDestroy()
    {                         
        cardSequence.Stop();
    }
     
    private BannerCardVisual GetFreeCard()
    {
        foreach (BannerCardVisual card in cardVisuals)
        {
            if (card.Role == BannerCardRole.None)
                return card;
        }

        Debug.LogError(
            "PlayerUIAnimator: No free banner card visual available.");

        return null;
    }

    private BannerCardVisual FindCard(CardInfo cardInfo)
    {
        foreach (BannerCardVisual card in cardVisuals)
        {
            if (!card.CurrentCard.HasValue)
                continue;

            if (card.CurrentCard.Value.Equals(cardInfo))
                return card;
        }

        return null;
    }   

    public void PlayTurnAnimation(CardInfo[] cards)
    {
        if (cards == null || cards.Length == 0)
            return;

        cardSequence.Stop();
        cardSequence = Sequence.Create();

        leftHandCard = null;
        rightHandCard = null;     

        if (cards.Length >= 1)
        {
            BannerCardVisual right = GetFreeCard();

            if (right != null)
            {
                right.SetCard(cards[0]);
                right.SetRole(BannerCardRole.Hand);
                right.Show();

                rightHandCard = right;

                cardSequence.Chain(
                    PlayRightCard(right)
                );
            }
        }
    
        if (cards.Length >= 2)
        {
            BannerCardVisual left = GetFreeCard();

            if (left != null)
            {
                left.SetCard(cards[1]);
                left.SetRole(BannerCardRole.Hand);
                left.Show();

                leftHandCard = left;

                cardSequence
                    .ChainDelay(cardDelay)
                    .Chain(
                        PlayLeftCard(left)
                    );
            }
        }
    }
  
    public void PlayPlacedCard(CardInfo playedCard)
    {
        BannerCardVisual card = FindCard(playedCard);

        if (card == null)
        {
            Debug.LogWarning(
                $"PlayerUIAnimator: Could not find visual for played card {playedCard}.");

            return;
        }
     
        if (card.Role == BannerCardRole.Backup)
        {
            PlayBackupCard(card);
        }
        else if (card.Role == BannerCardRole.Hand)
        {
            PlayHandCard(card);
        }
    }

    private void PlayHandCard(BannerCardVisual card)
    {       
        card.SetRole(BannerCardRole.None);

        PlayCardAnimation(card);
    }

    private void PlayBackupCard(BannerCardVisual card)
    {        
        if (backupCard == card)
            backupCard = null;

        card.SetRole(BannerCardRole.None);

        PlayCardAnimation(card);
    }    

    public void MoveCardToBackup(CardInfo backupCardInfo)
    {
        BannerCardVisual card = FindCard(backupCardInfo);

        if (card == null)
        {
            Debug.LogWarning(
                $"PlayerUIAnimator: Could not find visual for backup card {backupCardInfo}.");

            return;
        }

        MoveCardToBackup(card);
    }

    private void MoveCardToBackup(BannerCardVisual card)
    {
        if (card == null)
            return;

        card.SetRole(BannerCardRole.Backup);

        RectTransform rect =
            card.GetComponent<RectTransform>();

        Vector3 backupPosition =
            GetBackupPosition();

        Tween.Position(
            rect,
            backupPosition,
            moveDuration,
            ease: Ease.InOutQuad
        );

        backupCard = card;
    }

    private Vector3 GetBackupPosition()
    {
        return infrontParent.TransformPoint(new Vector3(backupEndPosition.x,backupEndPosition.y, 0f));
       
    }

    public void PlayBackup(CardInfo backupCardInfo)
    {
        BannerCardVisual card = FindCard(backupCardInfo);

        if (card == null)
        {
            Debug.LogWarning(
                $"PlayerUIAnimator: Could not find backup card {backupCardInfo}.");

            return;
        }

        PlayBackupCard(card);
    }    

    public void DiscardCardAnimation(CardInfo discardedCardInfo)
    {
        BannerCardVisual card = FindCard(discardedCardInfo);

        if (card == null)
        {
            Debug.LogWarning(
                $"PlayerUIAnimator: Could not find discarded card {discardedCardInfo}.");

            return;
        }
                
        if (card.Role != BannerCardRole.Hand)
            return;

        DiscardCardAnimation(card);
    }


    public void DiscardBackup(CardInfo backupDiscarded)
    {
        BannerCardVisual card = FindCard(backupDiscarded);

        if (card == null)
        {
            Debug.LogWarning(
                $"PlayerUIAnimator: Could not find backup card {backupDiscarded}.");

            return;
        }

        DiscardCardAnimation(card);
    }

    // =========================================================
    // DRAWING ANIMATION
    // =========================================================

    private Sequence PlayRightCard(BannerCardVisual card)
    {
        RectTransform rect = card.GetComponent<RectTransform>();

        SetCardBehind(rect);

        Vector3 point1 =
            GetSplinePoint(drawingCardsSpline, 0f);

        Vector3 point2 =
            GetSplinePoint(drawingCardsSpline, 0.5f);

        Vector3 point3 =
            GetDrawingCardEndPoint(rightCardEndPosition);

        card.transform.position = point1;

        Quaternion startRotation =
            card.transform.rotation;

        Quaternion rotationAtPoint2 =
            Quaternion.Euler(
                startRotation.eulerAngles.x,
                startRotation.eulerAngles.y,
                -9f
            );

        Quaternion rotationAtPoint3 =
            Quaternion.Euler(
                startRotation.eulerAngles.x,
                startRotation.eulerAngles.y,
                4f
            );

        Sequence sequence = Sequence.Create();

        sequence.Group(
            Tween.Position(
                card.transform,
                point2,
                moveDuration,
                ease: Ease.OutQuad
            )
        );

        sequence.Group(
            Tween.Rotation(
                card.transform,
                rotationAtPoint2,
                moveDuration,
                ease: Ease.OutQuad
            )
        );

        sequence.ChainCallback(() =>
        {
            SetCardInFront(rect);
        });

        sequence.Chain(
            Tween.Position(
                rect,
                point3,
                moveDuration,
                ease: Ease.InOutQuad
            )
        );

        sequence.Group(
            Tween.Rotation(
                card.transform,
                rotationAtPoint3,
                moveDuration,
                ease: Ease.OutQuad
            )
        );

        return sequence;
    }

    private Sequence PlayLeftCard(BannerCardVisual card)
    {
        RectTransform rect = card.GetComponent<RectTransform>();

        SetCardBehind(rect);

        Vector3 point1 =
            GetSplinePoint(drawingCardsSpline, 0f);

        Vector3 point2 =
            GetSplinePoint(drawingCardsSpline, 0.5f);

        Vector3 point3 =
            GetDrawingCardEndPoint(leftCardEndPosition);

        card.transform.position = point1;

        Quaternion startRotation =
            card.transform.rotation;

        Quaternion rotationAtPoint2 =
            Quaternion.Euler(
                startRotation.eulerAngles.x,
                startRotation.eulerAngles.y,
                -9f
            );

        Quaternion rotationAtPoint3 =
            Quaternion.Euler(
                startRotation.eulerAngles.x,
                startRotation.eulerAngles.y,
                -7f
            );

        Sequence sequence = Sequence.Create();

        sequence.Group(
            Tween.Position(
                card.transform,
                point2,
                moveDuration,
                ease: Ease.OutQuad
            )
        );

        sequence.Group(
            Tween.Rotation(
                card.transform,
                rotationAtPoint2,
                moveDuration,
                ease: Ease.OutQuad
            )
        );

        sequence.ChainCallback(() =>
        {
            SetCardInFront(rect);
        });

        sequence.Chain(
            Tween.Position(
                rect,
                point3,
                moveDuration,
                ease: Ease.InOutQuad
            )
        );

        sequence.Group(
            Tween.Rotation(
                card.transform,
                rotationAtPoint3,
                moveDuration,
                ease: Ease.OutQuad
            )
        );

        return sequence;
    }

    private Sequence PlayCardAnimation(BannerCardVisual card)
    {
        RectTransform rect =
            card.GetComponent<RectTransform>();

        Vector3 splinePoint2 =
            GetSplinePoint(playingCardSpline, 0.5f);

        Vector3 splineEnd =
            GetSplinePoint(playingCardSpline, 1f);

        Quaternion startRotation =
            card.transform.rotation;

        Quaternion rotationAtPoint2 =
            Quaternion.Euler(
                startRotation.eulerAngles.x,
                startRotation.eulerAngles.y,
                playingCardRotationAtPoint2
            );

        Quaternion endRotation =
            Quaternion.Euler(
                startRotation.eulerAngles.x,
                startRotation.eulerAngles.y,
                playingCardEndRotation
            );

        SetCardInFront(rect);

        Sequence sequence = Sequence.Create();

        sequence.Group(
            Tween.Position(
                card.transform,
                splinePoint2,
                playingCardDuration,
                ease: Ease.OutQuad
            )
        );

        sequence.Group(
            Tween.Rotation(
                card.transform,
                rotationAtPoint2,
                playingCardDuration,
                ease: Ease.OutQuad
            )
        );

        sequence.ChainCallback(() =>
        {
            SetCardBehind(rect);
        });

        sequence.Chain(
            Tween.Position(
                card.transform,
                splineEnd,
                playingCardDuration,
                ease: Ease.InOutQuad
            )
        );

        sequence.Group(
            Tween.Rotation(
                card.transform,
                endRotation,
                playingCardDuration,
                ease: Ease.OutQuad
            )
        );

        sequence.ChainCallback(() =>
        {
            ResetCardVisual(card);
        });

        return sequence;
    }

    private Sequence DiscardCardAnimation(BannerCardVisual card)
    {
        if (card == null)
            return Sequence.Create();

        Transform cardTransform = card.transform;

        Vector3 currentPosition = cardTransform.position;

        Vector3 targetPosition = currentPosition;

        targetPosition.y -= discardDownAmount;

        Quaternion targetRotation =
            Quaternion.Euler(
                -90f,
                cardTransform.eulerAngles.y,
                cardTransform.eulerAngles.z
            );

        Sequence sequence = Sequence.Create();

        sequence.Group(
            Tween.Position(
                cardTransform,
                targetPosition,
                discardDuration,
                ease: discardEase
            )
        );

        sequence.Group(
            Tween.Rotation(
                cardTransform,
                targetRotation,
                discardDuration,
                ease: Ease.InOutQuad
            )
        );

        sequence.ChainCallback(() =>
        {
            ResetCardVisual(card);
        });

        return sequence;
    }   

    private void ResetCardVisual(BannerCardVisual card)
    {
        if (card == null)
            return;

        card.SetRole(BannerCardRole.None);
        card.ClearCard();

        SetCardBehind(card.transform);

        card.Hide();

        RectTransform rect =
            card.GetComponent<RectTransform>();

        rect.anchoredPosition = Vector2.zero;

        card.transform.localRotation =
            Quaternion.Euler(-180f, 0f, 0f);

        if (leftHandCard == card)
            leftHandCard = null;

        if (rightHandCard == card)
            rightHandCard = null;

        if (backupCard == card)
            backupCard = null;
    } 

    private void SetCardBehind(Transform card)
    {
        card.SetParent(behindParent, true);
    }

    private void SetCardInFront(Transform card)
    {
        card.SetParent(infrontParent, true);
    }

    private Vector3 GetSplinePoint(
        SplineContainer spline,
        float t)
    {
        return spline.EvaluatePosition(t);
    }

    private Vector3 GetDrawingCardEndPoint(Vector2 endPosition)
    {
        Vector3 localPoint = new Vector3(endPosition.x, endPosition.y, 0f);

        return behindParent.TransformPoint(localPoint);
    }
}

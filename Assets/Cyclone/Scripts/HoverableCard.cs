using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverableCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private CardData cardData;

    private Sequence hoverSequence;

    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private bool hoverEnabled = false;

    private void Awake()
    {
        cardData = GetComponent<CardData>();
        cardData.OnStateChanged += StateChanged;
    }

    public void SetHoverEnabled(bool enabled)
    {
        hoverEnabled = enabled;

        if (!enabled)
        {
            PlayerHandVisuals.Instance.ClearHovered(this);
            StopHover();
        }
    }

    public void SetRestingTransform()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        originalScale = transform.localScale;
    }

    private void StateChanged(CardVisualState state)
    {
        if (state == CardVisualState.Hand ||
            state == CardVisualState.Backup)
        {
            PlayerHandVisuals.Instance.RefreshCardPosition(this);
        }
        else
        {
            PlayerHandVisuals.Instance.UnregisterCard(this);

            ResetHover();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!hoverEnabled)
            return;

        if (cardData.State != CardVisualState.Hand &&
            cardData.State != CardVisualState.Backup)
            return;

        PlayerHandVisuals.Instance.SetHovered(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PlayerHandVisuals.Instance.ClearHovered(this);
    }

    public void StopHover()
    {
        hoverSequence.Stop();
    }

    public void PlayHover(Vector3 targetPosition, Quaternion targetRotation, Vector3 targetScale, float duration)
    {
        hoverSequence.Stop();

        bool positionChanged =
            Vector3.Distance(transform.localPosition, targetPosition) > 0.001f;

        bool rotationChanged =
            Quaternion.Angle(transform.localRotation, targetRotation) > 0.1f;

        bool scaleChanged =
            Vector3.Distance(transform.localScale, targetScale) > 0.001f;

        if (!positionChanged && !rotationChanged && !scaleChanged)
        {
            return;
        }

        hoverSequence = Sequence.Create();

        if (positionChanged)
        {
            hoverSequence.Group(
                Tween.LocalPosition(
                    transform,
                    targetPosition,
                    duration,
                    ease: Ease.OutQuad));
        }

        if (rotationChanged)
        {
            hoverSequence.Group(
                Tween.LocalRotation(
                    transform,
                    targetRotation,
                    duration,
                    ease: Ease.OutQuad));
        }

        if (scaleChanged)
        {
            hoverSequence.Group(
                Tween.Scale(
                    transform,
                    targetScale,
                    duration,
                    ease: Ease.OutQuad));
        }
    }

    public void ResetHover(float duration = 0.15f)
    {
        hoverSequence.Stop();

        hoverSequence = Sequence.Create();

        hoverSequence.Group(
            Tween.LocalPosition(
                transform,
                originalPosition,
                duration,
                ease: Ease.OutQuad));

        hoverSequence.Group(
            Tween.LocalRotation(
                transform,
                originalRotation,
                duration,
                ease: Ease.OutQuad));

        hoverSequence.Group(
            Tween.Scale(
                transform,
                originalScale,
                duration,
                ease: Ease.OutQuad));
    }

    private void OnDestroy()
    {
        hoverSequence.Stop();

        if (cardData != null)
            cardData.OnStateChanged -= StateChanged;

        if (PlayerHandVisuals.Instance != null)
            PlayerHandVisuals.Instance.UnregisterCard(this);
    }


}

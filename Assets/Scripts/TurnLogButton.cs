using PrimeTween;
using UnityEngine;

public class TurnLogButton : MonoBehaviour
{
    [SerializeField] RectTransform turnLogVisual;
    private bool isVisible = false;

    [Header("Popup Target")]
    [SerializeField] private Vector2 squashScale = new Vector2(0.175f, 0.4f);
    [SerializeField] private float squashEndDelay = .3f;
    [SerializeField] private Vector2 pinchSize = new Vector2(0.7f, 0.15f);
    [SerializeField] private float pinchEndDelay = .3f;
    [SerializeField] private Vector2 targetSize = new Vector2(0.45f, 0.2f);

    private Sequence startSequence;

    public void ShowTurnLog()
    {
        if (isVisible)
            return;

        ShowTurnLogVisual();
    }

    public void CloseTurnLog()
    {
        startSequence.Stop();
        isVisible = false;
        turnLogVisual.gameObject.SetActive(false);
    }

    private void ShowTurnLogVisual()
    {
        isVisible = true;
        turnLogVisual.gameObject.SetActive(true);

        startSequence = Sequence.Create();

        startSequence
            .Group(
                Tween.UISizeDelta(
                    turnLogVisual,
                    squashScale,
                    0.12f,
                    ease: Ease.InQuad,
                    endDelay: squashEndDelay

                )
            )
            .Group(
                Tween.LocalRotation(
                    turnLogVisual,
                     new Vector3(0, 0, -25),
                     0
                    )
            )
            .Chain(
                Tween.UISizeDelta(
                    turnLogVisual,
                    pinchSize,
                    0.10f,
                    ease: Ease.OutQuad,
                    endDelay: pinchEndDelay
                )
            )
             .Group(
                Tween.LocalRotation(
                    turnLogVisual,
                     new Vector3(0, 0, 0),
                     0
                    )
            )
            .Chain(
                Tween.UISizeDelta(
                    turnLogVisual,
                    targetSize,
                    0.18f,
                    ease: Ease.OutBack
                )
            );         
    }
}

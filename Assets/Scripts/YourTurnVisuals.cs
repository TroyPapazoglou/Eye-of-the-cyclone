using Mirror.Examples.EdgegapLobby;
using PrimeTween;
using System.Collections;
using TMPro;
using UnityEngine;

public class YourTurnVisuals : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI yourTurnText;
    [SerializeField] private RectTransform yourTurnBox;

    [Header("Popup Timing")]
    [SerializeField] private float minTimeBetweenPopup = 4.5f;
    [SerializeField] private float maxTimeBetweenPopup = 6f;

    [Header("Turn Reminder")]
    [SerializeField] private float warningShakeAmount = 0.01f;
    [SerializeField] private float warningShakeDuration = 0.04f;
    [SerializeField] private float minTurnShakeDelay = 8f;
    [SerializeField] private float maxTurnShakeDelay = 13f;

    [Header("Popup Target")]
    [SerializeField] private Vector2 squashScale = new Vector2(0.175f, 0.4f);
    [SerializeField] private float squashEndDelay = .3f;
    [SerializeField] private Vector2 pinchSize = new Vector2(0.7f, 0.15f);
    [SerializeField] private float pinchEndDelay = .3f;
    [SerializeField] private Vector2 targetSize = new Vector2(0.45f, 0.2f);    

    private Vector3 originalWarningLocalPosition;
    private Vector3 originalWarningScale;
    private float originalTextMaxSize;  
    

    private Sequence warningShakeSequence;
    private Sequence popupDelaySequence;

    private bool turnActive;

    public static YourTurnVisuals Instance { get; private set; }

    private void Awake()
    {
        Instance = this;         

        originalWarningLocalPosition = yourTurnBox.transform.localPosition;

        originalWarningScale = yourTurnBox.sizeDelta;

        originalTextMaxSize = yourTurnText.fontSizeMax;
    }

    private void Start()
    {
        CycloneGameManager.OnGameEnd += HideTurnText;

        CardManager.OnPlayerDrawingCards += ShowTurnText;
        CardManager.OnPlayerFinishedTurn += HideTurnText;

        SteamLobbyManager.Instance.OnLobbyClosed += LobbyClosed;
        CycloneNetworkManager.OnDisconnected += LobbyClosed;
    }

    private void OnDestroy()
    {
        CycloneGameManager.OnGameEnd -= HideTurnText;

        CardManager.OnPlayerDrawingCards -= ShowTurnText;
        CardManager.OnPlayerFinishedTurn -= HideTurnText;

        SteamLobbyManager.Instance.OnLobbyClosed -= LobbyClosed;
        CycloneNetworkManager.OnDisconnected -= LobbyClosed;

        warningShakeSequence.Stop();
        popupDelaySequence.Stop();
    }   

    private void ShowTurnText(CardInfo[] cards)
    {
        ShowTurnTextInternal();
    }

    private void ShowTurnTextInternal()
    {
        popupDelaySequence.Stop();
        warningShakeSequence.Stop();

        turnActive = true;

        yourTurnBox.gameObject.SetActive(true);

        Sequence startSequence = Sequence.Create();

        startSequence           
            .ChainCallback(() =>
            {
                yourTurnBox.rotation = Quaternion.Euler(0, 0, -25);
            })           
            .Chain(
                Tween.UISizeDelta(
                    yourTurnBox,
                    squashScale,
                    0.12f,
                    ease: Ease.InQuad,
                    endDelay: squashEndDelay
                )
            )
                       
            .ChainCallback(() =>
            {
                yourTurnBox.rotation = Quaternion.identity;
            })            
            .Chain(
                Tween.UISizeDelta(
                    yourTurnBox,
                    pinchSize,
                    0.10f,
                    ease: Ease.OutQuad,
                    endDelay: pinchEndDelay
                )
            )
                       
            .Chain(
                Tween.UISizeDelta(
                    yourTurnBox,
                    targetSize,
                    0.18f,
                    ease: Ease.OutBack
                )
            )
            .OnComplete(() =>
            {
                if (!turnActive)
                    return;

                StartTurnReminder();
            });
    }

    private void HideTurnText(PlayerNetworkObject winner)
    {
        HideTurnText();
    }
    private void HideTurnText()
    {
        turnActive = false;
      
        popupDelaySequence.Stop();
        warningShakeSequence.Stop();

        Sequence endSequence = Sequence.Create();

        endSequence
            .Group(
                Tween.UIAnchoredPosition(
                    yourTurnBox,
                    originalWarningLocalPosition,
                    2f,
                    ease: Ease.OutQuad
                )
            )
            .Group(
                Tween.UISizeDelta(
                    yourTurnBox,
                    originalWarningScale,
                    2f,
                    ease: Ease.OutQuad
                )
            )
            .Group(
                Tween.Custom(
                    yourTurnText.fontSizeMax,
                    originalTextMaxSize,
                    2f,
                    ease: Ease.OutQuad,
                    onValueChange: value =>
                        yourTurnText.fontSizeMax = value
                )
            )
            .OnComplete(() =>
            {
                if (turnActive)
                    return;

                ResetPopup();
            });
    }

    private void ResetPopup()
    {
        yourTurnBox.gameObject.SetActive(false);

        yourTurnBox.anchoredPosition = originalWarningLocalPosition;
        yourTurnBox.sizeDelta = originalWarningScale;
        yourTurnText.fontSizeMax = originalTextMaxSize;
    }

    private void LobbyClosed()
    {
        turnActive = false;

        popupDelaySequence.Stop();
        warningShakeSequence.Stop();

        ResetPopup();
    }

    private void StartTurnReminder()
    {
        warningShakeSequence.Stop();

        if (!turnActive)
            return;

        PlayTurnReminderShake();
    }

    private void PlayTurnReminderShake()
    {
        if (!turnActive)
            return;

        float delay = Random.Range(
            minTurnShakeDelay,
            maxTurnShakeDelay
        );

        Vector2 startPosition = yourTurnBox.anchoredPosition;
        
        Vector2 direction = Random.insideUnitCircle.normalized;
       
        float amount = Random.Range(
            warningShakeAmount * 0.7f,
            warningShakeAmount * 1.3f
        );

        Vector2 offset = direction * amount;

        warningShakeSequence = Sequence.Create();

        warningShakeSequence
            .ChainDelay(delay)

            .Chain(
                Tween.UIAnchoredPosition(
                    yourTurnBox,
                    startPosition - offset,
                    warningShakeDuration,
                    ease: Ease.OutQuad
                )
            )

            .Chain(
                Tween.UIAnchoredPosition(
                    yourTurnBox,
                    startPosition + offset,
                    warningShakeDuration * 2f,
                    ease: Ease.InOutQuad
                )
            )

            .Chain(
                Tween.UIAnchoredPosition(
                    yourTurnBox,
                    startPosition - offset,
                    warningShakeDuration * 2f,
                    ease: Ease.InOutQuad
                )
            )

            .Chain(
                Tween.UIAnchoredPosition(
                    yourTurnBox,
                    startPosition,
                    warningShakeDuration,
                    ease: Ease.OutQuad
                )
            )

            .OnComplete(() =>
            {
                if (turnActive)
                    PlayTurnReminderShake();
            });
    }

    public void CloseWarningPopup()
    {
        turnActive = false;

        popupDelaySequence.Stop();
        warningShakeSequence.Stop();

        ResetPopup();

        StartPopupDelay();
    }

    private void StartPopupDelay()
    {
        float delay = Random.Range(
            minTimeBetweenPopup,
            maxTimeBetweenPopup
        );

        popupDelaySequence = Sequence.Create();

        popupDelaySequence
            .ChainDelay(delay)
            .OnComplete(() =>
            {
                if (!turnActive)
                    ShowTurnTextInternal();
            });
    }
}


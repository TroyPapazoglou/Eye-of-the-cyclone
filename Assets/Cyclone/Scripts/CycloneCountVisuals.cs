using PrimeTween;
using TMPro;
using UnityEngine;

public class CycloneCountVisuals : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI cycloneCountText;

    [Header("Cyclone")]
    [SerializeField] private int cycloneLimit = 75;
    [SerializeField] private int dangerThreshold = 10;   
    [SerializeField] private int constantWobbleThreshold = 10;

    [Header("Colours")]
    [SerializeField] private Color normalColour = Color.white;
    [SerializeField] private Color dangerColour = Color.red;

    [Header("Impact")]
    [SerializeField] private float increaseScale = 1.25f;
    [SerializeField] private float impactDuration = 0.2f;

    [Header("Decrease")]
    [SerializeField] private float decreaseScale = 0.9f;
    [SerializeField] private float decreaseDuration = 0.2f;  

    [Header("Danger Shake")]
    [SerializeField] private float minShakeAmount = 0.5f;
    [SerializeField] private float maxShakeAmount = 4f;

    [SerializeField] private float minShakeSpeed = 0.15f;
    [SerializeField] private float maxShakeSpeed = 0.05f;

    [Header("Limit Animation")]
    [SerializeField] private float limitScale = 1.4f;
    [SerializeField] private float limitHoldDuration = 0.5f;
    [SerializeField] private float limitFadeDuration = 0.8f;
    [SerializeField] private float limitMoveDistance = 0.08f;

    private Color originalColour;
    private Vector3 originalScale;
    private Quaternion originalRotation;

    private Sequence countAnimation;
    private Sequence shakeAnimation;

    private int currentCount;

    private Vector2 originalAnchoredPosition;

    public static CycloneCountVisuals Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        originalScale = cycloneCountText.transform.localScale;
        originalRotation = cycloneCountText.transform.localRotation;
        originalColour = cycloneCountText.color;

        cycloneCountText.color = normalColour;

        originalAnchoredPosition = cycloneCountText.rectTransform.anchoredPosition;
    }

    private void Start()
    {
        CycloneGameManager.OnCycloneCountUpdated += SetCount;
        CycloneGameManager.OnGameEnd += PlayLimitAnimation;

        CardManager.OnFirstCardPlaced += ShowCycloneText;

        SteamLobbyManager.Instance.OnLobbyClosed += LobbyClosed;

        CycloneNetworkManager.OnDisconnected += LobbyClosed;
    }

    private void OnDestroy()
    {
        CycloneGameManager.OnCycloneCountUpdated -= SetCount;
        CycloneGameManager.OnGameEnd -= PlayLimitAnimation;

        CardManager.OnFirstCardPlaced -= ShowCycloneText;

        SteamLobbyManager.Instance.OnLobbyClosed -= LobbyClosed;

        CycloneNetworkManager.OnDisconnected -= LobbyClosed;
    }

    public void EndGame()
    {
        HideCycloneText();
    }

    private void LobbyClosed()
    {
        HideCycloneText();
    }  

    private void ShowCycloneText()
    {
        cycloneCountText.gameObject.SetActive(true);

        Vector2 target = originalAnchoredPosition;
        target.y += 0.09f;

        Tween.UIAnchoredPosition(
            cycloneCountText.rectTransform,
            target,
            2,
            ease: Ease.OutQuad
        );
    }

    private void HideCycloneText()
    {
        cycloneCountText.gameObject.SetActive(false);      

        cycloneCountText.rectTransform.anchoredPosition = originalAnchoredPosition;
    }

    public void SetCount(int newCount)
    {
        int previousCount = currentCount;

        currentCount = newCount;

        cycloneCountText.text = currentCount.ToString();

        if (!cycloneCountText.gameObject.activeInHierarchy)
            return;

        UpdateColour();
        UpdateDangerShake();

        if (newCount > previousCount)
        {
            PlayIncreaseAnimation();
        }
        else if (newCount < previousCount)
        {
            PlayDecreaseAnimation();
        }
    }

    private void UpdateColour()
    {
        float dangerStart = cycloneLimit - dangerThreshold;

        float t = Mathf.InverseLerp(
            dangerStart,
            cycloneLimit,
            currentCount);

        cycloneCountText.color = Color.Lerp(
            normalColour,
            dangerColour,
            t);
    }

    private void PlayIncreaseAnimation()
    {
        if (!cycloneCountText.gameObject.activeInHierarchy)
            return;

        countAnimation.Stop();

        countAnimation = Sequence.Create()
                       
            .ChainCallback(() =>
            {
                cycloneCountText.transform.localScale =
                    originalScale * increaseScale;
            })
                        
            .Chain(
                Tween.Scale(
                    cycloneCountText.transform,
                    new Vector3(
                        originalScale.x * 1.1f,
                        originalScale.y * 0.9f,
                        originalScale.z),
                    impactDuration * 0.5f,
                    ease: Ease.OutQuad))
                      
            .Chain(
                Tween.Scale(
                    cycloneCountText.transform,
                    originalScale,
                    impactDuration,
                    ease: Ease.OutBack));
    }

    private void PlayDecreaseAnimation()
    {
        if (!cycloneCountText.gameObject.activeInHierarchy)
            return;

        countAnimation.Stop();

        countAnimation = Sequence.Create()

            .Chain(
                Tween.Scale(
                    cycloneCountText.transform,
                    originalScale * decreaseScale,
                    decreaseDuration * 0.5f,
                    ease: Ease.OutQuad))

            .Chain(
                Tween.Scale(
                    cycloneCountText.transform,
                    originalScale,
                    decreaseDuration,
                    ease: Ease.OutQuad));
    } 

    private void UpdateDangerShake()
    {
        if (!cycloneCountText.gameObject.activeInHierarchy)
        {
            shakeAnimation.Stop();
            return;
        }

        int dangerStart = cycloneLimit - constantWobbleThreshold;

        if (currentCount < dangerStart)
        {
            shakeAnimation.Stop();

            cycloneCountText.transform.localRotation = originalRotation;

            return;
        }

        float dangerAmount = Mathf.InverseLerp(
            dangerStart,
            cycloneLimit,
            currentCount);

        float shakeAmount = Mathf.Lerp(
            minShakeAmount,
            maxShakeAmount,
            dangerAmount);

        float shakeSpeed = Mathf.Lerp(
            minShakeSpeed,
            maxShakeSpeed,
            dangerAmount);

        shakeAnimation.Stop();

        shakeAnimation = Sequence.Create();

        AddShakeStep(shakeAnimation, shakeAmount, shakeSpeed, 1);
        AddShakeStep(shakeAnimation, shakeAmount, shakeSpeed, -1);
        AddShakeStep(shakeAnimation, shakeAmount, shakeSpeed, -1);
        AddShakeStep(shakeAnimation, shakeAmount, shakeSpeed, 1);

        shakeAnimation.ChainCallback(UpdateDangerShake);
    }

    private void AddShakeStep(
       Sequence sequence,
       float amount,
       float duration,
       float direction)
    {
        sequence.Chain(
            Tween.LocalRotation(
                cycloneCountText.transform,
                originalRotation *
                Quaternion.Euler(
                    0f,
                    0f,
                    amount * direction),
                duration,
                ease: Ease.InOutSine));
    }

    public void PlayLimitAnimation(PlayerNetworkObject winner)
    {
        shakeAnimation.Stop();
        countAnimation.Stop();

        Transform textTransform = cycloneCountText.transform;

        textTransform.localRotation = originalRotation;
        textTransform.localScale = originalScale;

        cycloneCountText.gameObject.SetActive(true);

        cycloneCountText.color = dangerColour;
               
        textTransform.localScale = originalScale * limitScale;

        Sequence limitSequence = Sequence.Create()
                       
            .ChainDelay(limitHoldDuration)
                       
            .Group(
                Tween.UIAnchoredPosition(
                    cycloneCountText.rectTransform,
                    originalAnchoredPosition +
                    Vector2.down * limitMoveDistance,
                    limitFadeDuration,
                    ease: Ease.InQuad))

            .Group(
                Tween.Alpha(
                    cycloneCountText,
                    0f,
                    limitFadeDuration,
                    ease: Ease.InQuad))

            .Group(
                Tween.Scale(
                    textTransform,
                    Vector3.zero,
                    limitFadeDuration,
                    ease: Ease.InQuad))

            .OnComplete(() =>
            {              
                cycloneCountText.rectTransform.anchoredPosition =
                    originalAnchoredPosition;

                cycloneCountText.color = normalColour;

                Color colour = cycloneCountText.color;
                colour.a = 1f;
                cycloneCountText.color = colour;

                cycloneCountText.gameObject.SetActive(false);
            });
    }
}

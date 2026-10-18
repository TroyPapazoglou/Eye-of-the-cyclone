using PrimeTween;
using TMPro;
using UnityEngine;

public class InteractionHUD : MonoBehaviour
{
    public static InteractionHUD Instance { get; private set; }

    [Header("References")] 
    [SerializeField] private RectTransform reticle;
    [SerializeField] private GameObject pickupIcon;
    [SerializeField] private GameObject punchIcon;
    [SerializeField] private GameObject interactIcon;
    [SerializeField] private TextMeshProUGUI interactText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("HUD Fade")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private Ease fadeEase = Ease.OutQuad;

    private NetworkInteractable currentInteractable;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        HideHUD();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void UpdateHUD(NetworkInteractable interactable)
    {
        if (currentInteractable == interactable)
            return;

        if(currentInteractable is WorldButton previousButton)
        {
            previousButton.OnHoverExit();
        }

        currentInteractable = interactable;

        if (interactable == null)
        {
            HideHUD();         
            return;
        }

        bool canPickup = interactable.CanBePickedUp();
        bool canPunch = interactable.CanPunch;

        pickupIcon.SetActive(canPickup);
        punchIcon.SetActive(canPunch);
        interactIcon.SetActive(!canPickup && !canPunch);

        if(interactable is WorldButton worldButton)
        {
            worldButton.OnHoverEnter();

            string text = worldButton.HoverText;

            if (string.IsNullOrWhiteSpace(text))
            {
                HideHoverText();
            }
            else
            {
                ShowHoverText(text);
            }
        }
        else
        {
            HideHoverText();
        }


       
    }

    private void ShowHoverText(string text)
    {
        interactText.gameObject.SetActive(true);
        interactText.text = text;
    }

    private void HideHoverText()
    {
        interactText.gameObject.SetActive(false);
    }

    public void HideHUD()
    {
        currentInteractable = null;

        pickupIcon.SetActive(false);
        punchIcon.SetActive(false);
        interactIcon.SetActive(false);
        interactText.gameObject.SetActive(false);
        //reticle.gameObject.SetActive(false);
    }

    public void FadeHUDIn()
    {     
        Tween.Alpha(
            canvasGroup,
            1f,
            fadeDuration,
            fadeEase
        );
    }

    public void FadeHUDOut()
    {
        Tween.Alpha(
            canvasGroup,
            0f,
            fadeDuration,
            fadeEase
        ).OnComplete(() => HideHUD());
    }
}

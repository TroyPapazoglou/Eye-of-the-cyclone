using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DoubleClickButton : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private float doubleClickDelay = 0.5f;
    [SerializeField] private UnityEvent onDoubleClick;
    private UnityAction codeDoubleClickAction;

    [Header("Optional Colour Image")]
    [SerializeField] private Graphic image;
    [SerializeField] private Color normalImageColor = Color.white;
    [SerializeField] private Color clickImageColor = Color.blue;

    [Header("Optional Click Image")]
    [SerializeField] private Image clickImage;

    [Header("Optional Click Text")]
    [SerializeField] private Graphic clickText;
    [SerializeField] private Color normalTextColor = Color.black;
    [SerializeField] private Color clickTextColor = Color.white;


    private int clickCount;
    private float firstClickTime;

    private void Awake()
    {      

        if (clickImage != null)
            clickImage.enabled = false;

        if (clickText != null)
            clickText.color = normalTextColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (clickCount == 0)
        {
            ButtonSelectionManager.Instance.Select(this);

            clickCount = 1;
            firstClickTime = Time.unscaledTime;

            if (clickImage != null)
                clickImage.enabled = true;

            if (clickText != null)
                clickText.color = clickTextColor;

            if (image != null)
                image.color = clickImageColor;

            return;
        }

        if (Time.unscaledTime - firstClickTime <= doubleClickDelay)
        {
            clickCount = 0;
            onDoubleClick?.Invoke();
            codeDoubleClickAction?.Invoke();
        }
        else
        {
            firstClickTime = Time.unscaledTime;
            clickCount = 1;
        }
    }

    public void ResetVisuals()
    {
        if (clickImage != null)
            clickImage.enabled = false;

        if (clickText != null)
            clickText.color = normalTextColor;

        if (image != null)
            image.color = normalImageColor;

        clickCount = 0;
    }

    public void SetDoubleClickAction(UnityAction action)
    {
        codeDoubleClickAction = action;
    }

    private void Update()
    {
        if (clickCount == 1 &&
            Time.unscaledTime - firstClickTime > doubleClickDelay)
        {
            clickCount = 0;
        }
    }
}

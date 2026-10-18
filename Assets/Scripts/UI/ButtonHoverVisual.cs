using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHoverVisual : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Optional Hover Image")]
    [SerializeField] private Image hoverImage;

    [Header("Optional Hover Text")]
    [SerializeField] private Graphic hoverText;
    [SerializeField] private Color normalTextColor = Color.black;
    [SerializeField] private Color hoverTextColor = Color.white;

    private void Awake()
    {
        if (hoverImage != null)
            hoverImage.enabled = false;

        if (hoverText != null)
            hoverText.color = normalTextColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverImage != null)
            hoverImage.enabled = true;

        if (hoverText != null)
            hoverText.color = hoverTextColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverImage != null)
            hoverImage.enabled = false;

        if (hoverText != null)
            hoverText.color = normalTextColor;
    }

    public void ResetVisual()
    {
        if (hoverImage != null)
            hoverImage.enabled = false;

        if (hoverText != null)
            hoverText.color = normalTextColor;
    }
}
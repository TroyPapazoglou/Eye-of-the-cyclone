using UnityEngine;
using UnityEngine.UI;

public class ToggleVisual : MonoBehaviour
{
    [SerializeField] private Toggle toggle;
    [SerializeField] private Image background;

    [SerializeField] private Color onColor;
    [SerializeField] private Color offColor;

    private void Awake()
    {
        toggle.onValueChanged.AddListener(UpdateVisual);
        UpdateVisual(toggle.isOn);
    }

    private void OnDestroy()
    {
        toggle.onValueChanged.RemoveListener(UpdateVisual);
    }

    private void UpdateVisual(bool isOn)
    {
        background.color = isOn ? onColor : offColor;
    }
}

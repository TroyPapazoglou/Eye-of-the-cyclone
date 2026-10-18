using UnityEngine;

public class ButtonSelectionManager : MonoBehaviour
{
    public static ButtonSelectionManager Instance { get; private set; }

    private DoubleClickButton selectedButton;

    private void Awake()
    {
        Instance = this;
    }

    public void Select(DoubleClickButton button)
    {
        if (selectedButton != null && selectedButton != button)
        {
            selectedButton.ResetVisuals();
        }

        selectedButton = button;
    }

    public void Deselect()
    {
        if (selectedButton == null)
            return;

        selectedButton.ResetVisuals();
        selectedButton = null;
    }

    public void Clear(DoubleClickButton button)
    {
        if (selectedButton == button)
            selectedButton = null;
    }
}

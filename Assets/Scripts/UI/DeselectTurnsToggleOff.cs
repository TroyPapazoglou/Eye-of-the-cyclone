using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeselectTurnsToggleOff : MonoBehaviour, IDeselectHandler
{
    private Toggle toggle;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        toggle.isOn = false;
    }
}

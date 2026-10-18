using UnityEngine;
using UnityEngine.EventSystems;

public class DeselectButton : MonoBehaviour
{
    [SerializeField] private GameObject target;

    public void OnButtonClick()
    {       
        if(!target.activeSelf)
            EventSystem.current.SetSelectedGameObject(null);
    }
}

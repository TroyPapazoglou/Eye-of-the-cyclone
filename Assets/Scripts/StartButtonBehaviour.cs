using UnityEngine;
using UnityEngine.UI;

public class StartButtonBehaviour : MonoBehaviour
{
    public static StartButtonBehaviour Instance { get; private set; }

    [SerializeField] private Toggle toggle;
    [SerializeField] private GameObject startMenu;

    public bool IsOn => toggle.isOn;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        toggle.onValueChanged.AddListener(OnToggleChanged);
      
        OnToggleChanged(toggle.isOn);
    }

    private void OnDisable()
    {
        toggle.onValueChanged.RemoveListener(OnToggleChanged);
    }

    public void SetState(bool value)
    {
        toggle.isOn = value;
    }

    public void Toggle()
    {
        toggle.isOn = !toggle.isOn;
    }

    private void OnToggleChanged(bool isOn)
    {      

        if (isOn)
        {
            startMenu.SetActive(true);
        }
        else
        {
            startMenu.SetActive(false);
        }
    }

}

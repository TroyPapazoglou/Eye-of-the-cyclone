using UnityEngine;
using UnityEngine.UI;

public class TaskbarItem : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image icon;

    private PCApplicationData applicationData;

    public void Setup(PCApplicationData data)
    {
        applicationData = data;
        icon.sprite = data.icon;

        button.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        PCBehaviour.Instance.ToggleApplication(applicationData);
    }
}

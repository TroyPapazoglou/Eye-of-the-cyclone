using TMPro;
using UnityEngine;

public class StatsFileViewer : MonoBehaviour
{
    public static StatsFileViewer Instance { get; private set; }

    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text fileText;
    [SerializeField] private GameObject fileObject;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowFile(string fileName, string contents)
    {
        fileObject.SetActive(true);

        titleText.text = fileName;

        fileText.text = contents;
    }
}

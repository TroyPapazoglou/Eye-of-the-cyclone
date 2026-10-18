using System.IO;
using UnityEngine;

public class StatsFileBrowser : MonoBehaviour
{
    [SerializeField] private Transform gridParent;
    [SerializeField] private GameObject filePrefab;

    private void Start()
    {
        if (SteamManager.Instance == null)
        {
            Debug.LogError("SteamManager does not exist!");
            return;
        }

        if (SteamManager.Initialized)
        {
            PopulateFiles();
        }
        else
        {
            SteamManager.Instance.OnSteamInitialized += PopulateFiles;
        }
    }

    private void OnDestroy()
    {
        if (SteamManager.Instance != null)
            SteamManager.Instance.OnSteamInitialized -= PopulateFiles;
    }

    private void OnDisable()
    {
        if (SteamManager.Instance != null)
            SteamManager.Instance.OnSteamInitialized -= PopulateFiles;
    } 

    private void PopulateFiles()
    {
        string[] files = Directory.GetFiles(
            Application.persistentDataPath,
            "*.json");

        foreach (string filePath in files)
        {
            GameObject fileObject = Instantiate(
                filePrefab,
                gridParent);

            StatsFileItem fileItem =
                fileObject.GetComponent<StatsFileItem>();

            fileItem.Setup(filePath);
        }
    }
}

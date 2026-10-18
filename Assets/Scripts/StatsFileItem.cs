using System.IO;
using TMPro;
using UnityEngine;

public class StatsFileItem : MonoBehaviour
{
    [SerializeField] private TMP_Text fileNameText;

    private string filePath;
    private string fileName;

    public void Setup(string path)
    {
        filePath = path;

        FileInfo fileInfo = new FileInfo(path);

        string displayName = Path.GetFileNameWithoutExtension(fileInfo.Name);

        fileName = displayName;       

        fileNameText.text =
            $"{displayName}\nText document\n{FormatFileSize(fileInfo.Length)}";
    }

    private string FormatFileSize(long bytes)
    {
        if (bytes <= 0)
            return "1KB";

        float kb = bytes / 1024f;

        return $"{Mathf.Max(1f, kb):0.#}KB";
    }

    public void OpenFile()
    {
        if (!File.Exists(filePath))
            return;

        string contents = File.ReadAllText(filePath);

        StatsFileViewer.Instance.ShowFile(
            fileName,
            contents);
    }
}

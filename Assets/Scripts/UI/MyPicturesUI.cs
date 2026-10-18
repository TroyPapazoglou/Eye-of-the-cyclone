using UnityEngine;

public class MyPicturesUI : MonoBehaviour
{
    [SerializeField] private Transform gridParent;
    [SerializeField] private ImageFileUI imageFilePrefab;
    [SerializeField] private ChatRoomUI chatRoomUI;

    private void OnEnable()
    {
        Populate();
    }

    private void Populate()
    {
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }

        if (ImageLibrary.Instance == null)
            return;

        foreach (ImageData imageData in ImageLibrary.Instance.Images)
        {
            ImageFileUI file = Instantiate(imageFilePrefab, gridParent);

            file.Setup(imageData, chatRoomUI);
        }
    }
}

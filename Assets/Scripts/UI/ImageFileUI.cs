using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ImageFileUI : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text fileNameText;
    [SerializeField] private DoubleClickButton doubleClickButton;

    private ImageData imageData;
    private ChatRoomUI chatRoomUI;

    public void Setup(ImageData data, ChatRoomUI chatRoomUI)
    {
        imageData = data;
        this.chatRoomUI = chatRoomUI;

        image.sprite = data.Sprite;
        fileNameText.text = data.FileName;

        doubleClickButton.SetDoubleClickAction(SelectImage);
    }

    private void SelectImage()
    {
        chatRoomUI.SetAttachment(imageData);
    }
}

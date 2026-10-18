using Steamworks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ChatMessageUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform message;
    [SerializeField] private RectTransform messageOffset;
    [SerializeField] private Image avatar;
    [SerializeField] private RectTransform textBorder;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Image messageImage;
    [SerializeField] private TMP_Text timeStamp;
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private HorizontalLayoutGroup horizontalLayoutGroup;

    [Header("Image")]
    [SerializeField] private float maxImageWidth = 700f;
    [SerializeField] private float maxImageHeight = 300f;
       
    private LayoutElement avatarLayout;
    private LayoutElement textBorderLayout;
    private LayoutElement imageLayout;

    private void Awake()
    {
        avatarLayout = avatar.GetComponent<LayoutElement>();
        textBorderLayout = textBorder.GetComponent<LayoutElement>();
        imageLayout = messageImage.GetComponent<LayoutElement>();

        HideAvatar();
        HideImage();
        HideUsername();
        HideTimeStampText();
    }

    public void SetText(string text)
    {
        messageText.gameObject.SetActive(true);
        textBorder.gameObject.SetActive(true);

        messageImage.gameObject.SetActive(false);

        messageText.text = text;
    }

    public void SetImage(Sprite sprite)
    {
        messageText.gameObject.SetActive(false);
        textBorder.gameObject.SetActive(false);

        messageImage.gameObject.SetActive(true);
        messageImage.sprite = sprite;

        ResizeImage(sprite);
    }

    public void ShowAvatar(Sprite sprite, bool isLocalPlayer)
    {
        if(sprite == null)
        {
            HideAvatar();
            SetMessageSide(isLocalPlayer);
            return;
        }

        avatar.enabled = true;
        avatar.sprite = sprite;

        SetMessageSide(isLocalPlayer);

        LayoutRebuilder.ForceRebuildLayoutImmediate(message);
    }

    public void HideAvatar()
    {
        avatar.enabled = false;
    }


    public void SetMessageSide(bool isLocalPlayer)
    {
        if (isLocalPlayer)
        {
            avatar.transform.SetAsLastSibling();

            messageOffset.anchoredPosition =
                new Vector2(
                    870,
                    messageOffset.anchoredPosition.y
                );

            usernameText.rectTransform.anchoredPosition =
                new Vector2(
                    1272.9f,
                    usernameText.rectTransform.anchoredPosition.y
                );

            usernameText.alignment =
                TextAlignmentOptions.Right;

            horizontalLayoutGroup.childAlignment =
                TextAnchor.UpperRight;
        }
        else
        {
            avatar.transform.SetAsFirstSibling();

            // Put the message back on the left.
            messageOffset.anchoredPosition =
                new Vector2(
                    0,
                    messageOffset.anchoredPosition.y
                );

            usernameText.rectTransform.anchoredPosition =
                new Vector2(
                    0,
                    usernameText.rectTransform.anchoredPosition.y
                );

            usernameText.alignment =
                TextAlignmentOptions.Left;

            horizontalLayoutGroup.childAlignment =
                TextAnchor.UpperLeft;
        }
    }

    public void ShowUsername(string username)
    {
        usernameText.gameObject.SetActive(true);
        usernameText.text = username;
    }

    private void HideUsername()
    {
        usernameText.gameObject.SetActive(false);
    } 

    public void HideImage()
    {
        messageImage.gameObject.SetActive(false);
    }

    private void ResizeImage(Sprite sprite)
    {
        if (sprite == null)
            return;

        message.sizeDelta = new Vector2(message.sizeDelta.x, messageImage.rectTransform.sizeDelta.y);

        float aspect = sprite.rect.width / sprite.rect.height;

        float width = maxImageWidth;
        float height = width / aspect;

        // Too tall
        if (height > maxImageHeight)
        {
            height = maxImageHeight;
            width = height * aspect;
        }

        imageLayout.preferredWidth = width;
        imageLayout.preferredHeight = height;
    }  


    public void ShowTimestamp(string time, bool isLocalPlayer, bool hasImage, bool hasText)
    {
        timeStamp.text = time;
        timeStamp.gameObject.SetActive(true);

        if (isLocalPlayer)
        {
            timeStamp.alignment =  TextAlignmentOptions.Left;

            if (hasImage && !hasText)
            {
                timeStamp.rectTransform.anchoredPosition =
                    new Vector2(
                        522.6f,
                        timeStamp.rectTransform.anchoredPosition.y
                    );
            }
            else
            {
                timeStamp.rectTransform.anchoredPosition =
                    new Vector2(
                        170,
                        timeStamp.rectTransform.anchoredPosition.y
                    );
            }
        }
        else
        {
            timeStamp.alignment =  TextAlignmentOptions.Right;

            // We need the correct left-side positions
            // based on your prefab.
        }
    }

    public static Texture2D ResizeTexture(Texture2D source, int maxWidth, int maxHeight)
    {
        float scale = Mathf.Min(1f, (float)maxWidth / source.width, (float)maxHeight / source.height);

        int width = Mathf.Max(1, Mathf.RoundToInt(source.width * scale));
        int height = Mathf.Max(1, Mathf.RoundToInt(source.height * scale));

        RenderTexture temporary = RenderTexture.GetTemporary(
            width,
            height,
            0,
            RenderTextureFormat.ARGB32
        );

        Graphics.Blit(source, temporary);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = temporary;

        Texture2D resized = new Texture2D(
            width,
            height,
            TextureFormat.RGBA32,
            false
        );

        resized.ReadPixels(
            new Rect(0, 0, width, height),
            0,
            0
        );

        resized.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(temporary);

        return resized;
    }


    private void HideTimeStampText()
    {
        timeStamp.gameObject.SetActive(false);
    }
}

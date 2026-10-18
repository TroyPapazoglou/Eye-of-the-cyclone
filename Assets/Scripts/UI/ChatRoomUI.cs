using Mirror;
using Mirror.BouncyCastle.Crypto;
using Steamworks;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ChatRoomUI : MonoBehaviour
{
    [Header("Chat Log")]
    [SerializeField] private GameObject chatLogVisual;

    [Header("Message Input")]
    [SerializeField] private TMP_InputField messageInput;

    [Header("Attachment")]
    [SerializeField] private TextMeshProUGUI attachmentText;
    [SerializeField] private GameObject attachmentPreviewObject;

    [Header("Pictures")]
    [SerializeField] private GameObject myPicturesUI;

    [Header("Messages")]
    [SerializeField] private Transform messageContainer;
    [SerializeField] private ChatMessageUI messagePrefab;


    private ImageData attachedImage;

    private bool hasPreviousMessage = false;
    private ulong lastMessageSteamId = 0;
    private long lastMessageTimestamp;

    public static ChatRoomUI Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        ClearAttachment();
    }

    private void OnEnable()
    {
        SteamLobbyManager.Instance.OnLobbyClosed += CloseChat;
        CycloneNetworkManager.OnDisconnected += CloseChat;
    }

    private void OnDisable()
    {
        if (SteamLobbyManager.Instance != null)
            SteamLobbyManager.Instance.OnLobbyClosed -= CloseChat;

        CycloneNetworkManager.OnDisconnected -= CloseChat;
    }

    public void OpenMyPictures()
    {
        myPicturesUI.SetActive(true);
    }

    public void SetAttachment(ImageData imageData)
    {
        attachedImage = imageData;

        attachmentText.text = imageData.FileName;
        attachmentPreviewObject.SetActive(true);

        myPicturesUI.SetActive(false);
    }

    public void ClearAttachment()
    {
        attachedImage = null;

        if (attachmentPreviewObject.activeSelf) 
        {
            attachmentText.text = "Name";
            attachmentPreviewObject.SetActive(false);
        }
       
    }

    public void SendMessage()
    {
        string text = messageInput.text;

        bool hasText = !string.IsNullOrWhiteSpace(text);

        bool hasImage =  attachedImage != null;

        if (!hasText && !hasImage)
            return;

        byte[] imageData = null;

        if (hasImage)
        {
            imageData = attachedImage.GetImageBytes();
        }

        ChatNetworkBehaviour.Instance.SendChatMessage(
            text,
            imageData,
            hasImage
                ? attachedImage.FileName
                : ""
        );

        messageInput.text = "";

        ClearAttachment();
    }

    public void AddMessage(ChatMessage message)
    {
        bool isLocalPlayer = message.steamId == SteamUser.GetSteamID().m_SteamID;

        bool isSameUser = hasPreviousMessage && lastMessageSteamId == message.steamId;

        long timeDifference = hasPreviousMessage ? message.timestamp - lastMessageTimestamp : 0;    

        bool showAvatar = !isSameUser || timeDifference >= 120;      

        bool showMessageHeader = !isSameUser || timeDifference >= 120;

        bool hasText = !string.IsNullOrWhiteSpace(message.text);

      
        if(hasText)
        {
            ChatMessageUI textUI = Instantiate(messagePrefab, messageContainer);

            textUI.SetText(message.text);

            textUI.SetMessageSide(isLocalPlayer);

            if (showMessageHeader)
            {
                textUI.ShowUsername(message.playerName); 
                RequestAvatar(message, textUI); 
                textUI.ShowTimestamp(FormatTimestamp(message.timestamp), isLocalPlayer, message.hasImage, true);
            }             

           
        }

        if (message.hasImage && message.imageData != null)
        {
            Sprite imageSprite = CreateSpriteFromBytes(message.imageData);

            if (imageSprite != null)
            {
                ChatMessageUI imageUI = Instantiate(messagePrefab, messageContainer);

                imageUI.SetImage(imageSprite);

                imageUI.SetMessageSide(isLocalPlayer);

                if (showMessageHeader)
                {
                    imageUI.ShowUsername(message.playerName);
                    RequestAvatar(message, imageUI);
                    imageUI.ShowTimestamp(FormatTimestamp(message.timestamp), isLocalPlayer, message.hasImage, false);
                }
            }

        }

        lastMessageSteamId = message.steamId;
        lastMessageTimestamp = message.timestamp;
        hasPreviousMessage = true;
    }

    private string FormatTimestamp(long timestamp) 
    { 
        return DateTimeOffset.FromUnixTimeSeconds(timestamp).ToLocalTime().ToString("h:mm tt"); 
    }

    private Sprite CreateSpriteFromBytes(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
            return null;

        Texture2D texture =
            new Texture2D(2, 2);

        if (!texture.LoadImage(bytes))
        {
            Destroy(texture);
            return null;
        }

        return Sprite.Create(
            texture,
            new Rect(
                0,
                0,
                texture.width,
                texture.height
            ),
            new Vector2(0.5f, 0.5f)
        );
    }

    private void RequestAvatar(ChatMessage message, ChatMessageUI messageUI)
    {
        if (SteamAvatarCache.Instance == null)
            return;

        SteamAvatarCache.Instance.RequestAvatar(
            new CSteamID(message.steamId),
            sprite =>
            {
                if (messageUI == null)
                    return;

                bool isLocalPlayer =
                    message.steamId ==
                    SteamUser.GetSteamID().m_SteamID;

                messageUI.ShowAvatar(
                    sprite,
                    isLocalPlayer
                );
            }
        );
    }

    private void ClearMessages()
    {
        for (int i = messageContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(messageContainer.GetChild(i).gameObject);
        }

        hasPreviousMessage = false;
        lastMessageSteamId = 0;
        lastMessageTimestamp = 0;
    }

    public void OpenChat()
    {
        if (!NetworkClient.isConnected)
            return;

        chatLogVisual.SetActive(true);

        //RefreshHistory();
    }

    public void CloseChat()
    {
        ClearMessages();
        ClearAttachment();

        chatLogVisual.SetActive(false);
    }

    private void RefreshHistory()
    {
        if (ChatNetworkBehaviour.Instance == null)
            return;

        foreach (ChatMessage message in ChatNetworkBehaviour.Instance.ChatHistory)
        {
            AddMessage(message);
        }
    }
}

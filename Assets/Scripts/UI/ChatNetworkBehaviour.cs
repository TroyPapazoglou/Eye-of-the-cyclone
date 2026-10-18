using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ChatMessage : NetworkMessage
{
    public ulong steamId;
    public string playerName;

    public string text;

    public bool hasImage;

    public byte[] imageData;
    public string imageFileName;

    public long timestamp;
}

public struct SendChatMessage : NetworkMessage
{
    public string text;

    public bool hasImage;

    public byte[] imageData;
    public string imageFileName;
}



public class ChatNetworkBehaviour : NetworkBehaviour
{
    public static ChatNetworkBehaviour Instance { get; private set; }

    private readonly List<ChatMessage> chatHistory = new();

    public IReadOnlyList<ChatMessage> ChatHistory => chatHistory;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        NetworkServer.RegisterHandler<SendChatMessage>(
            ServerReceiveMessage
        );
    }

    public override void OnStopServer()
    {
        chatHistory.Clear();

        base.OnStopServer();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        NetworkClient.RegisterHandler<ChatMessage>(ReceiveChatMessage);
    }

    public override void OnStopClient()
    {
        if (NetworkClient.active)
        {
            NetworkClient.UnregisterHandler<ChatMessage>();
        }

        base.OnStopClient();
    }

    // =========================
    // CLIENT
    // =========================

    public void SendChatMessage(string text, byte[] imageData, string imageFileName)
    {
        if (!NetworkClient.isConnected)
            return;

        if (string.IsNullOrWhiteSpace(text) &&
            (imageData == null || imageData.Length == 0))
            return;

        SendChatMessage message =
            new SendChatMessage
            {
                text = text,
                hasImage =
                    imageData != null &&
                    imageData.Length > 0,
                imageData = imageData,
                imageFileName = imageFileName
            };

        NetworkClient.Send(message);
    }

    // =========================
    // SERVER
    // =========================

    [Server]
    private void ServerReceiveMessage(NetworkConnectionToClient connection, SendChatMessage message)
    {
        if (connection.identity == null)
            return;

        PlayerNetworkObject player = connection.identity.GetComponent<PlayerNetworkObject>();

        if (player == null)
            return;

        ChatMessage chatMessage =
            new ChatMessage
            {
                steamId = player.steamId,
                playerName = player.playerName,

                text = message.text,

                hasImage = message.hasImage,
                imageData = message.imageData,
                imageFileName = message.imageFileName,
                timestamp = new DateTimeOffset(PCClock.Instance.GetCurrentTime()).ToUnixTimeSeconds()
            };

        chatHistory.Add(chatMessage);

        NetworkServer.SendToAll(chatMessage);
    }

    [Server]
    public void SendChatHistory(NetworkConnectionToClient connection)
    {
        const int maxBatchBytes = 400_000;

        List<ChatMessage> batch = new();
        int batchSize = 0;

        foreach (ChatMessage message in chatHistory)
        {
            int messageSize = EstimateMessageSize(message);

            // If this message by itself is too large,
            // it needs to be rejected/resized before
            // attempting to send it.
            if (messageSize > maxBatchBytes)
            {
                Debug.LogError($"Chat history message is too large: " + $"{messageSize} bytes.");

                continue;
            }

            // Would adding this message make the
            // batch too large?
            if (batch.Count > 0 && batchSize + messageSize > maxBatchBytes)
            {
                SendChatHistoryBatch(connection, batch.ToArray());

                batch.Clear();
                batchSize = 0;
            }

            batch.Add(message);
            batchSize += messageSize;
        }

        // Send remaining messages.
        if (batch.Count > 0)
        {
            SendChatHistoryBatch(connection, batch.ToArray());
        }
    }

    [Server]
    private void SendChatHistoryBatch(NetworkConnectionToClient connection, ChatMessage[] messages)
    {
        SendChatHistoryTarget(connection, messages);
    }


    [TargetRpc]
    private void SendChatHistoryTarget(NetworkConnectionToClient connection, ChatMessage[] messages)
    {
        StartCoroutine(AddMessagesWithDelay(messages));
    }

    private IEnumerator AddMessagesWithDelay(ChatMessage[] messages)
    {
        foreach (ChatMessage message in messages)
        {
            if (ChatRoomUI.Instance != null)
            {
                ChatRoomUI.Instance.AddMessage(message);
            }

            yield return new WaitForSeconds(0.05f);
        }
    }

    private int EstimateMessageSize(ChatMessage message)
    {
        int size = 0;

        size += sizeof(ulong);

        if (!string.IsNullOrEmpty(message.playerName))
            size += System.Text.Encoding.UTF8.GetByteCount(message.playerName);

        if (!string.IsNullOrEmpty(message.text))
            size += System.Text.Encoding.UTF8.GetByteCount(message.text);

        if (message.imageData != null)
            size += message.imageData.Length;

        if (!string.IsNullOrEmpty(message.imageFileName))
            size += System.Text.Encoding.UTF8.GetByteCount(message.imageFileName);

        // A little safety margin for Mirror's
        // serialization overhead.
        size += 1024;

        return size;
    }

    // =========================
    // CLIENT RECEIVING
    // =========================

    private void ReceiveChatMessage(ChatMessage message)
    {
        ChatRoomUI.Instance.AddMessage(message);
    } 
}

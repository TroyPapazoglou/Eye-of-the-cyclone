using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SteamAvatarCache : MonoBehaviour
{
    public static SteamAvatarCache Instance;

    private Callback<AvatarImageLoaded_t> avatarLoadedCallback;
    private Callback<PersonaStateChange_t> personaStateChangeCallback;

    private readonly Dictionary<CSteamID, Sprite> cache = new();
    private readonly Dictionary<CSteamID, List<Action<Sprite>>> pending = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;     

        avatarLoadedCallback = Callback<AvatarImageLoaded_t>.Create(OnAvatarLoaded);

        personaStateChangeCallback = Callback<PersonaStateChange_t>.Create(OnPersonaStateChanged);
    }

    private void Start()
    {
        if(SteamManager.Initialized)
        {
            RequestLocalAvatar(null);
        }
        else
        {
            SteamManager.Instance.OnSteamInitialized += HandleSteamInitialized;
        }
    }

    private void OnDestroy()
    {
        if(SteamManager.Instance != null)
        {
            SteamManager.Instance.OnSteamInitialized -= HandleSteamInitialized;
        }
    }

    private void HandleSteamInitialized()
    {
        RequestLocalAvatar(null);
    }

    public void RequestAvatar(CSteamID id, Action<Sprite> onReady)
    {
        if (cache.TryGetValue(id, out Sprite sprite))
        {
            onReady?.Invoke(sprite);
            return;
        }

        if (!pending.ContainsKey(id))
            pending[id] = new List<Action<Sprite>>();

        if (onReady != null)
            pending[id].Add(onReady);

        TryLoadAvatar(id);
    }

    public void RequestLocalAvatar(Action<Sprite> onReady)
    {
        RequestAvatar(SteamUser.GetSteamID(), onReady);
    }

    private void TryLoadAvatar(CSteamID id)
    {
        int imageId = SteamFriends.GetLargeFriendAvatar(id);

        //Debug.Log($"[Avatar] GetLargeFriendAvatar({id}) = {imageId}");

        // 0 = user has no avatar.
        if (imageId == 0)
        {
            Debug.Log($"[Avatar] User {id} has no avatar.");

            CompleteWithoutAvatar(id);
            return;
        }

        // -1 = Steam is downloading the avatar.
        if (imageId == -1)
        {
            Debug.Log($"[Avatar] Avatar for {id} is still loading.");

            SteamFriends.RequestUserInformation(id,false);

            return;
        }

        // Positive image ID = avatar is ready.
        ProcessImage(id, imageId);
    }

    private void OnAvatarLoaded(AvatarImageLoaded_t callback)
    {
        Debug.Log($"[Avatar] AvatarImageLoaded: " + $"{callback.m_steamID} / " + $"{callback.m_iImage}"
        );

        // Steam tells us the image finished loading.
        TryLoadAvatar(callback.m_steamID);
    }

    private void OnPersonaStateChanged(PersonaStateChange_t callback)
    {
        CSteamID id = new CSteamID(callback.m_ulSteamID);

        //Debug.Log($"[Avatar] PersonaStateChanged: {id}");
        
        if (pending.ContainsKey(id))
        {
            TryLoadAvatar(id);
        }
    }

    private void ProcessImage(CSteamID id, int imageId)
    {
        if (!SteamUtils.GetImageSize(imageId, out uint width, out uint height))
        {
            Debug.LogError($"[Avatar] Failed to get image size for {id}");

            return;
        }

        byte[] data = new byte[width * height * 4];

        if (!SteamUtils.GetImageRGBA(
                imageId,
                data,
                data.Length))
        {
            Debug.LogError($"[Avatar] Failed to get RGBA data for {id}");

            return;
        }

        Texture2D texture =
            new Texture2D(
                (int)width,
                (int)height,
                TextureFormat.RGBA32,
                false
            );

        texture.LoadRawTextureData(data);
        texture.Apply();

        Sprite sprite =
            Sprite.Create(
                texture,
                new Rect(
                    0,
                    0,
                    width,
                    height
                ),
                Vector2.one * 0.5f
            );

        sprite.name = $"SteamAvatar_{id}";

        cache[id] = sprite;

        //Debug.Log($"[Avatar] Cached avatar for {id}");

        if (!pending.TryGetValue(id, out List<Action<Sprite>> callbacks))
        {
            return;
        }

        foreach (Action<Sprite> callback in callbacks)
        {
            callback?.Invoke(sprite);
        }

        pending.Remove(id);
    }

    private void CompleteWithoutAvatar(CSteamID id)
    {
        if (!pending.TryGetValue(id, out List<Action<Sprite>> callbacks))
        {
            return;
        }

        foreach (Action<Sprite> callback in callbacks)
        {
            callback?.Invoke(null);
        }

        pending.Remove(id);
    }
}

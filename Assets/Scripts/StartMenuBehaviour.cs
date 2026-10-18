using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartMenuBehaviour : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI usernameText;
    [SerializeField] private Image avatarImage;

    private void Start()
    {
        if (SteamManager.Instance == null)
        {
            Debug.LogError("SteamManager does not exist!");
            return;
        }

        if (SteamManager.Initialized)
        {
            ShowLocalSteamProfile();
        }
        else
        {
            SteamManager.Instance.OnSteamInitialized += ShowLocalSteamProfile;
        }
    }

    private void OnDestroy()
    {
        if (SteamManager.Instance != null)
            SteamManager.Instance.OnSteamInitialized -= ShowLocalSteamProfile;
    }

    private void OnDisable()
    {
        if (SteamManager.Instance != null)
            SteamManager.Instance.OnSteamInitialized -= ShowLocalSteamProfile;
    }

    private void ShowLocalSteamProfile()
    {     
        CSteamID steamId = SteamUser.GetSteamID();      

        usernameText.text = SteamFriends.GetPersonaName();

        SteamAvatarCache.Instance.RequestAvatar(
            steamId,
            sprite =>
            {
                avatarImage.sprite = sprite;
            });
    }
}

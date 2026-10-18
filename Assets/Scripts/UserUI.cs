using PrimeTween;
using Steamworks;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserUI : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private Image avatarImage;
    [SerializeField] private Image borderImage;
    [SerializeField] private GameObject administratorText;

    [Header("Game")]
    [SerializeField] private Button startGameBtn;

    [Header("Winner")]
    [SerializeField] private GameObject crownImage;
    [SerializeField] private ParticleSystem particleCrown;
    [SerializeField] private RectTransform shineImage;

    private CSteamID steamId;

    private bool isLocalHost;

    private Vector3 crownRestPosition;
    private Quaternion crownRestRotation;

    public bool HasWinnerCrown { get; private set; }

    public PlayerNetworkObject Player { get; private set; }


    private void Awake()
    {
        crownRestPosition = crownImage.transform.localPosition;
        crownRestRotation = crownImage.transform.localRotation;
    }

    private void OnDestroy()
    {
        if (Player != null)
        {
            Player.OnHostChangedEvent -= UpdateHostUI;
        }

        if (isLocalHost)
        {
            PCBehaviour.OnApplicationGameStateChanged -=
                UpdateStartButton;
        }
    }

    public void Initialise(PlayerNetworkObject player)
    {
        Debug.Log($"UI Initialised with playerName='{player.playerName}'");

        Player = player;

        usernameText.text = player.playerName;      

        steamId = new CSteamID(player.steamId);

        UpdateHostUI(player.isHost);

        player.OnHostChangedEvent += UpdateHostUI;

        isLocalHost = player.isHost && player == PlayerNetworkObject.LocalPlayer;

        if (isLocalHost)
        {
            startGameBtn.interactable = isLocalHost;
            startGameBtn.onClick.AddListener(PCBehaviour.Instance.StartCurrentApplication);

            PCBehaviour.OnApplicationGameStateChanged += UpdateStartButton;
        }

        SteamAvatarCache.Instance.RequestAvatar(steamId, OnAvatarReady);
    }

    private void UpdateHostUI(bool isHost)
    {
        administratorText.SetActive(isHost);       
    }

    private void UpdateStartButton(bool gameActive)
    {
        if (!isLocalHost)
            return;

        startGameBtn.interactable = !gameActive;
    }

    private void OnAvatarReady(Sprite sprite)
    {
        avatarImage.sprite = sprite;
    }

    public void SetWinner(bool winner)
    {
        crownImage.SetActive(winner);

        borderImage.color = winner
            ? new Color32(255, 183, 67, 255)
            : Color.white;
    }

    public void SetWinnerBorder(bool winner)
    {       

        borderImage.color = winner
            ? new Color32(255, 183, 67, 255)
            : Color.white;
    }

  

    public void PlayWinnerCrownAnimation(Action onLand)
    {
        crownImage.SetActive(true);

        Transform crown = crownImage.transform;

        crown.SetLocalPositionAndRotation(
            crownRestPosition + Vector3.up * 0.08f,
            Quaternion.Euler(0, 0, -35f));

        crown.localScale = Vector3.zero;


        Sequence.Create()

            .Group(
                Tween.LocalPosition(
                    crown,
                    crownRestPosition,
                    0.48f,
                    ease: Ease.InQuad))

            .Group(
                Tween.LocalRotation(
                    crown,
                    crownRestRotation,
                    0.48f,
                    ease: Ease.OutQuad))

            .Group(
                Tween.Scale(
                    crown,
                    Vector3.one * 1.25f,
                    0.42f,
                    ease: Ease.OutBack))

            .Chain(
                Tween.Scale(
                    crown,
                    Vector3.one,
                    0.1f,
                    ease: Ease.OutQuad))

            // Crown has landed
            .ChainCallback(() =>
            {
                onLand?.Invoke();
            });
    }

    public IEnumerator PlayChampionAnimation()
    {
        yield return new WaitForSeconds(1f);

        particleCrown.Play();

        PlayShine();
        
    }

    public void PlayShine()
    {
        Vector3 startPos = shineImage.localPosition;

        Tween.LocalPosition(
            shineImage,
            new Vector3(0.02f, 0.02f, 0f),
            0.8f,
            ease: Ease.Linear).OnComplete(() => shineImage.localPosition = startPos);
    }

   
}




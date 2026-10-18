using PrimeTween;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


public class PCScreenUIManager : MonoBehaviour
{

    [SerializeField] private List<UIScreen> uiScreens;

    [SerializeField] private UserUI userEntryPrefab;
    [SerializeField] private Transform playerListParent;

    [SerializeField] private GameObject hostLobbyMsg;
    [SerializeField] private GameObject turnLogBtn;

    [SerializeField] private VerticalLayoutGroup layout;
    [SerializeField] private Transform winnerOverlay;

    [SerializeField]
    private ParticleSystem confetti;


    private readonly Dictionary<uint, UserUI> playerEntries = new();

    private UserUI currentWinner;

    public event Action OnWinnerPresented;

    private PCApplicationData currentLobbyApplication;

    public static PCScreenUIManager Instance { get; private set; }
      

    public enum ScreenType
    {
        SteamWarning,
        Menu,
        Lobby,
        LobbyWarning
    }

    [System.Serializable]
    public class UIScreen
    {
        public ScreenType type;
        public GameObject screen;
    }

    private void Awake()
    {
        Instance = this;
    }


    private void Start()
    {
        SteamLobbyManager.Instance.OnLobbyClosed += HideAllUI;
        SteamLobbyManager.Instance.OnFailToHostLobby += DisplayWarning;

        SteamManager.Instance.OnFizzySteamWorksStateChanged += HandleSteamState;

        CycloneNetworkManager.OnDisconnected += HideAllUI;

        GameEvents.OnNotEnoughPlayersNeeded += DisplayLobbyWarning;
        CycloneGameManager.OnGameStart += GameStart;
        GameEvents.OnAnyGameEnd += GameEnd;


    }

    private void OnDestroy()
    {      
        SteamLobbyManager.Instance.OnLobbyClosed -= HideAllUI;
        SteamLobbyManager.Instance.OnFailToHostLobby -= DisplayWarning;

        SteamManager.Instance.OnFizzySteamWorksStateChanged -= HandleSteamState;

        CycloneNetworkManager.OnDisconnected -= HideAllUI;

        GameEvents.OnNotEnoughPlayersNeeded -= DisplayLobbyWarning;
        CycloneGameManager.OnGameStart -= GameStart;
        GameEvents.OnAnyGameEnd -= GameEnd;
    }

    private void OnEnable()
    {
        PlayerNetworkObject.OnPlayerReady += AddPlayerUI;
        PlayerNetworkObject.OnPlayerLeft += RemovePlayerUI;
    }

    private void OnDisable()
    {
        PlayerNetworkObject.OnPlayerReady -= AddPlayerUI;
        PlayerNetworkObject.OnPlayerLeft -= RemovePlayerUI;
    }

    private void HandleSteamState(bool running)
    {
        if (running)
            Debug.Log("running steam!");        
        else
            DisplayWarning();
    }

    private void AddPlayerUI(PlayerNetworkObject player)
    {
        if (playerEntries.ContainsKey(player.netId))
            return;

        PCApplicationData application = PCBehaviour.Instance.GetApplicationData(player.application);

        PCBehaviour.Instance.OpenApplication(application);

        if (application == null)
        {
            Debug.LogWarning("Cannot create player UI because no application is open.");

            return;
        }

        if (application.userEntryPrefab == null)
        {
            Debug.LogWarning($"No user entry prefab assigned for " + $"{application.application}");

            return;
        }

        UserUI entry = Instantiate(application.userEntryPrefab, playerListParent);

        entry.Initialise(player);

        playerEntries.Add(player.netId, entry);

        DisplayLobby();
    }

    private void RemovePlayerUI(PlayerNetworkObject player)
    {
        if (!playerEntries.TryGetValue(player.netId, out UserUI entry))
            return;

        Destroy(entry.gameObject);
        playerEntries.Remove(player.netId);     
    }

    public UserUI GetPlayerUI(uint netId)
    {
        if (playerEntries.TryGetValue(netId, out UserUI entry))
            return entry;

        return null;
    }


    private void DisplayWarning()
    {
        DisplayUI(ScreenType.SteamWarning);
    }
    
    public void DisplayLobby()
    {
        DisplayUI(ScreenType.Lobby);

        bool isHost = PlayerNetworkObject.LocalPlayer != null &&
                 PlayerNetworkObject.LocalPlayer.isHost;

        hostLobbyMsg.SetActive(isHost);
    }

    public void DisplayLobbyWarning()
    {
        ShowUI(ScreenType.LobbyWarning);
    }

    public void DisplayUI(ScreenType screenType)
    {
        foreach (var ui in uiScreens)
        {
            ui.screen.SetActive(ui.type == screenType);
        }
    }

    public void ShowUI(ScreenType screenType)
    {
        foreach(var ui in uiScreens)
        {
            if (ui.type == screenType)
                ui.screen.SetActive(true);
        }
    }

    public void HideAllUI()
    {
        foreach (var ui in uiScreens)
        {           
            ui.screen.SetActive(false);
        }
        turnLogBtn.SetActive(false);
    }
   
    private void GameStart()
    {
        hostLobbyMsg.SetActive(false);
        turnLogBtn.SetActive(true);
    }

    private void GameEnd(PlayerNetworkObject player)
    {
        hostLobbyMsg.SetActive(true);
        turnLogBtn.SetActive(false);

        PlayWinnerAnimation(player);
    }

    public void PlayConfetti()
    {
        confetti.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        confetti.Play();
    }


    public void PlayWinnerAnimation(PlayerNetworkObject winner)
    {
        if (!playerEntries.TryGetValue(winner.netId, out UserUI winnerUI))
            return;

        bool isWinStreak = currentWinner == winnerUI;

        if (!isWinStreak && currentWinner != null)
        {
            currentWinner.SetWinner(false);
        }

        StartCoroutine(WinnerAnimationRoutine(winnerUI, isWinStreak));

        currentWinner = winnerUI;
    }

    private IEnumerator WinnerAnimationRoutine(UserUI winnerUI, bool isWinStreak)
    {
        yield return null;

        PlayConfetti();
        winnerUI.SetWinnerBorder(true);

        Vector3 startPosition = winnerUI.transform.position;
        Vector3 startScale = winnerUI.transform.localScale;

        Transform originalParent = winnerUI.transform.parent;
        int originalIndex = winnerUI.transform.GetSiblingIndex();

        layout.enabled = false;       

        winnerUI.transform.SetParent(winnerOverlay, true);

        Vector3 centre =
            CameraManager.Instance.GetScreenCenterWorldPosition(.2f);

        const float moveDuration = 0.55f;     


        Vector3 winnerScale = startScale * 1.4f;
        Vector3 impactScale = startScale * 1.75f;

        Sequence.Create()

            // Move into centre
            .Group(
                Tween.Position(
                    winnerUI.transform,
                    centre,
                    moveDuration,
                    ease: Ease.OutBack))

            // Initial pop
            .Group(
                Tween.Scale(
                    winnerUI.transform,
                    impactScale,
                    0.25f,
                    ease: Ease.OutBack))           

            // Smoothly settle to final winner size
            .Chain(
                Tween.Scale(
                    winnerUI.transform,
                    winnerScale,
                    0.35f,
                    ease: Ease.OutCubic))

            // Hold winner pose
            .ChainDelay(2f)

            // Return
            .Chain(
                Tween.Position(
                    winnerUI.transform,
                    startPosition,
                    0.5f,
                    ease: Ease.InOutCubic))

            .Group(
                Tween.Scale(
                    winnerUI.transform,
                    startScale,
                    0.5f,
                    ease: Ease.InOutCubic))

            .ChainCallback(() =>
            {
                winnerUI.transform.SetParent(originalParent, true);
                winnerUI.transform.SetSiblingIndex(originalIndex);

                layout.enabled = true;

                LayoutRebuilder.ForceRebuildLayoutImmediate(
                    layout.GetComponent<RectTransform>());

                OnWinnerPresented?.Invoke();
            });
                

        // Crown/champion effect timing
        Tween.Delay(.25f)
            .OnComplete(() =>
            {
                if (isWinStreak)
                {
                    StartCoroutine(winnerUI.PlayChampionAnimation());
                }
                else
                {
                    winnerUI.PlayWinnerCrownAnimation(
                        () => PlayBannerSquash(winnerUI, winnerScale));
                }
            });
    }

    private void PlayBannerSquash(UserUI winnerUI, Vector3 normalScale)
    {
        Vector3 originalPosition = winnerUI.transform.position;

        Vector3 squashPosition = originalPosition + Vector3.down * 0.015f;

        Vector3 squashScale = new Vector3(
            normalScale.x * 1.25f,
            normalScale.y * 0.75f,
            normalScale.z);


        Sequence.Create()

            // Squash down
            .Group(
                Tween.Position(
                    winnerUI.transform,
                    squashPosition,
                    0.18f,
                    ease: Ease.OutQuad))

            .Group(
                Tween.Scale(
                    winnerUI.transform,
                    squashScale,
                    0.18f,
                    ease: Ease.OutQuad))


            // Spring back to original position
            .Chain(
                Tween.Position(
                    winnerUI.transform,
                    originalPosition,
                    0.58f,
                    ease: Ease.OutBack))

            .Group(
                Tween.Scale(
                    winnerUI.transform,
                    normalScale,
                    0.58f,
                    ease: Ease.OutElastic));
    }

  


}

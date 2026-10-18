using Steamworks;
using Unity.VisualScripting;
using UnityEngine;

public class TurnLogManager : MonoBehaviour
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private TurnLogEntry entryPrefab;
    [SerializeField] private GameObject turnLogVisual;

    public static TurnLogManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        CycloneGameManager.OnGameStart += ClearEntries;
        SteamLobbyManager.Instance.OnLobbyClosed += OnLobbyClosed;
        CycloneNetworkManager.OnDisconnected += OnLobbyClosed;
    }

    private void OnDestroy()
    {
        CycloneGameManager.OnGameStart -= ClearEntries;
        SteamLobbyManager.Instance.OnLobbyClosed -= OnLobbyClosed;
        CycloneNetworkManager.OnDisconnected -= OnLobbyClosed;
    }

    public void AddEntry(
        string playerName,
        CSteamID steamId,
        TurnLogAction action,
        CardInfo card)
    {
        TurnLogEntry entry = Instantiate(
            entryPrefab,
            contentParent);

        entry.Setup(
            playerName,
            steamId,
            action,
            card);
    }

    public void ClearEntries()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }

    private void OnLobbyClosed()
    {
        ClearEntries();
        turnLogVisual.SetActive(false);
    }
}

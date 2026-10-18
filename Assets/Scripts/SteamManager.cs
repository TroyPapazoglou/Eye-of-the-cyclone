using Mirror.FizzySteam;
using Steamworks;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SteamManager : MonoBehaviour
{
    public static bool Initialized { get; private set; }   

    public event Action<bool> OnFizzySteamWorksStateChanged;

    public event Action OnSteamInitialized;

    public static SteamManager Instance; 



    [SerializeField] private GameObject fizzySteamworks;

    private void Awake()
    {
        Instance = this;

        IntialiseSteam();
    }

    private void Start()
    {
        AnimationManager.Instance.OnIntroComplete += Setup;
    }

    private void IntialiseSteam()
    {
        if (Initialized)
            return;

        if (!SteamAPI.IsSteamRunning())       
            return;
        
        Initialized = SteamAPI.Init();

        OnSteamInitialized?.Invoke();

        //Debug.Log($"Steam initialized: {Initialized}");
    }

    private void Setup()
    {
        if(Initialized)
        {
            fizzySteamworks.SetActive(true);

            OnFizzySteamWorksStateChanged?.Invoke(true);
        }
        else
        {
            OnFizzySteamWorksStateChanged?.Invoke(false);
        }

       

        
    }

    public bool CheckIfSteamRunning()
    {
        if (Initialized)
            return true;

        if (!SteamAPI.IsSteamRunning())
        {
            return false;
        }
        else
        {
            IntialiseSteam();
            fizzySteamworks.SetActive(true);
            return true;
        }        

    }

    private void Update()
    {
        if (Initialized)
        {
            SteamAPI.RunCallbacks();
        }
    }

    private void OnDestroy()
    {
        if (Initialized)
        {
            SteamAPI.Shutdown();
            Initialized = false;
        }

        AnimationManager.Instance.OnIntroComplete -= Setup;
    }
}
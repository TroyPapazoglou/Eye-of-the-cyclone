using System;
using TMPro;
using UnityEngine;

public class PCClock : MonoBehaviour
{
    [SerializeField] private TMP_Text clockText;

    private DateTime currentTime;
    
    public static PCClock Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        currentTime = DateTime.Now;
        InvokeRepeating(nameof(UpdateClock), 0f, 1f);
    }

    private void UpdateClock()
    {
        currentTime = currentTime.AddSeconds(1);

        clockText.text = currentTime.ToString("h:mm tt");
    }

    public DateTime GetCurrentTime()
    {
        return currentTime;
    }

    public string GetTime()
    {
        return currentTime.ToString("h:mm tt");
    }

    public long GetTimestamp()
    {
        return new DateTimeOffset(currentTime).ToUnixTimeSeconds();
    }
}

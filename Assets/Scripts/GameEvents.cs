using Steamworks;
using System;
using UnityEngine;

public static class GameEvents
{
    public static event Action<PlayerNetworkObject> OnAnyGameEnd;

    public static event Action OnNotEnoughPlayersNeeded;

    public static void InvokeNotEnoughPlayersNeeded()
    {
        OnNotEnoughPlayersNeeded?.Invoke();
    }

    public static void InvokeOnAnyGameEnd(PlayerNetworkObject winner)
    {
        OnAnyGameEnd?.Invoke(winner);
    }
}
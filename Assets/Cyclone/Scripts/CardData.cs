using System;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;


public class CardData : MonoBehaviour
{
    public CardInfo card;
    public CardVisualState State { get; private set; }

    public event Action<CardVisualState> OnStateChanged;

    public void SetState(CardVisualState newState)
    {
        State = newState;

        OnStateChanged?.Invoke(newState);

        if (newState == CardVisualState.Hand ||
            newState == CardVisualState.Backup)
        {
            PlayerHandVisuals.Instance.RefreshCardPosition(
                GetComponent<HoverableCard>());
        }
    }

    public override string ToString()
    {
        return card.ToString();
    }
}
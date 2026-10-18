using Mirror.Examples.Common;
using PrimeTween;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandVisuals : MonoBehaviour
{
    [SerializeField] private float hoverHeight = 0.03f;
    [SerializeField] private float spreadAmount = 0.02f;
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float animationTime = 0.15f;

    private readonly List<HoverableCard> cards = new();

    private readonly Dictionary<HoverableCard, Vector3> restingPositions = new();
    private readonly Dictionary<HoverableCard, Quaternion> restingRotations = new();

    private HoverableCard hoveredCard;

    public static PlayerHandVisuals Instance;

    private bool hoverEnabled = true;

    private void Awake()
    {
        Instance = this;
    }

    public void EnableHoverForAllCards()
    {
        foreach (HoverableCard card in cards)
        {
            if (card == null)
                continue;

            card.SetHoverEnabled(true);
        }
    }

    public void RefreshCardPosition(HoverableCard card)
    {
        if (!cards.Contains(card))
        {
            RegisterCard(card);         
        }

        restingPositions[card] = card.transform.localPosition;
        restingRotations[card] = card.transform.localRotation;

        card.SetRestingTransform();
    }

    public void RegisterCard(HoverableCard card)
    {
        if (cards.Contains(card))
            return;

        cards.Add(card);

        restingPositions[card] = card.transform.localPosition;
        restingRotations[card] = card.transform.localRotation;

        card.SetRestingTransform();
    }


    public void UnregisterCard(HoverableCard card)
    {
        if (card == null)
            return;

        cards.Remove(card);

        restingPositions.Remove(card);
        restingRotations.Remove(card);

        if (hoveredCard == card)
            hoveredCard = null;

        UpdateLayout();
    }

    public void UnregisterAllCards()
    {
        foreach (HoverableCard card in cards)
        {
            if (card == null)
                continue;

            card.ResetHover();
        }

        hoveredCard = null;

        cards.Clear();

        restingPositions.Clear();
        restingRotations.Clear();
    }

    public void SetHoverEnabled(bool enabled)
    {
        hoverEnabled = enabled;

        if (!enabled)
        {
            ResetHover();
        }
    }


    public void SetHovered(HoverableCard card)
    {
        if (!hoverEnabled)
            return;

        hoveredCard = card;
        UpdateLayout();
    }

    public void ClearHovered(HoverableCard card)
    {
        if (hoveredCard != card)
            return;

        hoveredCard = null;
        UpdateLayout();
    }  
    
    private void UpdateLayout()
    {
        foreach (HoverableCard card in cards)
        {
            Vector3 targetPos = restingPositions[card];
            Quaternion targetRot = restingRotations[card];
            Vector3 targetScale = Vector3.one;

            if (hoveredCard != null)
            {
                if (card == hoveredCard)
                {
                    targetPos += Vector3.up * hoverHeight;

                    targetRot = GetHoveredRotation(card);

                    targetScale *= hoverScale;
                }
                else
                {
                    int hoveredIndex = cards.IndexOf(hoveredCard);
                    int myIndex = cards.IndexOf(card);

                    if (myIndex < hoveredIndex)
                        targetPos += Vector3.left * spreadAmount;
                    else
                        targetPos += Vector3.right * spreadAmount;
                }
            }

            card.PlayHover(
                targetPos,
                targetRot,
                targetScale,
                animationTime);
        }
    }

    private Quaternion GetHoveredRotation(HoverableCard card)
    {
        Vector3 direction =
            CameraManager.Instance.transform.position -
            card.transform.position;

        direction.y = 0f;     

        Quaternion lookRotation = Quaternion.LookRotation(direction);
            
        lookRotation *= Quaternion.Euler(0f, 90f, -45f);

        return lookRotation;
    }

    public void ResetHover()
    {
        hoveredCard = null;

        foreach (HoverableCard card in cards)
        {
            if (card == null)
                continue;

            Tween.StopAll(card.transform);

            Tween.LocalPosition(
                card.transform,
                restingPositions[card],
                animationTime);

            Tween.LocalRotation(
                card.transform,
                restingRotations[card],
                animationTime);

            Tween.Scale(
                card.transform,
                Vector3.one,
                animationTime);
        }
    }  
}

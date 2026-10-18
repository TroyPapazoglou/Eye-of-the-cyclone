using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCustomisationMachine : MonoBehaviour
{
    public enum CustomisationPart
    {
        Body,
        LeftEye,
        RightEye,
        LeftHand,
        RightHand
    }

    public void ChangeSelection(PlayerNetworkObject player, int direction)
    {
        if (player == null)
            return;

        GameplayPlayer gameplayPlayer = player.Sandbox.GameplayPlayer;

        if (gameplayPlayer == null)
            return;

        CustomisationPart currentPart = gameplayPlayer.CurrentCustomisationPart;

        int partCount =  System.Enum.GetValues(typeof(CustomisationPart)).Length;

        int currentIndex = (int)currentPart;

        currentIndex += direction;

        if (currentIndex < 0)
            currentIndex = partCount - 1;

        if (currentIndex >= partCount)
            currentIndex = 0;

        CustomisationPart newPart = (CustomisationPart)currentIndex;

        gameplayPlayer.SetCustomisationPart(newPart);     
    }

    public void SelectColour(PlayerNetworkObject player, int colourIndex)
    {
        if (player == null)
            return;

        if (colourIndex < 0 || colourIndex >= PlayerColours.Count)
        {
            Debug.LogWarning($"Invalid colour index: {colourIndex}"
            );

            return;
        }

        GameplayPlayer gameplayPlayer = player.Sandbox.GameplayPlayer;

        if (gameplayPlayer == null)
            return;

        CustomisationPart currentPart = gameplayPlayer.CurrentCustomisationPart;

        PlayerAppearance.AppearancePart appearancePart = ConvertPart(currentPart);

        PlayerAppearance playerAppearance = gameplayPlayer.PlayerAppearance;

        if (playerAppearance == null)
        {
            Debug.LogWarning(
                $"Could not find PlayerAppearance on {player.name}."
            );

            return;
        }

        playerAppearance.SetColourServer( appearancePart, (byte)colourIndex);       
    }

    private PlayerAppearance.AppearancePart ConvertPart(CustomisationPart part)
    {
        return part switch
        {
            CustomisationPart.Body =>
                PlayerAppearance.AppearancePart.Body,

            CustomisationPart.LeftEye =>
                PlayerAppearance.AppearancePart.LeftEye,

            CustomisationPart.RightEye =>
                PlayerAppearance.AppearancePart.RightEye,

            CustomisationPart.LeftHand =>
                PlayerAppearance.AppearancePart.LeftHand,

            CustomisationPart.RightHand =>
                PlayerAppearance.AppearancePart.RightHand,

            _ =>
                PlayerAppearance.AppearancePart.Body
        };
    }

    public string GetCurrentPartDisplay(PlayerNetworkObject player)
    {
        if (player == null || player.Sandbox == null || player.Sandbox.GameplayPlayer == null)
        {
            return "";
        }

        return player.Sandbox.GameplayPlayer.CurrentCustomisationPart switch
        {
            CustomisationPart.Body =>
                "Body",

            CustomisationPart.LeftEye =>
                "Left Eye",

            CustomisationPart.RightEye =>
                "Right Eye",

            CustomisationPart.LeftHand =>
                "Left Hand",

            CustomisationPart.RightHand =>
                "Right Hand",

            _ =>
                ""
        };
    }
}

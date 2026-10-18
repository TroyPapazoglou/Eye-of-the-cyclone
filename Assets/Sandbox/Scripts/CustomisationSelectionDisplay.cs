using TMPro;
using UnityEngine;

public class CustomisationSelectionDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text selectionText;

    private GameplayPlayer localPlayer;

    private void OnEnable()
    {
        GameplayPlayer.OnLocalPlayerReady += HandleLocalPlayerReady;
    }

    private void Start()
    {
        // In case the local player already exists before this object starts.
        if (GameplayPlayer.LocalPlayer != null)
        {
            HandleLocalPlayerReady(GameplayPlayer.LocalPlayer);
        }
    }

    private void OnDisable()
    {
        GameplayPlayer.OnLocalPlayerReady -= HandleLocalPlayerReady;

        if (localPlayer != null)
            localPlayer.OnCustomisationPartChangedEvent -= HandlePartChanged;
    }

    private void HandleLocalPlayerReady(GameplayPlayer player)
    {
        if (localPlayer != null)
            localPlayer.OnCustomisationPartChangedEvent -= HandlePartChanged;

        localPlayer = player;

        localPlayer.OnCustomisationPartChangedEvent += HandlePartChanged;

        UpdateDisplay(localPlayer.CurrentCustomisationPart);
    }

    private void HandlePartChanged(PlayerCustomisationMachine.CustomisationPart part)
    {
        UpdateDisplay(part);
    }

    private void UpdateDisplay(PlayerCustomisationMachine.CustomisationPart part)
    {
        if (selectionText == null)
            return;

        selectionText.text = part switch
        {
            PlayerCustomisationMachine.CustomisationPart.Body =>
                "Body",

            PlayerCustomisationMachine.CustomisationPart.LeftEye =>
                "Left Eye",

            PlayerCustomisationMachine.CustomisationPart.RightEye =>
                "Right Eye",

            PlayerCustomisationMachine.CustomisationPart.LeftHand =>
                "Left Hand",

            PlayerCustomisationMachine.CustomisationPart.RightHand =>
                "Right Hand",

            _ => ""
        };
    }
}

using Mirror;
using Mirror.BouncyCastle.Asn1.Mozilla;
using UnityEngine;

public class PlayerAppearance : NetworkBehaviour
{
    public enum AppearancePart
    {
        Body,
        LeftEye,
        RightEye,
        LeftHand,
        RightHand
    }

    [Header("Renderers")]
    [SerializeField] private Renderer bodyRenderer;
    [SerializeField] private Renderer leftEyePupilRenderer;
    [SerializeField] private Renderer rightEyePupilRenderer;
    [SerializeField] private Renderer leftHandRenderer;
    [SerializeField] private Renderer rightHandRenderer;
    [SerializeField] private Renderer leftEyeRenderer;
    [SerializeField] private Renderer rightEyeRenderer;

    [Header("Default Colour")]
    [SerializeField] private int defaultColourIndex = 1;

    private const byte UninitializedColour = 255;

    [Header("Networked Colours")]
    [SyncVar(hook = nameof(OnBodyColourChanged))]
    private byte bodyColourIndex = UninitializedColour;

    [SyncVar(hook = nameof(OnLeftEyeColourChanged))]
    private byte leftEyeColourIndex = UninitializedColour;

    [SyncVar(hook = nameof(OnRightEyeColourChanged))]
    private byte rightEyeColourIndex = UninitializedColour;

    [SyncVar(hook = nameof(OnLeftHandColourChanged))]
    private byte leftHandColourIndex = UninitializedColour;

    [SyncVar(hook = nameof(OnRightHandColourChanged))]
    private byte rightHandColourIndex = UninitializedColour;

    [SerializeField]
    private string colourProperty = "_BaseColor";

    public GameObject Body => bodyRenderer.gameObject;
    public GameObject LeftEye => leftEyeRenderer.gameObject;
    public GameObject RightEye => rightEyeRenderer.gameObject;
    public GameObject RightEyePupil => rightEyePupilRenderer.gameObject;
    public GameObject LeftEyePupil => leftEyePupilRenderer.gameObject;

   




    //public override void OnStartServer()
    //{
    //    base.OnStartServer();

    //    bodyColourIndex = (byte)defaultColourIndex;
    //    leftEyeColourIndex = 0;
    //    rightEyeColourIndex = 0;
    //    leftHandColourIndex = (byte)defaultColourIndex;
    //    rightHandColourIndex = (byte)defaultColourIndex;
    //}


    //public override void OnStartClient()
    //{
    //    base.OnStartClient();

    //    ApplyAllColours();
    //}


    // =========================================================
    // SYNCVAR HOOKS
    // =========================================================

    private void OnBodyColourChanged(byte oldValue, byte newValue)
    {
        ApplyColour(bodyRenderer, newValue);
    }

    private void OnLeftEyeColourChanged(byte oldValue, byte newValue)
    {
        ApplyColour(leftEyePupilRenderer, newValue);
    }

    private void OnRightEyeColourChanged(byte oldValue, byte newValue)
    {
        ApplyColour(rightEyePupilRenderer, newValue);
    }

    private void OnLeftHandColourChanged(byte oldValue, byte newValue)
    {
        ApplyColour(leftHandRenderer, newValue);
    }

    private void OnRightHandColourChanged(byte oldValue, byte newValue)
    {
        ApplyColour(rightHandRenderer, newValue);
    }


    // =========================================================
    // APPLY COLOURS
    // =========================================================

    private void ApplyAllColours()
    {
        ApplyColour(bodyRenderer, bodyColourIndex);
        ApplyColour(leftEyePupilRenderer, leftEyeColourIndex);
        ApplyColour(rightEyePupilRenderer, rightEyeColourIndex);
        ApplyColour(leftHandRenderer, leftHandColourIndex);
        ApplyColour(rightHandRenderer, rightHandColourIndex);
    }


    private void ApplyColour(Renderer renderer, int colourIndex)
    {
        if (renderer == null)
            return;

        renderer.material.SetColor(
            colourProperty,
            PlayerColours.GetColour(colourIndex)
        );
    }


    // =========================================================
    // PUBLIC
    // =========================================================

    public int GetColourIndex(AppearancePart part)
    {
        return part switch
        {
            AppearancePart.Body => bodyColourIndex,
            AppearancePart.LeftEye => leftEyeColourIndex,
            AppearancePart.RightEye => rightEyeColourIndex,
            AppearancePart.LeftHand => leftHandColourIndex,
            AppearancePart.RightHand => rightHandColourIndex,
            _ => 0
        };
    }

    [Server]
    public void SetColourServer(AppearancePart part, byte colourIndex)
    {
        if (colourIndex >= PlayerColours.Count)
            return;

      //      Debug.Log(
      //    $"[SERVER] Setting {part} to colour {colourIndex} for {netId}"
      //);


        switch (part)
        {
            case AppearancePart.Body:
                bodyColourIndex = colourIndex;
                break;

            case AppearancePart.LeftEye:
                leftEyeColourIndex = colourIndex;
                break;

            case AppearancePart.RightEye:
                rightEyeColourIndex = colourIndex;
                break;

            case AppearancePart.LeftHand:
                leftHandColourIndex = colourIndex;
                break;

            case AppearancePart.RightHand:
                rightHandColourIndex = colourIndex;
                break;
        }
    }
}

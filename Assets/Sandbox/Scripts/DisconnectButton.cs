using Mirror;
using PrimeTween;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class DisconnectButton : WorldButtonAction
{
    [SerializeField] private GameObject glassCase;

    private Tween caseTween;

    public override string HoverText
    {
        get
        {
            if (NetworkServer.active)
            {
                return "Close the session?";
            }

            return "Leave the session?";
        }
    }

    public override void Execute(PlayerNetworkObject player)
    {
        StartCoroutine(CloseLobbyNextFrame());
    }

    private IEnumerator CloseLobbyNextFrame()
    {
        yield return null;

        SteamLobbyManager.Instance.HandleLobbyState();
    }

    public override void OnHoverEnter()
    {
        base.OnHoverEnter();

        FlipOpenCase();
    }

    public override void OnHoverExit()
    {
        base.OnHoverExit();

        FlipCloseCase();
    }

    public void FlipOpenCase()
    {
        if (caseTween.isAlive)
            caseTween.Stop();

        caseTween = Tween.LocalRotation(
            glassCase.transform,
            new Vector3(-90f, 0, 0),
            1,
            Ease.OutElastic
        );
    }

    public void FlipCloseCase()
    {
        if (caseTween.isAlive)
            caseTween.Stop();

        caseTween = Tween.LocalRotation(
           glassCase.transform,
           new Vector3(0, 0, 0),
           .4f,
           Ease.InQuad
       );
    }


}

using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class WorldButton : NetworkInteractable
{
    [SyncVar]
    private bool isPressed;

    [SerializeField] private float interactionCooldown = 0.5f;

    [Header("Button Action")]
    [SerializeField]
    private WorldButtonAction buttonAction;
    
    public string HoverText => buttonAction != null ? buttonAction.HoverText : string.Empty;

    private double lastInteractionTime;
    private Coroutine resetCoroutine;

    [Server]
    public override void Interact(PlayerNetworkObject player)
    {
        if (isPressed)
            return;

        if (NetworkTime.time <
            lastInteractionTime + interactionCooldown)
        {
            return;
        }

        lastInteractionTime = NetworkTime.time;
        isPressed = true;
      
        if (buttonAction != null)
        {
            buttonAction.Execute(player);
        }

        if (!NetworkServer.active)
            return;          

        resetCoroutine = StartCoroutine(ResetButtonAfterDelay());
    }

    private IEnumerator ResetButtonAfterDelay()
    {
        yield return new WaitForSeconds(interactionCooldown);

        if (NetworkServer.active)
            ResetButton();

        resetCoroutine = null;
    }


    [Server]
    private void ResetButton()
    {
        isPressed = false;
    }

    public void OnHoverEnter()
    {
        if (buttonAction != null)
            buttonAction.OnHoverEnter();
    }

    public void OnHoverExit()
    {
        if (buttonAction != null)
            buttonAction.OnHoverExit();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        isPressed = false;
        lastInteractionTime = 0;
        resetCoroutine = null;
    }

    public override void OnStopServer()
    {
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
            resetCoroutine = null;
        }

        base.OnStopServer();
    }



}

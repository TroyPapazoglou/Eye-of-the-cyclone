using Mirror;
using Mirror.Examples.Common;
using System;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class SandboxPlayerNetwork : NetworkBehaviour
{
    private PlayerNetworkObject player;

    private GameplayPlayer gameplayPlayer;

    public GameplayPlayer GameplayPlayer => gameplayPlayer;

    


    private void Awake()
    {
        player = GetComponent<PlayerNetworkObject>();
    }


    public override void OnStartLocalPlayer()
    {
        SandboxAnimator.OnIntroComplete += CmdFinishedSandboxAnimation;
    }

    public override void OnStopClient()
    {
        SandboxAnimator.OnIntroComplete -= CmdFinishedSandboxAnimation;
    }

    [Command]
    public void CmdFinishedSandboxAnimation()
    {
        SandboxManager.Instance.PlayerFinishedSandboxAnimation(connectionToClient);
    }

    [Command]
    public void CmdInteract()
    {
        SandboxManager.Instance.Interact(player);
    }

    [Command]
    public void CmdPunch()
    {
        SandboxManager.Instance.Punch(player);
    }

    [Command]
    public void CmdPickUpBarrel()
    {
        SandboxManager.Instance.PickUpBarrel(player);
    }

    [Command]
    public void CmdThrowBarrel()
    {
        if (gameplayPlayer.interactable == null)
            return;

        SandboxManager.Instance.ThrowBarrel(player);
        player.Sandbox.gameplayPlayer.HandController.SetHoldingItem(false);
    }

    [Command]
    public void CmdApplyExplosionForce(Vector3 direction, float force)
    {
        TargetApplyExplosionForce(connectionToClient, direction, force);
    }


    [TargetRpc]
    public void TargetApplyExplosionForce(NetworkConnectionToClient target, Vector3 direction, float force)
    {
            
        if (GameplayPlayer == null)
        {
            Debug.LogError("[TargetRpc] GameplayPlayer is null.");
            return;
        }

        if (GameplayPlayer.PlayerMovement == null)
        {
            Debug.LogError("[TargetRpc] PlayerMovement is null.");
            return;
        }

        GameplayPlayer.PlayerMovement.ApplyExplosionForce(direction, force);
    }

    [TargetRpc]
    public void TargetMoveToTableSide(NetworkConnectionToClient target, AirHockeyNavigation.TableSide tableSide)
    {
        GameplayPlayer gameplayPlayer = GameplayPlayer;

        if (gameplayPlayer == null)
            return;

        PlayerMovement movement = gameplayPlayer.PlayerMovement;

        if (movement == null || movement.CharacterController == null)
            return;

        CameraType isFrontStand = tableSide == AirHockeyNavigation.TableSide.Front ? CameraType.FrontStand : CameraType.BackStand;

        CameraManager.Instance.SetActiveCamera(isFrontStand);

        CharacterController controller = movement.CharacterController;

        AirHockeyNavigation.Instance.MovePlayerToSide(
            controller.transform,
            controller,
            movement,
            tableSide);

       
    }

    public void TryPlayLocalInteractionAnimation()
    {
        if (gameplayPlayer == null)
            return;

        if (gameplayPlayer.HandController == null)
            return;

        Ray ray = new Ray(
            gameplayPlayer.CameraTarget.position,
            gameplayPlayer.CameraLookAtTarget.transform.forward
        );

        if (!Physics.Raycast(
                ray,
                out RaycastHit hit,
                gameplayPlayer.InteractionRange,
                gameplayPlayer.InteractionLayer,
                QueryTriggerInteraction.Ignore))
        {
            return;
        }

        NetworkInteractable interactable =
            hit.collider.GetComponentInParent<NetworkInteractable>();

        if (interactable == null)
            return;

        if (interactable.HandTarget == null)
            return;

        InteractionHand hand = UnityEngine.Random.value < 0.5f ? InteractionHand.Left : InteractionHand.Right;     

        gameplayPlayer.HandController.PlayInteraction(interactable, hand, HandController.HandPose.Interacting);
    }

    public void TryPlayLocalPunchAnimation()
    {
        if (gameplayPlayer == null)
            return;

        if (gameplayPlayer.HandController == null)
            return;

        Ray ray = new Ray(
            gameplayPlayer.CameraTarget.position,
            gameplayPlayer.CameraLookAtTarget.transform.forward
        );

        if (!Physics.Raycast(
                ray,
                out RaycastHit hit,
                gameplayPlayer.InteractionRange,
                gameplayPlayer.InteractionLayer,
                QueryTriggerInteraction.Ignore))
        {
            return;
        }

        NetworkInteractable interactable = hit.collider.GetComponentInParent<NetworkInteractable>();

        if (interactable == null)
            return;       

        InteractionHand hand = UnityEngine.Random.value < 0.5f ? InteractionHand.Left : InteractionHand.Right;      

        gameplayPlayer.HandController.PlayInteraction(interactable, hand, HandController.HandPose.Punching);
    }

    public void SetGameplayPlayer(GameplayPlayer gameplayPlayer)
    {
        this.gameplayPlayer = gameplayPlayer;
    }

    public bool CanPunch()
    {
        GameplayPlayer gameplayPlayer = GameplayPlayer;

        if (gameplayPlayer == null)
            return false;

        Vector3 origin = gameplayPlayer.InteractionOrigin.position;
        Vector3 direction = gameplayPlayer.InteractionOrigin.forward;

        if (!Physics.Raycast(
            origin,
            direction,
            out RaycastHit hit,
            gameplayPlayer.InteractionRange))
        {
            return false;
        }

        NetworkInteractable interactable = hit.collider.GetComponentInParent<NetworkInteractable>();

        if (interactable == null)
            return false;

        return interactable.CanPunch;
    }

}

using Mirror;
using Unity.Cinemachine;
using UnityEngine;

public class PaddleInteractable : NetworkInteractable 
{
    [SerializeField] private AirHockeyNavigation.TableSide tableSide;

    [Server]
    public override void Interact(PlayerNetworkObject player)
    {
        GameplayPlayer gameplayPlayer = player.Sandbox.GameplayPlayer;

        if (gameplayPlayer == null)
            return;

        player.Sandbox.TargetMoveToTableSide(
            gameplayPlayer.connectionToClient,
            tableSide);
    }
}

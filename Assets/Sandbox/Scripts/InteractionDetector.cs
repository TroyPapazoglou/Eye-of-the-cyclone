using UnityEngine;

public class InteractionDetector : MonoBehaviour
{
    [SerializeField] private GameplayPlayer gameplayPlayer;

    public NetworkInteractable CurrentTarget { get; private set; }   

    private void Update()
    {

        if (gameplayPlayer == null)
        {
            Debug.LogWarning($"[{name}] Detection stopped: gameplayPlayer reference is NULL.");
            return;
        }

        if (GameplayPlayer.LocalPlayer == null)
        {
            Debug.LogWarning($"[{name}] Detection stopped: GameplayPlayer.LocalPlayer is NULL.");
            return;
        }

        if (!GameplayPlayer.LocalPlayer.Player.isLocalPlayer)
        {
            Debug.LogWarning($"[{name}] Detection stopped: local player's NetworkBehaviour is not local.");
            return;
        }

        DetectTarget();
    }

    private void DetectTarget()
    {      

        NetworkInteractable target = null;

        if (Physics.Raycast(
            gameplayPlayer.InteractionOrigin.position,
            gameplayPlayer.InteractionOrigin.forward,
            out RaycastHit hit,
            gameplayPlayer.InteractionRange,
            gameplayPlayer.InteractionLayer,
            QueryTriggerInteraction.Ignore))
        {
            target = hit.collider.GetComponentInParent<NetworkInteractable>();
        }
               
        if (CurrentTarget == target)
            return;

        CurrentTarget = target;

        if (InteractionHUD.Instance != null)
            InteractionHUD.Instance.UpdateHUD(CurrentTarget);
    }  
}

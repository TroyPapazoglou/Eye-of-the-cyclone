using Mirror;
using UnityEngine;

public enum InteractionPositionMode
{
    HandTarget,
    ColliderSurface
}

public abstract class NetworkInteractable : NetworkBehaviour
{  

    [SerializeField] private InteractionPositionMode interactionPositionMode;


    [Header("Hand Interaction")]
    [SerializeField] private Transform handTarget;
    [SerializeField] private Collider interactionCollider;  

    public virtual bool CanPunch => false;

    public Transform HandTarget => handTarget;

    public virtual bool CanBePickedUp()
    {
        return false;
    }
    
    [Server]
    public virtual void PickUp(PlayerNetworkObject player)
    {
    }


    public virtual void Interact(PlayerNetworkObject player)
    {
    }

    [Server]
    public virtual void Punch(PlayerNetworkObject player)
    {
    }

    [Server]
    public virtual void Throw(Vector3 direction)
    {
    }

    public virtual Vector3 GetInteractionPosition(Transform player)
    {
        if (interactionPositionMode == InteractionPositionMode.HandTarget)
        {
            return handTarget.position;
        }

        if (interactionCollider != null)
        {
            return interactionCollider.ClosestPoint(player.position);
        }

        return handTarget != null
            ? handTarget.position
            : transform.position;
    }

    public virtual Quaternion GetInteractionRotation(Transform player)
    {
        if (interactionPositionMode == InteractionPositionMode.HandTarget)
        {
            return handTarget.rotation;
        }

        Vector3 direction = (GetInteractionPosition(player) - player.position).normalized;

        return Quaternion.LookRotation(direction);
    }
}

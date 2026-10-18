using UnityEngine;

public abstract class WorldButtonAction : MonoBehaviour
{

    public virtual string HoverText => null;
    public abstract void Execute(PlayerNetworkObject player);

    public virtual void OnHoverEnter() { }

    public virtual void OnHoverExit() { }
}
using UnityEngine;

[System.Serializable]
public class HandInteractionTarget
{
    public Transform leftHandPoint;
    public Transform rightHandPoint;
}

public enum InteractionHand
{
    Left,
    Right
}

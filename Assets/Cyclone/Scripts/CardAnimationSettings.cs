using PrimeTween;
using UnityEngine;

[System.Serializable]
public class CardAnimationSettings
{
    public float moveDuration = 0.4f;
    public float rotationDuration = 0.2f;

    public Ease moveEase = Ease.OutQuint;
    public Ease rotationEase = Ease.OutQuint;
}

[System.Serializable]
public class FirstCardAnimationSettings
{
    [Header("Move X Settings")]
    public float moveXEndValue = 0.1f;
    public float moveXDuration = 0.5f;
    public Ease moveXEase = Ease.OutQuint;
    public float moveXStartDelay = 0.5f;

    [Header("Rotation Settings")]
    public float rotationDuration = 1f;
    public Ease rotationEase = Ease.OutQuint;
    public float rotationStartDelay = 0.6f;

    [Header("Move Y Settings")]
    public float moveYEndValue = 0f;
    public float moveYDuration = 1f;
    public Ease moveYEase = Ease.OutQuint;
    public float moveYStartDelay = 0.7f;

}

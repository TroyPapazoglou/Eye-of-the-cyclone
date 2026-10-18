using PrimeTween;
using System;
using UnityEngine;

public class SandboxAnimator : MonoBehaviour
{
    public static SandboxAnimator Instance;

    [SerializeField] private Transform sandbox;

    [Header("Sandbox Intro")]
    [SerializeField] private float sanboxAnimationDuration = 1f;

    private Sequence sandboxSequence; 

    public static event Action OnIntroComplete;

    private void Awake()
    {
        Instance = this;
    }

    public void PlayIntroAnimation()
    {
        //ShowSandbox();

        //sandboxSequence.Stop();

        //sandbox.localScale = Vector3.zero;
        //sandbox.localRotation = Quaternion.identity;

        //PlaySandboxIntro();

        OnIntroAnimationComplete();
    }

    private void PlaySandboxIntro()
    {
        sandboxSequence = Sequence.Create();

        sandboxSequence
            .Group(
                Tween.Scale(
                    sandbox,
                    Vector3.one,
                    sanboxAnimationDuration,
                    ease: Ease.OutBack
                )
            )
            .Group(
                Tween.LocalRotation(
                    sandbox,
                    Quaternion.Euler(0f, 360f, 0f),
                    sanboxAnimationDuration,
                    ease: Ease.OutCubic
                )
            ).OnComplete ( OnIntroAnimationComplete );
    }

    private void ShowSandbox()
    {
        sandbox.gameObject.SetActive(true);
    }

    public void HideSandbox()
    {
        sandbox.gameObject.SetActive(false);
    }

    private void OnIntroAnimationComplete()
    {
        OnIntroComplete?.Invoke();
    }
}

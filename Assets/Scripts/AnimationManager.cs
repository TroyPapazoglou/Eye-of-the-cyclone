using PrimeTween;
using System;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    [SerializeField] private GameObject sceneCube;
    [SerializeField] private GameObject floor;
    [SerializeField] private GameObject pc;
    [SerializeField] private BackgroundAnimationManager backgroundAnimationManager;

    public static AnimationManager Instance;

    public event Action OnIntroComplete;

    public event Action OnIntroStart;

    private void Awake()
    {
       Instance = this;
    }

    private void Start()
    {
        PlayIntroAnimation();
    }

    private void PlayIntroAnimation()
    {
        OnIntroStart?.Invoke();
        Tween.Scale(
            sceneCube.transform,
            new Vector3(1, 1, 1),
            1,
            ease: Ease.Linear,
            startDelay: 1f

        ).Chain(
            Tween.Scale(
                sceneCube.transform,
                new Vector3(220, 150, 150),
                3,
                ease: Ease.Linear
            ).Group(
                Tween.Scale(
                    floor.transform,
                    new Vector3(5f, 1, 5f),
                    2.5f,
                    ease: Ease.Linear,
                    startDelay: .45f
                   
                    
                    
            ).Group(
                Tween.Scale(
                    pc.transform,
                    new Vector3(1, 1, 1),
                    2.5f,
                    Ease.InExpo,
                    startDelay: .65f
                    

            ).Group(
                Tween.LocalEulerAngles(
                    pc.transform,
                    startValue: pc.transform.localEulerAngles,
                    endValue: pc.transform.localEulerAngles + new Vector3(0, 360, 0),
                    duration: 2.5f,
                    Ease.InExpo,
                    startDelay: .65f

            ).Chain(
                Tween.ScaleY(
                    pc.transform,
                    1.5f,
                    0.2f,                       
                    Ease.OutExpo                                       
            ).Group(
                Tween.ScaleZ(
                    pc.transform,
                    0.6f,
                    0.2f,
                    Ease.OutExpo

            ).Chain(
                Tween.ScaleY(
                    pc.transform,
                    0.8f,
                    0.35f,
                    Ease.InExpo

                )
            ).Group(
                Tween.ScaleZ(
                    pc.transform,
                    1,
                    0.35f,
                    Ease.InExpo
                )
            
            ).Chain(
                Tween.ScaleY(
                    pc.transform,
                    1,
                    0.2f,
                    Ease.OutExpo
                    )).OnComplete(() =>
                {
                    OnIntroComplete?.Invoke();
                    PlayBackgroundAnimation();
                })
            )
        )))));

    }
        
    private void PlayBackgroundAnimation()
    {
        backgroundAnimationManager.StartCoroutine(backgroundAnimationManager.PlayAnimation());
    }

    
      
}

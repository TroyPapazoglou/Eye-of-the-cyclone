using PrimeTween;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public enum CardSplineType
{
    DeckToPlayPile,
    PlayerToPlayPile,
    PlayerToDiscard,
    PlayPileToDiscard,
    DiscardToDeck,
    DeckToPc,
    PcToPlayPile,
    PcToDiscardPile
}

public class CardSplineAnimator : MonoBehaviour
{
    public static CardSplineAnimator Instance;

    [System.Serializable]
    public class SplineAnimation
    {
        public CardSplineType type;

        public SplineContainer spline;

        [Header("Timing")]
        public float duration = 0.5f;

        [Header("Rotation")]
        [Range(0f, 1f)]
        public float rotationStartTime = 0.4f;      
    }

    [SerializeField]
    private List<SplineAnimation> splines = new();

    private Dictionary<CardSplineType, SplineAnimation> splineLookup;

    private void Awake()
    {
        Instance = this;

        splineLookup = new();

        foreach (SplineAnimation animation in splines)
        {
            splineLookup[animation.type] = animation;
        }
    }

    public void PlaySpline(Transform card, CardSplineType type, Vector3? customStart = null, Vector3? customEnd = null, Quaternion? customRotation = null, Ease ease = Ease.Linear, Action onComplete = null)
    {
        if (!splineLookup.TryGetValue(type, out SplineAnimation animation))
        {
            Debug.LogError($"Missing spline animation: {type}");
            return;
        }

        StartCoroutine(MoveCard(card, animation, customStart, customEnd, customRotation, ease, onComplete)
        );
    }

    IEnumerator MoveCard(Transform card, SplineAnimation animation, Vector3? customStart, Vector3? customEnd, quaternion? customRotation, Ease ease, Action onComplete)
    {
        SplineContainer splineContainer = animation.spline;
        Spline spline = splineContainer.Spline;      

        if (customStart.HasValue)
        {
            BezierKnot startKnot = spline[0];

            startKnot.Position = customStart.Value;

            spline.SetKnot(
                0,
                startKnot
            );
        }
              

        int lastIndex = spline.Count - 1;

        if (customEnd.HasValue)
        {
            BezierKnot endKnot = spline[lastIndex];

            Vector3 localEnd =  splineContainer.transform.InverseTransformPoint(customEnd.Value);

            endKnot.Position = localEnd;

            spline.SetKnot(lastIndex, endKnot);
        }
      
        spline.Evaluate(
            1f,
            out float3 endPos,
            out _,
            out _
        );

        Quaternion startRot = card.rotation;

        Quaternion endRot;      

        if (customRotation.HasValue)
        {           
            endRot = customRotation.Value;
        }
        else
        {            
            Vector3 direction = CameraManager.Instance.StartPos - (Vector3)endPos;

            endRot =  Quaternion.LookRotation(direction, Vector3.up) * Quaternion.Euler(0f, 90f, 0f);
        }

        float timer = 0f;

        while (timer < animation.duration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / animation.duration);

            float easedT = Mathf.Clamp01(Easing.Evaluate(t, ease));


            spline.Evaluate(
                easedT,
                out float3 position,
                out _,
                out _
            );


            card.position = position;


            float rotationT = Mathf.InverseLerp(
                animation.rotationStartTime,
                1f,
                t
            );


            rotationT = Mathf.SmoothStep(
                0f,
                1f,
                rotationT
            );


            card.rotation = Quaternion.Slerp(
                startRot,
                endRot,
                rotationT
            );


            yield return null;
        }


        card.SetPositionAndRotation(
            endPos,
            endRot
        );


        onComplete?.Invoke();
    }
}

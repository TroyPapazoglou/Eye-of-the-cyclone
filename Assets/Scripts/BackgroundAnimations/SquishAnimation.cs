using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;

public class SquishAnimation : MonoBehaviour, IPointerClickHandler
{
    

    private Vector3 originalScale;

    private Sequence clickSequence;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PlayClickAnimation();
    }

    private void PlayClickAnimation()
    {    
        if (clickSequence.isAlive)
        {
            clickSequence.Stop();
        }

        clickSequence = Sequence.Create()
                      
            .ChainCallback(() =>
            {
                transform.localScale = new Vector3(
                    originalScale.x * 0.55f,
                    originalScale.y * 1.65f,
                    originalScale.z);
            })
                       
            .ChainDelay(0.04f)
                       
            .Chain(
                Tween.Scale(
                    transform,
                    new Vector3(
                        originalScale.x * 1.6f,
                        originalScale.y * 0.5f,
                        originalScale.z),
                    0.2f,
                    ease: Ease.OutQuad))
                       
            .Chain(
                Tween.Scale(
                    transform,
                    originalScale,
                    0.4f,
                    ease: Ease.OutElastic));
            }   
    }

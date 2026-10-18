using System.Collections;
using UnityEngine;

public class BackgroundAnimationManager : MonoBehaviour
{
    [SerializeField]
    private BackgroundAnimation[] animations;

    public IEnumerator PlayAnimation()
    {
        while (true)
        {
            BackgroundAnimation anim = animations[Random.Range(0, animations.Length)];

            yield return StartCoroutine(anim.Play());
        }
    }
}

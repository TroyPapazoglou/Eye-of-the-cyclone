using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class RockingObject
{
    public MeshRenderer meshRenderer;
    public StartDirection startDirection;
}

public enum StartDirection
{
    Left,
    Right
}

public class BlinkRotateAnimation : BackgroundAnimation
{  
    [SerializeField] private RockingObject[] objects;

    const float duration = 8f;
    const float interval = 2f;

    public override IEnumerator Play()
    {      

        List<MeshRenderer> objectMeshRends = new();

        foreach (RockingObject obj in objects)
        {
            objectMeshRends.Add(obj.meshRenderer);
        }

        ImageAssignment.Assign(objectMeshRends.ToArray());

        foreach (RockingObject obj in objects)
        {
            obj.meshRenderer.transform.gameObject.SetActive(true);
            StartCoroutine(RockAnimation(obj.meshRenderer.transform.gameObject, obj.startDirection));
        }

        yield return new WaitForSeconds(duration);

        foreach (RockingObject obj in objects)
        {
            obj.meshRenderer.transform.gameObject.SetActive(false);
        }
    }

    private IEnumerator RockAnimation(GameObject obj, StartDirection startDirection)
    {
        bool left = startDirection == StartDirection.Left;

        Vector3 originalEuler = obj.transform.eulerAngles;

        float elapsed = 0f;

        while (elapsed < duration)
        {          

            obj.transform.rotation = Quaternion.Euler(
                originalEuler.x,
                originalEuler.y,
                left ? -25f : 25f);

            left = !left;

            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }
    }
}

using PrimeTween;
using System.Collections;
using UnityEngine;

public class DropRollAnimation : BackgroundAnimation
{  
    [SerializeField] private MeshRenderer[] objects;

    const float duration = 8f;
    const float distance = 4f;

    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;
    }

    public override IEnumerator Play()
    {      

        Vector3[] positions = new Vector3[objects.Length];
        Quaternion[] rotations = new Quaternion[objects.Length];

        float topY = mainCam.ViewportToWorldPoint(
            new Vector3(0.5f, 1f, distance)
        ).y + 2f;    

        for (int i = 0; i < objects.Length; i++)
        { 
            objects[i].transform.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);
            Vector3 newStartPos = new Vector3(position.x , topY, position.z);

            objects[i].transform.position = newStartPos;           

            objects[i].transform.gameObject.SetActive(true);

            positions[i] = objects[i].transform.position;
            rotations[i] = objects[i].transform.rotation;
        }

        ImageAssignment.Assign(objects);

        Vector3 startEuler = objects[0].transform.localEulerAngles;       
             
        Drop(objects[0].transform.gameObject, new Vector3(-2f, 0.75f, 5f), 0f);
        Drop(objects[1].transform.gameObject, new Vector3(0f, 0.75f, 5f), 0.5f);
        Drop(objects[2].transform.gameObject, new Vector3(2f, 0.75f, 5f), 1f);
        
        yield return new WaitForSeconds(3f);

        Roll(objects[2].transform.gameObject, startEuler, 0f);
        Roll(objects[1].transform.gameObject, startEuler, 0.5f);
        Roll(objects[0].transform.gameObject, startEuler, 1f);

        yield return new WaitForSeconds(duration);

        for (int i = 0; i < objects.Length; i++)
        {
            objects[i].transform.SetPositionAndRotation(positions[i], rotations[i]);

            objects[i].transform.gameObject.SetActive(false);
        }
    }

    private void Drop(GameObject obj, Vector3 targetPos, float delay)
    {
        Tween.Position(
            obj.transform,
            targetPos,
            2f,
            ease: Ease.OutQuint,
            startDelay: delay
        );
    }

    private void Roll(GameObject obj, Vector3 startEuler, float delay)
    {
        float targetPosX = mainCam.ViewportToWorldPoint(new Vector3(1f, 0.5f, distance)).x;
        targetPosX += 4f;
     

        Tween.EulerAngles(
            obj.transform,
            startEuler,
            new Vector3(43f, 0f, -360f),
            2f,
            ease: Ease.InExpo,
            startDelay: delay
        ).Group(
            Tween.Position(
                obj.transform,
                new Vector3(targetPosX, 0.75f, 5f),
                2f,
                ease: Ease.InExpo,
                startDelay: delay
            )
        );
    }
}

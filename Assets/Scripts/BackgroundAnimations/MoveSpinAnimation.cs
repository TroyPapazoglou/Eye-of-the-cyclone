using Mirror.BouncyCastle.Utilities;
using PrimeTween;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class MoveSpinAnimation : BackgroundAnimation
{  
    [SerializeField] private MeshRenderer[] target;

    private Camera mainCam;

    const float duration = 8f;
    const float distance = 4f;

    private void Start()
    {
        mainCam = Camera.main;        
    }

    public override IEnumerator Play()
    {

        float spawnPosX = mainCam.ViewportToWorldPoint(new Vector3(1f, 0.5f, distance)).x;
        spawnPosX += 4f;
        Vector3 spawnPos = new Vector3(spawnPosX, target[0].transform.position.y, target[0].transform.position.z);

        target[0].transform.position = spawnPos;      

        target[0].transform.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);
        target[0].transform.gameObject.SetActive(true);

        ImageAssignment.Assign(target);

        Vector3 startEuler = target[0].transform.localEulerAngles;

        float targetPosX = mainCam.ViewportToWorldPoint(new Vector3(0f, 0.5f, distance)).x;
        targetPosX -= 4f;
        Vector3 targetPos = new Vector3(targetPosX, target[0].transform.position.y, target[0].transform.position.z);

        MoveAndSpin(target[0].transform.gameObject, targetPos, startEuler,  0f);

        yield return new WaitForSeconds(duration);

        target[0].transform.SetPositionAndRotation(position, rotation);

        target[0].transform.gameObject.SetActive(false);
        
    }

    private void MoveAndSpin(GameObject obj, Vector3 targetPos, Vector3 startEuler, float delay)
    {
        Tween.Position(
            obj.transform,
            targetPos,
            6f,
            ease: Ease.Linear,
            startDelay: delay
        ).Group(
            Tween.EulerAngles(
                obj.transform,
                startEuler,
                new Vector3(0f, -360f, 0f),
                6f,
                ease: Ease.Linear,
                startDelay: delay
        ));
    }   
}

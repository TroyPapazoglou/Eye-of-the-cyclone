using PrimeTween;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class SnakeAnimation : BackgroundAnimation
{
    [SerializeField] private Transform topRow;
    [SerializeField] private Transform bottomRow;

    [SerializeField] private MeshRenderer[] topQuads;
    [SerializeField] private MeshRenderer[] bottomQuads;

    [SerializeField] private float travelTime = 8f;

    [SerializeField] private float waveHeight = 0.12f;
    [SerializeField] private float waveFrequency = 2f;
  
    [SerializeField] private float distance = 8f;
    [SerializeField] private float horizontalOffset = 4f;

    private Camera mainCam;

    private Vector3 topStartPosition;
    private Vector3 bottomStartPosition;

    private void Start()
    {
        mainCam = Camera.main;
    }

    private void Awake()
    {
        topStartPosition = topRow.position;
        bottomStartPosition = bottomRow.position;
    }

    public override IEnumerator Play()
    {
        topRow.gameObject.SetActive(true); 
        bottomRow.gameObject.SetActive(true); 
        
        ImageAssignment.Assign(topQuads, bottomQuads); 
        
        Transform camTransform = CameraManager.Instance.BackgroundCam.transform;
        Vector3 cameraPosition = camTransform.position; 
        Vector3 cameraForward = camTransform.forward; 
        Vector3 cameraRight = camTransform.right;

        Vector3 animationCenter = cameraPosition + cameraForward * distance; 
               
        float halfHeight = Mathf.Tan( mainCam.fieldOfView * 0.5f * Mathf.Deg2Rad ) * distance; 
                
        float aspect = (float)Screen.width / Screen.height; 
        float halfWidth = halfHeight * aspect;

        float topY = topRow.position.y;
        float bottomY = bottomRow.position.y;

        Vector3 leftEdge = animationCenter - cameraRight * halfWidth;   
        Vector3 rightEdge = animationCenter + cameraRight * halfWidth; 
      
        Vector3 topStart = leftEdge - cameraRight * horizontalOffset;
        topStart.y = topY;

        Vector3 bottomStart = rightEdge + cameraRight * horizontalOffset;
        bottomStart.y = bottomY;

        topRow.position = topStart; 
        bottomRow.position = bottomStart; 
      
        Vector3 topStartPosition = topRow.position;
        Vector3 bottomStartPosition = bottomRow.position; 
      
        Vector3 topEnd = rightEdge + cameraRight * horizontalOffset;
        topEnd.y = topY;
        Vector3 bottomEnd = leftEdge - cameraRight * horizontalOffset;
        bottomEnd.y = bottomY;

        //topRow.position = topStartPosition;
        //bottomRow.position = bottomStartPosition;

        //float leftEdge = mainCam.ViewportToWorldPoint(new Vector3(0f, 0.5f, distance)).x - horizontalOffset;
        //float rightEdge = mainCam.ViewportToWorldPoint(new Vector3(1f, 0.5f, distance)).x + horizontalOffset;

        //topRow.gameObject.SetActive(true);
        //bottomRow.gameObject.SetActive(true);

        //ImageAssignment.Assign(topQuads, bottomQuads);    

        //Vector3 topStart = topRow.position;
        //topStart.x = leftEdge;
        //topRow.position = topStart;

        //Vector3 bottomStart = bottomRow.position;
        //bottomStart.x = rightEdge;
        //bottomRow.position = bottomStart;

        //Vector3 topEnd = topRow.position;
        //topEnd.x = rightEdge;

        //Vector3 bottomEnd = bottomRow.position;
        //bottomEnd.x = leftEdge;       

        Tween.Position(topRow, topEnd, travelTime, ease: Ease.Linear);
        Tween.Position(bottomRow, bottomEnd, travelTime, ease: Ease.Linear);

        float elapsed = 0f;

        while (elapsed < travelTime)
        {
            AnimateWave(topQuads, elapsed);
            AnimateWave(bottomQuads, elapsed + Mathf.PI);

            elapsed += Time.deltaTime;
            yield return null;
        }

        topRow.position = topStartPosition;
        bottomRow.position = bottomStartPosition;

        topRow.gameObject.SetActive(false);
        bottomRow.gameObject.SetActive(false);
    }   

    private void AnimateWave(MeshRenderer[] quads, float time)
    {
        for (int i = 0; i < quads.Length; i++)
        {
            Transform t = quads[i].transform;

            Vector3 pos = t.localPosition;

            float phase = i * 0.65f;

            pos.y = Mathf.Sin(time * waveFrequency + phase) * waveHeight;

            t.localPosition = pos;
        }
    }
}

using PrimeTween;
using System.Collections;
using UnityEngine;

public class WaveRowsAnimation : BackgroundAnimation
{
    [Header("Rows")]
    [SerializeField] private Transform topRowParent;
    [SerializeField] private Transform bottomRowParent;

    [SerializeField] private MeshRenderer[] topQuads;
    [SerializeField] private MeshRenderer[] bottomQuads;

    [Header("Movement")]
    [SerializeField] private float travelTime = 8f;

    [Header("Bounce")]
    [SerializeField] private float bounceHeight = 0.4f;
    [SerializeField] private float bounceSideAmount = 0.15f;
    [SerializeField] private float bounceSpeed = 3f;
    [SerializeField] private float waveOffset = 0.7f;

    [Header("Screen")]
    [SerializeField] private float distance = 8f;
    [SerializeField] private float horizontalOffset = 4f;
   

    private Camera mainCam;

    private void Awake()
    {
        mainCam = Camera.main;
    }

    public override IEnumerator Play()
    {
        topRowParent.gameObject.SetActive(true);
        bottomRowParent.gameObject.SetActive(true);

        ImageAssignment.Assign(topQuads, bottomQuads);
        
        Transform camTransform = CameraManager.Instance.BackgroundCam.transform;
        Vector3 cameraPosition = camTransform.position;
        Vector3 cameraForward = camTransform.forward;
        Vector3 cameraRight = camTransform.right;

        Vector3 animationCenter = cameraPosition + cameraForward * distance;

        float halfHeight = Mathf.Tan(mainCam.fieldOfView * .5f * Mathf.Deg2Rad);

        float aspect = (float)Screen.width / Screen.height;

        float halfWidth = halfHeight * aspect;

        Vector3 leftEdge = animationCenter - cameraRight * halfWidth;
        Vector3 rightEdge = animationCenter + cameraRight * halfWidth;

        Vector3 topStart = leftEdge - cameraRight * horizontalOffset;
        Vector3 bottomStart = rightEdge + cameraRight * horizontalOffset;

        Vector3 topEnd = rightEdge + cameraRight * horizontalOffset;
        Vector3 bottomEnd = leftEdge - cameraRight * horizontalOffset;

        //float leftEdge = mainCam.ViewportToWorldPoint(new Vector3(0f, 0.5f, distance)).x - horizontalOffset;
        //float rightEdge = mainCam.ViewportToWorldPoint(new Vector3(1f, 0.5f, distance)).x + horizontalOffset;

        //Vector3 topStart = topRowParent.position;
        //topStart.x = leftEdge;
        //topRowParent.position = topStart;

        //Vector3 bottomStart = bottomRowParent.position;
        //bottomStart.x = rightEdge;
        //bottomRowParent.position = bottomStart;      

        //Vector3 topEnd = topRowParent.position;
        //topEnd.x = rightEdge;

        //Vector3 bottomEnd = bottomRowParent.position;
        //bottomEnd.x = leftEdge;

        topRowParent.position = topStart;
        bottomRowParent.position = bottomStart;

        Vector3 topStartingPosition = topRowParent.position;
        Vector3 bottomStartingPosition = bottomRowParent.position;

        Tween.Position(topRowParent, topEnd, travelTime, ease: Ease.Linear);
        Tween.Position(bottomRowParent, bottomEnd, travelTime, ease: Ease.Linear);

      


        float elapsed = 0f;

        while (elapsed < travelTime)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / travelTime);           

            AnimateRow(
                topQuads,
                topRowParent.transform.position.x,
                elapsed,
                false);

            AnimateRow(
                bottomQuads,
                bottomRowParent.transform.position.x,
                elapsed,
                true);

            yield return null;
        }

        topRowParent.position = topStartingPosition;
        bottomRowParent.position = bottomStartingPosition;

        topRowParent.gameObject.SetActive(false);
        bottomRowParent.gameObject.SetActive(false);    
}

    private void AnimateRow(MeshRenderer[] quads, float rowX, float elapsed, bool reverse)
    {
        for (int i = 0; i < quads.Length; i++)
        {
            Transform quad = quads[i].transform;

            float phase = i * waveOffset;
            
            float hop =  Mathf.Abs(Mathf.Sin(elapsed * bounceSpeed + phase));
                      
            hop = Mathf.Pow(hop, 1.5f);
                        
            float side = Mathf.Sin(elapsed * bounceSpeed + phase) * bounceSideAmount;

            float tilt = Mathf.Sin(elapsed * bounceSpeed + phase) * 8f;

            quad.rotation =  Quaternion.Euler(0f, 0f, tilt);
                     
            float spacing = i * 1.1f;

            float x = reverse ? rowX - spacing : rowX + spacing;

            x += side;

            Vector3 pos = quad.position;

            pos.x = x;
            pos.y = hop * bounceHeight;

            quad.position = pos;
        }
    }   
}

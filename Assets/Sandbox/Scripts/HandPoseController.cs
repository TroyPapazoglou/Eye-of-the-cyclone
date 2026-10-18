using UnityEngine;

public enum HandPose
{
    Idle,
    Running,
    Jumping,  
}
public class HandPoseController : MonoBehaviour
{
    [SerializeField] private HandPoseTarget idle;
    [SerializeField] private HandPoseTarget running;
    [SerializeField] private HandPoseTarget jumping;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

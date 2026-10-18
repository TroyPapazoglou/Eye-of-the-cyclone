using System;
using Unity.Cinemachine;
using UnityEngine;

public class CameraBlendDebug : MonoBehaviour
{
    [SerializeField] private CinemachineCamera playerCamera;

    private void Start()
    {
        CinemachineCore.BlendFinishedEvent.AddListener(OnBlendFinished);
        CinemachineCore.CameraActivatedEvent.AddListener(OnCameraActivated);
        Debug.Log("subscribed to cinemachine blend-finsiehd event");
    }

    private void OnDisable()
    {
        CinemachineCore.BlendFinishedEvent.RemoveListener(OnBlendFinished);
        CinemachineCore.CameraActivatedEvent.RemoveListener(OnCameraActivated);

    }

    private void OnCameraActivated(ICinemachineMixer.ActivationEventParams mix)
    {
        Debug.Log($"camera activated");
      
    }

    private void OnBlendFinished(ICinemachineMixer mix, ICinemachineCamera incomingCamera)
    {
        Debug.Log($"blend finished! incoming camera: {incomingCamera.Name}");

        if(incomingCamera == (ICinemachineCamera)playerCamera)
        {
            Debug.Log("playe camera blend finished!!!");
        }
    }

    public void YAYBLENDFINISHED(ICinemachineMixer mix, ICinemachineCamera incomingCamera)
    {
        Debug.Log("A BLEND WAS DONE!!!");
    }
}

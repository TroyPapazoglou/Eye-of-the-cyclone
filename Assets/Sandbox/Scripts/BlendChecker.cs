using Unity.Cinemachine;
using UnityEngine;

public class BlendChecker : MonoBehaviour
{
    [SerializeField] private CinemachineBrain brain;

    private void Update()
    {
        CinemachineBlend blend = brain.ActiveBlend;
        if (blend != null && !blend.IsComplete)
        {
            Debug.Log($"Blending from {blend.CamA?.Name} to {blend.CamB?.Name} at {blend.BlendWeight:F2} progress.");
        }
    }
}

using TMPro;
using UnityEngine;

public class UsernameDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI usernameText;

    private Camera mainCamera;

    public void SetUsername(string username)
    {
        usernameText.text = username;
    }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    //private void LateUpdate()
    //{
    //    if (mainCamera == null)
    //        return;

    //    transform.forward = mainCamera.transform.forward;
    //}

    private void LateUpdate()
    {
        if (mainCamera == null)
            return;

        Vector3 direction = transform.position - mainCamera.transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(direction);
    }
}

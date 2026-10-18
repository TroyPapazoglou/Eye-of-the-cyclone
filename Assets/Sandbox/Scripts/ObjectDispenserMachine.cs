using Mirror;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectDispenserMachine : NetworkBehaviour
{
    [Header("Objects")]
    [SerializeField] private List<SpawnableObjectData> objects;

    [Header("Display")]
    [SerializeField] private TMP_Text objectNameText;
    [SerializeField] private Transform previewPoint;
    [SerializeField] private float previewRotationSpeed = 45f;

    [Header("Spawning")]
    [SerializeField] private Transform spawnPoint;

    [SyncVar(hook = nameof(OnSelectedIndexChanged))]
    private int selectedIndex;

    private GameObject previewObject;

    public SpawnableObjectData SelectedObject
    {
        get
        {
            if (objects == null || objects.Count == 0)
                return null;

            if (selectedIndex < 0 || selectedIndex >= objects.Count)
                return null;

            return objects[selectedIndex];
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        UpdateDisplay();
    }

    private void Update()
    {
        if (previewObject == null)
            return;

        previewObject.transform.Rotate(
            Vector3.up,
            previewRotationSpeed * Time.deltaTime,
            Space.World);
    }

    [Server]
    public void SpawnSelectedObject()
    {
        SpawnableObjectData selected = SelectedObject;

        if (selected == null || selected.prefab == null)
            return;

        if (spawnPoint == null)
        {
            Debug.LogWarning("Object Dispenser has no spawn point assigned.");
            return;
        }
        
        GameObject spawnedObject = Instantiate(
            selected.prefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        NetworkServer.Spawn(spawnedObject);
    }

    [Server]
    public void SelectNext()
    {
        if (objects == null || objects.Count == 0)
            return;

        selectedIndex = (selectedIndex + 1) % objects.Count;
    }

    [Server]
    public void SelectPrevious()
    {
        if (objects == null || objects.Count == 0)
            return;

        selectedIndex =
            (selectedIndex - 1 + objects.Count) % objects.Count;
    }
    
    private void OnSelectedIndexChanged(int oldIndex, int newIndex)
    {
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        SpawnableObjectData selected = SelectedObject;

        if (objectNameText != null)
        {
            objectNameText.text = selected != null
                ? selected.displayName
                : "No Objects Available";
        }

        UpdatePreview(selected);
    }

    private void UpdatePreview(SpawnableObjectData selected)
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
            previewObject = null;
        }

        if (selected == null ||
            selected.prefab == null ||
            previewPoint == null)
        {
            return;
        }

        previewObject = Instantiate(
            selected.prefab,
            previewPoint.position,
            previewPoint.rotation);

        // This is a visual preview, not a networked object.
        foreach (NetworkIdentity identity in
                 previewObject.GetComponentsInChildren<NetworkIdentity>(true))
        {
            identity.enabled = false;
        }

        foreach (Rigidbody rb in
                 previewObject.GetComponentsInChildren<Rigidbody>(true))
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        foreach (Collider col in
                 previewObject.GetComponentsInChildren<Collider>(true))
        {
            col.enabled = false;
        }
    }
}

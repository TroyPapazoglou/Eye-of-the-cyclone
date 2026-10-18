using UnityEngine;

[CreateAssetMenu(menuName = "Sandbox/Spawnable Object")]
public class SpawnableObjectData : ScriptableObject
{
    public string displayName;
    public string description;
    public Sprite icon;
    public GameObject prefab;
}
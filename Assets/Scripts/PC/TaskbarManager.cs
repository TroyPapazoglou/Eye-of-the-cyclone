using System.Collections.Generic;
using UnityEngine;

public class TaskbarManager : MonoBehaviour
{
    public static TaskbarManager Instance { get; private set; }

    [SerializeField] private Transform content;
    [SerializeField] private TaskbarItem taskbarItemPrefab;

    private Dictionary<PCApplication, TaskbarItem> taskbarItems = new();

    private void Awake()
    {
        Instance = this;
    }

    public void AddApplication(PCApplicationData data)
    {
        if (taskbarItems.ContainsKey(data.application))
            return;

        TaskbarItem item = Instantiate(
            taskbarItemPrefab,
            content
        );

        item.Setup(data);

        taskbarItems.Add(data.application, item);
    }

    public void RemoveApplication(PCApplication application)
    {
        if (!taskbarItems.TryGetValue(
            application,
            out TaskbarItem item))
        {
            return;
        }

        Destroy(item.gameObject);

        taskbarItems.Remove(application);
    }
}

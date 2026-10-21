using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MazeZoneTrigger : MonoBehaviour
{
    public MazeCycleManager cycleManager;

    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (cycleManager != null) cycleManager.OnPlayerEnterMaze();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (cycleManager != null) cycleManager.OnPlayerExitMaze();
    }
}

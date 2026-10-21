using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int collectedCount = 0;
    public int totalMushroomsInMaze = 0;

    void Awake()
    {
        Instance = this;
    }

    public void SetMazeMushroomTotal(int total)
    {
        totalMushroomsInMaze = total;
    }

    public void CollectMushroom()
    {
        collectedCount++;
        totalMushroomsInMaze = Mathf.Max(0, totalMushroomsInMaze - 1);
    }
}

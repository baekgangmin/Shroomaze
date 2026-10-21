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

    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 24;
        style.normal.textColor = Color.white;

        GUI.Label(new Rect(20, 20, 400, 40), "수집한 버섯: " + collectedCount, style);
        GUI.Label(new Rect(20, 50, 400, 40), "미로 안 남은 버섯: " + totalMushroomsInMaze, style);
    }
}

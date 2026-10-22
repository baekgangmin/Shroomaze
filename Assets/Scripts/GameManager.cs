using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int[] collectedByColor = new int[3];
    public int currentMazeRemaining = 0;
    public int currentMazeColorIndex = 0;
    public int targetPerColor = 15;

    // Fired the instant currentMazeRemaining hits zero. Since each maze only ever spawns as many
    // mushrooms as still needed to reach that color's target, clearing it always means the target
    // was just reached.
    public System.Action<int> OnMazeCleared;

    void Awake()
    {
        Instance = this;
    }

    public void SetMazeMushroomTotal(int total, int colorIndex)
    {
        currentMazeRemaining = total;
        currentMazeColorIndex = colorIndex;
    }

    public void CollectMushroom(int colorIndex)
    {
        if (colorIndex >= 0 && colorIndex < collectedByColor.Length) collectedByColor[colorIndex]++;

        if (currentMazeRemaining > 0)
        {
            currentMazeRemaining--;
            if (currentMazeRemaining == 0) OnMazeCleared?.Invoke(colorIndex);
        }
    }
}

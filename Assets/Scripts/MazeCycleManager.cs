using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MazeCycleManager : MonoBehaviour
{
    public enum CycleState { Hub, InMaze, Success, Maintenance, Transitioning, GameOver }

    [Header("References")]
    public MazeGenerator maze;
    public DoorController door;
    public MazeTimer timer;
    public CameraFollow cameraFollow;

    [Header("Mushroom colors (in cycle order)")]
    public GameObject[] mushroomPrefabsByColor;
    public string[] colorNames = { "빨강", "보라", "금색" };

    [Header("Timing")]
    public float cycleDuration = 90f;
    public float successCloseDelay = 2f;
    public float maintenanceDuration = 20f;
    public float transitionShakeDuration = 0.5f;
    public float transitionShakeMagnitude = 0.3f;

    public CycleState State { get; private set; }
    public int CurrentColorIndex { get; private set; }
    public string CurrentColorName => colorNames[CurrentColorIndex % colorNames.Length];

    public System.Action OnGameOver;
    public System.Action<int> OnColorChanged;

    void Start()
    {
        timer.OnTimeUp += HandleTimeUp;

        CurrentColorIndex = 0;
        maze.mushroomPrefab = mushroomPrefabsByColor[CurrentColorIndex];
        maze.Generate();
        door.SnapOpen();
        State = CycleState.Hub;
    }

    public void OnPlayerEnterMaze()
    {
        if (State != CycleState.Hub) return;
        State = CycleState.InMaze;
        timer.StartTimer(cycleDuration);
    }

    public void OnPlayerExitMaze()
    {
        if (State != CycleState.InMaze) return;
        State = CycleState.Success;
        timer.StopTimer();
        StartCoroutine(SuccessSequence());
    }

    void HandleTimeUp()
    {
        if (State != CycleState.InMaze) return;
        State = CycleState.GameOver;
        OnGameOver?.Invoke();
    }

    IEnumerator SuccessSequence()
    {
        yield return new WaitForSeconds(successCloseDelay);

        door.Close();
        yield return new WaitForSeconds(door.slideDuration);

        State = CycleState.Maintenance;
        yield return new WaitForSeconds(maintenanceDuration);

        State = CycleState.Transitioning;
        if (cameraFollow != null) cameraFollow.Shake(transitionShakeDuration, transitionShakeMagnitude);
        yield return new WaitForSeconds(transitionShakeDuration);

        CurrentColorIndex = (CurrentColorIndex + 1) % mushroomPrefabsByColor.Length;
        maze.mushroomPrefab = mushroomPrefabsByColor[CurrentColorIndex];
        maze.Generate();
        OnColorChanged?.Invoke(CurrentColorIndex);

        door.Open();
        yield return new WaitForSeconds(door.slideDuration);

        State = CycleState.Hub;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

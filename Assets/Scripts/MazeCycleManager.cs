using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

// Drives the whole hub <-> maze loop: Hub (waiting at the gate) -> InMaze (timer running) ->
// Success/GameOver decided only when the timer hits zero, based on whether the player is
// inside or outside the maze at that moment (or instantly, if the maze is fully cleared first) ->
// Maintenance (shop window) -> Transitioning (color swap + maze reshuffle) -> back to Hub.
// Colors rotate red -> purple -> gold -> red -> ... and each color needs a cumulative
// GameManager.targetPerColor mushrooms before it's skipped for good; the whole game ends once
// every color has hit that target.
public class MazeCycleManager : MonoBehaviour
{
    public enum CycleState { Hub, InMaze, Success, Maintenance, Transitioning, GameOver, GameComplete }

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
    public float retryBonusDuration = 60f;
    public float successCloseDelay = 1f;
    public float maintenanceDuration = 20f;
    public float transitionShakeDuration = 0.5f;
    public float transitionShakeMagnitude = 0.3f;

    public CycleState State { get; private set; }
    public int CurrentColorIndex { get; private set; }
    public string CurrentColorName => colorNames[CurrentColorIndex % colorNames.Length];

    public System.Action OnGameOver;
    public System.Action OnGameComplete;
    public System.Action<int> OnColorChanged;
    public System.Action<string> OnStageMessage;

    bool playerInsideMaze;
    int[] attemptCount;
    PlayerController player;
    Vector3 hubSpawnPos;
    Quaternion hubSpawnRot;

    void Start()
    {
        timer.OnTimeUp += HandleTimeUp;
        if (GameManager.Instance != null) GameManager.Instance.OnMazeCleared += HandleMazeCleared;

        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            player = playerGO.GetComponent<PlayerController>();
            hubSpawnPos = playerGO.transform.position;
            hubSpawnRot = playerGO.transform.rotation;
        }

        attemptCount = new int[mushroomPrefabsByColor.Length];
        CurrentColorIndex = 0;
        GenerateMazeForCurrentColor();
        door.SnapOpen();
        State = CycleState.Hub;
    }

    void GenerateMazeForCurrentColor()
    {
        attemptCount[CurrentColorIndex]++;

        int remaining = GameManager.Instance != null
            ? GameManager.Instance.targetPerColor - GameManager.Instance.collectedByColor[CurrentColorIndex]
            : maze.mushroomCount;
        maze.mushroomCount = Mathf.Max(0, remaining);
        maze.mushroomPrefab = mushroomPrefabsByColor[CurrentColorIndex];
        maze.currentColorIndex = CurrentColorIndex;
        maze.Generate();
    }

    public void OnPlayerEnterMaze()
    {
        if (State == CycleState.Hub)
        {
            State = CycleState.InMaze;
            // a color that wasn't cleared on an earlier attempt gets extra time on the retry,
            // flat (not stacking further on a 3rd, 4th, ... attempt of the same color)
            bool isRetry = attemptCount[CurrentColorIndex] > 1;
            timer.StartTimer(cycleDuration + (isRetry ? retryBonusDuration : 0f));
            if (isRetry) OnStageMessage?.Invoke(CurrentColorName + " 미로: 추가 시간 1분이 주어집니다!");
        }
        if (State == CycleState.InMaze) playerInsideMaze = true;
    }

    public void OnPlayerExitMaze()
    {
        // just tracks where the player currently is — walking in and out freely during the
        // time limit doesn't end the cycle either way; only the timer running out (or clearing
        // the maze) decides it
        if (State != CycleState.InMaze) return;
        playerInsideMaze = false;
    }

    // Fires the instant every mushroom in the current maze has been collected — since the maze
    // only ever spawns as many as still needed for this color's target, that always means the
    // target was just reached, so the round ends right away instead of waiting for the timer.
    void HandleMazeCleared(int colorIndex)
    {
        if (colorIndex != CurrentColorIndex || State != CycleState.InMaze) return;
        timer.StopTimer();
        State = CycleState.Success;
        StartCoroutine(RoundCompleteSequence(withGraceDelay: false, teleportPlayerToHub: true));
    }

    // Fires when the full cycle timer runs out: success if the player is outside the maze
    // right then, game over if they're still inside.
    void HandleTimeUp()
    {
        if (State != CycleState.InMaze) return;

        if (playerInsideMaze)
        {
            State = CycleState.GameOver;
            door.Close();
            if (player != null) player.movementEnabled = false;
            OnGameOver?.Invoke();
        }
        else
        {
            State = CycleState.Success;
            StartCoroutine(RoundCompleteSequence(withGraceDelay: true, teleportPlayerToHub: false));
        }
    }

    IEnumerator RoundCompleteSequence(bool withGraceDelay, bool teleportPlayerToHub)
    {
        bool colorJustCompleted = GameManager.Instance != null
            && GameManager.Instance.collectedByColor[CurrentColorIndex] >= GameManager.Instance.targetPerColor;

        if (colorJustCompleted)
            OnStageMessage?.Invoke(CurrentColorName + " 버섯 " + GameManager.Instance.targetPerColor + "개 달성! " + CurrentColorName + " 단계 완료!");

        // cleared the maze while still inside it — freeze the player and drop them back at the
        // hub so they aren't left stranded inside while the maze closes up and regenerates
        if (teleportPlayerToHub && player != null)
        {
            player.movementEnabled = false;
            player.Teleport(hubSpawnPos, hubSpawnRot);
        }

        if (withGraceDelay)
        {
            // small grace period so the door doesn't slam shut the instant the player's foot
            // crosses the maze boundary
            yield return new WaitForSeconds(successCloseDelay);
        }

        door.Close();
        yield return new WaitForSeconds(door.slideDuration);

        State = CycleState.Maintenance;
        timer.StartTimer(maintenanceDuration); // drives the maintenance countdown shown in GameUI
        yield return new WaitForSeconds(maintenanceDuration);

        State = CycleState.Transitioning;
        if (cameraFollow != null) cameraFollow.Shake(transitionShakeDuration, transitionShakeMagnitude);
        yield return new WaitForSeconds(transitionShakeDuration);

        // advance to the next color in rotation, skipping any that already hit its target;
        // if every color is already complete, the whole game is done
        int next = -1;
        for (int step = 1; step <= mushroomPrefabsByColor.Length; step++)
        {
            int candidate = (CurrentColorIndex + step) % mushroomPrefabsByColor.Length;
            if (GameManager.Instance == null || GameManager.Instance.collectedByColor[candidate] < GameManager.Instance.targetPerColor)
            {
                next = candidate;
                break;
            }
        }

        if (next < 0)
        {
            OnStageMessage?.Invoke("모든 버섯을 종류별로 다 모았습니다! 게임 클리어!");
            State = CycleState.GameComplete;
            if (teleportPlayerToHub && player != null) player.movementEnabled = true;
            OnGameComplete?.Invoke();
            yield break;
        }

        CurrentColorIndex = next;
        GenerateMazeForCurrentColor();
        OnColorChanged?.Invoke(CurrentColorIndex);

        door.Open();
        yield return new WaitForSeconds(door.slideDuration);

        if (teleportPlayerToHub && player != null) player.movementEnabled = true;
        State = CycleState.Hub;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public MazeCycleManager cycleManager;
    public MazeTimer timer;
    public GameManager gameManager;

    public Text timerText;
    public Text collectedText;
    public Text remainingText;
    public Text statusText;
    public GameObject gameOverPanel;
    public Button restartButton;

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (cycleManager != null) cycleManager.OnGameOver += HandleGameOver;
        if (restartButton != null) restartButton.onClick.AddListener(HandleRestartClicked);
    }

    void OnDestroy()
    {
        if (cycleManager != null) cycleManager.OnGameOver -= HandleGameOver;
    }

    void Update()
    {
        if (timerText != null)
        {
            if (timer != null && timer.IsRunning)
            {
                int seconds = Mathf.CeilToInt(timer.remainingTime);
                timerText.text = string.Format("{0:00}:{1:00}", seconds / 60, seconds % 60);
            }
            else
            {
                timerText.text = "--:--";
            }
        }

        if (collectedText != null && gameManager != null)
            collectedText.text = "수집한 버섯: " + gameManager.collectedCount;

        if (remainingText != null && gameManager != null)
            remainingText.text = "미로 안 남은 버섯: " + gameManager.totalMushroomsInMaze;

        if (statusText != null && cycleManager != null)
            statusText.text = DescribeState(cycleManager.State) + " (" + cycleManager.CurrentColorName + ")";
    }

    string DescribeState(MazeCycleManager.CycleState state)
    {
        switch (state)
        {
            case MazeCycleManager.CycleState.Hub: return "대기 중";
            case MazeCycleManager.CycleState.InMaze: return "미로 탐색 중";
            case MazeCycleManager.CycleState.Success: return "탈출 성공!";
            case MazeCycleManager.CycleState.Maintenance: return "정비 시간";
            case MazeCycleManager.CycleState.Transitioning: return "미로 교체 중...";
            case MazeCycleManager.CycleState.GameOver: return "게임 오버";
            default: return "";
        }
    }

    void HandleGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    void HandleRestartClicked()
    {
        if (cycleManager != null) cycleManager.RestartGame();
    }
}

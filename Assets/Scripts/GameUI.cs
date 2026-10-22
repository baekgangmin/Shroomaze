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

    string pendingMessage;
    float messageTimer;

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (cycleManager != null)
        {
            cycleManager.OnGameOver += HandleGameOver;
            cycleManager.OnStageMessage += HandleStageMessage;
        }
        if (restartButton != null) restartButton.onClick.AddListener(HandleRestartClicked);
    }

    void OnDestroy()
    {
        if (cycleManager != null)
        {
            cycleManager.OnGameOver -= HandleGameOver;
            cycleManager.OnStageMessage -= HandleStageMessage;
        }
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

        if (collectedText != null && gameManager != null && cycleManager != null)
        {
            var sb = new System.Text.StringBuilder("수집한 버섯\n");
            for (int i = 0; i < gameManager.collectedByColor.Length; i++)
            {
                if (i > 0) sb.Append("  ");
                sb.Append(cycleManager.colorNames[i % cycleManager.colorNames.Length]).Append(": ")
                  .Append(gameManager.collectedByColor[i]).Append("/").Append(gameManager.targetPerColor);
            }
            collectedText.text = sb.ToString();
        }

        if (remainingText != null && gameManager != null && cycleManager != null)
        {
            string colorName = cycleManager.colorNames[gameManager.currentMazeColorIndex % cycleManager.colorNames.Length];
            remainingText.text = "미로 안 남은 버섯 (" + colorName + "): " + gameManager.currentMazeRemaining;
        }

        if (statusText != null && cycleManager != null)
        {
            if (messageTimer > 0f)
            {
                messageTimer -= Time.deltaTime;
                statusText.text = pendingMessage;
            }
            else
            {
                statusText.text = DescribeState(cycleManager.State) + " (" + cycleManager.CurrentColorName + ")";
            }
        }
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
            case MazeCycleManager.CycleState.GameComplete: return "게임 클리어!";
            default: return "";
        }
    }

    void HandleGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    void HandleStageMessage(string message)
    {
        pendingMessage = message;
        messageTimer = 3.5f;
    }

    void HandleRestartClicked()
    {
        if (cycleManager != null) cycleManager.RestartGame();
    }
}

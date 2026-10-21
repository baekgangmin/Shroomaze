using UnityEngine;

public class MazeTimer : MonoBehaviour
{
    public float remainingTime { get; private set; }
    public bool IsRunning { get; private set; }

    public System.Action OnTimeUp;

    public void StartTimer(float duration)
    {
        remainingTime = duration;
        IsRunning = true;
    }

    public void StopTimer()
    {
        IsRunning = false;
    }

    void Update()
    {
        if (!IsRunning) return;

        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            IsRunning = false;
            OnTimeUp?.Invoke();
        }
    }
}

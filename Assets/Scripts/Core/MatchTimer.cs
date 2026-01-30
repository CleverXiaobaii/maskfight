//csharp Assets/Scripts/Core/MatchTimer.cs
using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 比赛时长计时器（用于单局计时）
/// 提供 Start/Stop/Reset，并通过事件通知剩余时间或时间到
/// </summary>
public class MatchTimer : MonoBehaviour
{
    public event Action<float> OnTick; // 参数为剩余秒数（浮点）
    public event Action OnTimeUp;

    [Tooltip("默认时长（秒），可在 GameManager 中设置")]
    public float durationSeconds = 180f;

    private float remaining;
    private bool running = false;
    private Coroutine timerCoroutine;

    public void SetDuration(int seconds)
    {
        durationSeconds = seconds;
        remaining = durationSeconds;
    }

    public void StartTimer()
    {
        if (running) return;
        remaining = durationSeconds;
        timerCoroutine = StartCoroutine(Tick());
    }

    public void StopTimer()
    {
        if (!running) return;
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        running = false;
    }

    public void ResetTimer()
    {
        StopTimer();
        remaining = durationSeconds;
        OnTick?.Invoke(remaining);
    }

    private IEnumerator Tick()
    {
        running = true;
        remaining = Mathf.Max(0f, remaining);
        while (remaining > 0f)
        {
            OnTick?.Invoke(remaining);
            yield return null;
            remaining -= Time.deltaTime;
        }
        remaining = 0f;
        OnTick?.Invoke(remaining);
        running = false;
        OnTimeUp?.Invoke();
    }

    public float GetRemainingSeconds() => remaining;
    public bool IsRunning() => running;
}
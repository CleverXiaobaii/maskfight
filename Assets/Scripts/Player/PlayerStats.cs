//csharp Assets/Scripts/Player/PlayerStats.cs
using System;
using UnityEngine;

/// <summary>
/// 玩家血量与死亡管理
/// TODO: 将 PlayerStats 组件挂在 Player Prefab 根节点，并在 Inspector 中设置初始血量等
/// TODO: 可在 TakeDamage 中触发受击动画 / 音效（通过 Animator/AudioSource 调用）
/// </summary>
public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    [Tooltip("初始血量（格数），可在 Inspector 调整")]
    public int maxHealth = 3;

    public int CurrentHealth { get; private set; }

    public event Action<PlayerStats> OnDeath; // 传递死掉的玩家自身

    private void Awake()
    {
        ResetStats();
    }

    /// <summary>
    /// 重置血量与状态（比赛开始时调用）
    /// </summary>
    public void ResetStats()
    {
        CurrentHealth = Mathf.Max(1, maxHealth);
    }

    /// <summary>
    /// 造成伤害并检测死亡
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);

        // TODO: 在此处触发受击动画/音效（例如通过 Animator）
        if (CurrentHealth == 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // TODO: 播放死亡动画/效果
        OnDeath?.Invoke(this);

        // 通知 GameManager（若需要由 GameManager 判定胜负）
        if (GameManager.Instance != null)
        {
            GameManager.Instance.NotifyPlayerDead(this);
        }
    }
}
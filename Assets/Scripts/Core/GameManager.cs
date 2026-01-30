//csharp Assets/Scripts/Core/GameManager.cs
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 中央流程管理：开始界面 -> 5秒倒计时 -> 游戏进行 -> 游戏结束
/// 负责：比赛状态、面具克制规则、胜负判定通知
/// TODO: 将 PlayerStats 属性在 Inspector 中绑定到场景里的玩家对象（或设置 playerPrefab 以自动生成）
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        StartScreen,
        Countdown,
        Playing,
        Ended
    }

    [Header("Match Settings")]
    [Tooltip("开局倒计时秒数")]
    public int countdownSeconds = 5;

    [Tooltip("单局总时长（秒），MatchTimer 也会使用此值")]
    public int matchDurationSeconds = 180;

    [Header("Players (assign in Inspector)")]
    public PlayerStats player1; // TODO: 将 Player1 的 PlayerStats 组件拖到这里，或设置 playerPrefab 使 GameManager 自动生成
    public PlayerStats player2; // TODO: 将 Player2 的 PlayerStats 组件拖到这里，或设置 playerPrefab 使 GameManager 自动生成

    [Header("Player Spawning (optional)")]
    [Tooltip("用于自动创建玩家的 Prefab，应包含 PlayerStats, PlayerController, PlayerAttack, MaskPickup 等组件")]
    public GameObject playerPrefab;
    [Tooltip("可选：玩家1出生点（为空则使用默认偏移）")]
    public Transform player1Spawn;
    [Tooltip("可选：玩家2出生点（为空则使用默认偏移）")]
    public Transform player2Spawn;

    [Header("UI / Other")]
    [Tooltip("UI 管理器：用于显示开始/倒计时/结束界面（可选）")]
    public UIManager uiManager; // TODO: 若实现 UIManager，绑定到此处

    public GameState State { get; private set; } = GameState.StartScreen;

    // 面具类型与克制关系
    public enum MaskType { Red = 0, Orange, Yellow, Green, Blue, Purple }

    // 内部映射：谁克制谁 (A -> B 表示 A 克制 B)
    private Dictionary<MaskType, MaskType> maskCounter = new Dictionary<MaskType, MaskType>();

    // 事件
    public event Action<GameState> OnStateChanged;
    public event Action<PlayerStats> OnMatchEnd; // 传胜利者（null 表示平局）

    private MatchTimer matchTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitMaskRules();
    }

    private void Start()
    {
        // 尝试找到 MatchTimer（可放到同一 GameObject 或场景中的其它对象）
        matchTimer = GetComponent<MatchTimer>();
        if (matchTimer == null)
        {
            matchTimer = gameObject.AddComponent<MatchTimer>();
        }
        matchTimer.SetDuration(matchDurationSeconds);
        matchTimer.OnTimeUp += HandleTimeUp;
    }

    private void InitMaskRules()
    {
        // 环形克制关系：Red->Orange->Yellow->Green->Blue->Purple->Red
        maskCounter.Clear();
        maskCounter[MaskType.Red] = MaskType.Orange;
        maskCounter[MaskType.Orange] = MaskType.Yellow;
        maskCounter[MaskType.Yellow] = MaskType.Green;
        maskCounter[MaskType.Green] = MaskType.Blue;
        maskCounter[MaskType.Blue] = MaskType.Purple;
        maskCounter[MaskType.Purple] = MaskType.Red;
    }

    /// <summary>
    /// 判断 attackerMask 是否克制 defenderMask
    /// </summary>
    public bool IsCounter(MaskType attackerMask, MaskType defenderMask)
    {
        return maskCounter.ContainsKey(attackerMask) && maskCounter[attackerMask] == defenderMask;
    }

    #region Flow Control

    public void StartFromStartScreen()
    {
        // 被 UI 的“按任意键开始”触发
        if (State != GameState.StartScreen) return;
        StartCoroutine(DoCountdownAndStart());
    }

    private IEnumerator DoCountdownAndStart()
    {
        SetState(GameState.Countdown);
        Debug.Log("[GameManager] 倒计时开始...");

        // UI 倒计时展示（若有 UIManager）
        if (uiManager != null) uiManager.ShowCountdown(countdownSeconds);

        int t = countdownSeconds;
        while (t > 0)
        {
            if (uiManager != null) uiManager.UpdateCountdown(t);
            yield return new WaitForSeconds(1f);
            t--;
        }

        // 倒计时结束
        if (uiManager != null) uiManager.HideCountdown();
        StartMatch();
    }

    public void StartMatch()
    {
        // 确保场景存在两个玩家实例（若未手动放置则尝试自动创建）
        EnsurePlayersExist();

        // 尝试启动 MaskSpawner（若场景中存在）
        var spawner = FindObjectOfType<MaskSpawner>();
        if (spawner != null)
        {
            spawner.StartSpawning();
        }

        // 重置玩家数据
        if (player1 != null) player1.ResetStats();
        if (player2 != null) player2.ResetStats();

        // 启动计时器
        matchTimer.SetDuration(matchDurationSeconds);
        matchTimer.StartTimer();

        SetState(GameState.Playing);
        Debug.Log("[GameManager] 比赛开始！");
    }

    public void EndMatch(PlayerStats winner)
    {
        if (State == GameState.Ended) return;

        // 停止生成（如果有）
        var spawner = FindObjectOfType<MaskSpawner>();
        if (spawner != null)
        {
            spawner.StopSpawning();
        }

        SetState(GameState.Ended);

        // 停止计时器
        if (matchTimer != null) matchTimer.StopTimer();

        // 通知 UI
        if (uiManager != null) uiManager.ShowEndScreen(winner);

        // 广播事件
        OnMatchEnd?.Invoke(winner);
    }

    private void HandleTimeUp()
    {
        // 平局（时间到且未分出胜负）
        PlayerStats winner = null;
        // 若想在时间到时按血量判定胜负，请在此加入逻辑
        EndMatch(winner);
    }

    private void SetState(GameState newState)
    {
        State = newState;
        OnStateChanged?.Invoke(newState);
    }

    #endregion

    #region Player Notifications

    /// <summary>
    /// 被 PlayerStats 调用：当某玩家血量归0时调用以触发比赛结束
    /// </summary>
    public void NotifyPlayerDead(PlayerStats deadPlayer)
    {
        PlayerStats winner = (deadPlayer == player1) ? player2 : player1;
        EndMatch(winner);
    }

    #endregion

    /// <summary>
    /// 确保场景中存在两个玩家；若未设置 player1/player2 且配置了 playerPrefab 则自动实例化并完成必需引用绑定
    /// </summary>
    private void EnsurePlayersExist()
    {
        // 如果已经手动放置了两个玩家，则不做创建
        if (player1 != null && player2 != null) return;

        if (playerPrefab == null)
        {
            Debug.LogWarning("[GameManager] player1/player2 未绑定且 playerPrefab 未设置，无法自动创建玩家。请在 Inspector 绑定 PlayerStats 或设置 playerPrefab。");
            return;
        }

        // 创建 player1（如果缺失）
        if (player1 == null)
        {
            Vector3 pos = player1Spawn != null ? player1Spawn.position : new Vector3(-2f, 0f, 0f);
            GameObject go1 = Instantiate(playerPrefab, pos, Quaternion.identity);
            var stats1 = go1.GetComponent<PlayerStats>();
            if (stats1 == null)
            {
                Debug.LogError("[GameManager] playerPrefab 缺少 PlayerStats 组件，无法赋值为 player1。");
            }
            player1 = stats1;

            var ctrl1 = go1.GetComponent<PlayerController>();
            if (ctrl1 != null) ctrl1.isPlayerOne = true;
            var att1 = go1.GetComponent<PlayerAttack>();
            if (att1 != null) att1.isPlayerOne = true;
            var pick1 = go1.GetComponent<MaskPickup>();
            if (pick1 != null) pick1.isPlayerOne = true;
        }

        // 创建 player2（如果缺失）
        if (player2 == null)
        {
            Vector3 pos = player2Spawn != null ? player2Spawn.position : new Vector3(2f, 0f, 0f);
            GameObject go2 = Instantiate(playerPrefab, pos, Quaternion.identity);
            var stats2 = go2.GetComponent<PlayerStats>();
            if (stats2 == null)
            {
                Debug.LogError("[GameManager] playerPrefab 缺少 PlayerStats 组件，无法赋值为 player2。");
            }
            player2 = stats2;

            var ctrl2 = go2.GetComponent<PlayerController>();
            if (ctrl2 != null) ctrl2.isPlayerOne = false;
            var att2 = go2.GetComponent<PlayerAttack>();
            if (att2 != null) att2.isPlayerOne = false;
            var pick2 = go2.GetComponent<MaskPickup>();
            if (pick2 != null) pick2.isPlayerOne = false;
        }

        // 互相设置对手引用（PlayerAttack.opponentStats / opponentTransform）
        if (player1 != null && player2 != null)
        {
            var att1 = player1.GetComponent<PlayerAttack>();
            var att2 = player2.GetComponent<PlayerAttack>();
            if (att1 != null)
            {
                att1.opponentStats = player2;
                att1.opponentTransform = player2.transform;
            }
            if (att2 != null)
            {
                att2.opponentStats = player1;
                att2.opponentTransform = player1.transform;
            }
        }

        Debug.Log("[GameManager] EnsurePlayersExist: player1/player2 已确保存在（若由 playerPrefab 自动创建）。");
    }

    // 在 GameManager 中新增：
    public void ReturnToStartScreen()
    {
        // 停止计时器与重置
        if (matchTimer != null) matchTimer.StopTimer();
        SetState(GameState.StartScreen);

        // 停止生成并清理场景中现有的面具实例（直接查找 Mask 并销毁）
        var spawner = FindObjectOfType<MaskSpawner>();
        if (spawner != null) spawner.StopSpawning();

        var masks = FindObjectsOfType<Mask>();
        foreach (var m in masks)
        {
            if (m != null && m.gameObject != null) Destroy(m.gameObject);
        }

        // 通知 UI
        if (uiManager != null) uiManager.ShowStartScreen();
    }
}
//csharp Assets/Scripts/UI/UIManager.cs
using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI 管理器：控制顶部计时、开局倒计时、开始界面、结束界面，以及左右玩家 HUD（血量/面具）
/// - 需要在 Inspector 中绑定各个 UI 元素（Text / Image / Panels）
/// - MatchTimer 的 OnTick 会用于更新顶部计时显示（自动订阅 GameManager 附带的 MatchTimer）
/// - 请在 PlayerStats.TakeDamage / ResetStats 调用 UpdatePlayerHealth(...) 来更新血量显示
/// - 请在 PlayerAttack.SetMask(...) 或 MaskPickup 完成拾取后调用 UpdatePlayerMaskIcon(...)
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Top Timer")]
    [SerializeField] private Text timerText;

    [Header("Start / Countdown")]
    [SerializeField] private GameObject startScreenPanel; // 开始界面（按任意键开始）
    [SerializeField] private GameObject countdownPanel;   // 居中倒计时面板
    [SerializeField] private Text countdownText;

    [Header("End Screen")]
    [SerializeField] private GameObject endPanel;
    [SerializeField] private Text endResultText; // 显示 "玩家1获胜" / "平局"

    [Header("Player HUD - Player1 (左下)")]
    [SerializeField] private Image p1CharacterIcon;
    [SerializeField] private Image p1MaskIcon;
    [SerializeField] private Image p1HealthFill; // 使用 Image.fillAmount (0..1)

    [Header("Player HUD - Player2 (右下)")]
    [SerializeField] private Image p2CharacterIcon;
    [SerializeField] private Image p2MaskIcon;
    [SerializeField] private Image p2HealthFill; // 使用 Image.fillAmount (0..1)

    [Header("Defaults")]
    [SerializeField] private Sprite defaultMaskSprite; // 无面具时显示的图标

    private MatchTimer matchTimer;

    private void Start()
    {
        // 初始 UI 状态
        if (startScreenPanel != null) startScreenPanel.SetActive(true);
        if (countdownPanel != null) countdownPanel.SetActive(false);
        if (endPanel != null) endPanel.SetActive(false);

        // 默认面具图标
        if (p1MaskIcon != null) p1MaskIcon.sprite = defaultMaskSprite;
        if (p2MaskIcon != null) p2MaskIcon.sprite = defaultMaskSprite;

        // 尝试订阅 MatchTimer 的 OnTick（MatchTimer 在 GameManager 中）
        if (GameManager.Instance != null)
        {
            matchTimer = GameManager.Instance.GetComponent<MatchTimer>();
            if (matchTimer != null)
            {
                matchTimer.OnTick += OnTimerTick;
            }

            // 订阅状态变更以控制界面切换
            GameManager.Instance.OnStateChanged += OnGameStateChanged;
        }
    }

    private void OnDestroy()
    {
        if (matchTimer != null)
        {
            matchTimer.OnTick -= OnTimerTick;
        }
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= OnGameStateChanged;
        }
    }

    private void OnTimerTick(float secondsRemaining)
    {
        UpdateTimerDisplay(secondsRemaining);
    }

    private void OnGameStateChanged(GameManager.GameState state)
    {
        switch (state)
        {
            case GameManager.GameState.StartScreen:
                ShowStartScreen();
                break;
            case GameManager.GameState.Countdown:
                // GameManager 会调用 ShowCountdown / UpdateCountdown / HideCountdown
                // 这里确保 start/end 隐藏
                if (startScreenPanel != null) startScreenPanel.SetActive(false);
                if (endPanel != null) endPanel.SetActive(false);
                break;
            case GameManager.GameState.Playing:
                HideStartAndCountdown();
                if (endPanel != null) endPanel.SetActive(false);
                break;
            case GameManager.GameState.Ended:
                // EndPanel 将由 GameManager.ShowEndScreen 调用 ShowEndScreen
                break;
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.State == GameManager.GameState.StartScreen)
        {
            if (Input.anyKeyDown)
            {
                Debug.Log("[Game] 任意键按下：开始倒计时并进入比赛（StartFromStartScreen）");
                GameManager.Instance.StartFromStartScreen();
                
            }
        }
        else if (GameManager.Instance.State == GameManager.GameState.Ended)
        {
            if (Input.anyKeyDown)
            {
                // 返回到开始界面
                GameManager.Instance.ReturnToStartScreen();
            }
        }
    }

    #region Countdown / Start / End UI (called by GameManager)

    public void ShowCountdown(int seconds)
    {
        if (countdownPanel != null) countdownPanel.SetActive(true);
        if (countdownText != null) countdownText.text = seconds.ToString();
    }

    public void UpdateCountdown(int seconds)
    {
        if (countdownText != null) countdownText.text = seconds.ToString();
    }

    public void HideCountdown()
    {
        if (countdownPanel != null) countdownPanel.SetActive(false);
    }

    public void ShowStartScreen()
    {
        if (startScreenPanel != null) startScreenPanel.SetActive(true);
        if (countdownPanel != null) countdownPanel.SetActive(false);
        if (endPanel != null) endPanel.SetActive(false);
    }

    private void HideStartAndCountdown()
    {
        if (startScreenPanel != null) startScreenPanel.SetActive(false);
        if (countdownPanel != null) countdownPanel.SetActive(false);
    }

    /// <summary>
    /// 显示结束面板。winner 为 null 表示平局
    /// </summary>
    public void ShowEndScreen(PlayerStats winner)
    {
        if (endPanel != null) endPanel.SetActive(true);
        if (endResultText != null)
        {
            if (winner == null)
            {
                endResultText.text = "平局";
            }
            else
            {
                if (GameManager.Instance != null && GameManager.Instance.player1 == winner)
                {
                    endResultText.text = "玩家1获胜";
                }
                else
                {
                    endResultText.text = "玩家2获胜";
                }
            }
        }
    }

    #endregion

    #region Timer Display

    private void UpdateTimerDisplay(float seconds)
    {
        if (timerText == null) return;
        timerText.text = FormatTime((int)Mathf.Ceil(seconds));
    }

    private string FormatTime(int totalSeconds)
    {
        int m = totalSeconds / 60;
        int s = totalSeconds % 60;
        return $"{m:D1}:{s:D2}";
    }

    #endregion

    #region Player HUD Updates (call from Player scripts)

    /// <summary>
    /// 更新玩家血量显示。playerIndex: 1 或 2
    /// 请在 PlayerStats.ResetStats() 和 TakeDamage(...) 后调用以保证 UI 实时同步
    /// </summary>
    public void UpdatePlayerHealth(int playerIndex, int current, int max)
    {
        float fill = 0f;
        if (max > 0) fill = Mathf.Clamp01((float)current / max);

        if (playerIndex == 1 && p1HealthFill != null)
        {
            p1HealthFill.fillAmount = fill;
        }
        else if (playerIndex == 2 && p2HealthFill != null)
        {
            p2HealthFill.fillAmount = fill;
        }
    }

    /// <summary>
    /// 更新玩家当前面具图标（拾取/丢弃时调用）
    /// playerIndex: 1 或 2
    /// 若 icon 为 null 则显示默认图标（或隐藏）
    /// </summary>
    public void UpdatePlayerMaskIcon(int playerIndex, Sprite icon)
    {
        if (icon == null) icon = defaultMaskSprite;

        if (playerIndex == 1 && p1MaskIcon != null)
        {
            p1MaskIcon.sprite = icon;
            p1MaskIcon.enabled = icon != null;
        }
        else if (playerIndex == 2 && p2MaskIcon != null)
        {
            p2MaskIcon.sprite = icon;
            p2MaskIcon.enabled = icon != null;
        }
    }

    #endregion
}
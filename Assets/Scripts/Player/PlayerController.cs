//csharp Assets/Scripts/Player/PlayerController.cs
using System.Collections;
using UnityEngine;

/// <summary>
/// 玩家移动控制（基于 Rigidbody2D）
/// - 使用 InputManager 提供的移动向量
/// - 支持临时拾取加速（0.5s）
/// TODO: 将此脚本挂在 Player Prefab 根节点，绑定 Rigidbody2D
/// TODO: 在 Prefab 上为不同玩家设置 isPlayerOne = true/false
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Player Identity")]
    [Tooltip("是否为玩家1（使用 InputManager.Player1Move），否则使用 Player2Move")]
    public bool isPlayerOne = true;

    [Header("Movement")]
    [Tooltip("基础移动速度（单位：unit/s），在 Inspector 调整以平衡")]
    public float moveSpeed = 4f;

    [Tooltip("拾取后加速倍率（例如 1.5 表示 50% 加速）")]
    public float pickupSpeedMultiplier = 1.5f;

    [Tooltip("拾取后加速持续时间（秒），GDD 推荐 0.5s")]
    public float pickupSpeedDuration = 0.5f;

    private Rigidbody2D rb;
    private float currentSpeed;
    private Coroutine speedBuffCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        currentSpeed = moveSpeed;
    }

    private void Update()
    {
        // 读取输入（由 InputManager 缓存）
        Vector2 move = Vector2.zero;
        if (InputManager.Instance != null)
        {
            move = isPlayerOne ? InputManager.Instance.Player1Move : InputManager.Instance.Player2Move;
        }
        // 使用物理移动在 FixedUpdate
        ApplyMovement(move);
    }

    private void ApplyMovement(Vector2 dir)
    {
        // 直接设置 velocity 更简洁（Rigidbody2D）：
        rb.velocity = dir * currentSpeed;
    }

    /// <summary>
    /// 在拾取面具时调用以触发短时加速
    /// </summary>
    public void ApplyPickupSpeedBuff()
    {
        if (speedBuffCoroutine != null)
        {
            StopCoroutine(speedBuffCoroutine);
        }
        speedBuffCoroutine = StartCoroutine(SpeedBuffCoroutine());
    }

    private IEnumerator SpeedBuffCoroutine()
    {
        currentSpeed = moveSpeed * pickupSpeedMultiplier;
        float timer = pickupSpeedDuration;
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        currentSpeed = moveSpeed;
        speedBuffCoroutine = null;
    }

    /// <summary>
    /// 外部用于临时设置移动速度（例如 UI 调试）
    /// </summary>
    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
        currentSpeed = newSpeed;
    }
}
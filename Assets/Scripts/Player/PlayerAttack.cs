//csharp Assets/Scripts/Player/PlayerAttack.cs
using System.Collections;
using UnityEngine;

/// <summary>
/// 玩家攻击逻辑：按键触发，冷却，范围判定，面具克制判定
/// - 需要在 Inspector 中设置：是否为 Player1、伤害值、攻击半径、目标引用（对手）
/// - 当前面具使用 GameManager.MaskType（若后续使用 ScriptableObject，可替换）
/// TODO: 将此脚本挂在 Player Prefab；在 Inspector 指定 opponentStats（对手的 PlayerStats）
/// TODO: 若使用 Hitbox prefab 或动画事件，可在 Attack 方法中触发
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    [Header("Identity")]
    [Tooltip("是否为玩家1（决定从 InputManager 读取按键）")]
    public bool isPlayerOne = true;

    [Header("Attack Settings")]
    [Tooltip("攻击伤害（整数）")]
    public int attackDamage = 1;

    [Tooltip("攻击判定半径（单位：unit）")]
    public float attackRange = 1.0f;

    [Tooltip("攻击冷却（秒），GDD 指定 1 秒")]
    public float attackCooldown = 1.0f;

    [Tooltip("攻击判定点（默认为自身位置，可指定子物体 Transform）")]
    public Transform attackPoint;

    [Header("References (assign in Inspector)")]
    [Tooltip("对手的 PlayerStats（用于直接伤害调用）")]
    public PlayerStats opponentStats;

    [Tooltip("对手 Transform（用于距离判定），可与 opponentStats.transform 相同")]
    public Transform opponentTransform;

    // 当前玩家佩戴的面具（若无则为 null）
    [Tooltip("当前面具类型；若使用 ScriptableObject 管理面具，可在拾取逻辑中设置此值")]
    public GameManager.MaskType? currentMask = null;

    private float lastAttackTime = -999f;

    private void Reset()
    {
        // 默认使用自身 transform 作为 attackPoint，如果未指定
        if (attackPoint == null) attackPoint = this.transform;
    }

    private void Update()
    {
        // 读取按键触发一次性按下（InputManager 保存了按键 Down 状态）
        bool attackPressed = false;
        if (InputManager.Instance != null)
        {
            attackPressed = isPlayerOne ? InputManager.Instance.Player1AttackPressed : InputManager.Instance.Player2AttackPressed;
        }

        if (attackPressed) TryAttack();
    }

    private void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        lastAttackTime = Time.time;

        // 必须有面具才能攻击
        if (currentMask == null) return;

        // 检查对手是否在范围内
        if (opponentTransform == null || opponentStats == null) return;
        float dist = Vector2.Distance(attackPoint.position, opponentTransform.position);
        if (dist > attackRange) return;

        // 检查克制关系：当前玩家的面具是否克制对手的面具
        if (opponentStats != null)
        {
            // 需要对手面具信息：这里假设对手的 PlayerAttack 脚本也挂在对手并公开 currentMask
            PlayerAttack opponentAttack = opponentStats.GetComponent<PlayerAttack>();
            GameManager.MaskType? opponentMask = opponentAttack != null ? opponentAttack.currentMask : null;

            if (opponentMask == null) return; // 对手无面具，攻击无效（GDD 指出：未佩戴面具无法攻击对方）
            // 只有当 attackerMask 克制 defenderMask 时才生效
            if (GameManager.Instance != null && GameManager.Instance.IsCounter(currentMask.Value, opponentMask.Value))
            {
                // 命中并造成伤害
                opponentStats.TakeDamage(attackDamage);

                // TODO: 播放命中音效 / 特效
            }
            else
            {
                // 攻击无效（无克制），可在此播放失败音效或特效反馈
            }
        }
    }

    /// <summary>
    /// 外部调用：设置当前面具类型（例如 MaskPickup 在拾取后调用）
    /// </summary>
    public void SetMask(GameManager.MaskType? mask)
    {
        currentMask = mask;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) attackPoint = this.transform;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
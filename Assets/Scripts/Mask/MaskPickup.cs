//csharp Assets/Scripts/Mask/MaskPickup.cs
using UnityEngine;

/// <summary>
/// 玩家拾取面具逻辑：挂在 Player 根节点
/// - 监听 InputManager 的拾取按键（按下一次触发）
/// - 在 pickupRange 内检测面具（使用 LayerMask 过滤）
/// - 拾取后：替换玩家面具（PlayerAttack.SetMask）、销毁面具、触发短时加速（PlayerController.ApplyPickupSpeedBuff）
/// TODO: 在 Inspector 设置 pickupRange、maskLayer，并确保 Player 上挂有 PlayerAttack 和 PlayerController 组件
/// </summary>
[RequireComponent(typeof(PlayerAttack))]
[RequireComponent(typeof(PlayerController))]
public class MaskPickup : MonoBehaviour
{
    [Tooltip("是否为玩家1（对应 InputManager）")]
    public bool isPlayerOne = true;

    [Tooltip("拾取判定半径（单位：unit）")]
    public float pickupRange = 0.8f;

    [Tooltip("面具所在的 Layer（用于过滤 OverlapCircle）")]
    public LayerMask maskLayer;

    // 组件缓存
    private PlayerAttack attackComp;
    private PlayerController controllerComp;

    private void Awake()
    {
        attackComp = GetComponent<PlayerAttack>();
        controllerComp = GetComponent<PlayerController>();
    }

    private void Update()
    {
        bool pickupPressed = false;
        if (InputManager.Instance != null)
        {
            pickupPressed = isPlayerOne ? InputManager.Instance.Player1PickupPressed : InputManager.Instance.Player2PickupPressed;
        }

        if (pickupPressed)
        {
            TryPickup();
        }
    }

    private void TryPickup()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pickupRange, maskLayer);
        if (hits == null || hits.Length == 0) return;

        // 选择第一个可拾取的面具
        Mask chosen = null;
        foreach (var c in hits)
        {
            var m = c.GetComponent<Mask>();
            if (m != null)
            {
                chosen = m;
                break;
            }
        }
        if (chosen == null) return;

        // 处理当前面具：若已有旧面具，直接替换（GDD 指示销毁旧面具）
        attackComp.SetMask(chosen.GetMaskType());

        // 触发加速
        controllerComp.ApplyPickupSpeedBuff();

        // 销毁面具对象（MaskSpawner 会通过 MaskLifecycleWatcher 处理记录）
        Destroy(chosen.gameObject);

        // TODO: 播放拾取音效 / 特效 / 更新 UI（例如通知 UIManager 更新当前面具图标）
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}
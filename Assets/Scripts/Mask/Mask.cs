//csharp Assets/Scripts/Mask/Mask.cs
using UnityEngine;

/// <summary>
/// 面具场景实例组件：挂到面具 Prefab 根节点上
/// - 被拾取时由 Player 的 MaskPickup 处理
/// - 可持有 MaskData 以便 UI/显示使用
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Mask : MonoBehaviour
{
    [Tooltip("面具数据（ScriptableObject），可在生成时赋值")]
    public MaskData data;

    /// <summary>
    /// 方便直接通过脚本设置面具数据并更新显示（在 MaskSpawner 中调用）
    /// </summary>
    public void Initialize(MaskData maskData)
    {
        data = maskData;
        ApplyVisuals();
    }

    private void ApplyVisuals()
    {
        if (data == null) return;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && data.worldSprite != null)
        {
            sr.sprite = data.worldSprite;
        }
        // 其他视觉/色彩设置可以在这里扩展
    }

    public GameManager.MaskType GetMaskType()
    {
        return data != null ? data.maskType : GameManager.MaskType.Red;
    }
}
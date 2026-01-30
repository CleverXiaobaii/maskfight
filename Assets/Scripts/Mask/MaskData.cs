//csharp Assets/Scripts/Mask/MaskData.cs
using UnityEngine;

/// <summary>
/// 面具数据（ScriptableObject）
/// 包含面具类型、精灵和颜色等信息
/// </summary>
[CreateAssetMenu(menuName = "Mask/MaskData", fileName = "NewMaskData")]
public class MaskData : ScriptableObject
{
    [Tooltip("对应的面具类型（与 GameManager.MaskType 保持一致）")]
    public GameManager.MaskType maskType;

    [Tooltip("面具在 UI 中显示的图标 / 精灵")]
    public Sprite icon;

    [Tooltip("面具在场景中显示的精灵（可选，Prefab 通常自带 SpriteRenderer）")]
    public Sprite worldSprite;

    [Tooltip("面具代表颜色，供渲染/调试使用")]
    public Color color = Color.white;

    [Tooltip("附加描述（调试用）")]
    [TextArea(2,4)]
    public string description;
}
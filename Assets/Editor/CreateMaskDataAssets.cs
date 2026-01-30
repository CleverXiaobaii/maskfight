//csharp Assets/Editor/CreateMaskDataAssets.cs
using UnityEditor;
using UnityEngine;

/// <summary>
/// 编辑器工具：在 Assets/ScriptableObjects/MaskDatas 下生成 6 个默认 MaskData 资源（仅包含颜色与类型）
/// 使用：Unity 菜单 Mask -> Create Default MaskData Assets
/// </summary>
public static class CreateMaskDataAssets
{
    [MenuItem("Mask/Create Default MaskData Assets")]
    public static void CreateDefaultMaskDatas()
    {
        string parentFolder = "Assets/ScriptableObjects";
        string folder = parentFolder + "/MaskDatas";
        if (!AssetDatabase.IsValidFolder(parentFolder))
        {
            AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
        }
        if (!AssetDatabase.IsValidFolder(folder))
        {
            AssetDatabase.CreateFolder(parentFolder, "MaskDatas");
        }

        string[] names = { "Red", "Orange", "Yellow", "Green", "Blue", "Purple" };
        Color[] colors = {
            Color.red,
            new Color(1f, 0.5f, 0f), // orange
            Color.yellow,
            Color.green,
            Color.blue,
            new Color(0.5f, 0f, 0.5f) // purple
        };

        for (int i = 0; i < names.Length; i++)
        {
            string path = $"{folder}/MaskData_{names[i]}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<MaskData>(path);
            if (existing != null)
            {
                Debug.Log($"[MaskData] 已存在：{path}，跳过。");
                continue;
            }

            var data = ScriptableObject.CreateInstance<MaskData>();
            data.maskType = (GameManager.MaskType)i;
            data.color = colors[i];
            data.description = $"{names[i]} mask (auto-created)";
            AssetDatabase.CreateAsset(data, path);
            Debug.Log($"[MaskData] 创建：{path}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[MaskData] 默认 6 个 MaskData 已生成（或已存在时跳过）。");
    }
}
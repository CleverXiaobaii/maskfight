using UnityEngine;

// 游戏管理器 - 单例模式 + 定时生成Mask预制体
public class GameManager : MonoBehaviour
{
    // 单例实例（全局唯一访问点）
    public static GameManager Instance;

    [Header("Mask预制体生成配置")]
    [Tooltip("需要生成的Mask预制体（必须赋值）")]
    public GameObject maskPrefab1; // 拖入你的Mask预制体
    public GameObject maskPrefab2; // 拖入你的Mask预制体
    public GameObject maskPrefab3;
    public GameObject maskPrefab4;
    [Tooltip("生成间隔（秒）")]
    public float spawnInterval = 5f; // 每隔3秒生成一个
    [Tooltip("生成延迟（游戏启动后多久开始第一次生成，秒）")]
    public float startDelay = 1f; // 1秒后开始生成
    [Tooltip("生成位置范围（以当前物体为中心）")]
    public Vector2 spawnRange = new Vector2(8f, 5f); // X轴±5，Y轴±3

    // 初始化单例
    private void Awake()
    {
        /**
        // 单例逻辑：确保只有一个GameManager实例
        if (Instance == null)
        {
            Instance = this;
            // 可选：场景切换时不销毁（根据需求决定是否开启）
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // 销毁重复的实例
        }*/
    }

    // 游戏启动时开始定时生成
    private void Start()
    {
        /**
        // 安全校验：如果预制体未赋值，给出提示并停止逻辑
        if (maskPrefab == null)
        {
            Debug.LogError("GameManager中Mask预制体未赋值！请在Inspector面板拖入预制体");
            return;
        }*/

        // 启动定时生成：参数（生成方法名，首次延迟，重复间隔）
        InvokeRepeating("SpawnMask", startDelay, spawnInterval);
        Debug.Log($"GameManager启动，将在{startDelay}秒后开始，每隔{spawnInterval}秒生成Mask");
    }

    // 核心方法：生成Mask预制体
    private void SpawnMask()
    {
        // 1. 计算随机生成位置（以GameManager所在物体为中心）
        float randomX = transform.position.x + Random.Range(-spawnRange.x, spawnRange.x);
        float randomY = transform.position.y + Random.Range(-spawnRange.y, spawnRange.y);
        // Z轴保持和GameManager一致（2D游戏可忽略Z轴）
        Vector3 spawnPos = new Vector3(randomX, randomY, transform.position.z);

        // 2. 实例化预制体
        GameObject newMask;
        int d = Random.Range(1, 5);
        if (d == 1)
            newMask = Instantiate(maskPrefab1, spawnPos, Quaternion.identity);
        else if (d == 2)
            newMask = Instantiate(maskPrefab2, spawnPos, Quaternion.identity);
        else if (d == 3)
            newMask = Instantiate(maskPrefab3, spawnPos, Quaternion.identity);
        else if (d == 4)
            newMask = Instantiate(maskPrefab4, spawnPos, Quaternion.identity);


        // 可选：给生成的Mask命名（方便调试）
        //newMask.name = "Mask_" + Time.time.ToString("F0");

        Debug.Log($"生成Mask预制体，位置：{spawnPos}");
    }

    // 可选方法：手动停止生成（比如游戏结束时调用）
    public void StopSpawnMask()
    {
        CancelInvoke("SpawnMask");
        Debug.Log("已停止生成Mask预制体");
    }

    // 可选方法：手动触发一次生成（比如玩家触发事件时）
    /**public void SpawnMaskOnce()
    {
        if (maskPrefab != null)
        {
            SpawnMask();
        }
    }*/
}
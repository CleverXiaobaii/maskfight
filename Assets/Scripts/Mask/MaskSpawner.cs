//csharp Assets/Scripts/Mask/MaskSpawner.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 面具生成器（基于 Update 的调度）
/// - 在 GameManager.State == Playing 且启用时按随机间隔在随机位置生成面具
/// - 保留 public StartSpawning()/StopSpawning()/ClearAllMasks()/NotifyMaskDestroyed() 接口
/// - spawnIntervalMin/Max 控制随机间隔；使用 nextSpawnTime 调度下一次生成
/// TODO: 在 Inspector 指定 maskPrefabs（一个通用面具 prefab 列表）与 maskDatas（6 个 MaskData）
/// </summary>
public class MaskSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("面具 Prefab 列表（通常都是同一个 prefab，但可以不同）")]
    public List<GameObject> maskPrefabs;

    [Tooltip("对应的 MaskData 列表（数量与 maskPrefabs 可不同，但至少包含面具类型数据）")]
    public List<MaskData> maskDatas;

    [Tooltip("生成间隔最小值（秒）")]
    public float spawnIntervalMin = 3f;

    [Tooltip("生成间隔最大值（秒）")]
    public float spawnIntervalMax = 5f;

    [Tooltip("场景内最多同时存在的面具数量")]
    public int maxMasks = 4;

    [Header("Spawn Area")]
    [Tooltip("指定一个 Collider2D 作为生成范围（例如一个 BoxCollider2D 或 PolygonCollider2D），随机点将在其 bounds 内生成")]
    public Collider2D spawnAreaCollider; // TODO: 指定场景中的 Collider2D（边界内）

    [Header("Safety")]
    [Tooltip("生成点与玩家的最小距离，避免生成到玩家脚下")]
    public float minDistanceFromPlayers = 1.0f;

    [Tooltip("在生成检测时使用的 LayerMask（用于检测玩家/障碍等）")]
    public LayerMask groundAndPlayerMask;

    private List<GameObject> spawned = new List<GameObject>();

    // 使用 Update 调度
    private bool spawningEnabled = false;
    private float nextSpawnTime = 0f;

    private void Start()
    {
        // 订阅状态变更，兼容事件驱动或显式控制两种用法
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
        }
    }

    private void HandleStateChanged(GameManager.GameState state)
    {
        if (state == GameManager.GameState.Playing)
        {
            // 当 GameManager 进入 Playing 时启动（如果没有显式调用 StartSpawning 也能工作）
            StartSpawning();
        }
        else
        {
            // 非 Playing 状态时停止生成并清理现有面具
            StopSpawning();
            ClearAllMasks();
        }
    }

    private void Update()
    {
        // 仅在启用且处于 Playing 状态下工作
        if (!spawningEnabled) return;
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.State != GameManager.GameState.Playing) return;
        //Debug.Log(Time.time);

        // 如果已达上限，延迟下一次检查
        if (spawned.Count >= maxMasks)
        {
            // 延后下一次尝试，避免每帧检测造成无效开销
            nextSpawnTime = Time.time + Random.Range(spawnIntervalMin, spawnIntervalMax);
            return;
        }

        if (Time.time >= nextSpawnTime)
        {
            TrySpawnOne();
            // 计划下一次生成时间
            nextSpawnTime = Time.time + Random.Range(spawnIntervalMin, spawnIntervalMax);
        }
    }

    /// <summary>
    /// 公共接口：开始生成面具（幂等）
    /// </summary>
    public void StartSpawning()
    {
        if (spawningEnabled) return;
        spawningEnabled = true;
        // 立即安排第一次生成（或延后一个随机间隔）
        nextSpawnTime = Time.time + Random.Range(spawnIntervalMin, spawnIntervalMax);
    }

    /// <summary>
    /// 公共接口：停止生成面具（幂等）
    /// </summary>
    public void StopSpawning()
    {
        spawningEnabled = false;
    }

    private void TrySpawnOne()
    {
        if (spawnAreaCollider == null) return;
        if (spawned.Count >= maxMasks) return;
        //Debug.Log("Trying to spawn mask...");
        Vector2 point;
        int attempts = 0;
        bool found = false;
        do
        {
            point = GetRandomPointInCollider(spawnAreaCollider);
            attempts++;
            // 避免生成到玩家附近
            if (IsPointFarFromPlayers(point) && !Physics2D.OverlapPoint(point, groundAndPlayerMask))
            {
                found = true;
                break;
            }
        } while (attempts < 10);

        if (!found) return;
        //Debug.Log("Trying to spawn mask...");
        // 随机选择一种面具类型（均等概率）
        int dataIndex = 0;
        if (maskDatas != null && maskDatas.Count > 0)
        {
            dataIndex = Random.Range(0, maskDatas.Count);
        }

        GameObject prefab = null;
        if (maskPrefabs != null && maskPrefabs.Count > 0)
        {
            prefab = maskPrefabs[Random.Range(0, maskPrefabs.Count)];
        }

        if (prefab == null) return;
        GameObject go = Instantiate(prefab, point, Quaternion.identity, transform);
        Debug.Log("Trying to spawn mask...");
        Mask maskComp = go.GetComponent<Mask>();
        if (maskComp != null && maskDatas != null && maskDatas.Count > 0)
        {
            maskComp.Initialize(maskDatas[dataIndex]);
        }
        spawned.Add(go);

        // 当面具被销毁（被拾取或其它原因）时，自动从列表移除
        MaskLifecycleWatcher watcher = go.AddComponent<MaskLifecycleWatcher>();
        watcher.Init(this, go);
    }

    private Vector2 GetRandomPointInCollider(Collider2D col)
    {
        Bounds b = col.bounds;
        float x = Random.Range(b.min.x, b.max.x);
        float y = Random.Range(b.min.y, b.max.y);
        return new Vector2(x, y);
    }

    private bool IsPointFarFromPlayers(Vector2 point)
    {
        if (GameManager.Instance == null) return true;
        bool ok = true;
        if (GameManager.Instance.player1 != null)
        {
            ok &= Vector2.Distance(point, GameManager.Instance.player1.transform.position) >= minDistanceFromPlayers;
        }
        if (GameManager.Instance.player2 != null)
        {
            ok &= Vector2.Distance(point, GameManager.Instance.player2.transform.position) >= minDistanceFromPlayers;
        }
        return ok;
    }

    /// <summary>
    /// 清理当前场景中的所有已生成面具（public）
    /// </summary>
    public void ClearAllMasks()
    {
        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            if (spawned[i] != null) Destroy(spawned[i]);
        }
        spawned.Clear();
    }

    /// <summary>
    /// 当某个面具被销毁/拾取时由 MaskLifecycleWatcher 调用，移除记录以允许继续生成
    /// </summary>
    public void NotifyMaskDestroyed(GameObject maskGO)
    {
        if (spawned.Contains(maskGO)) spawned.Remove(maskGO);
    }

    // 内部类：用于监听单个面具的销毁并回调 spawner（避免每帧检测）
    private class MaskLifecycleWatcher : MonoBehaviour
    {
        private MaskSpawner spawner;
        private GameObject target;

        public void Init(MaskSpawner spawner, GameObject target)
        {
            this.spawner = spawner;
            this.target = target;
        }

        private void OnDestroy()
        {
            if (spawner != null && target != null)
            {
                spawner.NotifyMaskDestroyed(target);
            }
        }
    }
}
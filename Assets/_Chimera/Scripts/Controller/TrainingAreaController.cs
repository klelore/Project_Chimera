using UnityEngine;
using System.Collections.Generic;

public class TrainingAreaController : MonoBehaviour
{
    [Header("Prefabs & References")]
    public GameObject obstaclePrefab;
    public Transform obstacleHolder;
    public Transform agentTransform;
    public Transform targetTransform;

    [Header("Spawn Settings")]

    public Vector2 roomSize = new Vector2(30, 30);
    public float wallPadding = 4.0f;
    public float minDistance = 2.0f;

    [Header("Debug")]
    public bool showDebugLogs = true; // 开启这个可以在控制台看到计算出的范围

    private List<GameObject> spawnedObstacles = new List<GameObject>();

    // 记录初始位置
    private Vector3 startAgentPos;
    private Quaternion startAgentRot;
    private Vector3 startTargetPos;
    private Quaternion startTargetRot;

    private void Awake()
    {
        // 1. 记录你一开始摆好的位置
        if (agentTransform != null)
        {
            startAgentPos = agentTransform.localPosition;
            startAgentRot = agentTransform.localRotation;
        }

        if (targetTransform != null)
        {
            startTargetPos = targetTransform.localPosition;
            startTargetRot = targetTransform.localRotation;
        }
    }

    public void ResetArea()
    {
        ClearObstacles();

        ResetAgentAndTarget();

        SpawnObstacles();
    }

    void ResetAgentAndTarget()
    {
        if (agentTransform != null)
        {
            agentTransform.localPosition = startAgentPos;
            agentTransform.localRotation = startAgentRot;
            Physics.SyncTransforms(); // 强制刷新物理位置
        }
        if (targetTransform != null)
        {
            targetTransform.localPosition = startTargetPos;
            targetTransform.localRotation = startTargetRot;
            Physics.SyncTransforms();
        }
    }

    void SpawnObstacles()
    {
        // 检查设置是否正确
        float xLimitTest = (roomSize.x / 2) - wallPadding;
        if (xLimitTest <= 0 && showDebugLogs)
        {
            Debug.LogError($"[TrainingArea] 警告！生成范围过小！RoomSize: {roomSize}, WallPadding: {wallPadding}. 计算出的范围是: {xLimitTest}。请在 Inspector 面板把 RoomSize 改大（比如 30,30）！");
        }

        for (int i = 0; i < 20; i++) // 假设生成 10 个
        {
            GameObject obj = Instantiate(obstaclePrefab, obstacleHolder);
            spawnedObstacles.Add(obj);
            MoveToRandomPosition(obj.transform);
        }
    }

    void ClearObstacles()
    {
        foreach (var obj in spawnedObstacles)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedObstacles.Clear();
    }

    void MoveToRandomPosition(Transform t)
    {
        bool validPosition = false;
        Vector3 newPos = Vector3.zero;
        int attempts = 0;

        // 计算边界
        float xLimit = (roomSize.x / 2) - wallPadding;
        float zLimit = (roomSize.y / 2) - wallPadding;

        // 如果算出来是负数或0，强制给一个最小范围防止卡死，并报错
        if (xLimit <= 0) xLimit = 1f;
        if (zLimit <= 0) zLimit = 1f;

        while (!validPosition && attempts < 50)
        {
            attempts++;

            // 直接使用随机范围，不要乘 10 了，否则 11米*10=110米，会飞出地图
            float x = Random.Range(-xLimit, xLimit);
            float z = Random.Range(-zLimit, zLimit);

            // 相对于父物体(TrainingArea)的中心点生成
            newPos = transform.position + new Vector3(x, 0.5f, z);

            if (IsPositionSafe(newPos))
            {
                validPosition = true;
            }
        }

        if (showDebugLogs && attempts == 1)
        {
            // 只在第一次打印，避免刷屏
            // Debug.Log($"生成位置: {newPos} (范围: -{xLimit} ~ {xLimit})");
        }

        t.position = newPos;
        t.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
    }

    bool IsPositionSafe(Vector3 pos)
    {
        // 避开障碍物
        foreach (var obstacle in spawnedObstacles)
        {
            if (obstacle == null || obstacle.transform.position == Vector3.zero) continue;
            if (Vector3.Distance(pos, obstacle.transform.position) < minDistance) return false;
        }
        // 避开 Agent (此时 Agent 已在固定位置)
        if (agentTransform != null && Vector3.Distance(pos, agentTransform.position) < minDistance) return false;
        // 避开 Target (此时 Target 已在固定位置)
        if (targetTransform != null && Vector3.Distance(pos, targetTransform.position) < minDistance) return false;

        return true;
    }
}
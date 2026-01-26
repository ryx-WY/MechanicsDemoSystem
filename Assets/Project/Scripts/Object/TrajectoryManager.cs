using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 轨迹管理器（全局单例）：管理所有历史轨迹，解耦小球与轨迹
/// 挂载到空物体TrajectoryManager上
/// </summary>
public class TrajectoryManager : MonoBehaviour
{
    public static TrajectoryManager Instance;

    [Header("轨迹默认参数")]
    public float lineWidth = 0.05f;
    public float recordInterval = 0.01f;
    // 轨迹父物体（用于整理Hierarchy，避免杂乱）
    public Transform trajectoryParent;

    // 所有历史轨迹的容器
    private List<LineRenderer> allTrajectories = new List<LineRenderer>();
    // 当前正在记录的轨迹
    private LineRenderer currentTrajectory;
    // 当前轨迹的点集合
    private List<Vector3> currentTrajectoryPoints = new List<Vector3>();
    private float lastRecordTime;
    private bool isRecording;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 自动创建轨迹父物体（如果未指定）
        if (trajectoryParent == null)
        {
            GameObject parent = new GameObject("Trajectories");
            trajectoryParent = parent.transform;
        }
    }

    /// <summary>
    /// 开始记录新轨迹（每次点击"开始"时调用）
    /// </summary>
    /// <param name="startPos">小球起始位置</param>
    /// <returns>本次轨迹的随机颜色</returns>
    public Color StartNewTrajectory(Vector3 startPos)
    {
        // 停止上一次记录（防止重复）
        StopRecording();

        // 1. 创建新的轨迹LineRenderer
        GameObject trajectoryObj = new GameObject($"Trajectory_{allTrajectories.Count + 1}");
        trajectoryObj.transform.SetParent(trajectoryParent);
        currentTrajectory = trajectoryObj.AddComponent<LineRenderer>();

        // 2. 初始化轨迹样式
        Color randomColor = GetRandomColor();
        currentTrajectory.material = new Material(Shader.Find("Unlit/Color"));
        currentTrajectory.material.color = randomColor;
        currentTrajectory.startColor = randomColor;
        currentTrajectory.endColor = randomColor;
        currentTrajectory.startWidth = lineWidth;
        currentTrajectory.endWidth = lineWidth;
        currentTrajectory.useWorldSpace = true;
        currentTrajectory.positionCount = 0;

        // 3. 初始化轨迹点
        currentTrajectoryPoints.Clear();
        currentTrajectoryPoints.Add(startPos); // 记录起始点
        currentTrajectory.positionCount = 1;
        currentTrajectory.SetPosition(0, startPos);

        // 4. 开始记录
        isRecording = true;
        lastRecordTime = Time.time;

        // 5. 将新轨迹加入历史列表
        allTrajectories.Add(currentTrajectory);

        return randomColor;
    }

    /// <summary>
    /// 实时记录轨迹点（Update中调用）
    /// </summary>
    /// <param name="currentPos">小球当前位置</param>
    public void RecordTrajectoryPoint(Vector3 currentPos)
    {
        if (!isRecording || currentTrajectory == null) return;

        // 按间隔记录，避免点过多
        if (Time.time - lastRecordTime >= recordInterval)
        {
            currentPos.z = 0; // 固定Z轴，保持2D效果
            // 避免重复记录同一位置
            if (currentTrajectoryPoints.Count == 0 ||
                Vector3.Distance(currentPos, currentTrajectoryPoints[^1]) > 0.005f)
            {
                currentTrajectoryPoints.Add(currentPos);
                // 更新轨迹渲染
                currentTrajectory.positionCount = currentTrajectoryPoints.Count;
                currentTrajectory.SetPositions(currentTrajectoryPoints.ToArray());
            }
            lastRecordTime = Time.time;
        }
    }

    /// <summary>
    /// 停止记录当前轨迹
    /// </summary>
    public void StopRecording()
    {
        isRecording = false;
        currentTrajectory = null;
        currentTrajectoryPoints.Clear();
    }

    /// <summary>
    /// 清空所有历史轨迹（仅重置场景时调用）
    /// </summary>
    public void ClearAllTrajectories()
    {
        StopRecording();
        // 销毁所有轨迹对象
        foreach (var line in allTrajectories)
        {
            if (line != null) Destroy(line.gameObject);
        }
        allTrajectories.Clear();
    }

    /// <summary>
    /// 生成随机轨迹颜色（保证亮度和透明度，便于区分）
    /// </summary>
    private Color GetRandomColor()
    {
        // 限制亮度范围（0.5~1），避免太暗；透明度固定0.8
        return new Color(
            Random.Range(0.5f, 1f),
            Random.Range(0.5f, 1f),
            Random.Range(0.5f, 1f),
            0.8f
        );
    }

    // 外部获取状态
    public bool IsRecording() => isRecording;
}

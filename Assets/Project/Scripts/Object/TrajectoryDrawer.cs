//--------------------------------------------------------------------------------------------------
//E:\Unity\Project\MechanicsDemoSystem\Assets\Project\Scripts\Object\TrajectoryDrawer.cs
using UnityEngine;
using System.Collections.Generic;

public class TrajectoryDrawer : MonoBehaviour
{
    [Header("轨迹配置（保留完整路径）")]
    public LineRenderer lineRenderer;
    [Tooltip("轨迹点记录间隔（越小越密，建议0.01）")]
    public float recordInterval = 0.01f;
    [Tooltip("轨迹线宽度")]
    public float lineWidth = 0.05f;
    [Tooltip("轨迹颜色")]
    public Color trajectoryColor = new Color(1, 0, 0, 0.8f);

    // 核心：存储所有轨迹点（从抛出到当前），不再限制数量
    private List<Vector3> trajectoryPoints = new List<Vector3>();
    private ObjectPhysicsController physicsController;
    private float lastRecordTime;
    // 标记：是否已记录初始抛出点（避免漏记出发点）
    private bool hasRecordedStartPoint = false;

    void Awake()
    {
        physicsController = GetComponent<ObjectPhysicsController>();
        InitLineRenderer();
        lastRecordTime = 0;
    }

    /// <summary>
    /// 初始化轨迹渲染器
    /// </summary>
    private void InitLineRenderer()
    {
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        // 优化材质，避免重影
        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        lineRenderer.material.color = trajectoryColor;
        lineRenderer.startColor = trajectoryColor;
        lineRenderer.endColor = trajectoryColor;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;
    }

    void Update()
    {
        // 仅在运动中且未暂停时记录轨迹
        if (GlobalUIController.Instance.IsMotionRunning() && !GlobalUIController.Instance.IsMotionPaused())
        {
            // 强制记录初始抛出点（运动开始时第一时间记录）
            if (!hasRecordedStartPoint)
            {
                RecordStartPoint();
                hasRecordedStartPoint = true; // 仅记录一次
            }

            // 按间隔记录后续轨迹点（确保连续）
            if (Time.time - lastRecordTime >= recordInterval)
            {
                RecordTrajectoryPoint();
                lastRecordTime = Time.time;
            }
        }
        // 暂停时：保持轨迹显示（不清理、不修改）
    }

    /// <summary>
    /// 强制记录初始抛出点（确保出发点不丢失）
    /// </summary>
    private void RecordStartPoint()
    {
        Vector3 startPos = transform.position;
        startPos.z = 0; // 固定Z轴，避免3D偏移
        trajectoryPoints.Clear(); // 清空残留点，确保从出发点开始
        trajectoryPoints.Add(startPos);
        // 立即更新轨迹渲染
        lineRenderer.positionCount = trajectoryPoints.Count;
        lineRenderer.SetPositions(trajectoryPoints.ToArray());
    }

    /// <summary>
    /// 记录当前位置的轨迹点（保留所有点，不删除旧点）
    /// </summary>
    private void RecordTrajectoryPoint()
    {
        Vector3 currentPos = transform.position;
        currentPos.z = 0; // 固定Z轴

        // 避免重复记录同一位置（防止轨迹点冗余）
        if (trajectoryPoints.Count == 0 || Vector3.Distance(currentPos, trajectoryPoints[trajectoryPoints.Count - 1]) > 0.005f)
        {
            trajectoryPoints.Add(currentPos); // 只加不减，保留所有点
            // 更新轨迹渲染（包含从出发点到当前的所有点）
            lineRenderer.positionCount = trajectoryPoints.Count;
            lineRenderer.SetPositions(trajectoryPoints.ToArray());
        }
    }

    /// <summary>
    /// 重置轨迹（仅在场景重置时调用）
    /// </summary>
    public void ClearTrajectory()
    {
        trajectoryPoints.Clear();
        lineRenderer.positionCount = 0;
        lastRecordTime = 0;
        hasRecordedStartPoint = false; // 重置初始点标记
    }
}


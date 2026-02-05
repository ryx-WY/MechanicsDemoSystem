using UnityEngine;

/// <summary>
/// 轨迹绘制器（可选组件）
/// 挂载在小球上，如果需要轨迹则添加，不需要可省略
/// </summary>
[RequireComponent(typeof(PhysicsObjectBase))]
public class TrajectoryDrawer : MonoBehaviour
{
    private PhysicsObjectBase physicsObj;

    void Awake()
    {
        physicsObj = GetComponent<PhysicsObjectBase>();
        if (physicsObj == null)
        {
            Debug.LogError("TrajectoryDrawer需要PhysicsObjectBase组件");
            enabled = false;
        }
    }

    void Update()
    {
        // 运动中且未暂停时记录点
        if (SceneController.Instance != null &&
            SceneController.Instance.CurrentState == SimulationState.Running &&
            Time.timeScale > 0 &&
            TrajectoryManager.Instance.IsRecording())
        {
            TrajectoryManager.Instance.RecordTrajectoryPoint(transform.position);
        }
    }

    /// <summary>
    /// 开始记录本次轨迹（由物体控制器调用）
    /// </summary>
    public Color StartCurrentTrajectory()
    {
        return TrajectoryManager.Instance.StartNewTrajectory(gameObject, transform.position);
    }

    /// <summary>
    /// 停止记录本次轨迹（重置小球时调用，保留历史）
    /// </summary>
    public void StopCurrentTrajectory()
    {
        TrajectoryManager.Instance.StopRecording();
    }

    /// <summary>
    /// 清空所有历史轨迹（仅重置场景时调用）
    /// </summary>
    public void ClearAllTrajectories()
    {
        TrajectoryManager.Instance.ClearAllTrajectories();
    }
}
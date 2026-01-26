using UnityEngine;

public class TrajectoryDrawer : MonoBehaviour
{
    private ObjectPhysicsController physicsController;

    void Awake()
    {
        physicsController = GetComponent<ObjectPhysicsController>();
    }

    void Update()
    {
        // 运动中且未暂停时，通知管理器记录轨迹点
        if (GlobalUIController.Instance.IsMotionRunning() && 
            !GlobalUIController.Instance.IsMotionPaused() &&
            TrajectoryManager.Instance.IsRecording())
        {
            TrajectoryManager.Instance.RecordTrajectoryPoint(transform.position);
        }
    }

    /// <summary>
    /// 开始记录本次轨迹（由小球控制器调用）
    /// </summary>
    public Color StartCurrentTrajectory()
    {
        return TrajectoryManager.Instance.StartNewTrajectory(transform.position);
    }

    /// <summary>
    /// 停止记录本次轨迹
    /// </summary>
    public void StopCurrentTrajectory()
    {
        TrajectoryManager.Instance.StopRecording();
    }

    /// <summary>
    /// 清空所有轨迹（仅重置场景时调用）
    /// </summary>
    public void ClearAllTrajectories()
    {
        TrajectoryManager.Instance.ClearAllTrajectories();
    }
}
